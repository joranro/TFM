# Implementación de Máquina de Estados Finitos (FSM) para TurtleAgent

## Descripción
Esta implementación sustituye el comportamiento basado en ML-Agents por una máquina de estados finitos que replica exactamente la misma funcionalidad del agente original.

## Archivos Creados

### Core FSM Files:
- `TurtleStates.cs` - Enum que define todos los estados posibles
- `TurtleStateBase.cs` - Clase base abstracta para todos los estados
- `TurtleFSM.cs` - Máquina de estados finitos principal

### State Implementations:
- `TurtleIdleState.cs` - Estado de reposo
- `TurtleMovingState.cs` - Estado de movimiento hacia adelante
- `TurtleRotatingLeftState.cs` - Estado de giro a la izquierda
- `TurtleRotatingRightState.cs` - Estado de giro a la derecha
- `TurtleReachedGoalState.cs` - Estado cuando llega al objetivo
- `TurtleCollidingState.cs` - Estado cuando colisiona con paredes

### Agent and Utilities:
- `TurtleAgentFSM.cs` - Nueva versión del agente que trabaja con FSM
- `TurtleFSMSetup.cs` - Script de configuración automática

### Sistema de Métricas:
- `TurtleMetrics.cs` - Sistema completo de métricas de rendimiento
- `TurtleMetricsSetup.cs` - Configuración automática de métricas
- `TurtleMLAgentMetrics.cs` - Integración específica para ML-Agents
- `TurtleMLAgentMetricsSetup.cs` - Configuración automática para ML-Agents

## Análisis Detallado del Funcionamiento Interno de la FSM

### 1. Arquitectura General del Sistema

#### 1.1 Componentes Principales

**TurtleFSM (Controlador Principal)**
- **Responsabilidad**: Coordina todos los estados y maneja las transiciones
- **Patrón**: Implementa el patrón State Pattern con Dictionary para mapear estados
- **Ciclo de Vida**: 
  - `Awake()`: Inicializa el diccionario de estados
  - `Start()`: Establece el estado inicial (Idle)
  - `Update()`: Procesa input y actualiza el estado actual
  - `FixedUpdate()`: Ejecuta lógica física del estado actual

**TurtleAgentFSM (Agente de Comportamiento)**
- **Responsabilidad**: Proporciona la interfaz física y lógica del agente
- **Funciones Clave**:
  - Gestión de movimiento y rotación
  - Sistema de recompensas
  - Control de episodios
  - Interfaz con el sistema de métricas

**TurtleStateBase (Clase Base Abstracta)**
- **Responsabilidad**: Define la interfaz común para todos los estados
- **Métodos Virtuales**:
  - `Enter()`: Lógica de entrada al estado
  - `Update()`: Lógica de actualización por frame
  - `FixedUpdate()`: Lógica física por frame fijo
  - `Exit()`: Lógica de salida del estado
  - Eventos de colisión y triggers

#### 1.2 Flujo de Ejecución Principal

```
Inicialización:
TurtleFSM.Awake() → InitializeStates() → Crear Dictionary de estados
TurtleFSM.Start() → ChangeState(Idle) → TurtleIdleState.Enter()

Bucle Principal:
TurtleFSM.Update() → ProcessInput() → ChangeState() → Estado.Actualizar()
TurtleFSM.FixedUpdate() → Estado.FixedUpdate() → Movimiento Físico
```

### 2. Análisis Detallado de Métodos Clave

#### 2.1 TurtleFSM - Controlador Principal

**`InitializeStates()`**
```csharp
private void InitializeStates()
{
    states = new Dictionary<TurtleState, TurtleStateBase>
    {
        { TurtleState.Idle, new TurtleIdleState(this, agent) },
        { TurtleState.Moving, new TurtleMovingState(this, agent) },
        // ... otros estados
    };
}
```
- **Propósito**: Crea instancias de todos los estados posibles
- **Patrón**: Factory Pattern para instanciación de estados
- **Ventaja**: Centraliza la creación y facilita el mantenimiento

**`ProcessInput()`**
```csharp
private void ProcessInput()
{
    if (currentState == TurtleState.ReachedGoal || currentState == TurtleState.Colliding)
        return; // Estados especiales bloquean input

    if (autonomousMode)
        ProcessAutonomousInput();
    else
        ProcessManualInput();
}
```
- **Lógica**: Determina el tipo de input a procesar
- **Estados Especiales**: ReachedGoal y Colliding bloquean input
- **Modos**: Manual (teclado) vs Autónomo (navegación automática)

**`ProcessAutonomousInput()`**
```csharp
private void ProcessAutonomousInput()
{
    Vector3 goalPosition = agent.GetGoalPosition();
    Vector3 agentPosition = agent.GetAgentPosition();
    float agentRotation = agent.GetAgentRotation();
    
    Vector3 directionToGoal = (goalPosition - agentPosition).normalized;
    float distanceToGoal = Vector3.Distance(agentPosition, goalPosition);
    
    if (distanceToGoal < 0.3f)
    {
        ChangeState(TurtleState.Idle);
        return;
    }
    
    float targetAngle = Mathf.Atan2(directionToGoal.x, directionToGoal.z) * Mathf.Rad2Deg;
    float angleDifference = Mathf.DeltaAngle(agentRotation, targetAngle);
    
    if (Mathf.Abs(angleDifference) > 15f)
    {
        // Girar hacia el objetivo
        ChangeState(angleDifference > 0 ? TurtleState.RotatingRight : TurtleState.RotatingLeft);
    }
    else
    {
        // Navegar hacia el objetivo
        ChangeState(TurtleState.Navigating);
    }
}
```
- **Algoritmo**: Navegación basada en ángulos y distancias
- **Umbrales**: 0.3f para distancia, 15° para ángulo
- **Estados**: Idle (cerca), Rotating (orientar), Navigating (mover)

**`ChangeState(TurtleState newState)`**
```csharp
public void ChangeState(TurtleState newState)
{
    if (currentState == newState) return; // Evitar transiciones innecesarias

    if (debugMode)
        Debug.Log($"Turtle FSM: {currentState} -> {newState}");

    // Patrón: Exit → Cambio → Enter
    if (states.ContainsKey(currentState))
        states[currentState].Exit();

    currentState = newState;
    stateEnterTime = Time.time; // Para métricas

    if (states.ContainsKey(currentState))
        states[currentState].Enter();
}
```
- **Patrón**: Implementa el patrón State con transiciones limpias
- **Logging**: Registra transiciones para debugging
- **Métricas**: Registra tiempo de entrada para análisis

#### 2.2 TurtleAgentFSM - Agente de Comportamiento

**`Initialize()`**
```csharp
public void Initialize()
{
    Debug.Log("TurtleAgentFSM Initialize()");
    _currentStep = 0;
    _cumulativeReward = 0f;
    _renderer.material.color = Color.blue;
}
```
- **Propósito**: Reset completo del agente
- **Variables**: Contadores y recompensas
- **Visual**: Color inicial (azul)

**`ResetEpisode()`**
```csharp
public void ResetEpisode()
{
    var metrics = GetComponent<TurtleMetrics>();
    if (metrics != null)
    {
        metrics.OnEpisodeEnd(true); // Éxito
    }

    // Efectos visuales
    if (_groundRenderer != null && _cumulativeReward != 0f)
    {
        Color flashColor = (_cumulativeReward > 0f) ? Color.green : Color.red;
        _flashGroundCoroutine = StartCoroutine(FlashGround(flashColor, 3.0f));
    }

    // Reset completo
    _currentStep = 0;
    _cumulativeReward = 0f;
    _renderer.material.color = Color.blue;

    SpawnObjects(); // Nueva posición aleatoria
    _fsm.ChangeState(TurtleState.Idle); // Reset FSM
    
    // Nuevo episodio
    if (metrics != null)
        metrics.OnEpisodeStart();
}
```
- **Secuencia**: Métricas → Efectos → Reset → Spawn → FSM
- **Visual**: Flash del suelo según recompensa
- **Posicionamiento**: Spawn aleatorio de agente y objetivo

**`SpawnObjects()`**
```csharp
private void SpawnObjects()
{
    transform.localRotation = Quaternion.identity;
    transform.localPosition = new Vector3(0f, 0.15f, 0f);

    float randomAngle = Random.Range(0f, 360f);
    Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;
    float randomDistance = Random.Range(1f, 2.5f);
    Vector3 goalPosition = transform.localPosition + randomDirection * randomDistance;

    _goal.localPosition = new Vector3(goalPosition.x, 0.3f, goalPosition.z);
}
```
- **Agente**: Posición fija, rotación aleatoria
- **Objetivo**: Distancia aleatoria (1-2.5 unidades)
- **Altura**: Agente (0.15f), Objetivo (0.3f)

**`AddStepPenalty()`**
```csharp
public void AddStepPenalty()
{
    AddReward(-2f / _maxSteps);
}
```
- **Propósito**: Incentivar finalización rápida
- **Cálculo**: Penalización proporcional al máximo de pasos
- **Efecto**: -0.0004 por paso (con maxSteps = 5000)

#### 2.3 Estados Específicos - Análisis de Implementación

**TurtleMovingState**
```csharp
public override void Update()
{
    // El movimiento se maneja en FixedUpdate
}

public override void FixedUpdate()
{
    // Mover hacia adelante
    agent.transform.position += agent.transform.forward * agent.MoveSpeed * Time.fixedDeltaTime;
}
```
- **Movimiento**: Basado en forward vector
- **Física**: FixedUpdate para consistencia
- **Penalización**: Aplicada en ChangeState() para consistencia con ML-Agents

**TurtleRotatingLeftState / TurtleRotatingRightState**
```csharp
public override void Update()
{
    // La rotación se maneja en FixedUpdate
}

public override void FixedUpdate()
{
    // Girar a velocidad constante
    agent.transform.Rotate(0f, ±agent.RotationSpeed * Time.fixedDeltaTime, 0f);
}
```
- **Rotación**: Basada en velocidad constante
- **Física**: FixedUpdate para consistencia
- **Penalización**: Aplicada en ChangeState() para consistencia con ML-Agents

**TurtleIdleState**
```csharp
public override void Update()
{
    // En estado idle, el agente no hace nada
    // Las transiciones se manejan desde el FSM principal
}
```
- **Comportamiento**: Estado de reposo sin movimiento
- **Penalización**: Aplicada en ChangeState() para mantener consistencia con ML-Agents
- **Importancia**: Asegura que el tiempo de inactividad también se penalice

**TurtleNavigatingState**
```csharp
public override void Update()
{
    // Verificar dirección al objetivo periódicamente
    if (Time.time - lastDirectionCheck > DIRECTION_CHECK_INTERVAL)
    {
        lastDirectionCheck = Time.time;
        UpdateNavigation();
    }
}
```
- **Navegación**: Movimiento autónomo hacia el objetivo
- **Lógica**: Verificación periódica de dirección
- **Penalización**: Aplicada en ChangeState() para consistencia con ML-Agents

**TurtleCollidingState**
```csharp
public override void Update()
{
    // La penalización por colisión se maneja en FixedUpdate
}

public override void FixedUpdate()
{
    // Penalización continua mientras está colisionando
    agent.AddReward(-0.01f * Time.fixedDeltaTime);
}
```
- **Colisión**: Estado especial durante contacto con paredes
- **Penalización**: Solo colisión continua (paso se aplica en ChangeState)
- **Visual**: Color rojo inmediato

### 3. Sistema de Eventos y Colisiones

#### 3.1 Manejo de Colisiones
```csharp
private void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Wall"))
    {
        IsColliding = true;
        ChangeState(TurtleState.Colliding);
    }
}

private void OnCollisionExit(Collision collision)
{
    if (collision.gameObject.CompareTag("Wall"))
    {
        IsColliding = false;
        if (currentState == TurtleState.Colliding)
        {
            ChangeState(TurtleState.Idle);
        }
    }
}
```
- **Detección**: Basada en tags ("Wall", "Goal")
- **Estado**: IsColliding para tracking
- **Transición**: Automática entre Colliding e Idle

#### 3.2 Manejo de Triggers
```csharp
private void OnTriggerEnter(Collider other)
{
    if (other.gameObject.CompareTag("Goal"))
    {
        ChangeState(TurtleState.ReachedGoal);
    }
}
```
- **Objetivo**: Trigger para detección precisa
- **Inmediato**: Transición instantánea a ReachedGoal

### 4. Integración con Sistema de Métricas

#### 4.1 Puntos de Integración
```csharp
// En TurtleAgentFSM.Start()
var metrics = GetComponent<TurtleMetrics>();
if (metrics != null)
{
    metrics.OnEpisodeStart();
}

// En TurtleAgentFSM.ResetEpisode()
if (metrics != null)
{
    metrics.OnEpisodeEnd(true);
    metrics.OnEpisodeStart();
}
```
- **Sincronización**: Métricas se sincronizan con episodios
- **Datos**: Tiempo, pasos, recompensas, colisiones
- **Archivos**: CSV y TXT para análisis posterior

### 5. Consistencia del Sistema de Recompensas

#### 5.1 Comparación FSM vs ML-Agents

**Fórmula de Penalización por Paso:**
```csharp
// ML-Agents (TurtleAgent.cs)
AddReward(-2f / MaxStep); // En OnActionReceived()

// FSM (TurtleAgentFSM.cs)
AddReward(-2f / _maxSteps); // En AddStepPenalty()
```

**Frecuencia de Aplicación:**
- **ML-Agents**: Cada vez que recibe una acción del modelo (`OnActionReceived()`)
- **FSM**: Cada vez que cambia de estado (`ChangeState()`)

**Estados que Aplican Penalización:**
```csharp
// La penalización se aplica en el controlador FSM, no en los estados
public void ChangeState(TurtleState newState)
{
    // ... lógica de cambio de estado ...
    agent.AddStepPenalty(); // Consistente con ML-Agents
}
```

#### 5.2 Ventajas de la Implementación Unificada

1. **Comparación Justa**: Ambos agentes usan exactamente la misma fórmula
2. **Frecuencia Similar**: ML-Agents en cada decisión, FSM en cada cambio de estado
3. **Comportamiento Predecible**: FSM es determinista, ML-Agents varía
4. **Métricas Confiables**: Sistema de recompensas idéntico permite análisis directo

#### 5.3 Resultado Esperado en Métricas

Con esta implementación, las métricas deberían mostrar:
- **FSM**: Recompensa promedio más baja (penalizaciones consistentes)
- **ML-Agents**: Recompensa promedio más alta (navegación más eficiente)
- **Comparación Válida**: Diferencia en recompensas refleja diferencia en eficiencia

### 6. Ventajas Arquitectónicas de la Implementación

#### 6.1 Modularidad
- **Estados Independientes**: Cada estado es una clase separada
- **Fácil Extensión**: Nuevos estados sin modificar código existente
- **Mantenimiento**: Cambios localizados en estados específicos

#### 6.2 Flexibilidad
- **Modos de Operación**: Manual vs Autónomo
- **Configuración**: Parámetros ajustables desde Inspector
- **Debugging**: Logs detallados de transiciones

#### 6.3 Performance
- **Eficiencia**: Solo un estado activo por frame
- **Memoria**: Dictionary pre-allocado
- **Física**: FixedUpdate para consistencia

#### 6.4 Comparabilidad
- **Métricas Unificadas**: Mismo sistema que ML-Agents
- **Datos Estructurados**: CSV para análisis estadístico
- **Visualización**: Interfaz en tiempo real

### 6. Diferencias Clave con ML-Agents

#### 6.1 Comportamiento
- **FSM**: Determinista y predecible
- **ML-Agents**: Basado en aprendizaje, variable

#### 6.2 Control
- **FSM**: Estados explícitos y controlables
- **ML-Agents**: Red neuronal interna, menos transparente

#### 6.3 Debugging
- **FSM**: Estados visibles y logs detallados
- **ML-Agents**: Comportamiento más opaco

#### 6.4 Performance
- **FSM**: Más eficiente para comportamientos simples
- **ML-Agents**: Mejor para comportamientos complejos

#### 6.5 Sistema de Recompensas
- **FSM**: Sistema idéntico a ML-Agents (consistencia garantizada)
- **ML-Agents**: Mismo sistema de recompensas que FSM
- **Comparación**: Métricas de recompensa directamente comparables
- **Frecuencia**: Ambos aplican penalizaciones en cada frame de actualización

Esta implementación FSM proporciona una base sólida para el análisis comparativo con ML-Agents, manteniendo la misma interfaz y funcionalidad mientras ofrece transparencia total en el comportamiento del agente.

## Estados de la FSM

### 1. Idle
- **Descripción**: Estado inicial, esperando input del usuario
- **Transiciones**: 
  - → Moving (Flecha Arriba)
  - → RotatingLeft (Flecha Izquierda)
  - → RotatingRight (Flecha Derecha)

### 2. Moving
- **Descripción**: El agente se mueve hacia adelante
- **Acciones**: 
  - Mover hacia adelante a velocidad constante
  - Aplicar penalización por paso
- **Transiciones**:
  - → Idle (sin input)
  - → RotatingLeft (Flecha Izquierda)
  - → RotatingRight (Flecha Derecha)
  - → ReachedGoal (toca objetivo)
  - → Colliding (toca pared)

### 3. RotatingLeft
- **Descripción**: El agente gira a la izquierda
- **Acciones**:
  - Rotar a velocidad constante
  - Aplicar penalización por paso
- **Transiciones**: Igual que Moving

### 4. RotatingRight
- **Descripción**: El agente gira a la derecha
- **Acciones**:
  - Rotar a velocidad constante
  - Aplicar penalización por paso
- **Transiciones**: Igual que Moving

### 5. ReachedGoal
- **Descripción**: El agente ha llegado al objetivo
- **Acciones**:
  - Dar recompensa de +10
  - Cambiar color a verde
  - Reiniciar episodio después de 1 segundo
  - **Bloquea todo input** hasta que se complete el reset
- **Transiciones**: → Idle (cuando se completa el reset del episodio)

### 6. Navigating
- **Descripción**: El agente navega autónomamente hacia el objetivo
- **Acciones**:
  - Mover hacia adelante continuamente
  - Verificar dirección al objetivo periódicamente
  - Aplicar penalización por paso
- **Transiciones**:
  - → Idle (cuando está muy cerca del objetivo)
  - → RotatingLeft/Right (cuando necesita girar)
  - → ReachedGoal (cuando toca el objetivo)
  - → Colliding (cuando toca una pared)

### 7. Colliding
- **Descripción**: El agente está colisionando con una pared
- **Acciones**:
  - Cambiar color a rojo
  - Aplicar penalización inicial de -0.05
  - Penalización continua de -0.01 por frame
- **Transiciones**:
  - → Idle (cuando sale de la colisión)

## Configuración en Unity

### Paso 1: Preparar el GameObject
1. Selecciona el GameObject que tiene el componente `TurtleAgent` original
2. Asegúrate de que tiene:
   - Collider (para detectar colisiones)
   - Rigidbody (para física)
   - Renderer (para cambiar colores)

### Paso 2: Configuración Automática (Recomendado)
1. Añade el componente `TurtleFSMSetup` al GameObject
2. Marca la casilla "Auto Setup"
3. Ejecuta la escena - se configurará automáticamente

**NOTA IMPORTANTE**: Si ves errores de compilación, asegúrate de que:
- Todos los scripts FSM están en la carpeta `Assets/Scripts/FSM/`
- El componente `TurtleAgent` original ha sido eliminado
- Los componentes `TurtleAgentFSM` y `TurtleFSM` están presentes

### Paso 3: Configuración Manual
Si prefieres configurar manualmente:

1. **Eliminar componente ML-Agents**:
   - Elimina el componente `TurtleAgent` del GameObject

2. **Añadir componentes FSM**:
   - Añade `TurtleAgentFSM`
   - Añade `TurtleFSM`
   - Añade `TurtleMetricsSetup` (para métricas automáticas)

3. **Configurar referencias en el Inspector**:
       - **TurtleAgentFSM**:
      - Goal: Arrastra el Transform del objetivo
      - Ground Renderer: Arrastra el Renderer del suelo
      - Move Speed: 1.5 (por defecto)
      - Rotation Speed: 180 (por defecto)
      - Max Steps: 5000 (por defecto, modificable desde la interfaz)
      - Enable Reward Logs: Desactivado (para evitar logs constantes)

       - **TurtleFSM**:
      - Debug Mode: Activado para ver logs de transiciones
      - Autonomous Mode: Activado para navegación automática
      - Current State: Se actualiza automáticamente
   
   - **TurtleMetricsSetup** (opcional):
     - Auto Setup: Activado
     - Enable Metrics: Activado
     - Log To File: Activado
     - Max Episodes: 100 (por defecto)
     - Show On Screen: Activado
     - Toggle Key: F2 (por defecto)

## Controles

### Modo Manual (Autonomous Mode = false)
- **Flecha Arriba**: Mover hacia adelante
- **Flecha Izquierda**: Girar a la izquierda
- **Flecha Derecha**: Girar a la derecha

### Modo Autónomo (Autonomous Mode = true)
- **El agente navega automáticamente hacia el objetivo**
- **No requiere input del usuario**
- **Cambia entre rotación y movimiento según sea necesario**

### Debug
- **F2**: Mostrar/ocultar métricas de rendimiento

## Sistema de Recompensas

La FSM replica exactamente el sistema de recompensas del agente original, asegurando **consistencia total** con ML-Agents:

### **Recompensas Idénticas:**
- **Recompensa por llegar al objetivo**: +10
- **Penalización por paso**: -2/MaxSteps (aplicada en cada frame)
- **Penalización por colisión inicial**: -0.05
- **Penalización por colisión continua**: -0.01 por frame

### **Frecuencia de Aplicación Unificada:**
- **ML-Agents**: Penalización por paso en cada `OnActionReceived()` (cada decisión del modelo)
- **FSM**: Penalización por paso en cada `ChangeState()` (cada cambio de estado)

### **Estados que Aplican Penalización por Paso:**
- **Todos los estados**: ❌ Ya no aplican penalización por paso en cada frame
- **FSM Controller**: ✅ Aplica penalización por paso en cada `ChangeState()` (cada cambio de estado)

### **Implementación Técnica:**

**TurtleAgentFSM.AddStepPenalty():**
```csharp
public void AddStepPenalty()
{
    AddReward(-2f / _maxSteps);
}
```

**Aplicación en FSM Controller (TurtleFSM.ChangeState()):**
```csharp
public void ChangeState(TurtleState newState)
{
    // ... lógica de cambio de estado ...
    
    // Aplicar penalización por paso al cambiar de estado (consistente con ML-Agents OnActionReceived)
    if (agent != null)
    {
        agent.AddStepPenalty();
    }
}
```

### **Consistencia Garantizada:**

1. **Misma Fórmula**: Ambos agentes usan `-2f / MaxSteps`
2. **Frecuencia Similar**: ML-Agents en cada decisión, FSM en cada cambio de estado
3. **Mismos Valores**: Recompensas y penalizaciones idénticas
4. **Mismo Comportamiento**: Colisiones y objetivos se manejan igual

### **Resultado Esperado:**

Con esta implementación unificada, las métricas de recompensa deberían reflejar correctamente:
- **FSM**: Recompensa promedio más baja (por aplicar penalizaciones consistentemente)
- **ML-Agents**: Recompensa promedio más alta (por ser más eficiente en navegación)

Esto permite una **comparación justa** entre ambos sistemas de comportamiento.

## Debugging

### Métricas de Rendimiento (F2)
- **Tiempo por episodio**
- **Pasos por episodio**
- **Distancia recorrida**
- **Número de colisiones**
- **Cambios de dirección**
- **Eficiencia de ruta** (distancia directa vs recorrida)
- **Tasa de éxito**
- **Promedios estadísticos**
- **Exportación a CSV**

**Nota**: Los logs de debug en consola solo aparecen cuando hay transiciones de estado (si Debug Mode está activado en TurtleFSM). Los logs de recompensas están desactivados por defecto para evitar spam en la consola.

## Sistema de Métricas

### Métricas Capturadas:
- **Tiempo por episodio**: Duración total de cada episodio
- **Pasos por episodio**: Número de pasos hasta completar el objetivo
- **Distancia recorrida**: Distancia total que recorre el agente
- **Colisiones**: Número de veces que colisiona con paredes
- **Cambios de dirección**: Frecuencia de rotaciones del agente
- **Eficiencia de ruta**: Ratio entre distancia directa y distancia recorrida
- **Tasa de éxito**: Porcentaje de episodios exitosos
- **Recompensa final**: Recompensa acumulada al final del episodio

### Archivos Generados:
- **turtle_metrics.csv**: Datos detallados de cada episodio
- **turtle_metrics_summary.txt**: Resumen estadístico de todos los episodios

### Comparación FSM vs ML-Agents:
El sistema de métricas permite comparar directamente:
- **Consistencia**: FSM es determinista, ML-Agents varía
- **Eficiencia**: Tiempo y pasos promedio
- **Robustez**: Tasa de éxito en diferentes condiciones
- **Comportamiento**: Patrones de movimiento y colisiones

## Ventajas de la Implementación FSM

1. **Comportamiento Determinista**: No depende de aprendizaje
2. **Fácil Debugging**: Estados claros y visibles
3. **Modularidad**: Cada estado es independiente
4. **Extensibilidad**: Fácil añadir nuevos estados
5. **Performance**: Más eficiente que ML-Agents para comportamientos simples

## Migración desde ML-Agents

La implementación FSM mantiene la misma interfaz y comportamiento que el agente original:

- Mismos controles de entrada
- Mismo sistema de recompensas
- Mismos efectos visuales
- Misma lógica de spawn aleatorio
- Misma detección de colisiones

## Troubleshooting

### Problema: El agente no se mueve
- Verifica que el componente `TurtleFSM` esté presente
- Comprueba que las teclas de dirección funcionen
- Revisa los logs de la consola para errores

### Problema: No detecta colisiones
- Verifica que el GameObject tenga Collider
- Asegúrate de que las paredes tengan el tag "Wall"
- Comprueba que el objetivo tenga el tag "Goal"

### Problema: No cambia de color
- Verifica que el GameObject tenga Renderer
- Comprueba que el material sea modificable

## Extensión de la FSM

Para añadir nuevos estados:

1. Añade el nuevo estado al enum `TurtleStates`
2. Crea una nueva clase que herede de `TurtleStateBase`
3. Implementa los métodos necesarios (Enter, Update, Exit, etc.)
4. Añade el estado al diccionario en `TurtleFSM.InitializeStates()`
5. Añade las transiciones necesarias en `TurtleFSM.ProcessInput()`

## Configuración para ML-Agents

### Integración con Sistema de Métricas

Para que el agente ML-Agents original funcione correctamente con el sistema de métricas:

#### **Opción 1: Configuración Automática**
1. Añade el componente `TurtleMLAgentMetricsSetup` al GameObject del agente ML-Agents
2. El script configurará automáticamente:
   - `TurtleMLAgentMetrics` - Integración específica para ML-Agents
   - `TurtleMetrics` - Sistema de métricas general
3. Las métricas se mostrarán en la **esquina inferior izquierda**

#### **Opción 2: Configuración Manual**
1. Añade `TurtleMLAgentMetrics` al GameObject del agente ML-Agents
2. Añade `TurtleMetrics` al mismo GameObject
3. Configura los parámetros en el Inspector:
   - **Enable Metrics**: Habilitar/deshabilitar métricas
   - **Log to File**: Exportar datos a archivos
   - **Max Episodes**: Número máximo de episodios a registrar
   - **Show on Screen**: Mostrar métricas en pantalla
   - **Toggle Key**: Tecla para mostrar/ocultar (F2 por defecto)

### **Detección Automática de Tipo de Agente**

El sistema detecta automáticamente si es FSM o ML-Agents:
- **FSM**: Detecta `TurtleAgentFSM` + `TurtleFSM`
- **ML-Agents**: Detecta `TurtleAgent`
- **Posicionamiento**: FSM (esquina superior derecha), ML-Agents (esquina inferior izquierda)

### **Archivos Separados por Tipo**

- **FSM**: `turtle_metrics_FSM.csv` y `turtle_metrics_FSM_summary.txt`
- **ML-Agents**: `turtle_metrics_MLAgents.csv` y `turtle_metrics_MLAgents_summary.txt`

### **Comparación Simultánea**

Puedes ejecutar ambos agentes en la misma escena:
- ✅ **Sin solapamiento** de ventanas
- ✅ **Archivos separados** para análisis
- ✅ **Identificación clara** de cada agente
- ✅ **Comparación en tiempo real** del rendimiento 
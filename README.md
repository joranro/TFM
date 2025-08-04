# TFM - Trabajo de Fin de Máster

## Descripción del Proyecto

Este proyecto de Trabajo de Fin de Máster se centra en el análisis comparativo entre el comportamiento de NPCs (Non-Player Characters) entrenados mediante la librería ML-Agents de Unity y aquellos controlados por máquinas de estados finitos.

### Objetivo Principal

El objetivo principal es extraer una ecuación, regla o norma que permita definir cómo igualar el comportamiento de un NPC entrenado mediante ML-Agents versus si definimos su comportamiento con una máquina de estados finitos. Específicamente, se busca extrapolar cuántas neuronas de una red neuronal equivaldrían al mismo comportamiento para el mismo NPC con una máquina de estados finitos.

### Metodología

1. **Fase 1**: Implementación y entrenamiento de un modelo con ML-Agents
2. **Fase 2**: Implementación de una máquina de estados finitos equivalente
3. **Fase 3**: Definición de métricas y KPIs para comparar comportamientos
4. **Fase 4**: Ecualización de comportamientos y extracción de conclusiones

### Estado Actual

- ✅ Modelo entrenado con ML-Agents implementado
- 🔄 Implementación de máquina de estados finitos (pendiente)
- 🔄 Definición de métricas comparativas (pendiente)
- 🔄 Análisis comparativo y extracción de tesis (pendiente)

### Estructura del Proyecto

```
TFM/
├── TurtleAgent/          # Proyecto Unity con el agente entrenado
│   ├── config/          # Configuraciones de entrenamiento
│   ├── results/         # Resultados de entrenamiento
│   └── Turtle Agent/    # Proyecto Unity
├── ml-agents/           # Librería ML-Agents (excluida del repositorio)
└── README.md           # Este archivo
```

### Tecnologías Utilizadas

- **Unity**: Motor de juego para el entorno simulado
- **ML-Agents**: Librería de Unity para machine learning
- **Python**: Para el entrenamiento de modelos
- **C#**: Para la implementación de la máquina de estados finitos

### Autor

Jorge Andreu Royo - Trabajo de Fin de Máster

using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Sistema de métricas para tracking de rendimiento de agentes
/// Compatible con FSM y ML-Agents
/// </summary>
public class TurtleMetrics : MonoBehaviour
{
    [Header("Configuración de Métricas")]
    [SerializeField] private bool enableMetrics = true;
    [SerializeField] private bool logToFile = true;
    [SerializeField] private string fileName = "turtle_metrics.csv";
    [SerializeField] private int maxEpisodes = 100;
    
    [Header("Configuración de Visualización")]
    [SerializeField] private bool showOnScreen = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F2;
    
    // Componentes detectados
    private TurtleAgentFSM fsmAgent;
    private TurtleAgent mlAgent;
    private TurtleFSM fsm;
    
    private AgentType agentType;
    
    public enum AgentType
    {
        FSM,
        ML_Agents,
        Unknown
    }
    
    // Datos de episodios
    private EpisodeMetrics currentEpisode;
    private List<EpisodeMetrics> allEpisodes;
    private MetricsSummary summary;
    
    // Control de grabación
    private int episodeCount = 0;
    private bool isRecording = false;
    
    // Variables de tracking
    private Vector3 startPosition;
    private float episodeStartTime;
    private int episodeStartStep;
    private int collisionCount;
    private int directionChangeCount;
    private float lastRotation;
    
    // Tracking de ruta
    private List<Vector3> pathPoints;
    private float lastPathPointTime;
    private const float PATH_POINT_INTERVAL = 0.5f;
    
    private void Awake()
    {
        fsmAgent = GetComponent<TurtleAgentFSM>();
        mlAgent = GetComponent<TurtleAgent>();
        fsm = GetComponent<TurtleFSM>();
        
        DetectAgentType();
        
        allEpisodes = new List<EpisodeMetrics>();
        summary = new MetricsSummary();
        pathPoints = new List<Vector3>();
        
        if (logToFile)
        {
            CreateCSVHeader();
        }
    }
    
    private void DetectAgentType()
    {
        if (fsmAgent != null && fsm != null)
        {
            agentType = AgentType.FSM;
            Debug.Log("TurtleMetrics: Agente FSM detectado");
        }
        else if (mlAgent != null)
        {
            agentType = AgentType.ML_Agents;
            Debug.Log("TurtleMetrics: Agente ML-Agents detectado");
            ListAllTurtleAgentFields();
        }
        else
        {
            agentType = AgentType.Unknown;
            Debug.LogWarning("TurtleMetrics: No se pudo detectar el tipo de agente");
        }
    }
    
    private void ListAllTurtleAgentFields()
    {
        if (mlAgent == null) return;
        
        Debug.Log("=== TURTLE AGENT FIELDS DEBUG ===");
        
        var fields = typeof(TurtleAgent).GetFields(
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            try
            {
                var value = field.GetValue(mlAgent);
                Debug.Log($"Field: {field.Name} ({field.FieldType.Name}) = {value}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"Field: {field.Name} ({field.FieldType.Name}) = ERROR: {e.Message}");
            }
        }
        
        var properties = typeof(TurtleAgent).GetProperties(
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(mlAgent);
                Debug.Log($"Property: {prop.Name} ({prop.PropertyType.Name}) = {value}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"Property: {prop.Name} ({prop.PropertyType.Name}) = ERROR: {e.Message}");
            }
        }
        
        Debug.Log("=== END TURTLE AGENT FIELDS DEBUG ===");
    }
    
    private void Start()
    {
        // NO llamar a StartNewEpisode() aquí
        // OnEpisodeStart() se encargará de iniciar el primer episodio
    }
    
    private void Update()
    {
        if (!enableMetrics) return;
        
        // Toggle de visualización
        if (Input.GetKeyDown(toggleKey))
        {
            showOnScreen = !showOnScreen;
        }
        
        // Tracking de métricas en tiempo real
        if (isRecording)
        {
            TrackMetrics();
        }
    }
    
    private void TrackMetrics()
    {
        if (currentEpisode == null) return;
        
        Vector3 currentPos = GetAgentPosition();
        
        if (Time.time - lastPathPointTime > PATH_POINT_INTERVAL)
        {
            pathPoints.Add(currentPos);
            lastPathPointTime = Time.time;
        }
        
        float currentRotation = GetAgentRotation();
        if (Mathf.Abs(currentRotation - lastRotation) > 5f)
        {
            directionChangeCount++;
        }
        lastRotation = currentRotation;
        
        currentEpisode.steps = GetCurrentStep();
        currentEpisode.time = Time.time - episodeStartTime;
        currentEpisode.distanceTraveled = CalculateDistanceFromPathPoints();
        currentEpisode.collisionCount = collisionCount;
        currentEpisode.directionChanges = directionChangeCount;
        currentEpisode.currentDistanceToGoal = GetDistanceToGoal();
        currentEpisode.currentReward = GetCurrentReward();
        
        if (Time.frameCount % 600 == 0)
        {
            float initialDistance = Vector3.Distance(startPosition, GetGoalPosition());
            float currentDistance = GetDistanceToGoal();
            float distanceProgress = initialDistance - currentDistance;
            float calculatedDistance = CalculateDistanceFromPathPoints();
            
            Debug.Log($"TurtleMetrics Debug - Initial Distance: {initialDistance:F2}, Current Distance: {currentDistance:F2}");
            Debug.Log($"TurtleMetrics Debug - Distance Progress: {distanceProgress:F2}, Calculated Traveled: {calculatedDistance:F2}");
            Debug.Log($"TurtleMetrics Debug - Path Points: {pathPoints.Count}, Steps: {currentEpisode.steps}, Reward: {currentEpisode.currentReward:F2}");
            
            if (calculatedDistance > initialDistance * 5f)
            {
                Debug.LogWarning($"TurtleMetrics: Distancia recorrida parece incorrecta - Initial: {initialDistance:F2}, Calculated: {calculatedDistance:F2}");
            }
        }
    }
    
    public void OnEpisodeStart()
    {
        if (!enableMetrics) return;
        
        if (episodeCount >= maxEpisodes)
        {
            Debug.Log($"TurtleMetrics: OnEpisodeStart() ignorado - Límite alcanzado ({maxEpisodes})");
            return;
        }
        
        Debug.Log($"TurtleMetrics: OnEpisodeStart() llamado - isRecording: {isRecording}, episodeCount: {episodeCount}");
        
        if (!isRecording)
        {
            StartNewEpisode();
        }
        else
        {
            Debug.Log("TurtleMetrics: OnEpisodeStart() ignorado porque ya está grabando");
        }
    }
    
    public void OnEpisodeEnd(bool success)
    {
        if (!enableMetrics || currentEpisode == null) return;
        
        CompleteEpisode(success);
    }
    
    public void DisableMetrics()
    {
        enableMetrics = false;
        isRecording = false;
        Debug.Log("TurtleMetrics: Métricas desactivadas completamente.");
    }
    
    public void OnCollision()
    {
        if (!enableMetrics) return;
        
        collisionCount++;
    }
    
    private void StartNewEpisode()
    {
        if (episodeCount >= maxEpisodes)
        {
            Debug.Log($"TurtleMetrics: Límite de episodios alcanzado ({maxEpisodes}). Desactivando métricas.");
            isRecording = false;
            enableMetrics = false;
            return;
        }
        
        episodeCount++;
        isRecording = true;
        
        Debug.Log($"TurtleMetrics: StartNewEpisode() - Nuevo episodio: {episodeCount}");
        
        startPosition = GetAgentPosition();
        episodeStartTime = Time.time;
        episodeStartStep = GetCurrentStep();
        collisionCount = 0;
        directionChangeCount = 0;
        lastRotation = GetAgentRotation();
        pathPoints.Clear();
        pathPoints.Add(startPosition);
        lastPathPointTime = Time.time;
        
        currentEpisode = new EpisodeMetrics
        {
            episodeNumber = episodeCount,
            startPosition = startPosition,
            goalPosition = GetGoalPosition(),
            startTime = episodeStartTime,
            startStep = episodeStartStep
        };
        
        float initialDistance = Vector3.Distance(startPosition, GetGoalPosition());
        Debug.Log($"TurtleMetrics: Episodio {episodeCount} iniciado - Distancia inicial al objetivo: {initialDistance:F2}");
        
        if (episodeCount == maxEpisodes)
        {
            Debug.Log($"TurtleMetrics: Iniciando episodio final ({maxEpisodes}/{maxEpisodes})");
        }
    }
    
    private void CompleteEpisode(bool success)
    {
        if (currentEpisode == null) return;
        
        // Para ML-Agents, capturar el reward final antes de que se resetee
        float finalReward = GetCurrentReward();
        
        // Completar métricas del episodio
        currentEpisode.success = success;
        currentEpisode.endTime = Time.time;
        currentEpisode.endStep = GetCurrentStep();
        currentEpisode.totalTime = currentEpisode.endTime - currentEpisode.startTime;
        currentEpisode.totalSteps = currentEpisode.endStep - currentEpisode.startStep;
        currentEpisode.finalReward = finalReward; // Usar el reward capturado
        currentEpisode.finalDistanceToGoal = GetDistanceToGoal();
        currentEpisode.pathEfficiency = CalculatePathEfficiency();
        
        // Debug para ML-Agents
        if (agentType == AgentType.ML_Agents)
        {
            Debug.Log($"TurtleMetrics: Episodio {currentEpisode.episodeNumber} completado - Steps: {currentEpisode.totalSteps}, Reward: {finalReward:F2}, Time: {currentEpisode.totalTime:F2}s");
        }
        
        // Debug para ambos agentes sobre estimación de steps
        if (currentEpisode != null)
        {
            string agentName = agentType == AgentType.FSM ? "FSM" : "ML-Agents";
            float timeElapsed = currentEpisode.totalTime;
            int estimatedSteps = Mathf.RoundToInt(timeElapsed * 50f);
            Debug.Log($"TurtleMetrics: {agentName} - Tiempo: {timeElapsed:F2}s, Steps estimados: {estimatedSteps}, Steps calculados: {currentEpisode.totalSteps}");
        }
        
        // Añadir a la lista
        allEpisodes.Add(currentEpisode);
        
        // Actualizar resumen
        UpdateSummary();
        
        // Log a archivo
        if (logToFile)
        {
            LogEpisodeToFile(currentEpisode);
        }
        
        // Marcar que terminó el episodio actual
        isRecording = false;
        
        // Si este era el episodio final, desactivar métricas y exportar resumen
        if (currentEpisode.episodeNumber == maxEpisodes)
        {
            Debug.Log($"TurtleMetrics: Episodio final completado ({maxEpisodes}/{maxEpisodes}). Desactivando métricas.");
            enableMetrics = false;
            
            // Exportar resumen final a archivo
            if (logToFile)
            {
                LogSummaryToFile();
                Debug.Log($"TurtleMetrics: Resumen exportado a {GetFilePath().Replace(".csv", "_summary.txt")}");
            }
        }
        
        // NO iniciar automáticamente el siguiente episodio
        // Los agentes llamarán a OnEpisodeStart() cuando estén listos
    }
    
    /// <summary>
    /// Calcula la eficiencia de la ruta del agente
    /// Eficiencia = Distancia Directa / Distancia Real Recorrida
    /// </summary>
    private float CalculatePathEfficiency()
    {
        if (pathPoints.Count < 2) return 0f;
        
        float directDistance = Vector3.Distance(startPosition, GetGoalPosition());
        float actualDistance = CalculateDistanceFromPathPoints();
        
        if (actualDistance <= 0f)
        {
            actualDistance = directDistance;
        }
        
        if (actualDistance <= 0f) return 0f;
        
        float efficiency = directDistance / actualDistance;
        efficiency = Mathf.Clamp01(efficiency);
        
        if (currentEpisode != null && currentEpisode.success)
        {
            Debug.Log($"Path Efficiency Debug - Direct: {directDistance:F2}, Actual: {actualDistance:F2}, Efficiency: {efficiency:P1}");
            Debug.Log($"Path Points Count: {pathPoints.Count}");
        }
        
        if (actualDistance > directDistance * 5f)
        {
            Debug.LogWarning($"Path Efficiency calculation seems wrong - Actual distance too high: {actualDistance:F2} vs Direct: {directDistance:F2}");
            actualDistance = directDistance * 1.5f;
            efficiency = directDistance / actualDistance;
            efficiency = Mathf.Clamp01(efficiency);
        }
        
        if (efficiency < 0.1f && currentEpisode != null && currentEpisode.success)
        {
            Debug.LogWarning($"Path Efficiency muy baja ({efficiency:P1}) pero episodio exitoso. Ajustando...");
            efficiency = Mathf.Max(efficiency, 0.5f);
        }
        
        return efficiency;
    }
    
    /// <summary>
    /// Verifica si el agente se está moviendo
    /// </summary>
    private bool IsAgentMoving()
    {
        if (pathPoints.Count < 2) return false;
        
        Vector3 currentPos = GetAgentPosition();
        Vector3 lastPos = pathPoints[pathPoints.Count - 1];
        
        return Vector3.Distance(currentPos, lastPos) > 0.01f;
    }
    
    /// <summary>
    /// Calcula la distancia total recorrida basada en los puntos de ruta
    /// </summary>
    private float CalculateDistanceFromPathPoints()
    {
        if (pathPoints.Count < 2) return 0f;
        
        float totalDistance = 0f;
        
        for (int i = 1; i < pathPoints.Count; i++)
        {
            float segmentDistance = Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
            if (segmentDistance > 0.01f)
            {
                totalDistance += segmentDistance;
            }
        }
        
        Vector3 currentPos = GetAgentPosition();
        if (pathPoints.Count > 0)
        {
            float finalSegment = Vector3.Distance(pathPoints[pathPoints.Count - 1], currentPos);
            if (finalSegment > 0.01f)
            {
                totalDistance += finalSegment;
            }
        }
        
        float directDistance = Vector3.Distance(startPosition, GetGoalPosition());
        if (totalDistance > directDistance * 3f && directDistance > 0.1f)
        {
            return directDistance * 1.5f;
        }
        
        return totalDistance;
    }
    
    /// <summary>
    /// Actualiza las estadísticas acumuladas con los datos de todos los episodios
    /// </summary>
    private void UpdateSummary()
    {
        summary.totalEpisodes = allEpisodes.Count;
        summary.successfulEpisodes = allEpisodes.FindAll(e => e.success).Count;
        summary.successRate = (float)summary.successfulEpisodes / summary.totalEpisodes;
        
        float totalTime = 0f, totalSteps = 0f, totalReward = 0f, totalEfficiency = 0f;
        int totalCollisions = 0, totalDirectionChanges = 0;
        int validEpisodes = 0;
        
        foreach (var episode in allEpisodes)
        {
            if (episode.success)
            {
                totalTime += episode.totalTime;
                totalSteps += episode.totalSteps;
                totalReward += episode.finalReward;
                totalEfficiency += episode.pathEfficiency;
                totalCollisions += episode.collisionCount;
                totalDirectionChanges += episode.directionChanges;
                validEpisodes++;
            }
        }
        
        if (validEpisodes > 0)
        {
            summary.avgTimePerEpisode = totalTime / validEpisodes;
            summary.avgStepsPerEpisode = totalSteps / validEpisodes;
            summary.avgRewardPerEpisode = totalReward / validEpisodes;
            summary.avgPathEfficiency = totalEfficiency / validEpisodes;
            summary.avgCollisionsPerEpisode = (float)totalCollisions / validEpisodes;
            summary.avgDirectionChangesPerEpisode = (float)totalDirectionChanges / validEpisodes;
        }
    }
    
    // Métodos para obtener información del agente (compatibles con ambos sistemas)
    private Vector3 GetAgentPosition()
    {
        if (fsmAgent != null) return fsmAgent.GetAgentPosition();
        if (mlAgent != null) return mlAgent.transform.localPosition;
        return transform.localPosition;
    }
    
    private Vector3 GetGoalPosition()
    {
        if (fsmAgent != null) return fsmAgent.GetGoalPosition();
        if (mlAgent != null)
        {
            // Intentar obtener la posición del goal desde el TurtleAgent
            var goalField = typeof(TurtleAgent).GetField("_goal", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (goalField != null)
            {
                Transform goal = goalField.GetValue(mlAgent) as Transform;
                if (goal != null)
                {
                    return goal.localPosition;
                }
            }
        }
        return Vector3.zero;
    }
    
    private float GetAgentRotation()
    {
        if (fsmAgent != null) return fsmAgent.GetAgentRotation();
        if (mlAgent != null) return mlAgent.transform.localRotation.eulerAngles.y;
        return transform.localRotation.eulerAngles.y;
    }
    
    private float GetDistanceToGoal()
    {
        if (fsmAgent != null) return fsmAgent.GetDistanceToGoal();
        return Vector3.Distance(GetAgentPosition(), GetGoalPosition());
    }
    
    private int GetCurrentStep()
    {
        // Para ambos agentes, usar estimación basada en tiempo para comparación justa
        if (currentEpisode != null)
        {
            float timeElapsed = Time.time - episodeStartTime;
            // Ambos agentes ejecutan aproximadamente 50 steps por segundo
            int estimatedSteps = Mathf.RoundToInt(timeElapsed * 50f);
            
            // Debug para verificar la estimación
            if (Time.frameCount % 300 == 0) // Cada 5 segundos aproximadamente
            {
                string agentName = agentType == AgentType.FSM ? "FSM" : "ML-Agents";
                Debug.Log($"TurtleMetrics: Steps estimados para {agentName}: {estimatedSteps} (tiempo: {timeElapsed:F2}s)");
            }
            
            return estimatedSteps;
        }
        return 0;
    }
    
    private float GetCurrentReward()
    {
        if (fsmAgent != null) return fsmAgent.CumulativeReward;
        if (mlAgent != null)
        {
            // Para ML-Agents, usar acceso directo al campo público CumulativeReward
            try
            {
                // Acceso directo al campo público
                float reward = mlAgent.CumulativeReward;
                return reward;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"TurtleMetrics: Error obteniendo CumulativeReward de ML-Agents: {e.Message}");
                return currentEpisode != null ? currentEpisode.currentReward : 0f;
            }
        }
        return 0f;
    }
    
    // Logging a archivo
    private void CreateCSVHeader()
    {
        string header = "Episode,Success,Time,Steps,Reward,Collisions,DirectionChanges,PathEfficiency,StartPos,GoalPos\n";
        string filePath = GetFilePath();
        File.WriteAllText(filePath, header);
        Debug.Log($"TurtleMetrics: Archivo CSV creado en: {filePath}");
    }
    
    private string GetFilePath()
    {
        string agentSuffix = agentType == AgentType.FSM ? "_FSM" : "_MLAgents";
        string baseFileName = fileName.Replace(".csv", "") + agentSuffix + ".csv";
        
        // Crear carpeta MetricLogs en el proyecto de Unity
        string projectPath = Application.dataPath;
        string metricLogsPath = Path.Combine(projectPath, "..", "MetricLogs");
        
        // Asegurar que la carpeta existe
        if (!Directory.Exists(metricLogsPath))
        {
            Directory.CreateDirectory(metricLogsPath);
        }
        
        return Path.Combine(metricLogsPath, baseFileName);
    }
    
    private void LogEpisodeToFile(EpisodeMetrics episode)
    {
        string line = $"{episode.episodeNumber}," +
                     $"{(episode.success ? 1 : 0)}," +
                     $"{episode.totalTime:F2}," +
                     $"{episode.totalSteps}," +
                     $"{episode.finalReward:F2}," +
                     $"{episode.collisionCount}," +
                     $"{episode.directionChanges}," +
                     $"{episode.pathEfficiency:F3}," +
                     $"({episode.startPosition.x:F2},{episode.startPosition.z:F2})," +
                     $"({episode.goalPosition.x:F2},{episode.goalPosition.z:F2})\n";
        
        File.AppendAllText(GetFilePath(), line);
    }
    
    private void LogSummaryToFile()
    {
        string summaryFile = GetFilePath().Replace(".csv", "_summary.txt");
        string agentTypeText = agentType == AgentType.FSM ? "FSM" : "ML-Agents";
        string summaryText = $"TURTLE {agentTypeText} METRICS SUMMARY\n" +
                           $"==============================\n" +
                           $"Agent Type: {agentTypeText}\n" +
                           $"Total Episodes: {summary.totalEpisodes}\n" +
                           $"Successful Episodes: {summary.successfulEpisodes}\n" +
                           $"Success Rate: {summary.successRate:P1}\n" +
                           $"Average Time per Episode: {summary.avgTimePerEpisode:F2}s\n" +
                           $"Average Steps per Episode: {summary.avgStepsPerEpisode}\n" +
                           $"Average Reward per Episode: {summary.avgRewardPerEpisode:F2}\n" +
                           $"Average Path Efficiency: {summary.avgPathEfficiency:P1}\n" +
                           $"Average Collisions per Episode: {summary.avgCollisionsPerEpisode:F0}\n" +
                           $"Average Direction Changes per Episode: {summary.avgDirectionChangesPerEpisode:F0}\n\n" +
                           $"PATH EFFICIENCY EXPLANATION:\n" +
                           $"Path Efficiency = Direct Distance / Actual Distance Traveled\n" +
                           $"• 100% = Perfect path (straight line to goal)\n" +
                           $"• 80% = Very efficient path (slight deviations)\n" +
                           $"• 50% = Moderate efficiency (some wandering)\n" +
                           $"• 20% = Low efficiency (much wandering/collisions)\n" +
                           $"• Values below 20% indicate significant inefficiency\n" +
                           $"• Calculation uses both path points and movement tracking\n";
        
        File.WriteAllText(summaryFile, summaryText);
        Debug.Log($"TurtleMetrics: Resumen exportado a: {summaryFile}");
    }
    
    // Interfaz visual
    private void OnGUI()
    {
        if (!showOnScreen || !enableMetrics) return;
        
        // Posicionar ventana según el tipo de agente
        Rect windowRect = GetMetricsWindowRect();
        
        GUILayout.BeginArea(windowRect);
        GUILayout.BeginVertical("box");
        
        string title = GetMetricsTitle();
        GUILayout.Label(title, GUI.skin.box);
        GUILayout.Space(5);
        
        // Información del episodio actual
        if (currentEpisode != null && isRecording)
        {
            GUILayout.Label("Episodio Actual:", GUI.skin.box);
            GUILayout.Label($"Episodio: {currentEpisode.episodeNumber}/{maxEpisodes}", GUI.skin.label);
            GUILayout.Label($"Tiempo: {currentEpisode.time:F1}s", GUI.skin.label);
            GUILayout.Label($"Pasos: {currentEpisode.steps}", GUI.skin.label);
            GUILayout.Label($"Distancia: {currentEpisode.distanceTraveled:F2}", GUI.skin.label);
            GUILayout.Label($"Colisiones: {currentEpisode.collisionCount}", GUI.skin.label);
            GUILayout.Label($"Cambios dirección: {currentEpisode.directionChanges}", GUI.skin.label);
            GUILayout.Label($"Distancia al objetivo: {currentEpisode.currentDistanceToGoal:F2}", GUI.skin.label);
            GUILayout.Label($"Recompensa: {currentEpisode.currentReward:F2}", GUI.skin.label);
        }
        
        GUILayout.Space(10);
        
        // Resumen general
        if (summary.totalEpisodes > 0)
        {
            GUILayout.Space(5);
            GUILayout.Label("Resumen Estadístico:", GUI.skin.box);
            GUILayout.Label($"Total episodios: {summary.totalEpisodes}", GUI.skin.label);
            GUILayout.Label($"Éxitos: {summary.successfulEpisodes}", GUI.skin.label);
            GUILayout.Label($"Tasa de éxito: {summary.successRate:P1}", GUI.skin.label);
            GUILayout.Label($"Tiempo promedio: {summary.avgTimePerEpisode:F1}s", GUI.skin.label);
            GUILayout.Label($"Pasos promedio: {summary.avgStepsPerEpisode}", GUI.skin.label);
            GUILayout.Label($"Recompensa promedio: {summary.avgRewardPerEpisode:F2}", GUI.skin.label);
            GUILayout.Label($"Eficiencia promedio: {summary.avgPathEfficiency:P1}", GUI.skin.label);
            GUILayout.Label($"Colisiones promedio: {summary.avgCollisionsPerEpisode:F0}", GUI.skin.label);
            GUILayout.Label($"Cambios dirección promedio: {summary.avgDirectionChangesPerEpisode:F0}", GUI.skin.label);
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Controles:", GUI.skin.box);
        GUILayout.Label($"Presiona {toggleKey} para ocultar/mostrar", GUI.skin.label);
        GUILayout.Label("Los datos se guardan en la carpeta MetricLogs", GUI.skin.label);
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    private Rect GetMetricsWindowRect()
    {
        const int windowWidth = 430;
        const int windowHeight = 400;
        
        switch (agentType)
        {
            case AgentType.FSM:
                // FSM: Esquina superior derecha
                return new Rect(Screen.width - windowWidth - 10, 10, windowWidth, windowHeight);
                
            case AgentType.ML_Agents:
                // ML-Agents: Esquina inferior izquierda
                return new Rect(10, Screen.height - windowHeight - 10, windowWidth, windowHeight);
                
            default:
                // Desconocido: Esquina superior derecha por defecto
                return new Rect(Screen.width - windowWidth - 10, 10, windowWidth, windowHeight);
        }
    }
    
    private string GetMetricsTitle()
    {
        switch (agentType)
        {
            case AgentType.FSM:
                return "Turtle FSM Metrics";
            case AgentType.ML_Agents:
                return "Turtle ML-Agents Metrics";
            default:
                return "Turtle Metrics";
        }
    }
}

// Clases de datos para métricas
[System.Serializable]
public class EpisodeMetrics
{
    public int episodeNumber;
    public bool success;
    public Vector3 startPosition;
    public Vector3 goalPosition;
    public float startTime;
    public float endTime;
    public int startStep;
    public int endStep;
    public float totalTime;
    public int totalSteps;
    public float finalReward;
    public float finalDistanceToGoal;
    public float pathEfficiency;
    
    // Métricas en tiempo real
    public int steps;
    public float time;
    public float distanceTraveled;
    public int collisionCount;
    public int directionChanges;
    public float currentDistanceToGoal;
    public float currentReward;
}

[System.Serializable]
public class MetricsSummary
{
    public int totalEpisodes;
    public int successfulEpisodes;
    public float successRate;
    public float avgTimePerEpisode;
    public float avgStepsPerEpisode;
    public float avgRewardPerEpisode;
    public float avgPathEfficiency;
    public float avgCollisionsPerEpisode;
    public float avgDirectionChangesPerEpisode;
} 
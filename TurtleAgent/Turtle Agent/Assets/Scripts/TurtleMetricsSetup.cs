using UnityEngine;

/// <summary>
/// Configuración automática del sistema de métricas
/// </summary>
public class TurtleMetricsSetup : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private bool enableMetrics = true;
    [SerializeField] private bool logToFile = true;
    [SerializeField] private int maxEpisodes = 100;
    [SerializeField] private bool showOnScreen = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F2;
    
    private void Start()
    {
        if (autoSetup)
        {
            PerformAutoSetup();
        }
    }
    
    private void PerformAutoSetup()
    {
        var metrics = GetComponent<TurtleMetrics>();
        if (metrics == null)
        {
            metrics = gameObject.AddComponent<TurtleMetrics>();
            Debug.Log("TurtleMetricsSetup: Añadido TurtleMetrics al GameObject");
        }
        
        var enableMetricsField = typeof(TurtleMetrics).GetField("enableMetrics", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var logToFileField = typeof(TurtleMetrics).GetField("logToFile", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var maxEpisodesField = typeof(TurtleMetrics).GetField("maxEpisodes", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var showOnScreenField = typeof(TurtleMetrics).GetField("showOnScreen", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var toggleKeyField = typeof(TurtleMetrics).GetField("toggleKey", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (enableMetricsField != null) enableMetricsField.SetValue(metrics, enableMetrics);
        if (logToFileField != null) logToFileField.SetValue(metrics, logToFile);
        if (maxEpisodesField != null) maxEpisodesField.SetValue(metrics, maxEpisodes);
        if (showOnScreenField != null) showOnScreenField.SetValue(metrics, showOnScreen);
        if (toggleKeyField != null) toggleKeyField.SetValue(metrics, toggleKey);
        
        Debug.Log("TurtleMetricsSetup: Configuración completada para ambos tipos de agente");
    }
    
    [ContextMenu("Setup Metrics")]
    public void SetupMetrics()
    {
        PerformAutoSetup();
    }
} 
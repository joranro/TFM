using UnityEngine;

public class TurtleFSMSetup : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(3, 10)]
    [SerializeField] private string setupInstructions = 
        "INSTRUCCIONES DE CONFIGURACIÓN:\n\n" +
        "1. Elimina el componente TurtleAgent (ML-Agents) del GameObject\n" +
        "2. Añade el componente TurtleAgentFSM\n" +
        "3. Añade el componente TurtleFSM\n" +
        "4. Configura las referencias en el Inspector\n" +
        "5. Asegúrate de que el GameObject tiene Collider y Rigidbody\n\n" +
        "CONTROLES:\n" +
        "- Flecha Arriba: Mover hacia adelante\n" +
        "- Flecha Izquierda: Girar izquierda\n" +
        "- Flecha Derecha: Girar derecha\n" +
        "- F2: Mostrar/ocultar métricas de rendimiento";

    [Header("Auto Setup")]
    [SerializeField] private bool autoSetup = false;

    private void Start()
    {
        if (autoSetup)
        {
            PerformAutoSetup();
        }
    }

    private void PerformAutoSetup()
    {
        // Verificar si ya existe el setup correcto
        if (GetComponent<TurtleAgentFSM>() != null && GetComponent<TurtleFSM>() != null)
        {
            Debug.Log("Setup FSM ya está configurado correctamente.");
            return;
        }

        // Eliminar componente ML-Agents si existe
        var mlAgent = GetComponent<TurtleAgent>();
        if (mlAgent != null)
        {
            Debug.Log("Eliminando componente TurtleAgent (ML-Agents)...");
            DestroyImmediate(mlAgent);
        }

        // Añadir componentes FSM
        if (GetComponent<TurtleAgentFSM>() == null)
        {
            Debug.Log("Añadiendo componente TurtleAgentFSM...");
            gameObject.AddComponent<TurtleAgentFSM>();
        }

        if (GetComponent<TurtleFSM>() == null)
        {
            Debug.Log("Añadiendo componente TurtleFSM...");
            gameObject.AddComponent<TurtleFSM>();
        }

        Debug.Log("Auto setup completado. Revisa las referencias en el Inspector.");
    }

    [ContextMenu("Perform Manual Setup")]
    private void PerformManualSetup()
    {
        PerformAutoSetup();
    }

    [ContextMenu("Show Setup Instructions")]
    private void ShowSetupInstructions()
    {
        Debug.Log(setupInstructions);
    }
} 
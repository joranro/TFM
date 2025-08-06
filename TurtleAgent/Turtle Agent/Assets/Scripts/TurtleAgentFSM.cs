using UnityEngine;
using System.Collections;

public class TurtleAgentFSM : MonoBehaviour
{
    [Header("Agent Settings")]
    [SerializeField] private Transform _goal;
    [SerializeField] private Renderer _groundRenderer;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private int _maxSteps = 5000; // Aumentado a 5000
    [SerializeField] private bool _enableRewardLogs = false; // Desactivado por defecto

    [Header("FSM Component")]
    [SerializeField] private TurtleFSM _fsm;

    private Renderer _renderer;
    private int _currentStep = 0;
    private float _cumulativeReward = 0f;
    private Color _defaultGroundColor;
    private Coroutine _flashGroundCoroutine;

    // Propiedades públicas para acceso desde estados
    public float MoveSpeed => _moveSpeed;
    public float RotationSpeed => _rotationSpeed;
    public int MaxSteps => _maxSteps;
    public int CurrentStep => _currentStep;
    public float CumulativeReward => _cumulativeReward;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        
        // Si no se ha asignado el FSM, intentar encontrarlo
        if (_fsm == null)
        {
            _fsm = GetComponent<TurtleFSM>();
        }
        
        // Si aún no existe, crearlo
        if (_fsm == null)
        {
            _fsm = gameObject.AddComponent<TurtleFSM>();
        }

        if (_groundRenderer != null)
        {
            _defaultGroundColor = _groundRenderer.material.color;
        }
    }

    private void Start()
    {
        Initialize();
        
        // Notificar al sistema de métricas del inicio del episodio
        var metrics = GetComponent<TurtleMetrics>();
        if (metrics != null)
        {
            metrics.OnEpisodeStart();
        }
    }

    public void Initialize()
    {
        Debug.Log("TurtleAgentFSM Initialize()");
        _currentStep = 0;
        _cumulativeReward = 0f;
        _renderer.material.color = Color.blue;
    }

    public void ResetEpisode()
    {
        Debug.Log("TurtleAgentFSM ResetEpisode()");

        // Notificar al sistema de métricas del fin del episodio (éxito)
        var metrics = GetComponent<TurtleMetrics>();
        if (metrics != null)
        {
            metrics.OnEpisodeEnd(true); // Éxito porque llegó al objetivo
        }

        if (_groundRenderer != null && _cumulativeReward != 0f)
        {
            Color flashColor = (_cumulativeReward > 0f) ? Color.green : Color.red;

            if (_flashGroundCoroutine != null)
            {
                StopCoroutine(_flashGroundCoroutine);
            }

            _flashGroundCoroutine = StartCoroutine(FlashGround(flashColor, 3.0f));
        }

        _currentStep = 0;
        _cumulativeReward = 0f;
        _renderer.material.color = Color.blue;

        SpawnObjects();

        // Resetear la FSM al estado Idle
        if (_fsm != null)
        {
            _fsm.ChangeState(TurtleState.Idle);
        }
        
        // Iniciar nuevo episodio de métricas
        if (metrics != null)
        {
            metrics.OnEpisodeStart();
        }
    }

    private IEnumerator FlashGround(Color targetColor, float duration)
    {
        float elapsedTime = 0f;

        _groundRenderer.material.color = targetColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            _groundRenderer.material.color = Color.Lerp(targetColor, _defaultGroundColor, elapsedTime / duration);
            yield return null;
        }
    }

    private void SpawnObjects()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0.15f, 0f);

        // Randomizar dirección en el eje Y (ángulo en grados)
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        // Randomizar distancia dentro del rango [1, 2.5]
        float randomDistance = Random.Range(1f, 2.5f);

        // Calcular posición del objetivo
        Vector3 goalPosition = transform.localPosition + randomDirection * randomDistance;

        // Aplicar la posición calculada al objetivo
        _goal.localPosition = new Vector3(goalPosition.x, 0.3f, goalPosition.z);
    }

    private void Update()
    {
        // Incrementar contador de pasos
        _currentStep++;

        // Verificar si se ha excedido el máximo de pasos
        if (_currentStep >= _maxSteps)
        {
            Debug.Log("Máximo de pasos alcanzado. Desactivando métricas y deteniendo agente.");
            
            // Notificar al sistema de métricas del fin del episodio (fallo)
            var metrics = GetComponent<TurtleMetrics>();
            if (metrics != null)
            {
                metrics.OnEpisodeEnd(false); // Fallo porque se excedió el límite de pasos
                metrics.DisableMetrics(); // Desactivar completamente las métricas
            }
            
            // NO reiniciar episodio - dejar que el agente se detenga
            enabled = false; // Desactivar este script
        }
    }

    // Métodos para el sistema de recompensas
    public void AddReward(float reward)
    {
        _cumulativeReward += reward;
        // Solo log para recompensas importantes y si está habilitado
        if (_enableRewardLogs && Mathf.Abs(reward) > 0.1f)
        {
            Debug.Log($"Recompensa añadida: {reward}. Total acumulado: {_cumulativeReward}");
        }
    }

    public void AddStepPenalty()
    {
        // Penalización por paso para animar al agente a terminar rápidamente
        AddReward(-2f / _maxSteps);
    }

    // Métodos para cambiar el color del agente
    public void SetColor(Color color)
    {
        if (_renderer != null)
        {
            _renderer.material.color = color;
        }
    }

    // Métodos para obtener información del agente
    public Vector3 GetGoalPosition()
    {
        return _goal != null ? _goal.localPosition : Vector3.zero;
    }

    public Vector3 GetAgentPosition()
    {
        return transform.localPosition;
    }

    public float GetAgentRotation()
    {
        return transform.localRotation.eulerAngles.y;
    }

    public float GetDistanceToGoal()
    {
        if (_goal == null) return float.MaxValue;
        return Vector3.Distance(transform.localPosition, _goal.localPosition);
    }

    // Métodos para debugging
    public void LogAgentInfo()
    {
        Debug.Log($"TurtleAgentFSM Info - Posición: {transform.localPosition}, " +
                  $"Rotación: {transform.localRotation.eulerAngles.y}, " +
                  $"Pasos: {_currentStep}/{_maxSteps}, " +
                  $"Recompensa: {_cumulativeReward}");
    }

    // Método para obtener información del estado actual del FSM
    public string GetFSMInfo()
    {
        return _fsm != null ? _fsm.GetCurrentStateInfo() : "FSM no disponible";
    }
} 
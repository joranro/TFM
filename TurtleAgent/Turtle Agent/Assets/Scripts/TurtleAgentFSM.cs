using UnityEngine;
using System.Collections;

/// <summary>
/// Agente FSM que implementa navegación hacia un objetivo
/// usando una máquina de estados finitos
/// </summary>
public class TurtleAgentFSM : MonoBehaviour
{
    [Header("Configuración del Agente")]
    [SerializeField] private Transform _goal;
    [SerializeField] private Renderer _groundRenderer;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private int _maxSteps = 5000;
    [SerializeField] private bool _enableRewardLogs = false;

    [Header("Componente FSM")]
    [SerializeField] private TurtleFSM _fsm;

    private Renderer _renderer;
    private int _currentStep = 0;
    private float _cumulativeReward = 0f;
    private Color _defaultGroundColor;
    private Coroutine _flashGroundCoroutine;

    public float MoveSpeed => _moveSpeed;
    public float RotationSpeed => _rotationSpeed;
    public int MaxSteps => _maxSteps;
    public int CurrentStep => _currentStep;
    public float CumulativeReward => _cumulativeReward;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        
        if (_fsm == null)
        {
            _fsm = GetComponent<TurtleFSM>();
        }
        
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

        var metrics = GetComponent<TurtleMetrics>();
        if (metrics != null)
        {
            metrics.OnEpisodeEnd(true);
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

        _fsm.ChangeState(TurtleState.Idle);
        
        if (metrics != null)
        {
            metrics.OnEpisodeStart();
        }
    }

    /// <summary>
    /// Efecto visual de flash en el suelo al finalizar episodio
    /// </summary>
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

    /// <summary>
    /// Reposiciona el agente y el objetivo en posiciones aleatorias
    /// </summary>
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

    private void Update()
    {
        _currentStep++;

        if (_currentStep >= _maxSteps)
        {
            Debug.Log("Máximo de pasos alcanzado. Desactivando métricas y deteniendo agente.");
            
            var metrics = GetComponent<TurtleMetrics>();
            if (metrics != null)
            {
                metrics.OnEpisodeEnd(false);
                metrics.DisableMetrics();
            }
            
            enabled = false;
        }
    }

    /// <summary>
    /// Añade una recompensa al agente
    /// </summary>
    public void AddReward(float reward)
    {
        _cumulativeReward += reward;
        if (_enableRewardLogs)
        {
            Debug.Log($"Recompensa añadida: {reward}. Total acumulado: {_cumulativeReward}");
        }
    }

    /// <summary>
    /// Aplica penalización por paso para incentivar finalización rápida
    /// </summary>
    public void AddStepPenalty()
    {
        AddReward(-2f / _maxSteps);
    }

    /// <summary>
    /// Cambia el color visual del agente
    /// </summary>
    public void SetColor(Color color)
    {
        if (_renderer != null)
        {
            _renderer.material.color = color;
        }
    }

    /// <summary>
    /// Obtiene la posición del objetivo
    /// </summary>
    public Vector3 GetGoalPosition()
    {
        return _goal != null ? _goal.localPosition : Vector3.zero;
    }

    /// <summary>
    /// Obtiene la posición actual del agente
    /// </summary>
    public Vector3 GetAgentPosition()
    {
        return transform.localPosition;
    }

    /// <summary>
    /// Obtiene la rotación actual del agente en el eje Y
    /// </summary>
    public float GetAgentRotation()
    {
        return transform.localRotation.eulerAngles.y;
    }

    /// <summary>
    /// Calcula la distancia actual al objetivo
    /// </summary>
    public float GetDistanceToGoal()
    {
        if (_goal == null) return float.MaxValue;
        return Vector3.Distance(transform.localPosition, _goal.localPosition);
    }

    /// <summary>
    /// Muestra información de debug del agente
    /// </summary>
    public void LogAgentInfo()
    {
        Debug.Log($"TurtleAgentFSM Info - Posición: {transform.localPosition}, " +
                  $"Rotación: {transform.localRotation.eulerAngles.y}, " +
                  $"Pasos: {_currentStep}/{_maxSteps}, " +
                  $"Recompensa: {_cumulativeReward}");
    }
} 
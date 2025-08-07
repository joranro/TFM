using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;

/// <summary>
/// Agente de ML-Agents que implementa navegación hacia un objetivo
/// usando aprendizaje por refuerzo. El agente aprende a moverse hacia
/// el objetivo evitando colisiones con paredes.
/// </summary>
public class TurtleAgent : Agent
{
    [Header("Configuración del Agente")]
    [SerializeField] private Transform _goal;
    [SerializeField] private Renderer _groundRenderer;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;

    [Header("Estado del Agente")]
    private Renderer _renderer;
    [HideInInspector] public int CurrentEpisode = 0;
    [HideInInspector] public float CumulativeReward = 0f;

    [Header("Efectos Visuales")]
    private Color _defaultGroundColor;
    private Coroutine _flashGroundCoroutine;

    /// <summary>
    /// Inicialización del agente al comenzar el entrenamiento
    /// </summary>
    public override void Initialize()
    {
        Debug.Log("Initialize()");

        _renderer = GetComponent<Renderer>();
        CurrentEpisode = 0;
        CumulativeReward = 0f;

        if(_groundRenderer != null)
        {
            _defaultGroundColor = _groundRenderer.material.color;
        }
    }

    /// <summary>
    /// Se ejecuta al inicio de cada episodio de entrenamiento
    /// </summary>
    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");

        // Mostrar flash de color según el resultado del episodio anterior
        if (_groundRenderer != null && CumulativeReward != 0f)
        {
            Color flashColor = (CumulativeReward > 0f) ? Color.green : Color.red;

            if (_flashGroundCoroutine != null)
            {
                StopCoroutine(_flashGroundCoroutine);
            }

            _flashGroundCoroutine = StartCoroutine(FlashGround(flashColor, 3.0f));
        }

        CurrentEpisode++;
        
        // Integración con sistema de métricas
        var metrics = GetComponent<TurtleMetrics>();
        if (metrics != null)
        {
            if (CurrentEpisode == 1)
            {
                metrics.OnEpisodeStart();
            }
            else
            {
                metrics.OnEpisodeEnd(true);
                metrics.OnEpisodeStart();
            }
        }
        
        CumulativeReward = 0f;
        _renderer.material.color = Color.blue;

        SpawnObjects();
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

    /// <summary>
    /// Recopila las observaciones que el agente envía al modelo de ML
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Posición del objetivo normalizada
        float goalPosX_normalized = _goal.localPosition.x / 5f;
        float goalPosZ_normalized = _goal.localPosition.z / 5f;

        // Posición de la tortuga normalizada
        float turtlePosX_normalized = transform.localPosition.x / 5f;
        float turtlePosZ_normalized = transform.localPosition.z / 5f;

        // Rotación de la tortuga normalizada
        float turtleRotation_normalized = (transform.localRotation.eulerAngles.y / 360f) * 2f -1f;

        sensor.AddObservation(goalPosX_normalized);
        sensor.AddObservation(goalPosZ_normalized);
        sensor.AddObservation(turtlePosX_normalized);
        sensor.AddObservation(turtlePosZ_normalized);
        sensor.AddObservation(turtleRotation_normalized);
    }

    /// <summary>
    /// Control manual del agente para testing
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = 0; // No hacer nada

        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1; // Avanzar
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 2; // Girar izquierda
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 3; // Girar derecha
        }
    }

    /// <summary>
    /// Procesa las acciones recibidas del modelo de ML
    /// </summary>
    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);

        // Penalización por paso para incentivar finalización rápida
        AddReward(-2f / MaxStep);

        CumulativeReward = GetCumulativeReward();
    }

    /// <summary>
    /// Ejecuta el movimiento del agente según la acción recibida
    /// </summary>
    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];

        switch (action)
        {
            case 1: // Avanzar
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2: // Girar izquierda
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3: // Girar derecha
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }
    }

    /// <summary>
    /// Detecta cuando el agente toca el objetivo
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            GoalReached();
        }
    }

    /// <summary>
    /// Maneja el evento de llegar al objetivo
    /// </summary>
    private void GoalReached()
    {
        AddReward(10f); // Recompensa grande por alcanzar el objetivo
        CumulativeReward = GetCumulativeReward();

        EndEpisode();
    }

    /// <summary>
    /// Detecta colisiones con paredes
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.05f);
            CumulativeReward = GetCumulativeReward();

            if (_renderer != null)
            {
                _renderer.material.color = Color.red;
            }
            
            var metrics = GetComponent<TurtleMetrics>();
            if (metrics != null)
            {
                metrics.OnCollision();
            }
        }
    }

    /// <summary>
    /// Penalización continua mientras está colisionando
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.01f * Time.fixedDeltaTime);
            CumulativeReward = GetCumulativeReward();
        }
    }

    /// <summary>
    /// Restaura el color cuando sale de la colisión
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (_renderer != null)
            {
                _renderer.material.color = Color.blue;
            }
        }
    }
}

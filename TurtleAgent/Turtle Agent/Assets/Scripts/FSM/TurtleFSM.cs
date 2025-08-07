using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Máquina de Estados Finitos para el agente tortuga
/// Maneja las transiciones entre estados y el procesamiento de input
/// </summary>
public class TurtleFSM : MonoBehaviour
{
    [Header("Configuración FSM")]
    [SerializeField] private TurtleState currentState = TurtleState.Idle;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool autonomousMode = true;

    private Dictionary<TurtleState, TurtleStateBase> states;
    private TurtleAgentFSM agent;
    private float stateEnterTime;

    public TurtleState CurrentState => currentState;
    public bool IsColliding { get; private set; } = false;
    public float GetStateEnterTime() => stateEnterTime;

    private void Awake()
    {
        agent = GetComponent<TurtleAgentFSM>();
        InitializeStates();
    }

    private void InitializeStates()
    {
        states = new Dictionary<TurtleState, TurtleStateBase>
        {
            { TurtleState.Idle, new TurtleIdleState(this, agent) },
            { TurtleState.Moving, new TurtleMovingState(this, agent) },
            { TurtleState.RotatingLeft, new TurtleRotatingLeftState(this, agent) },
            { TurtleState.RotatingRight, new TurtleRotatingRightState(this, agent) },
            { TurtleState.Navigating, new TurtleNavigatingState(this, agent) },
            { TurtleState.ReachedGoal, new TurtleReachedGoalState(this, agent) },
            { TurtleState.Colliding, new TurtleCollidingState(this, agent) }
        };
    }

    private void Start()
    {
        ChangeState(TurtleState.Idle);
    }

    private void Update()
    {
        ProcessInput();
        
        if (states.ContainsKey(currentState))
        {
            states[currentState].Update();
        }
    }

    private void FixedUpdate()
    {
        if (states.ContainsKey(currentState))
        {
            states[currentState].FixedUpdate();
        }
    }

    private void ProcessInput()
    {
        if (currentState == TurtleState.ReachedGoal)
            return;

        if (autonomousMode)
        {
            ProcessAutonomousInput();
        }
        else
        {
            ProcessManualInput();
        }
    }

    private void ProcessManualInput()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            ChangeState(TurtleState.Moving);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            ChangeState(TurtleState.RotatingLeft);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            ChangeState(TurtleState.RotatingRight);
        }
        else
        {
            ChangeState(TurtleState.Idle);
        }
    }

    /// <summary>
    /// Procesa el input autónomo para navegación hacia el objetivo
    /// </summary>
    private void ProcessAutonomousInput()
    {
        Vector3 goalPosition = agent.GetGoalPosition();
        Vector3 agentPosition = agent.GetAgentPosition();
        float agentRotation = agent.GetAgentRotation();
        
        Vector3 directionToGoal = (goalPosition - agentPosition).normalized;
        float distanceToGoal = Vector3.Distance(agentPosition, goalPosition);
        
        float targetAngle = Mathf.Atan2(directionToGoal.x, directionToGoal.z) * Mathf.Rad2Deg;
        float angleDifference = Mathf.DeltaAngle(agentRotation, targetAngle);
        
        if (Mathf.Abs(angleDifference) > 15f)
        {
            if (angleDifference > 0)
            {
                ChangeState(TurtleState.RotatingRight);
            }
            else
            {
                ChangeState(TurtleState.RotatingLeft);
            }
        }
        else
        {
            ChangeState(TurtleState.Navigating);
        }
    }

    /// <summary>
    /// Cambia el estado actual de la FSM
    /// </summary>
    public void ChangeState(TurtleState newState)
    {
        if (currentState == newState) return;

        if (debugMode)
        {
            Debug.Log($"Turtle FSM: {currentState} -> {newState}");
        }

        if (states.ContainsKey(currentState))
        {
            states[currentState].Exit();
        }

        currentState = newState;
        stateEnterTime = Time.time;

        if (states.ContainsKey(currentState))
        {
            states[currentState].Enter();
        }
        
        if (agent != null)
        {
            agent.AddStepPenalty();
        }
    }

    /// <summary>
    /// Detecta cuando el agente toca el objetivo
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            ChangeState(TurtleState.ReachedGoal);
        }

        if (states.ContainsKey(currentState))
        {
            states[currentState].OnTriggerEnter(other);
        }
    }

    /// <summary>
    /// Detecta colisiones con paredes
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            IsColliding = true;
            ChangeState(TurtleState.Colliding);
        }

        if (states.ContainsKey(currentState))
        {
            states[currentState].OnCollisionEnter(collision);
        }
    }

    /// <summary>
    /// Maneja colisiones continuas con paredes
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            IsColliding = true;
        }

        if (states.ContainsKey(currentState))
        {
            states[currentState].OnCollisionStay(collision);
        }
    }

    /// <summary>
    /// Detecta cuando el agente sale de una colisión
    /// </summary>
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

        if (states.ContainsKey(currentState))
        {
            states[currentState].OnCollisionExit(collision);
        }
    }
} 
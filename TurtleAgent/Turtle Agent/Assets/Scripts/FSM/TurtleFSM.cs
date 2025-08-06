using UnityEngine;
using System.Collections.Generic;

public class TurtleFSM : MonoBehaviour
{
    [Header("FSM Settings")]
    [SerializeField] private TurtleState currentState = TurtleState.Idle;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool autonomousMode = true; // Modo autónomo

    private Dictionary<TurtleState, TurtleStateBase> states;
    private TurtleAgentFSM agent;
    private float stateEnterTime; // Tiempo de entrada al estado actual

    // Propiedades públicas para acceso desde estados
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
        // Iniciar en estado Idle
        ChangeState(TurtleState.Idle);
    }

    private void Update()
    {
        // Procesar input del usuario
        ProcessInput();
        
        // Actualizar estado actual
        if (states.ContainsKey(currentState))
        {
            states[currentState].Update();
        }
    }

    private void FixedUpdate()
    {
        // Actualizar estado actual en FixedUpdate
        if (states.ContainsKey(currentState))
        {
            states[currentState].FixedUpdate();
        }
    }

    private void ProcessInput()
    {
        // Solo procesar input si no está en estados especiales
        if (currentState == TurtleState.ReachedGoal || currentState == TurtleState.Colliding)
            return;

        // Si está en modo autónomo y no está en estados especiales, procesar input
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

    private void ProcessAutonomousInput()
    {
        // Lógica de navegación autónoma
        Vector3 goalPosition = agent.GetGoalPosition();
        Vector3 agentPosition = agent.GetAgentPosition();
        float agentRotation = agent.GetAgentRotation();
        
        // Calcular dirección al objetivo
        Vector3 directionToGoal = (goalPosition - agentPosition).normalized;
        float distanceToGoal = Vector3.Distance(agentPosition, goalPosition);
        
        // Si está muy cerca del objetivo, detenerse
        if (distanceToGoal < 0.3f)
        {
            ChangeState(TurtleState.Idle);
            return;
        }
        
        // Calcular el ángulo hacia el objetivo
        float targetAngle = Mathf.Atan2(directionToGoal.x, directionToGoal.z) * Mathf.Rad2Deg;
        float angleDifference = Mathf.DeltaAngle(agentRotation, targetAngle);
        
        // Si el ángulo es muy grande, girar primero
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
            // Si está orientado hacia el objetivo, usar estado de navegación
            ChangeState(TurtleState.Navigating);
        }
    }

    public void ChangeState(TurtleState newState)
    {
        if (currentState == newState) return;

        if (debugMode)
        {
            Debug.Log($"Turtle FSM: {currentState} -> {newState}");
        }

        // Salir del estado actual
        if (states.ContainsKey(currentState))
        {
            states[currentState].Exit();
        }

        // Cambiar al nuevo estado
        currentState = newState;
        stateEnterTime = Time.time; // Registrar tiempo de entrada

        // Entrar al nuevo estado
        if (states.ContainsKey(currentState))
        {
            states[currentState].Enter();
        }
        
        // Aplicar penalización por paso al cambiar de estado (consistente con ML-Agents OnActionReceived)
        if (agent != null)
        {
            agent.AddStepPenalty();
        }
    }

    // Métodos para manejar colisiones y triggers
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

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            IsColliding = false;
            // Volver al estado anterior si no está en un estado especial
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

    // Método público para forzar un estado (útil para testing)
    public void ForceState(TurtleState state)
    {
        ChangeState(state);
    }

    // Método para obtener información del estado actual
    public string GetCurrentStateInfo()
    {
        return $"Current State: {currentState}, IsColliding: {IsColliding}";
    }
} 
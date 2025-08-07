using UnityEngine;

/// <summary>
/// Estado que maneja el comportamiento del agente cuando colisiona con paredes
/// Implementa estrategias de escape proactivas
/// </summary>
public class TurtleCollidingState : TurtleStateBase
{
    private float lastEscapeAttemptTime;
    private const float ESCAPE_ATTEMPT_INTERVAL = 0.5f;

    public TurtleCollidingState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Enter()
    {
        agent.SetColor(Color.red);
        agent.AddReward(-0.05f);
        
        lastEscapeAttemptTime = Time.time;
        
        var metrics = agent.GetComponent<TurtleMetrics>();
        if (metrics != null)
        {
            metrics.OnCollision();
        }
    }

    public override void Update()
    {
        AttemptEscape();
    }

    public override void FixedUpdate()
    {
        agent.AddReward(-0.01f * Time.fixedDeltaTime);
    }

    private void AttemptEscape()
    {
        float currentTime = Time.time;
        
        if (currentTime - lastEscapeAttemptTime < ESCAPE_ATTEMPT_INTERVAL)
            return;

        lastEscapeAttemptTime = currentTime;
        TryEscapeStrategy();
    }

    private void TryEscapeStrategy()
    {
        Vector3 agentPosition = agent.GetAgentPosition();
        Vector3 goalPosition = agent.GetGoalPosition();
        
        if (TryEscapeFromCollision())
            return;
            
        TryRandomEscape();
    }

    private bool TryEscapeFromCollision()
    {
        Vector3 goalDirection = (agent.GetGoalPosition() - agent.GetAgentPosition()).normalized;
        Vector3 escapeDirection = -goalDirection;
        
        float targetAngle = Mathf.Atan2(escapeDirection.x, escapeDirection.z) * Mathf.Rad2Deg;
        float currentAngle = agent.GetAgentRotation();
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
        
        if (Mathf.Abs(angleDifference) > 30f)
        {
            if (angleDifference > 0)
            {
                fsm.ChangeState(TurtleState.RotatingRight);
            }
            else
            {
                fsm.ChangeState(TurtleState.RotatingLeft);
            }
        }
        else
        {
            fsm.ChangeState(TurtleState.Moving);
        }
        
        return true;
    }

    private void TryRandomEscape()
    {
        float randomValue = Random.Range(0f, 1f);
        
        if (randomValue < 0.4f)
        {
            fsm.ChangeState(TurtleState.RotatingLeft);
        }
        else if (randomValue < 0.8f)
        {
            fsm.ChangeState(TurtleState.RotatingRight);
        }
        else
        {
            fsm.ChangeState(TurtleState.Moving);
        }
    }

    public override void Exit()
    {
        agent.SetColor(Color.blue);
    }
} 
using UnityEngine;

/// <summary>
/// Estado de navegación autónoma hacia el objetivo
/// Combina movimiento continuo con corrección periódica de dirección
/// </summary>
public class TurtleNavigatingState : TurtleStateBase
{
    private float lastDirectionCheck = 0f;
    private const float DIRECTION_CHECK_INTERVAL = 0.1f;

    public TurtleNavigatingState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Update()
    {
        if (Time.time - lastDirectionCheck > DIRECTION_CHECK_INTERVAL)
        {
            lastDirectionCheck = Time.time;
            UpdateNavigation();
        }
    }

    public override void FixedUpdate()
    {
        agent.transform.position += agent.transform.forward * agent.MoveSpeed * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Actualiza la dirección de navegación hacia el objetivo
    /// </summary>
    private void UpdateNavigation()
    {
        Vector3 goalPosition = agent.GetGoalPosition();
        Vector3 agentPosition = agent.GetAgentPosition();
        float agentRotation = agent.GetAgentRotation();
        
        Vector3 directionToGoal = (goalPosition - agentPosition).normalized;
        float distanceToGoal = Vector3.Distance(agentPosition, goalPosition);
        
        float targetAngle = Mathf.Atan2(directionToGoal.x, directionToGoal.z) * Mathf.Rad2Deg;
        float angleDifference = Mathf.DeltaAngle(agentRotation, targetAngle);
        
        if (Mathf.Abs(angleDifference) > 20f)
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
    }
} 
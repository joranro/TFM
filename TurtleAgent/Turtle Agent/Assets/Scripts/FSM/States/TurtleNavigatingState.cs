using UnityEngine;

public class TurtleNavigatingState : TurtleStateBase
{
    private float lastDirectionCheck = 0f;
    private const float DIRECTION_CHECK_INTERVAL = 0.1f; // Verificar dirección cada 0.1 segundos

    public TurtleNavigatingState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Update()
    {
        // Verificar dirección al objetivo periódicamente
        if (Time.time - lastDirectionCheck > DIRECTION_CHECK_INTERVAL)
        {
            lastDirectionCheck = Time.time;
            UpdateNavigation();
        }
    }

    public override void FixedUpdate()
    {
        // Mover hacia adelante mientras navega
        agent.transform.position += agent.transform.forward * agent.MoveSpeed * Time.fixedDeltaTime;
    }

    private void UpdateNavigation()
    {
        Vector3 goalPosition = agent.GetGoalPosition();
        Vector3 agentPosition = agent.GetAgentPosition();
        float agentRotation = agent.GetAgentRotation();
        
        // Calcular dirección al objetivo
        Vector3 directionToGoal = (goalPosition - agentPosition).normalized;
        float distanceToGoal = Vector3.Distance(agentPosition, goalPosition);
        
        // Si está muy cerca del objetivo, detenerse
        if (distanceToGoal < 0.3f)
        {
            fsm.ChangeState(TurtleState.Idle);
            return;
        }
        
        // Calcular el ángulo hacia el objetivo
        float targetAngle = Mathf.Atan2(directionToGoal.x, directionToGoal.z) * Mathf.Rad2Deg;
        float angleDifference = Mathf.DeltaAngle(agentRotation, targetAngle);
        
        // Si el ángulo es muy grande, cambiar a estado de rotación
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
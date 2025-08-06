using UnityEngine;

public class TurtleMovingState : TurtleStateBase
{
    public TurtleMovingState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Update()
    {
        // El movimiento se maneja en FixedUpdate
    }

    public override void FixedUpdate()
    {
        // Mover hacia adelante
        agent.transform.position += agent.transform.forward * agent.MoveSpeed * Time.fixedDeltaTime;
    }
} 
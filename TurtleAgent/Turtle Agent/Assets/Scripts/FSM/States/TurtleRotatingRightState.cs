using UnityEngine;

public class TurtleRotatingRightState : TurtleStateBase
{
    public TurtleRotatingRightState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Update()
    {
        // La rotación se maneja en FixedUpdate
    }

    public override void FixedUpdate()
    {
        // Girar a la derecha
        agent.transform.Rotate(0f, agent.RotationSpeed * Time.fixedDeltaTime, 0f);
    }
} 
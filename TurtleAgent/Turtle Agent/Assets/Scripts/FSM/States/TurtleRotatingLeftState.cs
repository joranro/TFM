using UnityEngine;

public class TurtleRotatingLeftState : TurtleStateBase
{
    public TurtleRotatingLeftState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Update()
    {
        // La rotación se maneja en FixedUpdate
    }

    public override void FixedUpdate()
    {
        // Girar a la izquierda
        agent.transform.Rotate(0f, -agent.RotationSpeed * Time.fixedDeltaTime, 0f);
    }
} 
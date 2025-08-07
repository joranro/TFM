using UnityEngine;

/// <summary>
/// Estado de rotación hacia la izquierda
/// </summary>
public class TurtleRotatingLeftState : TurtleStateBase
{
    public TurtleRotatingLeftState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Update()
    {
        // La rotación se maneja en FixedUpdate
    }

    public override void FixedUpdate()
    {
        agent.transform.Rotate(0f, -agent.RotationSpeed * Time.fixedDeltaTime, 0f);
    }
} 
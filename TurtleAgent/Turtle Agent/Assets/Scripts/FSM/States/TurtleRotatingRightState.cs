using UnityEngine;

/// <summary>
/// Estado de rotación hacia la derecha
/// </summary>
public class TurtleRotatingRightState : TurtleStateBase
{
    public TurtleRotatingRightState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Update()
    {
        // La rotación se maneja en FixedUpdate
    }

    public override void FixedUpdate()
    {
        agent.transform.Rotate(0f, agent.RotationSpeed * Time.fixedDeltaTime, 0f);
    }
} 
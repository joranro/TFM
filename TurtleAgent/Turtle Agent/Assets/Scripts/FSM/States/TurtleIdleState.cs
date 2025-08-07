using UnityEngine;

/// <summary>
/// Estado de reposo del agente
/// </summary>
public class TurtleIdleState : TurtleStateBase
{
    public TurtleIdleState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Update()
    {
        // En estado idle, el agente no hace nada
        // Las transiciones se manejan desde el FSM principal
    }
} 
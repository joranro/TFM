using UnityEngine;

/// <summary>
/// Estado cuando el agente alcanza el objetivo
/// Maneja la recompensa y reinicio del episodio
/// </summary>
public class TurtleReachedGoalState : TurtleStateBase
{
    public TurtleReachedGoalState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Enter()
    {
        agent.AddReward(10f);
        agent.SetColor(Color.green);
        agent.ResetEpisode();
    }

    public override void Update()
    {
        // Mantener el estado hasta que se complete el reset del episodio
    }
} 
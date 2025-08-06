using UnityEngine;

public class TurtleReachedGoalState : TurtleStateBase
{
    public TurtleReachedGoalState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Enter()
    {
        // Dar recompensa por llegar al objetivo
        agent.AddReward(10f);
        
        // Cambiar color a verde
        agent.SetColor(Color.green);
        
        // NO llamar a OnEpisodeEnd() aquí - ResetEpisode() se encarga de completar el episodio
        
        // Reiniciar episodio inmediatamente (sin delay)
        agent.ResetEpisode();
    }

    public override void Update()
    {
        // Mantener el estado ReachedGoal hasta que se complete el reset del episodio
        // No cambiar de estado manualmente, el reset del episodio se encargará de todo
    }
} 
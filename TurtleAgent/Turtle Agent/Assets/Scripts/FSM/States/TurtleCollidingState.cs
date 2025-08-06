using UnityEngine;

public class TurtleCollidingState : TurtleStateBase
{
    public TurtleCollidingState(TurtleFSM fsm, TurtleAgentFSM agent) : base(fsm, agent) { }

    public override void Enter()
    {
        // Cambiar color a rojo
        agent.SetColor(Color.red);
        
        // Aplicar penalización inicial por colisión
        agent.AddReward(-0.05f);
        
        // Notificar al sistema de métricas de la colisión
        var metrics = agent.GetComponent<TurtleMetrics>();
        if (metrics != null)
        {
            metrics.OnCollision();
        }
    }

    public override void Update()
    {
        // La penalización por colisión se maneja en FixedUpdate
    }

    public override void FixedUpdate()
    {
        // Penalización continua mientras está colisionando
        agent.AddReward(-0.01f * Time.fixedDeltaTime);
    }

    public override void Exit()
    {
        // Restaurar color azul cuando sale de la colisión
        agent.SetColor(Color.blue);
    }
} 
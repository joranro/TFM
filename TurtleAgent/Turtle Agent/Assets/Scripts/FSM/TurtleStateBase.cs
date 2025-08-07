using UnityEngine;

/// <summary>
/// Clase base abstracta para todos los estados de la FSM
/// Define la interfaz común que deben implementar todos los estados
/// </summary>
public abstract class TurtleStateBase
{
    protected TurtleFSM fsm;
    protected TurtleAgentFSM agent;

    public TurtleStateBase(TurtleFSM fsm, TurtleAgentFSM agent)
    {
        this.fsm = fsm;
        this.agent = agent;
    }

    /// <summary>
    /// Se ejecuta al entrar al estado
    /// </summary>
    public virtual void Enter() { }
    
    /// <summary>
    /// Se ejecuta cada frame mientras el agente está en este estado
    /// </summary>
    public virtual void Update() { }
    
    /// <summary>
    /// Se ejecuta en cada FixedUpdate mientras el agente está en este estado
    /// </summary>
    public virtual void FixedUpdate() { }
    
    /// <summary>
    /// Se ejecuta al salir del estado
    /// </summary>
    public virtual void Exit() { }
    
    /// <summary>
    /// Eventos de colisión que pueden ser manejados por el estado
    /// </summary>
    public virtual void OnTriggerEnter(Collider other) { }
    public virtual void OnCollisionEnter(Collision collision) { }
    public virtual void OnCollisionStay(Collision collision) { }
    public virtual void OnCollisionExit(Collision collision) { }
} 
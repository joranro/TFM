using UnityEngine;

public abstract class TurtleStateBase
{
    protected TurtleFSM fsm;
    protected TurtleAgentFSM agent;

    public TurtleStateBase(TurtleFSM fsm, TurtleAgentFSM agent)
    {
        this.fsm = fsm;
        this.agent = agent;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
    public virtual void OnTriggerEnter(Collider other) { }
    public virtual void OnCollisionEnter(Collision collision) { }
    public virtual void OnCollisionStay(Collision collision) { }
    public virtual void OnCollisionExit(Collision collision) { }
} 
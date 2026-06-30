using UnityEngine;
using UnityEngine.Events;

public abstract class LeaderAction
{
    protected SquadLeader leader;
    public UnityEvent OnCompleted = new UnityEvent();

    virtual public void Init(SquadLeader _leader)
    {
        leader = _leader;
    }

    virtual public void Enter()
    {

    }

    virtual public void Exit()
    {
    }

    virtual public void Update()
    {

    }
}

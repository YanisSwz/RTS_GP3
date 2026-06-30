using System.Collections.Generic;
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
        List<Unit> units = leader.Squad.GetControlledUnits;
        foreach (Unit unit in units)
            unit.SquadOrder = null;
    }
}

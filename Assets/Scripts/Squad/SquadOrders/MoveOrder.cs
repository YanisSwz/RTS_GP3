using UnityEngine;

public class MoveOrder : SquadOrder
{
    public float StoppingDistance = 0.5f;
    public override void Enter(Unit unit)
    {
        base.Enter(unit);
        unit.MoveTo(unit.MoveToTarget);
    }

    public override void Exit(Unit unit) 
    { 
        base.Exit(unit); 
    }

    public override void Update(Unit unit)
    {
        base.Update(unit);

        if (isActionComplete == false && unit.HasReachDest(StoppingDistance))
        {
            unit.StopMoving();
            isActionComplete = true;
        }
    }
}

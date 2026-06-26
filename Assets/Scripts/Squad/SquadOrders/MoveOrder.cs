using UnityEngine;

public class MoveOrder : SquadOrder
{
    public float StoppingDistance = 0.5f;
    public override void Enter(Unit unit)
    {
        Debug.Log(unit.gameObject.name + " start move to order");
        base.Enter(unit);
        unit.MoveTo(unit.MoveToTarget);
    }

    public override void Exit(Unit unit) 
    {
        Debug.Log(unit.gameObject.name + " arrived to target pos");
        base.Exit(unit);
        unit.StopMoving();
    }

    public override void Update(Unit unit)
    {
        base.Update(unit);

        if (isActionComplete == false && unit.HasReachDest(StoppingDistance))
        {
            //unit.StopMoving();
            isActionComplete = true;
        }
    }
}

using UnityEngine;

public class AttackOrder : SquadOrder
{
    public override void Enter(Unit unit)
    {
        base.Enter(unit);
        if (unit.ComputeAttack() == false)
            isActionComplete = true;
    }

    public override void Exit(Unit unit)
    {
        base.Exit(unit);
        unit.EntityTarget = null;
    }

    public override void Update(Unit unit)
    {
        base.Update(unit);
        
        if (isActionComplete == false && unit.EntityTarget == null)
            isActionComplete = true;

        unit.ComputeAttack();
    }
}
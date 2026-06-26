using UnityEngine;

public class CaptureOrder : SquadOrder
{
    public override void Enter(Unit unit)
    {
        base.Enter(unit);

        unit.StartCapture(unit.CaptureTarget);
    }

    public override void Exit(Unit unit)
    {
        base.Exit(unit);

        unit.StopCapture();
    }

    public override void Update(Unit unit)
    {
        base.Update(unit);

        if (isActionComplete == false && unit.CanCapture(unit.CaptureTarget) == false)
        {
            isActionComplete = true;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class CaptureLeaderAction : LeaderAction
{
    public TargetBuilding targetCapture;

    public override void PauseAction()
    {
        base.PauseAction();
        leader.Squad.OnAllActionsCompleted.RemoveListener(CaptureCompleted);
    }

    public override void ResumeAction()
    {
        base.ResumeAction();
        Enter();
    }

    public override void Enter()
    {
        base.Enter();

        //create action sequence
        List<SquadAction> actions = new List<SquadAction>();

        //compute move to stopping distance
        float maxCaptureRadius = 0f;
        foreach (Unit unit in leader.Squad.GetControlledUnits)
        {
            float capDist = unit.GetUnitData.CaptureDistanceMax;
            if (capDist > maxCaptureRadius)
                maxCaptureRadius = capDist;
        }

        //add move to actions
        SquadMoveTo moveToAction = new SquadMoveTo();
        moveToAction.Init(leader.Squad, targetCapture.transform.position + ((leader.Squad.GetSquadAveragePos() - targetCapture.transform.position).normalized * 5f), 1f);
        moveToAction.distanceToFinalTarget = maxCaptureRadius;
        actions.Add(moveToAction);

        SquadCapture captureAction = new SquadCapture();
        //todo place ai by their own capture radius
        captureAction.Init(leader.Squad, targetCapture.gameObject, maxCaptureRadius * 0.7f);
        captureAction.minAngleBetweenUnitToCreateNewLine = 30f;
        actions.Add(captureAction);

        leader.Squad.GiveActions(actions);

        leader.Squad.OnAllActionsCompleted.AddListener(CaptureCompleted);
    }

    private void CaptureCompleted(Squad squad)
    {
        OnCompleted.Invoke();
    }
}

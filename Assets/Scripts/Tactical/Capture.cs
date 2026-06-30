using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Capture : Dispatch
{
    private TargetBuilding targetLab = null;

    public override void Enter(General owner, float power)
    {
        base.Enter(owner, power);

        targetLab = null;

        float bestDistance = Mathf.Infinity;
        Vector3 basePos = owner.Leaders[0].Squad.GetSquadAveragePos();
        foreach(TargetBuilding lab in owner.GetController.discoveredLabs) 
        {
            float dist = Vector3.Distance(basePos, lab.transform.position);
            if(dist < bestDistance)
            {
                bestDistance = dist;
                targetObject = lab.gameObject;
                targetLab = lab;
            }
        }

        //create action sequence
        List<SquadAction> actions = new List<SquadAction>();

        //compute move to stopping distance
        float maxCaptureRadius = 0f;
        foreach (Unit unit in owner.Leaders[0].Squad.GetControlledUnits)
        {
            float capDist = unit.GetUnitData.CaptureDistanceMax;
            if (capDist > maxCaptureRadius)
                maxCaptureRadius = capDist;
        }

        //add move to actions
        SquadMoveTo moveToAction = new SquadMoveTo();
        moveToAction.Init(owner.Leaders[0].Squad, targetObject.transform.position + ((owner.Leaders[0].Squad.GetSquadAveragePos() - targetObject.transform.position).normalized * 5f), 1f);
        moveToAction.distanceToFinalTarget = maxCaptureRadius;
        actions.Add(moveToAction);

        if (targetLab.GetTeam() != owner.GetController.GetTeam())
        {
            SquadCapture captureAction = new SquadCapture();
            //todo place ai by their own capture radius
            captureAction.Init(owner.Leaders[0].Squad, targetObject, maxCaptureRadius * 0.7f);
            captureAction.minAngleBetweenUnitToCreateNewLine = 30f;
            actions.Add(captureAction);
        }

        //send action
        owner.Leaders[0].Squad.GiveActions(actions);
    }
}

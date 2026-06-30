using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Capture : Dispatch
{
    public override void Enter(General owner, float power)
    {
        base.Enter(owner, power);

        TargetBuilding targetLab = null;

        float bestDistance = Mathf.Infinity;
        Vector3 basePos = owner.Leaders[0].Squad.GetSquadAveragePos();
        foreach (TargetBuilding lab in owner.GetController.discoveredLabs)
        {
            float dist = Vector3.Distance(basePos, lab.transform.position);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                targetObject = lab.gameObject;
                targetLab = lab;
            }
        }

        CaptureLeaderAction captureLeaderAction = new CaptureLeaderAction();
        captureLeaderAction.targetCapture = targetLab;
        captureLeaderAction.Init(owner.Leaders[0]);

        //send action
        owner.Leaders[0].GiveGeneralOrder(captureLeaderAction);
    }
}

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Capture : Dispatch
{
    public override void Enter(General owner, float power)
    {
        base.Enter(owner, power);

        foreach (SquadLeader leader in owner.Leaders)
        {
            TargetBuilding targetLab = null;

            float bestDistance = Mathf.Infinity;
            Vector3 basePos = leader.Squad.GetSquadAveragePos();
            foreach (TargetBuilding lab in owner.GetController.discoveredLabs)
            {
                float dist = Vector3.Distance(basePos, lab.transform.position);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    targetLab = lab;
                }
            }

            CaptureLeaderAction captureLeaderAction = new CaptureLeaderAction();
            captureLeaderAction.targetCapture = targetLab;
            captureLeaderAction.Init(leader);

            //send action
            leader.GiveGeneralOrder(captureLeaderAction);
        }
    }
}

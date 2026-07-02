using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Explore : Dispatch
{
    public float radius = 100f;
    public float tolerance = 50f;
    public float patrolRadius = 40f;

    public override GeneralAction GenerateCopy()
    {
        Explore explore = new Explore();
        explore.radius = radius;
        explore.tolerance = tolerance;
        explore.patrolRadius = patrolRadius;

        return explore;
    }

    // Send squad to explore a point near base, and from there explore randomly until finding a lab or another goal interrupts
    public override void Enter(General owner, float power)
    {
        base.Enter(owner, power);

        foreach (SquadLeader leader in owner.Leaders)
        {
           Vector3? test = GameServices.GetRandomPoint(owner.GetController.GetFactoryList[0].transform.position, new Vector3(1f, 0f, -1f), radius, 90f, tolerance);
            if (test.HasValue)
            {
                targetPosition = test.Value;
                ExploreLeaderAction exploreAction = new ExploreLeaderAction();
                exploreAction.exploreDest = targetPosition;
                exploreAction.exploreRadius = patrolRadius;
                exploreAction.tolerenceRadius = tolerance;
                exploreAction.Init(leader);

                leader.GiveGeneralOrder(exploreAction);
            }
        }
    }
}

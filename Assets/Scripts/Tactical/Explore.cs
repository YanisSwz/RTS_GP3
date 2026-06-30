using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Explore : Dispatch
{
    public float radius = 100f;
    public float tolerance = 50f;
    public float patrolRadius = 40f;

    public override void Enter(General owner, float power)
    {
        base.Enter(owner, power);

        Vector3? test = GameServices.GetRandomPoint(owner.GetController.GetFactoryList[0].transform.position, radius, 90f, tolerance);
        if (test.HasValue)
        {
            target = test.Value;
            Debug.Log(target);
            ExploreLeaderAction exploreAction = new ExploreLeaderAction();
            exploreAction.exploreDest = target;
            exploreAction.exploreRadius = patrolRadius;
            exploreAction.tolerenceRadius = tolerance;
            exploreAction.Init(owner.Leaders[0]);
            
            owner.Leaders[0].GiveGeneralOrder(exploreAction);
        }

        isComplete = true;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Explore : Dispatch
{
    public float radius = 100f;
    public float tolerance = 50f;

    public override void Enter(General owner, float power)
    {
        base.Enter(owner, power);

        Vector3? test = GetRandomPoint(owner.GetController.GetFactoryList[0].transform.position, radius, 90f);
        if (test.HasValue)
        {
            target = test.Value;
            Debug.Log(target);

            List<SquadAction> actions = new List<SquadAction>();
            SquadMoveTo moveTo = new SquadMoveTo();
            moveTo.Init(owner.Leaders[0].Squad, target, 1f);
            actions.Add(moveTo);
            owner.Leaders[0].Squad.GiveActions(actions);
        }

        isComplete = true;
    }

    private Vector3? GetRandomPoint(Vector3 position, float radius, float angle) 
    {
        Vector3 randomDirection = Quaternion.AngleAxis(Random.Range(0f, angle), Vector3.up) * Vector3.right;
        Vector3 randomPosition = position + randomDirection * radius;

        NavMeshHit hit;
        Vector3? finalPosition = null;
        if (NavMesh.SamplePosition(randomPosition, out hit, tolerance, 1))
            finalPosition = hit.position;

        return finalPosition;
    }
}

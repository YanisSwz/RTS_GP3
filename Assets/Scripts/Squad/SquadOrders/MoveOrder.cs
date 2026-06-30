using System.Net;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.UI.CanvasScaler;

public class MoveOrder : SquadOrder
{
    public float StoppingDistance = 0.5f;
    public bool stopOnArrived = false;

    //check for dest
    Vector3 prevDest = Vector3.zero;
    bool pathComputed = false;
    Vector3 endPoint = Vector3.zero;
    int nbTry = 3;
    public override void Enter(Unit unit)
    {
        base.Enter(unit);
        nbTry = 0;
        prevDest = unit.GetDestination();
        GetEndPos(unit);
        unit.MoveTo(unit.MoveToTarget);
    }

    private void GetEndPos(Unit unit)
    {
        if((unit.transform.position - unit.MoveToTarget).magnitude < StoppingDistance)
        {
            isActionComplete = true;
            return;
        }

        NavMeshPath navMeshPath = new NavMeshPath();
        pathComputed = NavMesh.CalculatePath(unit.transform.position, unit.MoveToTarget, unit.GetNavMeshArea(), navMeshPath);
        --nbTry;
        if (nbTry <= 0 && pathComputed == false && prevDest != unit.GetDestination())
        {
            pathComputed = true;
            endPoint = unit.GetDestination();
            return;
        }

        if (pathComputed)
            endPoint = navMeshPath.corners[^1];
    }

    public override void Exit(Unit unit) 
    {
        base.Exit(unit);
        if(stopOnArrived)
            unit.StopMoving();
    }

    public override void Update(Unit unit)
    {
        base.Update(unit);

        if (pathComputed == false)
            GetEndPos(unit);

        else if (isActionComplete == false && (endPoint - unit.transform.position).magnitude < StoppingDistance)
        {
            isActionComplete = true;
        }
    }
}

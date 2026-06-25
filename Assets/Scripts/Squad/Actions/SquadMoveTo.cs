using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SquadMoveTo : SquadAction
{
    //staticPath
    int currentIndex = 0;
    NavMeshPath staticPath = new NavMeshPath();

    bool IsStaticPath = false;

    [HideInInspector]
    public float distanceToFinalTarget = 0f;

    public override void Init(Squad _squad, Vector3 _staticTarget, float _distanceToTarget)
    {
        base.Init(_squad, _staticTarget, _distanceToTarget);
        IsStaticPath = true;
    }

    public override void Init(Squad _squad, GameObject _movingTarget, float _distanceToTarget)
    {
        base.Init(_squad, _movingTarget, _distanceToTarget);
        IsStaticPath = false;
    }

    public override void StartAction()
    {
        base.StartAction();

        Vector3 squadPos = squad.GetSquadAveragePos();
        //compute static path for virtual leader
        if (NavMesh.CalculatePath(squadPos, staticTarget, NavMesh.AllAreas, staticPath))
        {
            List<Vector3> poses = new List<Vector3>();
            switch (squad.GetFormationStyle)
            {
                case Squad.FormationStyle.None:
                    {
                        Vector3 target;
                        ComputeStaticPathPos(squadPos, out target);
                        poses = ComputeSquadFormation.FreestyleFormation(target, squad.GetFreestyleFormationPoses);
                        break;
                    }
            }

            GivePoses(poses);
        }
        else
            Debug.Log("path failed to compute");
    }


    public override void UpdateAction()
    {
        base.UpdateAction();
        if(IsStaticPath == false)
            staticTarget = movingTarget.transform.position;
        
        List<Vector3> poses = new List<Vector3>();

        switch(squad.GetFormationStyle)
        {
            case Squad.FormationStyle.None:
                {
                    Vector3 squadPos = squad.GetSquadAveragePos();

                    Vector3 target;
                    if(ComputeStaticPathPos(squadPos, out target))
                        poses = ComputeSquadFormation.FreestyleFormation(target, squad.GetFreestyleFormationPoses);
                    break;
                }
            case Squad.FormationStyle.Line:
                {
                    break;
                }
            case Squad.FormationStyle.Circle:
                {

                    break;
                }
        }

        GivePoses(poses);
    }

    private bool ComputeStaticPathPos(Vector3 squadPos, out Vector3 targetPos)
    {

        //get nearest unit pos to target --> if minDistToTarget <= distanceToFinalTarget = arrived stop moving
        bool considerFinalDist = distanceToFinalTarget > distanceToTarget;
        //float minDistToTarget = float.MaxValue;
        int indexUnitChecked = 0;

        //check if squad average pos is arrived to the waypoint
        bool squadArrived = (squadPos - staticPath.corners[currentIndex]).magnitude <= distanceToTarget;
        
        List<Unit> units = squad.GetControlledUnits;
        //check if all units arrived; this check because the terrain can affect the formation and shift the average squad pos
        if (squadArrived == false)
        {
            bool broke = false;
            foreach (Unit unit in units)
            {
                if (considerFinalDist)
                {
                    //get the nearest unit to the target
                    ++indexUnitChecked;
                    float distToTarget = (unit.transform.position - staticTarget).magnitude;

                    //this unit is close enough to the final target 
                    if (distToTarget <= distanceToFinalTarget)
                    {
                        StopAllUnit();

                        targetPos = squadPos;
                        return false;
                    }
                }

                if (unit.HasReachDest(distanceToTarget) == false)
                {
                    //a unit is still moving => not arrived
                    broke = true;
                    break;
                }
            }

            if (broke == false)
                squadArrived = true;
        }

        //continue to check the rest of the team if the prev foreach loop broke
        if (considerFinalDist && indexUnitChecked < units.Count)
        {
            for (; indexUnitChecked < units.Count; ++indexUnitChecked)
            {
                float distToTarget = (units[indexUnitChecked].transform.position - staticTarget).magnitude;
                //this unit is close enough to the final target 
                if (distToTarget <= distanceToFinalTarget)
                {
                    StopAllUnit();

                    targetPos = squadPos;
                    return false;
                }
            }
        }

        //next path point
        if (squadArrived)
        {
            ++currentIndex;

            //if last point
            if (currentIndex == staticPath.corners.Length)
            {
                targetPos = squadPos;
                OnComplete.Invoke();
                return false;
            }
        }

        targetPos = staticPath.corners[currentIndex];
        return squadArrived;
    }

    private void GivePoses(List<Vector3> poses)
    {
        if (poses.Count == 0)
            return;

        List<Unit> squadUnits = squad.GetControlledUnits;
        for (int i = 0; i < squadUnits.Count; ++i)
        {
            squadUnits[i].SetTargetPos(poses[i]);
        }
    }

    private void StopAllUnit()
    {
        foreach (Unit unit in squad.GetControlledUnits)
            unit.StopMoving();

        OnComplete.Invoke();
    }
}

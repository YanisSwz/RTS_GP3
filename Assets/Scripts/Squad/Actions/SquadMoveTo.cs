using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class SquadMoveTo : SquadAction
{
    //staticPath
    int currentIndex = 0;
    NavMeshPath staticPath = new NavMeshPath();

    bool IsStaticPath = false;

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
        bool squadArrived = (squadPos - staticPath.corners[currentIndex]).magnitude < distanceToTarget;

        //check if all units arrived at last path point
        if (squadArrived == false)
        {
            List<Unit> units = squad.GetControlledUnits;

            bool broke = false;
            foreach (Unit unit in units)
            {
                if (unit.HasReachDest(distanceToTarget) == false)
                {
                    broke = true;
                    break;
                }
            }

            if (broke == false)
                squadArrived = true;
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
}

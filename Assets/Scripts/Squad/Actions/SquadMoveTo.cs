using System.Collections.Generic;
using System.Linq;
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

    List<Vector3> arrivedPos = new List<Vector3>();


    Vector3 debugTarget;
    Vector3 debugFirstUnit;

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
        Debug.Log("Squad Start Moving to target");
        base.StartAction();

        Vector3 squadPos = squad.GetSquadAveragePos();

        debugTarget = staticTarget;
        debugFirstUnit = squad.GetControlledUnits[0].transform.position;

        if (CalculatePath(squad.GetControlledUnits[0].transform.position, debugTarget) == false)
        {
            Debug.Log("path failed to compute");
            OnAbort.Invoke();
        }

        ////compute static path for virtual leader
        //if (CalculatePath(squadPos, debugTarget) == false)
        //{

        //    //fail because average pos is not on the navmesh (like in a factory)
        //    //recompute a path with the first unit in the squad as the anchor
        //    if (CalculatePath(squad.GetControlledUnits[0].transform.position, debugTarget) == false)
        //    {
        //        Debug.Log("path failed to compute");
        //        OnAbort.Invoke();
        //    }
        //}


        /*
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
        {
            //fail because average pos is not on the navmesh (like in a factory)
            //recompute a path by th first unit in the squad

            debugFirstUnit = squad.GetControlledUnits[0].transform.position;
            if (NavMesh.CalculatePath(squad.GetControlledUnits[0].transform.position, staticTarget, NavMesh.AllAreas, staticPath))
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
            {
                Debug.Log("path failed to compute");
                OnAbort.Invoke();
            }
        }*/
    }

    private bool CalculatePath(Vector3 anchor, Vector3 target)
    {
        if (NavMesh.CalculatePath(anchor, target, NavMesh.AllAreas, staticPath))
        {
            switch (squad.GetFormationStyle)
            {
                case Squad.FormationStyle.None:
                    {
                        //Vector3 target;
                        //ComputeStaticPathPos(squadPos, out target);
                        arrivedPos = ComputeSquadFormation.FreestyleFormation(staticPath.corners[staticPath.corners.Length - 1]
                            , squad.GetFreestyleFormationPoses);

                        break;
                    }
            }

            GivePoses(arrivedPos);
            return true;
        }
        return false;
    }

    public override void ExitAction()
    {
        base.ExitAction();
        Debug.Log("Squad arrived to target");
    }

    public override void UpdateAction()
    {
        if (staticPath.corners.Length == 0)
        {
            OnAbort.Invoke();
            return;
        }

        base.UpdateAction();
        if (IsStaticPath == false)
            staticTarget = movingTarget.transform.position;

        List<Unit> units = squad.GetControlledUnits;
        for (int i = 0; i < units.Count; ++i)
        {
            if (units[i].SquadOrder == null && units[i].GetDestination() != arrivedPos[i])
                units[i].SetTargetPos(arrivedPos[i], distanceToTarget);
        }

        if (IsSquadArrived())
            OnComplete.Invoke();
    }

    private bool IsSquadArrived()
    {
        //get nearest unit pos to target --> if minDistToTarget <= distanceToFinalTarget = arrived stop moving
        bool considerFinalDist = distanceToFinalTarget > distanceToTarget;
        List<Unit> units = squad.GetControlledUnits;

        if (considerFinalDist)
        {
            for (int i = 0; i < units.Count; ++i)
            {
                float distToTarget = (units[i].transform.position - staticTarget).magnitude;
                //this unit is close enough to the final target 
                if (distToTarget <= distanceToFinalTarget)
                {
                    StopAllUnit();
                    return true;
                }
            }

            return false;
        }
        else
        {
            //check if some units still moving
            for (int i = 0; i < units.Count; ++i)
            {
                if (units[i].SquadOrder as MoveOrder != null)
                    return false;
            }
        }

        return true;
    }

    private void GivePoses(List<Vector3> poses)
    {
        if (poses.Count == 0)
            return;

        List<Unit> squadUnits = squad.GetControlledUnits;
        for (int i = 0; i < squadUnits.Count; ++i)
            squadUnits[i].SetTargetPos(poses[i], distanceToTarget, true);
    }

    private void StopAllUnit()
    {
        foreach (Unit unit in squad.GetControlledUnits)
            unit.SquadOrder = null;
    }

    public override void DrawGizmo()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < staticPath.corners.Count(); ++i)
        {
            if(i >= currentIndex)
                Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(staticPath.corners[i], 2f);
        }

        foreach (Vector3 v in arrivedPos)
            Gizmos.DrawCube(v, Vector3.one + Vector3.up * 3f);

        Gizmos.DrawCube(debugTarget, Vector3.one + Vector3.up * 3f);

        Gizmos.DrawCube(debugFirstUnit, Vector3.one + Vector3.up * 3f);
    }
}

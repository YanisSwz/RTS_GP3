using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SquadMoveTo : SquadAction
{
    //staticPath
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

        if (IsStaticPath == false)
            staticTarget = movingTarget.transform.position;

        debugTarget = staticTarget;
        debugFirstUnit = squad.GetControlledUnits[0].transform.position;

        if (CalculatePath(squad.GetControlledUnits[0].transform.position, staticTarget) == false)
        {
            Debug.Log("path failed to compute");
            OnAbort.Invoke();
        }
    }

    private bool CalculatePath(Vector3 anchor, Vector3 target)
    {
        if (NavMesh.CalculatePath(anchor, target, NavMesh.AllAreas, staticPath))
        {
            switch (squad.GetFormationStyle)
            {
                case Squad.FormationStyle.None:
                    {
                        arrivedPos = ComputeSquadFormation.FreestyleFormation(staticPath.corners[^1]
                            , squad.GetFreestyleFormationPoses);

                        break;
                    }

                case Squad.FormationStyle.Line:
                    {
                        arrivedPos = ComputeSquadFormation.PreComputeLine(squad.LinePoses, staticPath.corners[^1], target - anchor);
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
        {
            staticTarget = movingTarget.transform.position;
            CalculatePath(squad.GetControlledUnits[0].transform.position, staticTarget);
        }

        if (IsSquadArrived())
            OnComplete.Invoke();
    }

    private bool IsSquadArrived()
    {
        //get nearest unit pos to target --> if minDistToTarget <= distanceToFinalTarget = arrived stop moving
        //used for capture and attack
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
        foreach (Vector3 v in arrivedPos)
            Gizmos.DrawCube(v, Vector3.one + Vector3.up * 3f);

        Gizmos.DrawCube(debugTarget, Vector3.one + Vector3.up * 3f);

        Gizmos.DrawCube(debugFirstUnit, Vector3.one + Vector3.up * 3f);
    }
}

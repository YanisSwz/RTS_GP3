using System.Collections.Generic;
using UnityEngine;

public class SquadCapture : SquadAction
{
    public float minAngleBetweenUnitToCreateNewLine = 10f;
    private List<Vector3> captureFormation = new List<Vector3>();
    private TargetBuilding target;

    public override void Init(Squad _squad, GameObject _movingTarget, float _distanceToTarget)
    {
        base.Init(_squad, _movingTarget, _distanceToTarget);
        target = _movingTarget.GetComponent<TargetBuilding>();
        staticTarget = _movingTarget.transform.position;
    }

    public override void StartAction()
    {
        Debug.Log("Start Capture");
        base.StartAction();

        Vector3 squadPos = squad.GetSquadAveragePos();
        Vector3 dir = squadPos - staticTarget;
        int nbUnit = squad.GetNbUnit;

        float angleBetweenUnit = 360f / (float)(nbUnit);
        int nbLine = 1;
        if (angleBetweenUnit < minAngleBetweenUnitToCreateNewLine)
        {
            angleBetweenUnit = minAngleBetweenUnitToCreateNewLine;
            ++nbLine;
        }

        int nbUnitPerLine = Mathf.FloorToInt(360f / angleBetweenUnit);
       
        captureFormation = ComputeSquadFormation.CirclePoses(dir.normalized, staticTarget, distanceToTarget, 1.5f, squad.GetNbUnit, nbUnitPerLine, 0, angleBetweenUnit, 10f);

        List<Unit> units = squad.GetControlledUnits;
        for (int i = 0; i < captureFormation.Count; ++i)
        {
            units[i].SetTargetPos(captureFormation[i]);
        }
    }

    public override void UpdateAction()
    {
        List<Unit> units = squad.GetControlledUnits;
        for (int i = 0; i < captureFormation.Count; ++i)
        {
            if(units[i].IsCapturing() == false && units[i].HasReachDest(0.5f) && units[i].CanCapture(target))
            {
                units[i].SetCaptureTarget(target);

                //maybe rotate toward the field
            }
        }

        if (target.GetTeam() == units[0].GetTeam())
        {
            Debug.Log("End Capture");
            OnComplete.Invoke();
        }
    }

    public override void DrawGizmo()
    {
        List<Unit> units = squad.GetControlledUnits;

        for (int i = 0; i < captureFormation.Count; ++i)
        {

            Gizmos.color = units[i].HasReachDest(0.5f) ? Color.green : Color.blue;
            Gizmos.DrawWireSphere(captureFormation[i], 2f);
        }
    }
}

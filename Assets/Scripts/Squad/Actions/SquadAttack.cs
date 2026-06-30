using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class SquadAttack : SquadAction
{
    public BaseEntity enemyBaseTarget;

    public override void Init(Squad _squad, GameObject _movingTarget, float _distanceToTarget)
    {
        base.Init(_squad, _movingTarget, _distanceToTarget);
    }

    public override void Init(Squad _squad, Vector3 _staticTarget, float _distanceToTarget)
    {
        base.Init(_squad, _staticTarget, _distanceToTarget);
    }

    public override void StartAction()
    {
        base.StartAction();
        Debug.Log("Squad Start Attacking");

        //form attack formation

        SortedDictionary<int, List<Unit>> sortSquad = new SortedDictionary<int, List<Unit>>();

        foreach(Unit unit in squad.GetControlledUnits)
        {
            int attkDist = (int)unit.GetUnitData.AttackDistanceMax;
            if (sortSquad.ContainsKey(attkDist))
            {
                sortSquad[attkDist].Add(unit);
            }
            else
            {
                sortSquad[attkDist]= new List<Unit>();
                sortSquad[attkDist].Add(unit);
            }
        }

        float baseAngle = 10f;
        float angleInrease = 5f;

        Vector3 squadPos = squad.GetSquadAveragePos();

        float keyIndex = 0;
        float nbKey = sortSquad.Count;
        foreach (int attkDist in sortSquad.Keys)
        {
            List<Unit> units = sortSquad[attkDist];
            List<Vector3> poses = ComputeSquadFormation.CirclePoses((squadPos - enemyBaseTarget.transform.position).normalized, enemyBaseTarget.transform.position, attkDist * Mathf.Lerp(0.4f, 0.9f, keyIndex/nbKey), 1f
                , units.Count, units.Count, 0, baseAngle, 0f);
            for (int i = 0; i < poses.Count; i++)
            {
                units[i].SetTargetPos(poses[i], 0.5f, true);
            }

            ++keyIndex;
            baseAngle += angleInrease;
        }
    }
    public override void ExitAction()
    {
        base.ExitAction();
        foreach (Unit unit in squad.GetControlledUnits)
            unit.SquadOrder = null;
        Debug.Log("Squad End Attacking");
    }

    void GiveTarget()
    {
        foreach(Unit unit in squad.GetControlledUnits)
        {
            if (unit.SquadOrder == null)
            {
                //todo switch target
                if (unit.EntityTarget == null)
                {
                    if(enemyBaseTarget)
                        unit.SetAttackTarget(enemyBaseTarget);
                    else
                    {
                        Unit target = PickNearestEnemy(unit);
                        if(target != null)
                        {
                            if(unit.CanAttack(target))
                                unit.SetAttackTarget(target);
                            //else
                                //todo move to
                        }
                    }
                }
            }
        }
    }

    private Unit PickNearestEnemy(Unit unitAskTarget)
    {
        Unit result = null;
        Vector3 pos = unitAskTarget.transform.position;

        float nearestDist = float.MaxValue;
        List<Unit> enemiesInSight = squad.enemiesInSight;

        for (int i = 0; i < enemiesInSight.Count; ++i)
        {
            if (enemiesInSight[i] == null)
                continue;

            float dist = (enemiesInSight[i].transform.position - pos).magnitude;
            if (dist < nearestDist)
            {
                nearestDist = dist;
                result = enemiesInSight[i];
            }
        }

        return result;
    }

    private bool IsAllEnemyDead()
    {
        //todo see all target in sight
        return enemyBaseTarget == null && squad.enemiesInSight.Count == 0;
    }

    public override void UpdateAction()
    {
        base.UpdateAction();
        if (IsAllEnemyDead())
        {
            OnComplete.Invoke();
        }
        else
        {
            GiveTarget();
        }

    }


    public override void DrawGizmo()
    {
        base.DrawGizmo();
    }
}

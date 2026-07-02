using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Android;

public class SquadAttack : SquadAction
{
    BaseEntity enemyBaseTarget;

    public override void Init(Squad _squad, GameObject _movingTarget, float _distanceToTarget)
    {
        base.Init(_squad, _movingTarget, _distanceToTarget);
        enemyBaseTarget = _movingTarget.GetComponent<BaseEntity>();
    }

    public override void Init(Squad _squad, Vector3 _staticTarget, float _distanceToTarget)
    {
        base.Init(_squad, _staticTarget, _distanceToTarget);
    }

    public override void StartAction()
    {
        base.StartAction();
        Debug.Log("Squad Start Attacking");

        if (enemyBaseTarget == null)
            enemyBaseTarget = GetNearestEntityFromPos(squad.GetSquadAveragePos());


        if(enemyBaseTarget == null)
        {
            OnComplete.Invoke();
            return;
        }

        //form attack formation
        AttackFormation(enemyBaseTarget.transform.position);
    }

    private void AttackFormation(Vector3 target)
    {
        SortedDictionary<int, List<Unit>> sortSquad = new SortedDictionary<int, List<Unit>>();

        foreach (Unit unit in squad.GetControlledUnits)
        {
            int attkDist = (int)unit.GetUnitData.AttackDistanceMax;
            if (sortSquad.ContainsKey(attkDist))
            {
                sortSquad[attkDist].Add(unit);
            }
            else
            {
                sortSquad[attkDist] = new List<Unit>();
                sortSquad[attkDist].Add(unit);
            }
        }

        float baseAngle = 15f;
        float angleInrease = 10f;

        Vector3 squadPos = squad.GetSquadAveragePos();

        float keyIndex = 0;
        float nbKey = sortSquad.Count;
        foreach (int attkDist in sortSquad.Keys)
        {
            List<Unit> units = sortSquad[attkDist];
            List<Vector3> poses = ComputeSquadFormation.CirclePoses((squadPos - target).normalized, target, attkDist * Mathf.Lerp(0.4f, 0.9f, keyIndex / nbKey), 1f
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
    }

    void GiveTarget()
    {
        foreach(Unit unit in squad.GetControlledUnits)
        {
            if (unit.SquadOrder == null)
            {
                //if target killed
                if (unit.EntityTarget == null)
                {
                    //get new target
                    BaseEntity target = enemyBaseTarget != null ? enemyBaseTarget : PickNearestEnemy(unit);

                    //if no target stay idle
                    if(target != null)
                    {
                        //check attack
                        if(unit.SetAttackTarget(target) == false)
                        {
                            //if can't attack go to unit
                            unit.SetTargetPos(target.transform.position, unit.GetUnitData.AttackDistanceMax * 0.75f);
                        }
                    }
                }
                else
                    unit.SetTargetPos(unit.EntityTarget.transform.position, unit.GetUnitData.AttackDistanceMax * 0.75f);
            }
        }
    }

    private BaseEntity PickNearestEnemy(Unit unitAskTarget)
    {
        Vector3 pos = unitAskTarget.transform.position;
        return GetNearestEntityFromPos(pos);
    }

    BaseEntity GetNearestEntityFromPos(Vector3 pos)
    {
        BaseEntity result = null;

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

        List<Factory> factoriesInSight = squad.factoriesInSight;
        for (int i = 0; i < factoriesInSight.Count; ++i)
        {
            float dist = (factoriesInSight[i].transform.position - pos).magnitude;
            if (dist < nearestDist)
            {
                nearestDist = dist;
                result = factoriesInSight[i];
            }
        }

        return result;
    }

    private bool IsAllEnemyDead()
    {
        //todo see all target in sight
        return enemyBaseTarget == null && squad.enemiesInSight.Count == 0 && squad.factoriesInSight.Count == 0;
    }

    public override void UpdateAction()
    {
        base.UpdateAction();
        if (IsAllEnemyDead())
        {
            Vector3 squadPos = squad.GetSquadAveragePos();

            AttackFormation(squadPos + (squadPos - squad.GetControlledUnits[0].transform.position).normalized);
            Debug.Log("Squad End Attacking");

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

using NUnit.Framework;
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
        GiveTarget();
    }
    public override void ExitAction()
    {
        base.ExitAction();

        Debug.Log("Squad End Attacking");
    }

    void GiveTarget()
    {
        foreach(Unit unit in squad.GetControlledUnits)
        {
            //todo switch target
            if (unit.EntityTarget == null)
                unit.SetAttackTarget(enemyBaseTarget);
        }
    }

    private bool IsAllEnemyDead()
    {
        //todo see all target in sight
        return enemyBaseTarget == null;
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

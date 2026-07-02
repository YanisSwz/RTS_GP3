using UnityEngine;

public class FSM_RetaliateState : FSM_State
{
    public float chaseRadius = 10f;
    Vector3 basePos = Vector3.zero;
    public override void EnterState()
    {
        base.EnterState();
        fsmEntity.EntityTarget = fsmEntity.LastDamageDealer;
        basePos = fsmEntity.transform.position;
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void UpdateState()
    {
        base.UpdateState();

        if (stateCancel)
            return;

        //switch retaliate target
        if (CheckGiveUpRetaliateTarget() == false)
        {
            CancelRetaliate();
            return;
        }

        //check dist to base pos
        if (CheckDistToBasePos())
        {
            //check attack dist
            if (CanAttackTarget())
            {
                fsmEntity.ComputeAttack();
            }
            else
            {
                //move to target
                fsmEntity.MoveTo(fsmEntity.EntityTarget.transform.position);
            }
        }
        else
        {
            //go back to base pos
            fsmEntity.MoveTo(basePos);
        }
    }

    private bool CheckGiveUpRetaliateTarget()
    {
        //switch target
        if (fsmEntity.EntityTarget == null)
        {
            fsmEntity.EntityTarget = fsmEntity.LastDamageDealer;

            if (fsmEntity.EntityTarget == null)
                return false;
        }

        //check if target is too far
        if ((fsmEntity.transform.position - fsmEntity.EntityTarget.transform.position).magnitude >= chaseRadius + fsmEntity.GetUnitData.AttackDistanceMax)
            return false;

        return true;
    }

    private bool CanAttackTarget()
    {
        if(fsmEntity.EntityTarget == null)
            return false;

        return fsmEntity.CanAttack(fsmEntity.EntityTarget);
    }

    private bool CheckDistToBasePos()
    {
        return (fsmEntity.transform.position - basePos).magnitude <= chaseRadius;
    }

    private void CancelRetaliate()
    {
        fsmEntity.EntityTarget = null;
        fsmEntity.LastDamageDealer = null;
        fsmEntity.MoveTo(basePos);
    }
}

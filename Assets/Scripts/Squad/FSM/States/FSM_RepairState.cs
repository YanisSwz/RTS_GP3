using UnityEngine;

public class FSM_RepairState : FSM_State
{
    Vector3 basePos;
    public override void EnterState()
    {
        base.EnterState();

        basePos = fsmEntity.transform.position;
        if (PickNewTarget())
            fsmEntity.MoveTo(fsmEntity.EntityTarget.transform.position);

    }

    public override void ExitState()
    {
        base.ExitState();
        fsmEntity.EntityTarget = null;
        fsmEntity.entityToRepair.Clear();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        if (stateCancel)
            return;

        //check entity to repair status
        if (fsmEntity.EntityTarget == null || fsmEntity.EntityTarget.NeedsRepairing() == false)
        {
            if(PickNewTarget() == false)
            {
                //no more ally to repair => return to base pos
                fsmEntity.MoveTo(basePos);
                return;
            }
        }

        //check distance
        if (fsmEntity.CanRepair(fsmEntity.EntityTarget))
            fsmEntity.ComputeRepairing();

        //move to target
        else
            fsmEntity.MoveTo(fsmEntity.EntityTarget.transform.position);
    }

    private bool PickNewTarget()
    {
        //update list to repair
        fsmEntity.GetAllyNeedHeal();

        //no more ally to heal
        if (fsmEntity.entityToRepair.Count == 0)
            return false;

        //get nearest
        float dist = float.MaxValue;
        foreach(BaseEntity entity in fsmEntity.entityToRepair)
        {
            if(entity == null)
                continue;

            float distEntity = (entity.transform.position - fsmEntity.transform.position).magnitude;

            if(distEntity < dist)
            {
                fsmEntity.EntityTarget = entity;
                dist = distEntity;
            }
        }

        if(fsmEntity.EntityTarget == null)
            return false;

        return true;
    }
}

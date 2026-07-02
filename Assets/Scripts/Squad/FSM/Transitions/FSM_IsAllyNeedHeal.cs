using UnityEngine;

public class FSM_IsAllyNeedHeal : FSM_ConditionTransition
{
    public override bool CheckCondition(Unit fsmEntity)
    {
        if(fsmEntity.GetUnitData.CanRepair == false)
            return false;

        bool result = fsmEntity.entityToRepair.Count > 0;
        return reverseCondition ? !result : result;
    }
}
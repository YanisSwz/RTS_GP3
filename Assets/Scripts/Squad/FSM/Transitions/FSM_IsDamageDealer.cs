using UnityEngine;

public class FSM_IsDamageDealer : FSM_ConditionTransition
{
    public override bool CheckCondition(Unit fsmEntity)
    {
        bool result = fsmEntity.LastDamageDealer != null;
        return reverseCondition ? !result : result;
    }
}

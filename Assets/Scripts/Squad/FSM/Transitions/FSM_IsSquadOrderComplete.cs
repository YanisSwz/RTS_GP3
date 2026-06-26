using UnityEngine;

public class FSM_IsSquadOrderComplete : FSM_ConditionTransition
{
    public override bool CheckCondition(Unit fsmEntity)
    {
        bool result = fsmEntity.SquadOrder == null;
        return reverseCondition ? !result : result;
    }
}

using UnityEngine;

public class FSM_IsSquadOrderComplete : FSM_ConditionTransition
{
    public override bool CheckCondition(Unit fsmEntity)
    {
        return fsmEntity.GetSquadOrder == null;
    }
}

using UnityEngine;

public class FSM_SquadOrder : FSM_State
{
    
    public override void EnterState()
    {
        base.EnterState();
        fsmEntity.GetSquadOrder.Enter(fsmEntity);
    }

    public override void UpdateState()
    {
        base.UpdateState();
        fsmEntity.GetSquadOrder.Update(fsmEntity);
    }

    public override void ExitState()
    {
        base.ExitState();
        fsmEntity.GetSquadOrder.Exit(fsmEntity);
    }
}

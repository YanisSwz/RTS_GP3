using UnityEngine;

public class FSM_SquadOrder : FSM_State
{
    public override void EnterState()
    {
        base.EnterState();
        //squad order "Enter" handled on squadOrderSwap
    }

    public override void UpdateState()
    {
        base.UpdateState();

        if (fsmEntity.SquadOrder != null)
        {
            //if complete remove action --> transit an other state
            if (fsmEntity.SquadOrder.isActionComplete)
                fsmEntity.SquadOrder = null;
            else
                fsmEntity.SquadOrder.Update(fsmEntity);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        //squad order "Exit" handled on squadOrderSwap
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class FSM_IdleState : FSM_State
{
    public override void EnterState()
    {
        base.EnterState();
        //priority modif
    }

    public override void ExitState() 
    { 
        base.ExitState();
        //priority modif
    }

    public override void UpdateState()
    {
        base.UpdateState();

        if (stateCancel)
            return;

        //if can repair check around if ally needs to be repare
        if(fsmEntity.GetUnitData.CanRepair)
            fsmEntity.GetAllyNeedHeal();
    }
}

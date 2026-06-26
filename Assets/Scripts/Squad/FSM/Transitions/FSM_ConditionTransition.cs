using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class FSM_Transition : MonoBehaviour
{

}



//Check condition in tick
[Serializable]
public class FSM_ConditionTransition : FSM_Transition
{
    public bool reverseCondition = false;

    public virtual void ResetCondition(Unit fsmEntity)
    {

    }

    public virtual bool CheckCondition(Unit fsmEntity)
    {
        return true;
    }
}

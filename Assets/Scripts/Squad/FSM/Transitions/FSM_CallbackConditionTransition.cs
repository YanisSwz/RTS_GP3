using System;
using UnityEngine;
using UnityEngine.Events;

//make transition on callback called
[Serializable]
public class FSM_CallbackConditionTransition : FSM_Transition
{
    [HideInInspector]
    public UnityEvent CallbackCondition;

    public void CallBackTriggered()
    {
        CallbackCondition?.Invoke();
    }
}

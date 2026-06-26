using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class FSM_NewStateTransition
{
    //All conditions (condition tick and callback)
    public List<FSM_Transition> conditions = new List<FSM_Transition>();

    //Conditions to check on update
    private List<FSM_ConditionTransition> conditionTickCheck = new List<FSM_ConditionTransition>();
    
    //Conditions to check by callback
    private List<FSM_CallbackConditionTransition> conditionCallback = new List<FSM_CallbackConditionTransition>();

    //New state to transition to
    [HideInInspector]
    public FSM_State currentState = null;

    //New state to transition to
    public FSM_State newState = null;

    public void SortCondition(FSM_State state)
    {
        currentState = state;

        //sort conditions
        foreach (FSM_Transition transition in conditions)
        {
            if(transition is FSM_ConditionTransition tickCondition)
                conditionTickCheck.Add(tickCondition);
            else if(transition is FSM_CallbackConditionTransition callbackConditionTransition)
                conditionCallback.Add(callbackConditionTransition);
        }
    }

    public void BindCallbackConditions()
    {
        //bind change state event on callbackCondition Invoked
        foreach (FSM_CallbackConditionTransition condition in conditionCallback)
            condition.CallbackCondition.AddListener(CallbackChangeState);
    }

    public void UnBindCallbackConditions()
    {
        //unbind change state event to callbackCondition
        foreach (FSM_CallbackConditionTransition condition in conditionCallback)
            condition.CallbackCondition.RemoveListener(CallbackChangeState);
    }

    private void CallbackChangeState()
    {
        currentState.ChangeStateEvent.Invoke(newState);
    }

    public void ResetTransition(Unit fsmEntity)
    {
        foreach(FSM_ConditionTransition transition in conditionTickCheck)
        {
            transition.ResetCondition(fsmEntity);
        }
    }

    /// All tick condition must be true to enter
    public bool CheckCondition(Unit fsmEntity)
    {
        if (conditionTickCheck.Count == 0)
            return false;

        //check all condition => if 1 false break
        foreach (FSM_ConditionTransition condition in conditionTickCheck)
        {
            if (!condition.CheckCondition(fsmEntity))
                return false;
        }

        return true;
    }
}

public class FSM_State : MonoBehaviour
{
    public List<FSM_NewStateTransition> transitions = new List<FSM_NewStateTransition>();

    public Unit FsmEntity { set { fsmEntity = value; } }
    protected Unit fsmEntity;

    [HideInInspector]
    public UnityEvent<FSM_State> ChangeStateEvent;

    private void Awake()
    {
        foreach (FSM_NewStateTransition transition in transitions)
            transition.SortCondition(this);
    }

    public virtual int GetStateScore()
    {
        return 0;
    }

    public virtual void EnterState()
    {
        foreach(FSM_NewStateTransition transition in transitions)
        {
            transition.BindCallbackConditions();
            transition.ResetTransition(fsmEntity);
        }
    }

    public virtual void ExitState()
    {
        foreach (FSM_NewStateTransition transition in transitions)
        {
            transition.UnBindCallbackConditions();
        }
    }

    public virtual void UpdateState()
    {
        float bestScore = -1f;
        int choosenState = -1;

        int nbTransition = transitions.Count;
        for (int i = 0; i < nbTransition; i++)
        {
            if(transitions[i].CheckCondition(fsmEntity) && transitions[i].newState.GetStateScore() > bestScore)
                choosenState = i;
        }

        if(choosenState > -1)
            ChangeStateEvent.Invoke(transitions[choosenState].newState);
    }
}

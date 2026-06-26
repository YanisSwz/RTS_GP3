using System.Collections.Generic;
using UnityEngine;

public class FSM_Director : MonoBehaviour
{
    private List<FSM_State> m_states = new List<FSM_State>();
    private int m_currentState = 0;

    [HideInInspector]
    public Unit fsmEntity;
    void Start()
    {
        int nbChild = transform.childCount;
        for (int i = 0; i < nbChild; i++)
        {
            FSM_State state;
            transform.GetChild(i).TryGetComponent<FSM_State>(out state);
            if (state)
            {
                state.FsmEntity = fsmEntity;
                m_states.Add(state);
                state.ChangeStateEvent.AddListener(ChangeState);
            }
        }

        m_states[m_currentState].EnterState();
    }

    void Update()
    {
        m_states[m_currentState].UpdateState();
    }

    public void ChangeState(FSM_State newState)
    {
        if (!newState)
        {
            Debug.LogError("New State null, current State " + m_states[m_currentState].name);
            return;
        }

        int index = m_states.FindIndex(state => newState == state);
        if (index < 0)
        {
            Debug.LogError("Can't change state " + newState.name + ", current state " + m_states[m_currentState].name);
            return;
        }

        Debug.Log( fsmEntity.name + " // " + m_states[m_currentState].name + " ==> " + m_states[index].name);

        m_states[m_currentState].ExitState();

        m_currentState = index;
        m_states[m_currentState].EnterState();
       
    }
}

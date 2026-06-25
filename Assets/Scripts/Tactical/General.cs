using System.Collections.Generic;
using UnityEngine;

public enum GoalType
{
    None = 0,
    Build = 1,
    Attack = 2,
    Capture = 3
}

[System.Serializable]
public struct GoalSequence 
{
    public GoalSequence(GoalType _goalType, List<GeneralAction> _actions) 
    {
        goalType = _goalType;
        actions = _actions;
    }

    public GoalType goalType;
    [SerializeReference, SubclassSelector]
    public List<GeneralAction> actions;
}

[System.Serializable]
public class General
{
    private AIController owner = null;
    private int currentActionIndex = -1;

    public List<GoalSequence> sequences = new List<GoalSequence>();
    public List<GeneralAction> actions = new List<GeneralAction>();

    public void SetOwner(AIController controller) 
    {
        owner = controller;
    }

    public void SetGoal(Goal goal) 
    {
        GoalSequence? sequenceToExecute = sequences.Find(x => x.goalType == goal.Type);
        if(sequenceToExecute != null) 
        {
            actions = sequenceToExecute.Value.actions;
            if (actions.Count > 0)
            {
                currentActionIndex = 0;
                actions[currentActionIndex].Enter();
            }
        }
    }

    public void UpdateSequence()
    {
        if (currentActionIndex >= actions.Count)
            return;

        if (actions[currentActionIndex].IsComplete)
        {
            currentActionIndex++;
            if (currentActionIndex >= actions.Count)
                return;

            actions[currentActionIndex].Enter();
        }

        actions[currentActionIndex].Execute(owner);
    }
}

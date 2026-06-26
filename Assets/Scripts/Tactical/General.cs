using System.Collections.Generic;
using UnityEngine;

public enum GoalType
{
    None = 0,
    Build = 1,
    Attack = 2,
    Capture = 3,
    Explore = 4
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
    private Goal currentGoal = null;
    public Goal CurrentGoal { get { return currentGoal; } }

    public List<GoalSequence> sequences = new List<GoalSequence>();
    public List<GeneralAction> actions = new List<GeneralAction>();

    public void SetOwner(AIController controller) 
    {
        owner = controller;
    }

    public void SetGoal(Goal goal) 
    {
        int index = sequences.FindIndex(x => x.goalType == goal.Type);
        if(index != -1) 
        {
            actions = sequences[index].actions;
            if (actions.Count > 0)
            {
                currentActionIndex = 0;
                currentGoal = goal;
                actions[currentActionIndex].Enter(owner);
            }
        }
    }

    public void UpdateSequence()
    {
        if (currentActionIndex == -1 || currentActionIndex >= actions.Count)
        {
            currentGoal = null;
            return;
        }

        if (actions[currentActionIndex].IsComplete)
        {
            ++currentActionIndex;
            if (currentActionIndex >= actions.Count)
            {
                currentGoal = null;
                return;
            }

            actions[currentActionIndex].Enter(owner);
        }

        actions[currentActionIndex].Execute(owner);
    }
}

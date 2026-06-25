using System.Collections.Generic;
using UnityEngine;

public enum GoalType
{
    None = 0,
    Build = 1,
    Recruit = 2,
    Attack = 3,
    Capture = 4
}

[System.Serializable]
public struct GoalSequence 
{
    public GoalSequence(GoalType _goalType, List<string> _actions) 
    {
        goalType = _goalType;
        actions = _actions;
    }

    public GoalType goalType;
    public List<string> actions;
}

[System.Serializable]
public class General
{
    public List<GoalSequence> sequences = new List<GoalSequence>();
    public List<string> actions = new List<string>();

    public void SetGoal(Goal goal) 
    {
        GoalSequence? sequenceToExecute = sequences.Find(x => x.goalType == goal.Type);
        if(sequenceToExecute != null) 
        {
            actions = sequenceToExecute.Value.actions;
        }
    }
}

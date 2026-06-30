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
        actions = new (_actions);
    }

    public GoalType goalType;
    [SerializeReference, SubclassSelector]
    public List<GeneralAction> actions;
}

[System.Serializable]
public class General
{
    public AIController GetController { get { return owner; } }
    private AIController owner = null;
    private int currentActionIndex = -1;
    private float currentPower = 0f;
    private Goal currentGoal = null;
    public Goal CurrentGoal { get { return currentGoal; } }

    public List<GoalSequence> sequences = new List<GoalSequence>();
    public List<GeneralAction> actions = new List<GeneralAction>();
    private List<SquadLeader> leaders = new List<SquadLeader>();
    public List<SquadLeader> Leaders { get { return leaders; } }
    public void AddLeader(SquadLeader leader) 
    {
        leaders.Add(leader);
    }

    public void SetOwner(AIController controller) 
    {
        owner = controller;
    }

    public void SetGoal(Goal goal) 
    {
        if(goal == currentGoal)
        {
            currentGoal.SetUtility(goal.Utility);
            currentPower = currentGoal.Utility;
            return;
        }

        int index = sequences.FindIndex(x => x.goalType == goal.Type);
        if(index != -1) 
        {
            actions = sequences[index].actions;
            if (actions.Count > 0)
            {
                currentActionIndex = 0;
                currentGoal = goal;
                currentPower = currentGoal.Utility;
                actions[currentActionIndex].Enter(this, currentPower);
            }
        }
    }

    public void LeaderActionCompleted(SquadLeader leader)
    {
        List<SquadAction> actions = new List<SquadAction>();
        SquadMoveTo moveTo = new SquadMoveTo();
        moveTo.Init(leader.Squad, GameServices.GetRandomPoint(owner.GetFactoryList[0].transform.position, 30, 360, 50f).Value, 1f);
        actions.Add(moveTo);
        leader.Squad.GiveActions(actions);

        leader.Squad.DestroySquad(GetController);

        leaders.Remove(leader);
    }

    public void UpdateSequence()
    {
        if (currentActionIndex == -1 || currentActionIndex >= actions.Count)
        {
            return;
        }

        if (actions[currentActionIndex].IsComplete)
        {
            ++currentActionIndex;
            if (currentActionIndex >= actions.Count)
            {
                return;
            }

            actions[currentActionIndex].Enter(this, currentPower);
        }

        actions[currentActionIndex].Execute(this, currentPower);
    }
}

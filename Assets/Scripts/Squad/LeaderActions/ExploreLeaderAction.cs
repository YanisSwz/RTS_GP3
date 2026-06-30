using System.Collections.Generic;
using UnityEngine;

public class ExploreLeaderAction : LeaderAction
{
    public Vector3 exploreDest;
    public float exploreRadius = 30f;
    public float tolerenceRadius = 50f;

    public override void Enter()
    {
        base.Enter();

        SendSquadExplore(exploreDest);

        leader.Squad.OnLabInSight.AddListener(ExplorationCompleted);
        leader.Squad.OnAllActionsCompleted.AddListener(NextDestToSearch);
    }

    private void ExplorationCompleted(TargetBuilding target)
    {
        OnCompleted.Invoke();
    }

    private void SendSquadExplore(Vector3 dest)
    {
        List<SquadAction> actions = new List<SquadAction>();

        SquadMoveTo moveTo = new SquadMoveTo();
        moveTo.Init(leader.Squad, dest, 4f);
        moveTo.distanceToFinalTarget = 4f;

        actions.Add(moveTo);
        leader.Squad.GiveActions(actions);
    }

    private void NextDestToSearch(Squad squad)
    {
        Vector3? nextDest = GameServices.GetRandomPoint(squad.GetControlledUnits[0].transform.position, exploreRadius, 360f, 50f);

        if(nextDest.HasValue)
            SendSquadExplore(nextDest.Value);
        else
            NextDestToSearch(squad);
    }
}

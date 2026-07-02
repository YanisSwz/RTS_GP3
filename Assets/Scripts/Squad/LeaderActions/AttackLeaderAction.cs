using System.Collections.Generic;
using UnityEngine;

/*
 Send with a move to the squad at the selected menace point
On enemy in sight => pause action => attack => enemy kill => resume action
On resume or move to completed => explore "explorationTryBeforeFail" times around the menace pos selected to see if enemy still here
 */

public class AttackLeaderAction : LeaderAction
{
    public Vector3 menacePos;
    public float searchEnemyRadius = 50f;
    public int explorationTryBeforeFail = 5;

    public override void Enter()
    {
        base.Enter();

        float maxAttackRadius = 0f;
        foreach (Unit unit in leader.Squad.GetControlledUnits)
        {
            float atkDist = unit.GetUnitData.AttackDistanceMax;
            if (atkDist > maxAttackRadius)
                maxAttackRadius = atkDist;
        }

        List<SquadAction> actions = new List<SquadAction>();

        //add move to action
        SquadMoveTo moveToAction = new SquadMoveTo();
        moveToAction.Init(leader.Squad, menacePos + ((leader.Squad.GetSquadAveragePos() - menacePos).normalized * 10f), 1f);

        moveToAction.distanceToFinalTarget = maxAttackRadius;

        actions.Add(moveToAction);

        //send action
        leader.Squad.GiveActions(actions);

        //callback on arrived to menace pos 
        leader.Squad.OnAllActionsCompleted.AddListener(ArrivedToMenacePos);
    }

    public override void PauseAction()
    {
        base.PauseAction();
        //interupt on enemy/factory in sight, decision tree of the leader attack the target
        leader.Squad.OnAllActionsCompleted.RemoveListener(ArrivedToMenacePos);
        leader.Squad.OnAllActionsCompleted.RemoveListener(SearchForEnemy);
    }
    public override void ResumeAction()
    {
        base.ResumeAction();
        //when attacked target died => check around the base menace pos if there are still enemies
        ArrivedToMenacePos(leader.Squad);
    }

    void ArrivedToMenacePos(Squad squad)
    {
        //check around the base menace pos if there are still enemies
        leader.Squad.OnAllActionsCompleted.AddListener(SearchForEnemy);
        SearchForEnemy(leader.Squad);
    }

    void SearchForEnemy(Squad squad)
    {
        if (squad.GetControlledUnits.Count == 0)
        {
            OnCompleted.Invoke();
            return;
        }

        if(explorationTryBeforeFail <= 0)
        {
            AIController aiController = GameServices.GetControllerByTeam(squad.GetControlledUnits[0].GetTeam()) as AIController;
            if(aiController != null)
                aiController.RemoveMenace(menacePos, searchEnemyRadius);
    
            //end check around menace point
            OnCompleted.Invoke();
            return;
        }

        //search around the menacePos in a radius of "searchEnemyRadius"
        Vector3? nextDest = GameServices.GetRandomPoint(menacePos, squad.GetControlledUnits[0].transform.forward, searchEnemyRadius, 360f, 50f);

        if (nextDest.HasValue)
            SendSquadExplore(nextDest.Value);
        else
            SearchForEnemy(squad);
    }

    private void SendSquadExplore(Vector3 dest)
    {
        --explorationTryBeforeFail;
        List<SquadAction> actions = new List<SquadAction>();

        SquadMoveTo moveTo = new SquadMoveTo();
        moveTo.Init(leader.Squad, dest, 4f);
        moveTo.distanceToFinalTarget = 4f;

        actions.Add(moveTo);
        leader.Squad.GiveActions(actions);
    }


}

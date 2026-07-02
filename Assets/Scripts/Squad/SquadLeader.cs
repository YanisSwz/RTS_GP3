using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.GridLayoutGroup;

public class SquadLeader
{
    Squad controlledSquad = null;
    General general = null;
    LeaderAction generalOrder = null;

    public UnityEvent<SquadLeader> OnActionComplete = new UnityEvent<SquadLeader>();
    private bool actionPause = false;
    public Squad Squad { get { return controlledSquad; } }

    private bool isAttacking = false;

    public void GiveSquad(Squad _controlledSquad, General owner)
    {
        general = owner;
        OnActionComplete.AddListener(general.LeaderActionCompleted);
        controlledSquad = _controlledSquad;

        controlledSquad.OnSquadKilled.AddListener(SquadKilled);
        controlledSquad.OnEnemyInSight.AddListener(EnemiesInSightCallback);
        controlledSquad.OnLabInSight.AddListener(LabInSightCallback);
        controlledSquad.OnFactoryInSight.AddListener(EnemyFactoryInSightCallback);
    }

    private void SquadKilled(Squad squad)
    {
        Debug.Log("squad killed");
        controlledSquad = null;
        DestroyLeader();
    }

    public void GiveGeneralOrder(LeaderAction action)
    {
        if(generalOrder != null)
        {
            generalOrder.Exit();
        }

        generalOrder = action;

        if (generalOrder != null)
        {
            generalOrder.Enter();
            generalOrder.OnCompleted.AddListener(ActionComplete);
        }
    }
    public void DestroyLeader()
    {
        GiveGeneralOrder(null);
        OnActionComplete.Invoke(this);
    }

    private void ActionComplete()
    {
        DestroyLeader();
    }

    private void EnemyFactoryInSightCallback(Factory factory)
    {
        AIController controller = general.GetController;
        if(!controller.discoveredEnemyFactories.Contains(factory))
            controller.discoveredEnemyFactories.Add(factory);
    }

    private void LabInSightCallback(TargetBuilding lab)
    {
        AIController controller = general.GetController;
        if (!controller.discoveredLabs.Contains(lab))
            controller.discoveredLabs.Add(lab);
    }

    private void EnemiesInSightCallback(List<Unit> units)
    {
        MenaceMemory menace = new MenaceMemory();
        menace.time = Time.time;
        menace.nbEnemiesSpotted = units.Count;

        menace.enemyPower = 0;
        foreach (Unit unit in units)
            menace.enemyPower += unit.Cost;

        foreach (Unit unit in units)
            menace.enemyAveragePos += unit.transform.position;
        menace.enemyAveragePos /= (float)(menace.nbEnemiesSpotted);

        general.GetController.AddMenaceMemory(menace);


        DecisionOnEnemySee(menace, units);
    }

    private void DecisionOnEnemySee(MenaceMemory menace, List<Unit> units)
    {
        if (generalOrder != null && actionPause == false)
        {
            actionPause = true;
            generalOrder.PauseAction();
        }

        int teamPower = 0;

        if (Squad.allyFactoriesInSight.Count > 0)
            teamPower = int.MaxValue;
        else
        {
            foreach (Unit unit in Squad.allyInSight)
                teamPower += unit.Cost;

            foreach (Unit unit in Squad.GetControlledUnits)
                teamPower += unit.Cost;
        }


        if (teamPower >= menace.enemyPower || generalOrder as AttackLeaderAction != null)
        {
            //attack target
            if (isAttacking == false)
            {
                isAttacking = true;
                List<SquadAction> actions = new List<SquadAction>();
                SquadMoveTo moveTo = new SquadMoveTo();
                moveTo.Init(Squad, units[0].gameObject, 1f);

                float dist = 0f;
                foreach (Unit unit in Squad.GetControlledUnits)
                {
                    float attackDist = unit.GetUnitData.AttackDistanceMax;
                    if (attackDist > dist)
                        dist = attackDist;
                }

                moveTo.distanceToFinalTarget = dist;
                actions.Add(moveTo);

                SquadAttack squadAttack = new SquadAttack();
                squadAttack.Init(Squad, units[0].gameObject, dist);
                actions.Add(squadAttack);

                Squad.GiveActions(actions);

                Squad.OnAllActionsCompleted.AddListener(EnemyKilledCallback);
            }
        }
        else
        {
            //retreat
            DestroyLeader();
        }
    }

    private void EnemyKilledCallback(Squad squad)
    {
        //on enemies killed => resume general order
        if (generalOrder == null)
        {
            ActionComplete();
            return;
        }
        else
            generalOrder.ResumeAction();

        actionPause = false;
        isAttacking = false;
    }
}

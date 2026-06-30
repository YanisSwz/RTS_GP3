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

    public Squad Squad { get { return controlledSquad; } }

    public void GiveSquad(Squad _controlledSquad, General owner)
    {
        general = owner;
        OnActionComplete.AddListener(general.LeaderActionCompleted);
        controlledSquad = _controlledSquad;

        controlledSquad.OnEnemyInSight.AddListener(EnemiesInSightCallback);
        controlledSquad.OnLabInSight.AddListener(LabInSightCallback);
    }

    public void GiveGeneralOrder(LeaderAction action)
    {
        if(generalOrder != null)
            generalOrder.Exit();

        generalOrder = action;

        if (generalOrder != null)
        {
            generalOrder.Enter();
            generalOrder.OnCompleted.AddListener(ActionComplete);
        }
    }

    private void ActionComplete()
    {
        GiveGeneralOrder(null);

        OnActionComplete.Invoke(this);
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

        foreach (Unit unit in units)
            menace.enemyAveragePos += unit.transform.position;
        menace.enemyAveragePos /= (float)(menace.nbEnemiesSpotted);

        general.GetController.AddMenaceMemory(menace);
    }

}

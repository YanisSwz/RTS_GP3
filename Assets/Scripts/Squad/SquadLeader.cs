using System.Collections.Generic;
using UnityEngine;

public class SquadLeader
{
    Squad controlledSquad = null;
    General general = null;
    List<SquadAction> orders = new List<SquadAction>();

    public Squad Squad { get { return controlledSquad; } }

    public void GiveSquad(Squad _controlledSquad, General owner)
    {
        general = owner;
        controlledSquad = _controlledSquad;

        controlledSquad.OnEnemyInSight.AddListener(EnemiesInSightCallback);
        controlledSquad.OnLabInSight.AddListener(LabInSightCallback);
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

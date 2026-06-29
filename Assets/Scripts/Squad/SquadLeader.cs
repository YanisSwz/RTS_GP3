using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class SquadLeader
{
    List<Squad> controlledSquads = new List<Squad>();
    General general;

    public void GiveSquads(List<Squad> _controlledSquads)
    {
        controlledSquads = new List<Squad>(_controlledSquads);

        foreach (Squad squad in controlledSquads)
        {
            squad.OnEnemyInSight.AddListener(EnemiesInSightCallback);
            squad.OnLabInSight.AddListener(LabInSightCallback);
        }
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FormSquad : GeneralAction
{
    [SerializeField]
    private SquadDataPreset squadData = null;

    [SerializeField]
    private List<Unit> squadUnits = null;

    private Dictionary<int, int> unitsToRecruit = new Dictionary<int, int>();
    private Squad squad = null;
    private int squadSize = 0;
    private float squadCost = 0f;
    private int currentSquadBudget = 0;
    private int minUnitCost = int.MaxValue;

    public override void Enter(AIController controller, float power)
    {
        base.Enter(controller, power);

        unitsToRecruit = squadData.squadData.GetUnits;
        EvaluateSquadCost(controller, power);
    }

    public override void Execute(AIController controller, float power)
    {
        GetAvailableUnits(controller);
        RecruitUnits(controller);
        CheckSquadReadiness(controller);
    }

    private void EvaluateSquadCost(AIController controller, float power)
    {
        for (int i = 0; i < squadData.squadData.Lines.Count; ++i)
        {
            int cost = squadData.squadData.Lines[i].unitType.Cost;
            if (cost < minUnitCost)
                minUnitCost = cost;

            squadCost += cost * squadData.squadData.Lines[i].numberOfUnits;

        }
        currentSquadBudget = Mathf.RoundToInt(Mathf.Min(squadCost, controller.TotalBuildPoints * power));
    }

    private void GetAvailableUnits(AIController controller)
    {
        List<Unit> availableUnitsCopy = new List<Unit>(controller.availableUnits);
        for (int i = 0; i < availableUnitsCopy.Count; ++i)
        {
            int unitKey = availableUnitsCopy[i].GetTypeId;
            if (unitsToRecruit.ContainsKey(unitKey))
            {
                squadUnits.Add(availableUnitsCopy[i]);
                controller.availableUnits.Remove(availableUnitsCopy[i]);
                currentSquadBudget -= availableUnitsCopy[i].Cost;

                ++squadSize;
                unitsToRecruit[unitKey] = unitsToRecruit[unitKey] - 1;
                if (unitsToRecruit[unitKey] == 0)
                    unitsToRecruit.Remove(unitKey);
            }
        }
    }

    private void RecruitUnits(AIController controller)
    {
        Dictionary<int, int> unitsCopy = new(unitsToRecruit);
        foreach (KeyValuePair<int, int> entry in unitsCopy)
        {
            if (!controller.CanRecruitUnit(entry.Key))
            {
                unitsToRecruit.Remove(entry.Key);
                continue;
            }

            for (int j = 0; j < entry.Value; ++j)
            {
                UnityEvent<Unit> unitRecruited = controller.RecruitUnit(entry.Key);
                if (unitRecruited != null)
                {
                    unitRecruited.AddListener(AddUnit);

                    ++squadSize;
                    unitsToRecruit[entry.Key] = unitsToRecruit[entry.Key] - 1;
                    if (unitsToRecruit[entry.Key] == 0)
                        unitsToRecruit.Remove(entry.Key);
                }
            }
        }
    }

    private void CheckSquadReadiness(AIController controller)
    {
        if (squadUnits.Count == squadSize && currentSquadBudget < minUnitCost)
        {
            squad = new Squad();
            squad.FormSquad(controller, Squad.FormationStyle.None, squadUnits);

            // Actions test
            List<SquadAction> actions = new List<SquadAction>();
            SquadMoveTo moveTo = new SquadMoveTo();
            moveTo.Init(squad, new Vector3(308f, 0f, 234f), 1f);
            actions.Add(moveTo);
            squad.GiveActions(actions);

            isComplete = true;
        }
    }

    private void AddUnit(Unit unit)
    {
        squadUnits.Add(unit);
        currentSquadBudget -= unit.Cost;
    }
}

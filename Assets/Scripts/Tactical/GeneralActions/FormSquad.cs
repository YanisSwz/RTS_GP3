using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.GridLayoutGroup;

[System.Serializable]
public class FormSquad : GeneralAction
{
    [SerializeField]
    private List<SquadDataPreset> squadPresets = new List<SquadDataPreset>();

    private int currentSquadPresetIndex = -1;
    private List<Unit> squadUnits = new List<Unit>();
    private Dictionary<int, int> unitsToRecruit = new Dictionary<int, int>();
    private int squadSize = 0;
    private int squadCost = 0;
    private int currentSquadBudget = 0;
    private int minUnitCost = int.MaxValue;
    private bool recruited = false;
    private bool waitingToRally = false;

    public override GeneralAction GenerateCopy()
    {
        FormSquad formSquad = new FormSquad();
        formSquad.squadPresets = squadPresets;

        return formSquad;
    }

    public override void Enter(General owner, float power)
    {
        base.Enter(owner, power);

        GetPresetIndex(owner.GetController);

        // We get the units to recruit from the preset
        unitsToRecruit = squadPresets[currentSquadPresetIndex].squadData.GetUnits;
        if (unitsToRecruit.Count == 0)
        {
            owner.ActionFailed(this);
            return;
        }
        EvaluateSquadCost(owner, power);
    }

    public override void Execute(General owner, float power)
    {
        if (!waitingToRally)
        {
            GetAvailableUnits(owner.GetController);
            RecruitUnits(owner);
            CheckSquadReadiness(owner);
        }
    }

    private void EvaluateSquadCost(General owner, float power)
    {
        for (int i = 0; i < squadPresets[currentSquadPresetIndex].squadData.Lines.Count; ++i)
        {
            int cost = squadPresets[currentSquadPresetIndex].squadData.Lines[i].unitType.Cost;
            if (cost < minUnitCost)
                minUnitCost = cost;

            squadCost += cost * squadPresets[currentSquadPresetIndex].squadData.Lines[i].numberOfUnits;

        }
        currentSquadBudget = Mathf.RoundToInt(Mathf.Min(squadCost, owner.GetController.TotalBuildPoints * power));
    }

    private void GetPresetIndex(AIController controller) 
    {
        if (controller.availableUnits.Count > 0)
        {
            // We check the most available unit type, to avoid creating too much new units
            Dictionary<int, int> availableUnitsID = GetAvailableUnitsType(controller);
            List<int> IDs = availableUnitsID.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

            for (int j = 0; j < IDs.Count; ++j)
            {
                for (int i = 0; i < squadPresets.Count; ++i)
                {
                    if (squadPresets[i].squadData.GetUnits.ContainsKey(IDs[j]))
                    {
                        currentSquadPresetIndex = i;
                        break;
                    }
                }
            }

            if (currentSquadPresetIndex == -1)
                currentSquadPresetIndex = Random.Range(0, squadPresets.Count);
        }
        else
        {
            currentSquadPresetIndex = Random.Range(0, squadPresets.Count);
        }
    }

    private Dictionary<int, int> GetAvailableUnitsType(AIController controller)
    {
        Dictionary<int, int> availableUnitsID = new Dictionary<int, int>();
        for (int i = 0; i < controller.availableUnits.Count; ++i)
        {
            if (availableUnitsID.ContainsKey(controller.availableUnits[i].GetTypeId))
                availableUnitsID[controller.availableUnits[i].GetTypeId] += 1;
            else
                availableUnitsID[controller.availableUnits[i].GetTypeId] = 1;
        }
        return availableUnitsID;
    }

    private void GetAvailableUnits(AIController controller)
    {
        List<Unit> availableUnitsCopy = new List<Unit>(controller.availableUnits);
        for (int i = 0, j = 0; i < availableUnitsCopy.Count; ++i, ++j)
        {
            int unitKey = availableUnitsCopy[i].GetTypeId;
            if (unitsToRecruit.ContainsKey(unitKey))
            {
                squadUnits.Add(controller.availableUnits[j]);
                controller.availableUnits.Remove(availableUnitsCopy[i]);
                --j;
                currentSquadBudget -= availableUnitsCopy[i].Cost;

                ++squadSize;
                unitsToRecruit[unitKey] = unitsToRecruit[unitKey] - 1;
                if (unitsToRecruit[unitKey] == 0)
                    unitsToRecruit.Remove(unitKey);
            }
        }
    }

    private void RecruitUnits(General general)
    {
        Dictionary<int, int> unitsCopy = new(unitsToRecruit);
        foreach (KeyValuePair<int, int> entry in unitsCopy)
        {
            // If heavy factory not built
            if (!general.GetController.CanRecruitUnit(entry.Key))
            {
                unitsToRecruit.Remove(entry.Key);
                continue;
            }

            for (int j = 0; j < entry.Value; ++j)
            {
                UnityEvent<Unit> unitRecruited = general.GetController.RecruitUnit(entry.Key);
                if (unitRecruited != null)
                {
                    unitRecruited.AddListener(AddUnit);
                    recruited = true;
                    ++squadSize;
                    unitsToRecruit[entry.Key] = unitsToRecruit[entry.Key] - 1;
                    if (unitsToRecruit[entry.Key] == 0)
                        unitsToRecruit.Remove(entry.Key);
                }
            }
        }
    }

    private void CheckSquadReadiness(General owner)
    {
        if (squadUnits.Count == squadSize && (currentSquadBudget < minUnitCost || owner.GetController.TotalBuildPoints == 0))
        {
            if (squadSize == 0)
            {
                owner.ActionFailed(this);
                return;
            }

            Squad squad = new Squad();
            squad.LinePoses = new List<Line>(squadPresets[currentSquadPresetIndex].squadData.Lines);

            for (int i = 0; i < squadUnits.Count; ++i)
            {
                if (squadUnits[i] == null)
                {
                    squadUnits.RemoveAt(i);
                    --i;
                }
            }

            if (squadUnits.Count == 0)
            {
                owner.ActionFailed(this);
                return;
            }

            squad.FormSquad(owner.GetController, Squad.FormationStyle.Line, squadUnits);

            SquadLeader leader = new SquadLeader();
            leader.GiveSquad(squad, owner);
            owner.AddLeader(leader);

            if (recruited)
            {
                Vector3? rallyPoint = GameServices.GetRandomPoint(owner.GetController.GetFactoryList[0].transform.position, Vector3.right + Vector3.back, 60f, 45f, 35f);
                if (rallyPoint.HasValue)
                {
                    List<SquadAction> action = new List<SquadAction>();
                    SquadMoveTo moveTo = new SquadMoveTo();
                    moveTo.Init(squad, rallyPoint.Value, 1f);
                    action.Add(moveTo);
                    squad.GiveActions(action);
                    waitingToRally = true;
                    squad.OnAllActionsCompleted.AddListener(CompleteRally);
                }
            }
            else
            {
                Complete();
            }
        }
    }

    public override void Complete()
    {
        base.Complete();
        Reset();
    }

    public override void Abort()
    {
        base.Abort();
        Reset();
    }

    private void CompleteRally(Squad squad)
    {
        Complete();
    }

    private void Reset()
    {
        currentSquadPresetIndex = -1;
        squadUnits.Clear();
        squadSize = 0;
        squadCost = 0;
        currentSquadBudget = 0;
        minUnitCost = int.MaxValue;
        recruited = false;
        waitingToRally = false;
    }

    private void AddUnit(Unit unit)
    {
        squadUnits.Add(unit);
        currentSquadBudget -= unit.Cost;
        unit.OnDeadEvent += () =>
        {
            RemoveUnit(unit);
        };
    }

    private void RemoveUnit(Unit unit)
    {
        squadUnits.Remove(unit);
        currentSquadBudget += unit.Cost;
        --squadSize;

        if (unitsToRecruit.ContainsKey(unit.GetTypeId))
            unitsToRecruit[unit.GetTypeId] += 1;
        else
            unitsToRecruit[unit.GetTypeId] = 1;
    }
}

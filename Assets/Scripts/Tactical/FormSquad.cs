using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.CanvasScaler;

[System.Serializable]
public class FormSquad : GeneralAction
{
    [SerializeField]
    private SquadDataPreset squadData = null;

    [SerializeField]
    private List<Unit> squadUnits = null;

    private Squad squad = null;
    private int squadSize = 0;
    private int currentLineIndex = -1;
    private int currentUnitIndex = -1;

    public override void Enter(AIController controller)
    {
        base.Enter(controller);

        currentLineIndex = 0;
        currentUnitIndex = 0;
    }

    public override void Execute(AIController controller)
    {
        for (int i = currentLineIndex; i < squadData.squadData.Lines.Count; ++i)
        {
            for (int j = currentUnitIndex; j < squadData.squadData.Lines[i].numberOfUnits; ++j)
            {
                UnityEvent<Unit> unitRecruited = controller.RecruitUnit(squadData.squadData.Lines[i].unitType.TypeId);
                if (unitRecruited != null)
                {
                    ++squadSize;
                    unitRecruited.AddListener(AddUnit);
                }
                else 
                {
                    currentUnitIndex = j;
                    return;
                }
            }
            currentUnitIndex = 0;
            ++currentLineIndex;
        }

        if (squadUnits.Count == squadSize)
        {
            squad = new Squad();
            squad.FormSquad(controller, Squad.FormationStyle.None, squadUnits);

            // Actions test
            List<SquadAction> actions = new List<SquadAction>();
            SquadMoveTo moveTo = new SquadMoveTo();
            moveTo.Init(squad, new Vector3(310f, 0f, 236f), 1f);
            actions.Add(moveTo);
            squad.GiveActions(actions);

            isComplete = true;
        }
    }

    private void AddUnit(Unit unit)
    {
        squadUnits.Add(unit);
    }
}

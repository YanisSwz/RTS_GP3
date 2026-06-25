using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Squad
{
    public int GetNbUnit { get { return controlledUnits.Count; } }

    //return a copy
    public List<Unit> GetControlledUnits { get { return new List<Unit>(controlledUnits); } }
    List<Unit> controlledUnits = new List<Unit>();

    public enum FormationStyle
    {
        None,
        Line,
        Circle
    }
    public FormationStyle GetFormationStyle { get { return currentStyle; } }
    FormationStyle currentStyle = FormationStyle.None;
    
    //copy of this list
    public List<Vector3> GetFreestyleFormationPoses { get { return new List<Vector3>(freestyleFormationPos); } }
    List<Vector3> freestyleFormationPos = new List<Vector3>();

    [HideInInspector]
    public UnityEvent<Squad> OnAllActionsCompleted = new UnityEvent<Squad>();
    List<SquadAction> actions = new List<SquadAction>();
    int currentAction = -1;

    #region Squad Management
    public Vector3 GetSquadAveragePos()
    {
        Vector3 result = Vector3.zero;

        if (controlledUnits.Count == 0)
            return result;

        foreach (Unit unit in controlledUnits)
            result += unit.transform.position;

        return result / controlledUnits.Count;
    }

    public void FormSquad(UnitController controller, FormationStyle formationStyle, List<Unit> unitsRecruited)
    {
        controller.squads.Add(this);

        currentStyle = formationStyle;
        freestyleFormationPos.Clear();

        controlledUnits = new List<Unit> (unitsRecruited);

        AIController aiController = controller as AIController;

        foreach (Unit unit in controlledUnits)
        {
            //check if unit already in a squad
            if (unit.squadRef != null)
                unit.squadRef.RemoveUnit(unit, controller, true);

            //unit is in a squad => not available
            else if (aiController != null)
                aiController.availableUnits.Remove(unit);

            unit.squadRef = this;

            //remove unit of squad on its death 
            unit.OnDeadEvent += () =>
            {
                RemoveUnit(unit, controller);
            };
        }

        //save current pos of all unit in local pos compare to average pos
        if (formationStyle == FormationStyle.None)
        {
            Vector3 averagePos = GetSquadAveragePos();
            foreach (Unit unit in controlledUnits)
                freestyleFormationPos.Add(unit.transform.position - averagePos);
        }
    }

    public void RemoveUnit(Unit unitToRemove, UnitController controller, bool isSwapSquad = false)
    {
        int index = controlledUnits.IndexOf(unitToRemove);

        if (index != -1)
        {
            controlledUnits.RemoveAt(index);

            unitToRemove.squadRef = null;

            //only release unit if squad destroy
            if (isSwapSquad == false)
            {
                //unit no more in a squad => available
                AIController aiController = controller as AIController;
                if (aiController != null)
                    aiController.availableUnits.Add(unitToRemove);
            }


            //if no more unit => destrroy squad
            if (controlledUnits.Count == 0)
            {
                DestroySquad(controller);
                return;
            }

            if (currentStyle == FormationStyle.None)
            {
                //remove freestyle pos of this unit
                freestyleFormationPos.RemoveAt(index);

                //re compute freestyle pos
                Vector3 averagePos = GetSquadAveragePos();
                for (int i = 0; i < controlledUnits.Count; ++i)
                {
                    freestyleFormationPos[i] = controlledUnits[i].transform.position - averagePos;
                }
            }
        }
    }

    public void DestroySquad(UnitController controller)
    {
        freestyleFormationPos.Clear();

        AIController aiController = controller as AIController;

        foreach (Unit unit in controlledUnits)
        {
            unit.squadRef = null;

            //unit no more in a squad => available
            if (aiController)
                aiController.availableUnits.Add(unit);

            //flee to safe zone
        }

        controlledUnits.Clear();

        controller.squads.Remove(this);
    }
    #endregion

    #region Squad Action
    public void GiveActions(List<SquadAction> _actions)
    {
        actions.Clear();
        if(_actions.Count == 0)
        {
            currentAction = -1;
            return;
        }

        actions = _actions;
        currentAction = 0;

        foreach (SquadAction action in actions)
            action.OnComplete.AddListener(NextAction);

        actions[currentAction].StartAction();
    }

    private void NextAction()
    {
        ++currentAction;
        if (actions.Count == currentAction)
        {
            currentAction = -1;
            OnAllActionsCompleted.Invoke(this);
            actions.Clear();
            return;
        }

        actions[currentAction].StartAction();
    }

    public void Update()
    {
        if (currentAction >= 0)
            actions[currentAction].UpdateAction();
    }
    #endregion
}

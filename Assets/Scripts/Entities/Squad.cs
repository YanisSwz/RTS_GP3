using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Squad
{
    List<Unit> controlledUnits = new List<Unit>();

    List<Vector3> freestyleFormationPos = new List<Vector3>();
    FormationStyle currentStyle = FormationStyle.None;
    public enum FormationStyle
    {
        None,
        Line,
        Circle
    }
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

        controlledUnits = unitsRecruited;

        AIController aiController = controller as AIController;

        foreach (Unit unit in controlledUnits)
        {
            if (unit.squadRef != null)
                unit.squadRef.RemoveUnit(unit, controller);

            unit.squadRef = this;

            //unit is in a squad => not available
            if (aiController != null)
                aiController.availableUnits.Remove(unit);

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

    public void RemoveUnit(Unit unitToRemove, UnitController controller)
    {
        int index = controlledUnits.IndexOf(unitToRemove);

        if (index != -1)
        {
            controlledUnits.RemoveAt(index);

            unitToRemove.squadRef = null;

            //unit no more in a squad => available
            AIController aiController = controller as AIController;
            if (aiController != null)
                aiController.availableUnits.Add(unitToRemove);


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
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.CanvasScaler;

public class Squad
{
    public int GetNbUnit { get { return controlledUnits.Count; } }

    private bool isDestroyed = false;

    //return a copy
    public List<Unit> GetControlledUnits { get { return new List<Unit>(controlledUnits); } }
    List<Unit> controlledUnits = new List<Unit>();

    LayerMask detectionMask;

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

    //Actions
    [HideInInspector]
    public UnityEvent<Squad> OnAllActionsCompleted = new UnityEvent<Squad>();
    List<SquadAction> actions = new List<SquadAction>();
    int currentAction = -1;

    //Sensor
    public List<Unit> enemiesInSight = new List<Unit>();
    public UnityEvent<List<Unit>> OnEnemyInSight = new UnityEvent<List<Unit>>();

    public List<TargetBuilding> labsInSight = new List<TargetBuilding>();
    public UnityEvent<TargetBuilding> OnLabInSight = new UnityEvent<TargetBuilding>();

    public List<Factory> factoriesInSight = new List<Factory>();
    public UnityEvent<Factory> OnFactoryInSight = new UnityEvent<Factory>();

    List<Vector3> AABB = new List<Vector3>(new Vector3[4]);

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
        detectionMask = controller.detectionLayerForSquad;

        controller.AddSquad(this);

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
                RemoveUnit(unit, controller, true);
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
                if (freestyleFormationPos.Count > 0)
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

            if (currentAction >= 0)
                actions[currentAction].RecomputeAction(index);
        }
    }

    public void DestroySquad(UnitController controller)
    {
        isDestroyed = true;

        List<SquadAction> nullAction = new List<SquadAction>();
        GiveActions(nullAction);

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

        controller.RemoveSquad(this);
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
        {
            action.OnComplete.AddListener(NextAction);
            action.OnAbort.AddListener(AbortAction);
        }

        actions[currentAction].StartAction();
    }

    private void AbortAction()
    {
        Debug.Log("Abort sequence");
        currentAction = -1;
        OnAllActionsCompleted.Invoke(this);
        actions.Clear();
    }

    private void NextAction()
    {
        actions[currentAction].ExitAction();

        ++currentAction;
        if (actions.Count == currentAction)
        {
            currentAction = -1;
            actions.Clear();
            OnAllActionsCompleted.Invoke(this);
            return;
        }

        actions[currentAction].StartAction();
    }

    public void Update(UnitController controller)
    {
        CalculateAABB(controller);

        if (currentAction >= 0)
            actions[currentAction].UpdateAction();
    }

    private void CalculateAABB(UnitController controller)
    {
        Vector3 baseUnitPos = controlledUnits[0].transform.position;
        float baseAttakDist = controlledUnits[0].GetUnitData.AttackDistanceMax * 1.5f;
        float minX = baseUnitPos.x - baseAttakDist;
        float maxX = baseUnitPos.x + baseAttakDist;
        float minY = baseUnitPos.z - baseAttakDist;
        float maxY = baseUnitPos.z + baseAttakDist;

        for (int i = 1; i < controlledUnits.Count; ++i)
        {
            Vector3 unitPos = controlledUnits[i].transform.position;
            float attakDist = controlledUnits[i].GetUnitData.AttackDistanceMax * 1.5f;
            if (unitPos.x - attakDist < minX)
                minX = unitPos.x - attakDist;

            if (unitPos.x + attakDist > maxX)
                maxX = unitPos.x + attakDist;

            if (unitPos.z - attakDist < minY)
                minY = unitPos.z - attakDist;

            if (unitPos.z + attakDist > maxY)
                maxY = unitPos.z + attakDist;
        }

        AABB[0] = new Vector3(minX, baseUnitPos.y, minY);
        AABB[1] = new Vector3(maxX, baseUnitPos.y, minY);
        AABB[2] = new Vector3(maxX, baseUnitPos.y, maxY);
        AABB[3] = new Vector3(minX, baseUnitPos.y, maxY);
        Vector3 AABBCenter = (AABB[0] + AABB[1] + AABB[2] + AABB[3]) / 4f;

        //todo layer
        List<RaycastHit> hitObj = new List<RaycastHit>(Physics.BoxCastAll(AABBCenter
            , new Vector3((AABB[1] - AABB[0]).magnitude * 0.5f, 3f, (AABB[2] - AABB[1]).magnitude * 0.5f)
            , Vector3.up, Quaternion.identity, float.MaxValue, detectionMask));

        AIController aiController = controller as AIController;

        enemiesInSight.Clear();
        foreach (RaycastHit hit in hitObj)
        {
            if (isDestroyed)
                return;

            Unit unitInSight = null;
            if (hit.rigidbody && hit.rigidbody.gameObject.TryGetComponent<Unit>(out unitInSight))
            {
                if (unitInSight.GetTeam() != controlledUnits[0].GetTeam())
                    enemiesInSight.Add(unitInSight);

                continue;
            }

            TargetBuilding discoveredLab = null;
            if (hit.collider && hit.collider.gameObject.TryGetComponent<TargetBuilding>(out discoveredLab))
            {
                if (discoveredLab.GetTeam() != controlledUnits[0].GetTeam())
                {
                    labsInSight.Add(discoveredLab);
                    OnLabInSight.Invoke(discoveredLab);
                }
                continue;
            }

            Factory discoverFactory = null;
            if (hit.collider && hit.collider.gameObject.TryGetComponent<Factory>(out discoverFactory))
            {
                if (discoverFactory.GetTeam() != controlledUnits[0].GetTeam())
                {
                    factoriesInSight.Add(discoverFactory);
                    OnFactoryInSight.Invoke(discoverFactory);
                }
                continue;
            }
        }

        if (enemiesInSight.Count > 0)
            OnEnemyInSight.Invoke(enemiesInSight);
    }

    public void DrawGizmo()
    {
        if (currentAction >= 0 && actions.Count > 0)
            actions[currentAction].DrawGizmo();

        Gizmos.color = Color.yellow;
        foreach (Vector3 v in AABB)
            Gizmos.DrawCube(v, Vector3.one + Vector3.up * 3);

        Gizmos.color = Color.red;
        foreach (Unit unitInSight in enemiesInSight)
        {
            if(unitInSight)
                Gizmos.DrawCube(unitInSight.transform.position + Vector3.up * 2f, Vector3.one + Vector3.up);
        }
    }
    #endregion
}

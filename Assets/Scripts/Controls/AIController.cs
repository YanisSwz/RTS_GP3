using UnityEngine;
using System.Collections.Generic;

// $$$ TO DO :)

public sealed class AIController : UnitController
{
    [SerializeField]
    private List<Goal> goals = new List<Goal>();
    private Goal currentGoal = null;
    [SerializeField]
    private General general = null;
    [SerializeField]
    private List<Transform> buildPositions = null;
    private List<Transform> availableBuildPositions = new List<Transform>();
    public List<Transform> AvailableBuildPositions { get { return availableBuildPositions; } }

    [Header("--- DEBUG ---")]
    public string currentGoalName = "none";
    public float currentGoalUtility = -1f;

    //unit not recruited by a squad
    [HideInInspector]
    public List<Unit> availableUnits = new List<Unit>();

    #region MonoBehaviour methods

    protected override void Awake()
    {
        base.Awake();
        foreach (Goal goal in goals) 
            goal.LoadData();

        general.SetOwner(this);

        if(buildPositions.Count > 0)
            availableBuildPositions = new (buildPositions);
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void AddUnit(Unit unit)
    {
        base.AddUnit(unit);

        availableUnits.Add(unit);
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyUp(KeyCode.B))
        {
            //FactoryList[0].RequestUnitBuild(0);
            SelectFactory(FactoryList[0]);
            RequestUnitBuild(0);
        }

        //Debug.Log("UnitList.Count = " + UnitList.Count);

        float bestUtility = 0f;
        foreach (Goal goal in goals)
        {
            goal.Evaluate(this);
            if(goal.Utility > bestUtility) 
            {
                bestUtility = goal.Utility;
                currentGoal = goal;
                //Debug
                currentGoalName = goal.Name;
                currentGoalUtility = goal.Utility;
            }
        }

        general.SetGoal(currentGoal);
        general.UpdateSequence();
    }

    #endregion


    public bool TryBuildingFactory(int index) 
    {
        if(availableBuildPositions.Count == 0)
            return false;

        int buildIndex = Random.Range(0, availableBuildPositions.Count);
        Vector3 spawn = availableBuildPositions[index].position;
        if (BuildFactory(index, spawn))
        {
            availableBuildPositions.RemoveAt(index);
            return true;
        }
        else 
        {
            return false;
        }

    }

    private bool BuildFactory(int index, Vector3 position) 
    {
        SelectedFactory = FactoryList[0];
        return RequestFactoryBuild(index, position);
    }
}

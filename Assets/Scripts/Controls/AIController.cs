using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

// $$$ TO DO :)

public sealed class AIController : UnitController
{
    [SerializeField]
    private List<Transform> buildPositions = null;
    private List<Transform> availableBuildPositions = new List<Transform>();
    public List<Transform> AvailableBuildPositions { get { return availableBuildPositions; } }
    
    [SerializeField]
    private List<Goal> goals = new List<Goal>();
    private Goal currentGoal = null;
    [SerializeField]
    private List<General> generals = new List<General>();

    [Header("--- DEBUG ---")]
    public string currentGoalName = "none";
    public float currentGoalUtility = -1f;
    public List<TargetBuilding> discoveredLabs = new List<TargetBuilding>();


    [Header("--- Menace Memory ---")]
    public float timeOutMemory = 30f;
    public float toleranceRadiusMemory = 10f;
    public float refreshRate = 5f;
    private float currentRefreshTime = 0f;
    public List<MenaceMemory> menacesMemory = new List<MenaceMemory>();

    //unit not recruited by a squad
    [HideInInspector]
    public List<Unit> availableUnits = new List<Unit>();

    #region MonoBehaviour methods

    protected override void Awake()
    {
        base.Awake();
        foreach (Goal goal in goals) 
            goal.LoadData();

        foreach(General general in generals)
            general.SetOwner(this);

        if(buildPositions.Count > 0)
            availableBuildPositions = new (buildPositions);
    }

    protected override void Start()
    {
        base.Start();

        SelectedFactory = FactoryList[0];
    }

 

    protected override void Update()
    {
        base.Update();

        //check menace memory
        if(currentRefreshTime >= refreshRate)
        {
            currentRefreshTime = 0f;
            TimeOutMenaceMemoryClean();
        }
        else
            currentRefreshTime += Time.deltaTime;

        if (Input.GetKeyUp(KeyCode.B))
        {
            SelectFactory(FactoryList[0]);
            RequestUnitBuild(0);
        }

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

        foreach (General general in generals)
        {
            general.SetGoal(currentGoal);

            if (general.CurrentGoal != null)
                general.UpdateSequence();
        }
    }

    #endregion

    public override void AddUnit(Unit unit)
    {
        base.AddUnit(unit);
    }

    public override void CaptureTarget(int points, TargetBuilding lab)
    {
        base.CaptureTarget(points, lab);
        discoveredLabs.Remove(lab);
    }
    public override void LoseTarget(int points, TargetBuilding lab)
    {
        base.LoseTarget(points, lab);
        discoveredLabs.Add(lab);
    }

    public bool TryBuildingFactory(int index) 
    {
        if(availableBuildPositions.Count == 0)
            return false;

        int buildIndex = UnityEngine.Random.Range(0, availableBuildPositions.Count);
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

    public bool CanRecruitUnit(int unitType) 
    {
        bool can = false;

        foreach (Factory factory in FactoryList)
        {
            for (int i = 0; i < factory.AvailableUnitsCount; ++i)
            {
                if (factory.GetBuildableUnitData(i).TypeId == unitType)
                {
                    can = true;
                    break;
                }
            }
        }

        return can;
    }

    public UnityEvent<Unit> RecruitUnit(int unitType) 
    {
        SelectBestFactory(unitType);
        return RequestUnitBuild(GetUnitFactoryIndex(unitType));
    }

    private int GetUnitFactoryIndex(int unitType)
    {
        for (int i = 0; i < SelectedFactory.AvailableUnitsCount; ++i)
        {
            if (SelectedFactory.GetBuildableUnitData(i).TypeId == unitType)
            {
                return i;
            }
        }
        return -1;
    }

    private void SelectBestFactory(int unitType) 
    {
        Factory bestFactory = FactoryList[0];
        int bestQueueSize = int.MaxValue;
        foreach (Factory factory in FactoryList)
        {
            for (int i = 0; i < factory.AvailableUnitsCount; ++i)
            {
                if (factory.GetBuildableUnitData(i).TypeId == unitType)
                {
                    if(factory.BuildingQueueSize < bestQueueSize)
                    {
                        bestFactory = factory;
                        bestQueueSize = factory.BuildingQueueSize;
                        break;
                    }
                }
            }
        }

        SelectedFactory = bestFactory;
    }

    private void TimeOutMenaceMemoryClean()
    {
        float currentTime = Time.time;

        for (int i = 0; i < menacesMemory.Count; ++i)
        {
            if (currentTime - menacesMemory[i].time >= timeOutMemory)
            {
                menacesMemory.RemoveAt(i);
                --i;
            }
        }
    }

    public void AddMenaceMemory(MenaceMemory newMenace)
    {
        for(int i = 0; i < menacesMemory.Count;++i)
        {
            if ((menacesMemory[i].enemyAveragePos - newMenace.enemyAveragePos).magnitude <= toleranceRadiusMemory)
            {
                menacesMemory[i] = newMenace;
                return;
            }
        }

        menacesMemory.Add(newMenace);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        foreach (MenaceMemory menace in menacesMemory)
        {
            Gizmos.DrawWireSphere(menace.enemyAveragePos, toleranceRadiusMemory);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

// $$$ TO DO :)

public sealed class AIController : UnitController
{
    [SerializeField]
    private List<Goal> goals = new List<Goal>();
    private Goal currentGoal = null;
    [Header("--- DEBUG ---")]
    public string currentGoalName = "none";
    public float currentGoalUtility = -1f;

    #region MonoBehaviour methods

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
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
                currentGoalName = goal.Name;
                currentGoalUtility = goal.Utility;
            }
        }
    }

    #endregion
}

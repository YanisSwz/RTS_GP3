using UnityEngine;
using System.Collections.Generic;
using System;

public enum InputValue
{
    None = 0,
    BuildPoints = 1,
    ArmyPower = 2,
    CapturedLabs = 3,
    BuiltFactories = 4,
    AvailableUnits = 5,
    AvailableBuildPos = 6, 
    DiscoveredLabs = 7,
    MenacesCount = 8,
    DiscoveredEnemyFactories = 9
}

[System.Serializable]
public class Goal
{
    public Goal() 
    {
        LoadData();
    }

    public void LoadData() 
    {
        if (!goalData)
            return;

        type = goalData.type;
        activationThreshold = goalData.activationThreshold;
        minUtility = goalData.minUtility;
        maxUtility = goalData.maxUtility;
        utilityEvaluators = goalData.utilityEvaluators;
    }


    [SerializeField]
    private ScriptableGoal goalData;

    // TODO: Remove debug
    [SerializeField]
    private float utility = 0f;
    private GoalType type = GoalType.None;
    private float activationThreshold = 0f;
    private float minUtility = 0f;
    private float maxUtility = 1f;
    private List<UtilityEvaluator> utilityEvaluators = new List<UtilityEvaluator>();

    public GoalType Type { get { return type; } }
    public string Name { get { return type.ToString(); } }
    public float Utility { get { return utility; } }
    public void SetUtility(float newUtility) { utility = newUtility; }

    public void Evaluate(AIController controller)
    {
        utility = 0f;

        float totalWeight = 0f;
        foreach (UtilityEvaluator evaluator in utilityEvaluators)
            totalWeight += evaluator.Weight;

        foreach (UtilityEvaluator evaluator in utilityEvaluators)
        {
            float value = 0f;
            switch (evaluator.InputValue)
            {
                case InputValue.None:
                    break;
                case InputValue.BuildPoints:
                    value = controller.TotalBuildPoints;
                    break;
                case InputValue.ArmyPower:
                    value = controller.UnitList.Count;
                    break;
                case InputValue.CapturedLabs:
                    value = controller.CapturedTargets;
                    break;
                case InputValue.BuiltFactories:
                    value = controller.GetFactoryList.Count;
                    break;
                case InputValue.AvailableUnits:
                    value = controller.availableUnits.Count;
                    break;
                case InputValue.AvailableBuildPos:
                    value = controller.AvailableBuildPositions.Count;
                    break;
                case InputValue.DiscoveredLabs:
                    value = controller.discoveredLabs.Count;
                    break;
                case InputValue.MenacesCount:
                    value = controller.menacesMemory.Count;
                    break;
                case InputValue.DiscoveredEnemyFactories:
                    value = controller.discoveredEnemyFactories.Count;
                    break;
            }

            utility += evaluator.Evaluate(value) * evaluator.Weight;
        }

        utility /= totalWeight;
        utility = Mathf.Clamp(utility, minUtility, maxUtility);

        if (utility <= activationThreshold)
            utility = 0f;
    }
}



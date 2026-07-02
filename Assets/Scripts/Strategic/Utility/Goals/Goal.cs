using System.Collections.Generic;
using UnityEngine;

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

public enum AggregationType 
{
    None = 0,
    Ponder = 1,
    Minimize = 2,
    Maximize = 3
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
        aggregationType = goalData.aggregationType;
        activationThreshold = goalData.activationThreshold;
        minUtility = goalData.minUtility;
        maxUtility = goalData.maxUtility;
        utilityEvaluators = goalData.utilityEvaluators;
    }


    [SerializeField]
    private ScriptableGoal goalData;

    [SerializeField]
    private float utility = 0f;
    private GoalType type = GoalType.None;
    private AggregationType aggregationType = AggregationType.None;
    private float activationThreshold = 0f;
    private float minUtility = 0f;
    private float maxUtility = 1f;
    private List<UtilityEvaluator> utilityEvaluators = new List<UtilityEvaluator>();

    public GoalType Type { get { return type; } }
    public string Name { get { return type.ToString(); } }
    public float Utility { get { return utility; } }
    public void SetUtility(float newUtility) { utility = newUtility; }

    /// <summary>
    /// Translate input value into concrete data
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public float GetInputValue(AIController controller, InputValue input) 
    {
        float value = 0f;
        switch (input)
        {
            case InputValue.None:
                break;
            case InputValue.BuildPoints:
                value = controller.TotalBuildPoints;
                break;
            case InputValue.ArmyPower:
                value = controller.ArmyPower;
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
                value = controller.DiscoveredLabs.Count;
                break;
            case InputValue.MenacesCount:
                value = controller.menacesMemory.Count;
                break;
            case InputValue.DiscoveredEnemyFactories:
                value = controller.DiscoveredEnemyFactories.Count;
                break;
        }
        return value;
    }

    
    public void Evaluate(AIController controller)
    {
        if (aggregationType == AggregationType.Ponder)
            utility = 0f;
        else
            utility = utilityEvaluators[0].Evaluate(GetInputValue(controller, utilityEvaluators[0].InputValue));

        foreach (UtilityEvaluator evaluator in utilityEvaluators)
        {
            float value = GetInputValue(controller, evaluator.InputValue);
            switch (aggregationType) 
            {
                case AggregationType.None:
                    break;
                case AggregationType.Ponder:
                    utility += evaluator.Evaluate(value) * evaluator.Weight;
                    break;
                case AggregationType.Maximize:
                    utility = Mathf.Max(utility, evaluator.Evaluate(value));
                    break;
                case AggregationType.Minimize:
                    utility = Mathf.Min(utility, evaluator.Evaluate(value));
                    break;
            }
        }

        if (aggregationType == AggregationType.Ponder)
        {
            float totalWeight = 0f;
            foreach (UtilityEvaluator evaluator in utilityEvaluators)
                totalWeight += evaluator.Weight;

            utility /= totalWeight;
        }

        utility = Mathf.Clamp(utility, minUtility, maxUtility);
        if (utility <= activationThreshold)
            utility = 0f;
    }
}



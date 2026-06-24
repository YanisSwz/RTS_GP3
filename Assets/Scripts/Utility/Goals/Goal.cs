using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class Goal
{
    [SerializeField]
    private string name = "goal";
    [SerializeField]
    private float utility = 0f;
    [SerializeField]
    [Range(0f, 1f)]
    private float activationThreshold = 0f;
    [SerializeField]
    private float minUtility = 0f;
    [SerializeField]
    private float maxUtility = 1f;
    [SerializeField]
    private List<UtilityEvaluator> utilityEvaluators = new List<UtilityEvaluator>();

    public string Name { get { return name; } }
    public float Utility { get { return utility; } }

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
            }

            utility += evaluator.Evaluate(value) * evaluator.Weight;
        }

        utility /= totalWeight;
        utility = Mathf.Clamp(utility, minUtility, maxUtility);

        if (utility <= activationThreshold)
            utility = 0f;
    }
}



using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableGoal", menuName = "Scriptable Objects/Strategic Layer/ScriptableGoal")]
public class ScriptableGoal : ScriptableObject
{
    public GoalType type = GoalType.None;
    [Range(0f, 1f)]
    public float activationThreshold = 0f;
    public float minUtility = 0f;
    public float maxUtility = 1f;
    public List<UtilityEvaluator> utilityEvaluators = new List<UtilityEvaluator>();
}

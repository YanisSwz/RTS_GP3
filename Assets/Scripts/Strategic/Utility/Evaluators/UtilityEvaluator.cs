using UnityEngine;

[System.Serializable]
public class UtilityEvaluator
{
    [SerializeField]
    private float weight = 1f;
    public float Weight { get { return weight; } }
    [SerializeField]
    private InputValue inputValue = InputValue.None;
    public InputValue InputValue { get { return inputValue; } }
    [SerializeField]
    private AnimationCurve utilityCurve = null;

    public float Evaluate(float value) { return utilityCurve.Evaluate(value); }
}
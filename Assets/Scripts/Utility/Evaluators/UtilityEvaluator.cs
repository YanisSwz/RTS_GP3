using UnityEngine;

public enum InputValue
{
    None = 0,
    BuildPoints = 1,
    ArmyPower = 2,
    CapturedLabs = 3
}

[System.Serializable]
public class UtilityEvaluator
{
    [SerializeField]
    protected float weight = 1f;
    public float Weight { get { return weight; } }
    [SerializeField]
    private InputValue inputValue = InputValue.None;
    public InputValue InputValue { get { return inputValue; } }
    [SerializeField]
    protected AnimationCurve utilityCurve = null;

    public float Evaluate(float value) { return utilityCurve.Evaluate(value); }
}
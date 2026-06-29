using UnityEngine;

[System.Serializable]
public abstract class GeneralAction
{
    protected bool isComplete = false;
    public bool IsComplete { get { return isComplete; } }

    public virtual void Enter(General owner, float power) { isComplete = false; }
    public virtual void Execute(General owner, float power) { }
}

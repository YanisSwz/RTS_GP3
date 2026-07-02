using UnityEngine;

[System.Serializable]
public abstract class GeneralAction
{
    public virtual GeneralAction GenerateCopy() { return null; }
    protected bool isComplete = false;
    public bool IsComplete { get { return isComplete; } }
    public virtual void Complete() {  isComplete = true; }
    public virtual void Enter(General owner, float power) { isComplete = false; }
    public virtual void Execute(General owner, float power) { }
    public virtual void Abort() { }
}

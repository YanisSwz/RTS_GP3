using UnityEngine;

[System.Serializable]
public abstract class GeneralAction
{
    protected bool isComplete = false;
    public bool IsComplete { get { return isComplete; } }

    public virtual void Enter(AIController controller) { isComplete = false; }
    public virtual void Execute(AIController controller) { }
}

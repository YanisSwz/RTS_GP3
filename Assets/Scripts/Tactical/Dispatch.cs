using UnityEngine;

[System.Serializable]
public abstract class Dispatch : GeneralAction
{
    protected Vector3 targetPosition = Vector3.zero;
    protected GameObject targetObject = null;

    public override void Enter(General owner, float power)
    {
        base.Enter(owner, power);

        targetPosition = Vector3.zero;
        targetObject = null;
    }
}

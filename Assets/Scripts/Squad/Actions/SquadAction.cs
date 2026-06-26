using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class SquadAction
{
    protected Squad squad;

    [HideInInspector]
    public UnityEvent OnComplete = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnAbort = new UnityEvent();

    protected Vector3 staticTarget;
    protected GameObject movingTarget = null;
    protected float distanceToTarget = 0f;

    virtual public void Init(Squad _squad, Vector3 _staticTarget, float _distanceToTarget)
    {
        squad = _squad;
        staticTarget = _staticTarget;
        distanceToTarget = _distanceToTarget;
    }

    virtual public void Init(Squad _squad, GameObject _movingTarget, float _distanceToTarget)
    {
        squad = _squad;
        movingTarget = _movingTarget;
        distanceToTarget = _distanceToTarget;
    }
    virtual public void StartAction()
    {
    }

    virtual public void UpdateAction()
    {

    }

    virtual public void ExitAction()
    {

    }

    virtual public void DrawGizmo()
    {

    }
}

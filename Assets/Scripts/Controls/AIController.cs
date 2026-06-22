using UnityEngine;


// $$$ TO DO :)

public sealed class AIController : UnitController
{
    #region MonoBehaviour methods

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyUp(KeyCode.B))
        {
            //FactoryList[0].RequestUnitBuild(0);
            SelectFactory(FactoryList[0]);
            RequestUnitBuild(0);
        }

        //Debug.Log("UnitList.Count = " + UnitList.Count);
    }

    #endregion
}

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Build : GeneralAction
{
    public override void Execute(AIController controller)
    {
        if (controller.TotalBuildPoints >= 15f)
            controller.TryBuildingFactory(1);
        else
            controller.TryBuildingFactory(0);

        // TODO: Add fail
        isComplete = true;
    }
}

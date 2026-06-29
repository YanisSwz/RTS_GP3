using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Build : GeneralAction
{
    public override void Execute(AIController controller, float power)
    {
        Dictionary<int, int> factoryPrices = GameServices.GetGameServices().GetFactoryPrices();

        bool built = false;
        foreach (KeyValuePair<int, int> factoryPrice in factoryPrices.OrderByDescending(x => x.Value))
        {
            if (controller.TotalBuildPoints * power >= factoryPrice.Value)
            {
                controller.TryBuildingFactory(factoryPrice.Key);
                built = true;
                break;
            }
        }

        // TODO: Add fail
        isComplete = true;
    }
}

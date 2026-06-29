using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Build : GeneralAction
{
    public override void Execute(General owner, float power)
    {
        Dictionary<int, int> factoryPrices = GameServices.GetGameServices().GetFactoryPrices();

        bool built = false;
        foreach (KeyValuePair<int, int> factoryPrice in factoryPrices.OrderByDescending(x => x.Value))
        {
            if (owner.GetController.TotalBuildPoints * power >= factoryPrice.Value)
            {
                owner.GetController.TryBuildingFactory(factoryPrice.Key);
                built = true;
                break;
            }
        }

        // TODO: Add fail
        isComplete = true;
    }
}

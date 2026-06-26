using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Build : GeneralAction
{
    /// <summary>
    /// margin we want to guarantee before building (to avoid being at 0 after building)
    /// </summary>
    [SerializeField]
    [Range(1f, 4f)]
    private float budgetMargin = 1.5f;

    public override void Execute(AIController controller)
    {
        Dictionary<int, int> factoryPrices = GameServices.GetGameServices().GetFactoryPrices();

        bool built = false;
        foreach (KeyValuePair<int, int> factoryPrice in factoryPrices.OrderByDescending(x => x.Value))
        {
            if (controller.TotalBuildPoints >= factoryPrice.Value * budgetMargin)
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

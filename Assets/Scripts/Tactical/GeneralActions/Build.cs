using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Build : GeneralAction
{

    public override GeneralAction GenerateCopy()
    {
        return new Build();
    }

    public override void Execute(General owner, float power)
    {
        Dictionary<int, int> factoryPrices = GameServices.GetGameServices().GetFactoryPrices();

        bool built = false;
        foreach (KeyValuePair<int, int> factoryPrice in factoryPrices.OrderByDescending(x => x.Value))
        {
            if (owner.GetController.TotalBuildPoints >= factoryPrice.Value)
            {
                owner.GetController.TryBuildingFactory(factoryPrice.Key);
                built = true;
                break;
            }
        }

        if (built)
            isComplete = true;
        else
            owner.AbortSequence();
    }
}

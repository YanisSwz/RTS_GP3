using UnityEngine;

[System.Serializable]
public class Attack : Dispatch
{
    public int nbTrySearchEnemy = 5;
    public float searchEnemyRadius = 50f;

    public override GeneralAction GenerateCopy()
    {
        Attack copy = new Attack();
        copy.nbTrySearchEnemy = nbTrySearchEnemy;
        copy.searchEnemyRadius = searchEnemyRadius;

        return copy;
    }

    public override void Enter(General owner, float power)
    {
        base.Enter(owner, power);

        // If we found an enemy factory, attack it
        if (owner.GetController.DiscoveredEnemyFactories.Count > 0)
        {
            // Only one factory = enemy base
            if (owner.GetController.DiscoveredEnemyFactories.Count == 1)
            {
                targetPosition = owner.GetController.DiscoveredEnemyFactories[0].transform.position;
            }
            // Pick nearest factory
            else
            {
                foreach (SquadLeader leader in owner.Leaders)
                {
                    float bestDistance = Mathf.Infinity;
                    Vector3 basePos = leader.Squad.GetSquadAveragePos();
                    foreach (Factory factory in owner.GetController.DiscoveredEnemyFactories)
                    {
                        float dist = Vector3.Distance(basePos, factory.transform.position);
                        if (dist < bestDistance)
                        {
                            bestDistance = dist;
                            targetPosition = factory.transform.position;
                        }
                    }
                }
            }
        }
        else 
        {
            // Select the closest menace position
            foreach (SquadLeader leader in owner.Leaders)
            {
                float bestDistance = Mathf.Infinity;
                Vector3 basePos = leader.Squad.GetSquadAveragePos();
                foreach (MenaceMemory menace in owner.GetController.menacesMemory)
                {
                    float dist = Vector3.Distance(basePos, menace.enemyAveragePos);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        targetPosition = menace.enemyAveragePos;
                    }
                }
            }
        }

        // Dispatch squads to attack
        foreach (SquadLeader leader in owner.Leaders)
        {
            AttackLeaderAction attackAction = new AttackLeaderAction();
            attackAction.Init(leader);
            attackAction.menacePos = targetPosition;
            attackAction.explorationTryBeforeFail = nbTrySearchEnemy;
            attackAction.searchEnemyRadius = searchEnemyRadius;

            leader.GiveGeneralOrder(attackAction);
        }
    }
}

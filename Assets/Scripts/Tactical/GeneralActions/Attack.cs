using System.Collections.Generic;
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

        //select factory
        if (owner.GetController.discoveredEnemyFactories.Count > 0)
        {
            if (owner.GetController.discoveredEnemyFactories.Count == 1)
            {
                targetPosition = owner.GetController.discoveredEnemyFactories[0].transform.position;
            }
            else
            {
                foreach (SquadLeader leader in owner.Leaders)
                {
                    float bestDistance = Mathf.Infinity;
                    Vector3 basePos = leader.Squad.GetSquadAveragePos();
                    foreach (Factory factory in owner.GetController.discoveredEnemyFactories)
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
            //select menace pos
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

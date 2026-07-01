using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack : Dispatch
{
    public override void Enter(General owner, float power)
    {
        base.Enter(owner, power);

        if (owner.GetController.discoveredEnemyFactories.Count > 0)
        {
            if (owner.GetController.discoveredEnemyFactories.Count == 1)
            {
                targetObject = owner.GetController.discoveredEnemyFactories[0].gameObject;
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
                            targetObject = factory.gameObject;
                        }
                    }
                }
            }

            foreach (SquadLeader leader in owner.Leaders)
            {
                List<SquadAction> actions = new List<SquadAction>();
                SquadMoveTo moveTo = new SquadMoveTo();
                moveTo.Init(leader.Squad, targetObject, 1f);

                float bestDistance = 0f;
                foreach (Unit unit in leader.Squad.GetControlledUnits)
                {
                    float attackDist = unit.GetUnitData.AttackDistanceMax;
                    if (attackDist > bestDistance)
                        bestDistance = attackDist;
                }

                moveTo.distanceToFinalTarget = bestDistance;
                actions.Add(moveTo);

                SquadAttack squadAttack = new SquadAttack();
                squadAttack.Init(leader.Squad, targetObject, bestDistance);
                actions.Add(squadAttack);

                leader.Squad.GiveActions(actions);
            }
        }
        else 
        {
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

                List<SquadAction> actions = new List<SquadAction>();
                SquadMoveTo moveTo = new SquadMoveTo();
                moveTo.Init(leader.Squad, targetPosition, 1f);
                actions.Add(moveTo);
                leader.Squad.GiveActions(actions);
            }
        }
    }
}

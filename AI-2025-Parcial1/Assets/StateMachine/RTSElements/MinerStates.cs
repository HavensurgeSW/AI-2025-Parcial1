using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class MinerMovingState : State
{
    private Transform actualTarget;
    public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
    {
        actualTarget = parameters[0] as Transform;
        Transform minerTransform = parameters[1] as Transform;
        float speed = (float)parameters[2];
        float reachDistance = (float)parameters[3];
        float deltaTime = (float)parameters[4];

        BehaviourActions behaviourActions = new BehaviourActions();

        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {
            minerTransform.position += (actualTarget.position - minerTransform.position).normalized * speed * deltaTime;
        });

        behaviourActions.SetTransitionBehaviour(() =>
        {
            if (Vector3.Distance(minerTransform.position, actualTarget.position) <= reachDistance)
            {
                Debug.Log("Reached Target");
                OnFlag?.Invoke(Miner.Flags.OnTargetReach);
            }
        });
        return behaviourActions;
    }
}

public class MinerMoveToTown : State
{
    private Transform actualTarget;

    public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
    {
        actualTarget = parameters[0] as Transform; // townLocation
        Transform minerTransform = parameters[1] as Transform;
        float speed = (float)parameters[2];
        float reachDistance = (float)parameters[3];
        float deltaTime = (float)parameters[4];

        BehaviourActions behaviourActions = new BehaviourActions();

        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {
            if (actualTarget != null && minerTransform != null)
            {
                Vector3 direction = (actualTarget.position - minerTransform.position).normalized;
                minerTransform.position += direction * speed * deltaTime;
            }
        });

        behaviourActions.SetTransitionBehaviour(() =>
        {
            if (actualTarget != null && minerTransform != null)
            {
                if (Vector3.Distance(minerTransform.position, actualTarget.position) <= reachDistance)
                {
                    OnFlag?.Invoke(Miner.Flags.OnTargetReach);
                }
            }
        });

        return behaviourActions;
    }
}
public class MinerDepositingState : State
{
    public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
    {
        Townhall townhall = parameters[0] as Townhall;
        InventoryData inventory = parameters[1] as InventoryData;

        BehaviourActions behaviourActions = new BehaviourActions();

        behaviourActions.AddMainThreadableBehaviour(0, ()=>
        {
            if (inventory.inventory > 0)
            {
                townhall.Deposit(inventory.inventory);
                inventory.inventory = 0;
            }
        });


        behaviourActions.SetTransitionBehaviour(() =>
        {
            if (inventory.inventory == 0)
            {
                OnFlag?.Invoke(Miner.Flags.OnInventoryEmpty);
            }
        });

        return behaviourActions;
    }
    public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
    {
        Miner miner = parameters[0] as Miner;
        BehaviourActions behaviourActions = new BehaviourActions();
        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {
            miner.moveTarget = miner.mineLocation;
        });
        return behaviourActions;
    }
}

public class MinerMiningState : State
{
    public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
    {
        Debug.Log("Im mining!");
        GoldMine goldMine = parameters[0] as GoldMine;
        int miningRate = (int)parameters[1];
        InventoryData inv = parameters[2] as InventoryData;


        BehaviourActions behaviourActions = new BehaviourActions();


        behaviourActions.AddMultiThreadableBehaviour(0, () =>
        {
            int minedAmount = goldMine.Mine(miningRate);
            inv.inventory+= minedAmount;
        });

        behaviourActions.SetTransitionBehaviour(() =>
        {

            if (inv.inventory >= inv.maxInventory)
            {
                OnFlag?.Invoke(Miner.Flags.OnInventoryFull);
            }
            else if (goldMine.isDepleted)
            {

                // OnFlag?.Invoke(MinerFlags.OnMineDepleted);
                OnFlag?.Invoke(Miner.Flags.OnInventoryFull);
            }
        });

        return behaviourActions;
    }
    public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
    {
        Debug.Log("Inventory full!");
        Miner miner = parameters[0] as Miner;
        BehaviourActions behaviourActions = new BehaviourActions();
        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {

            miner.moveTarget = miner.townLocation;
        });
        return behaviourActions;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class MinerMovingState : State
{
    //Pathfinding 
    private GraphView GV;
    private Traveler traveler = new Traveler();
    private Node<Vector2Int> startNode = new Node<Vector2Int>();
    private Node<Vector2Int> destinationNode = new Node<Vector2Int>();
    List<Node<Vector2Int>> path = new List<Node<Vector2Int>>();

    //Game movement
    private Transform minerTransform;
    private int currentPathIndex;
    private float speed = 5f;
    private float nodeReachDistance = 0.1f;
    public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
    {
      
        startNode = parameters[0] as Node<Vector2Int>;
        //destinationNode = parameters[1] as Node<Vector2Int>;
        GV = parameters[1] as GraphView;
     
        path = traveler.FindPath(startNode, destinationNode, GV);
        currentPathIndex = 0;

        BehaviourActions behaviourActions = new BehaviourActions();
        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {
            
        });
        return behaviourActions;
    }

    public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
    {
        BehaviourActions behaviourActions = new BehaviourActions();
        minerTransform = parameters[0] as Transform;
        float deltaTime = (float)parameters[1];
        //speed = (float)parameters[1];


        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {
            Node<Vector2Int> targetNode = new Node<Vector2Int>();
            Vector2Int coord = new Vector2Int();

            targetNode = path[currentPathIndex];
            coord = targetNode.GetCoordinate();
            Vector3 targetPos = new Vector3(coord.x, coord.y, minerTransform.position.z);
            minerTransform.position = Vector3.MoveTowards(minerTransform.position, targetPos, speed * deltaTime);

            if (Vector3.Distance(minerTransform.position, targetPos) <= nodeReachDistance)
            {
                // Update the shared Node reference (Miner.graphPos) so Miner sees the new coordinate.
                // startNode was passed from Miner and is the same reference as Miner.graphPos.
                if (startNode != null)
                {
                    startNode.SetCoordinate(coord);
                }

                currentPathIndex++;
                Debug.Log("Reached Node: " + coord.x + " " + coord.y);
            }

        });

        behaviourActions.SetTransitionBehaviour(() =>
        {
            if (path == null || path.Count == 0)
            {
                OnFlag?.Invoke(Miner.Flags.OnTargetReach);
                return;
            }

            if (currentPathIndex >= path.Count)
            {
                // Ensure final graphPos coordinate reflects the path's last node
                if (startNode != null && path.Count > 0)
                {
                    startNode.SetCoordinate(path[path.Count - 1].GetCoordinate());
                }

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
    public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
    {
        Miner miner = parameters[0] as Miner;
        BehaviourActions behaviourActions = new BehaviourActions();
        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {
            //miner.moveTarget = miner.mineLocation;
        });
        return behaviourActions;
    }
}

public class MinerMiningState : State
{
    public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
    {        
        GoldMine goldMine = parameters[0] as GoldMine;        
        int miningRate = (int)parameters[1];        
        InventoryData inv = parameters[2] as InventoryData;
        int posX = (int)parameters[3];
        int postY = (int)parameters[4];


        BehaviourActions behaviourActions = new BehaviourActions();
        behaviourActions.AddMultiThreadableBehaviour(0, () =>
        {
            int minedAmount = goldMine.Mine(miningRate);
            Debug.Log("Miner at position: " + posX + ", " + postY);
            Debug.Log("Mining...");
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
    public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
    {
        Debug.Log("Inventory full!");
        Miner miner = parameters[0] as Miner;
        BehaviourActions behaviourActions = new BehaviourActions();
        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {

            //miner.moveTarget = miner.townLocation;
        });
        return behaviourActions;
    }
}
public class MinerIdle : State {
    public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
    {
        


        BehaviourActions behaviourActions = new BehaviourActions();


        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {           
             
        });

        behaviourActions.SetTransitionBehaviour(() =>
        {            
            OnFlag?.Invoke(Miner.Flags.OnFuckYou);            
        });

        return behaviourActions;
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Rendering;
using UnityEngine;


public class MinerMovingState : State
{
    //Pathfinding 
    private GraphView GV;
    private Traveler traveler = new Traveler();
    private Node<Vector2Int> startNode = new Node<Vector2Int>();
    private Node<Vector2Int> destination = new Node<Vector2Int>();
    List<Node<Vector2Int>> path = new List<Node<Vector2Int>>();

    //Game movement
    private Transform minerTransform;
    private int currentPathIndex;
    // Snapping configuration
    private float snapInterval = 0.5f;
    private float snapTimer = 0f;

    public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
    {
      
        startNode = parameters[0] as Node<Vector2Int>;   
        destination = parameters[1] as Node<Vector2Int>;
        GV = parameters[2] as GraphView;
        
        GoldMine nearestMine = new GoldMine();
        nearestMine = GV.mineManager.FindNearest(startNode.GetCoordinate());
        destination.SetCoordinate(new Vector2Int(nearestMine.Position.x, nearestMine.Position.y));      

        path = traveler.FindPath(startNode, destination , GV);
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
            
            snapTimer += deltaTime;
            
            if (snapTimer < snapInterval)
                return;
            
            while (snapTimer >= snapInterval && currentPathIndex < path.Count)
            {
                Node<Vector2Int> targetNode = path[currentPathIndex];
                Vector2Int coord = targetNode.GetCoordinate();
                Vector3 targetPos = new Vector3(coord.x, coord.y, minerTransform.position.z);
                
                minerTransform.position = targetPos;

                if (startNode != null)
                {
                    startNode.SetCoordinate(coord);
                }
                currentPathIndex++;
              
                snapTimer -= snapInterval;
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

                Debug.Log("Reached Mine");
                OnFlag?.Invoke(Miner.Flags.OnTargetReach);
            }
        });
        return behaviourActions;
    }
}

public class MinerMoveToTown : State
{
    //Pathfinding 
    private GraphView GV;
    private Traveler traveler = new Traveler();
    private Node<Vector2Int> startNode = new Node<Vector2Int>();
    private Node<Vector2Int> destination = new Node<Vector2Int>();
    List<Node<Vector2Int>> path = new List<Node<Vector2Int>>();

    //Game movement
    private Transform minerTransform;
    private int currentPathIndex;
    // Snapping configuration
    private float snapInterval = 0.5f;
    private float snapTimer = 0f;

    public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
    {
        Debug.Log("Heading home!");

        startNode = parameters[0] as Node<Vector2Int>;
        destination = parameters[1] as Node<Vector2Int>;
        GV = parameters[2] as GraphView;

        path = traveler.FindPath(startNode, destination, GV);
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

            snapTimer += deltaTime;

            if (snapTimer < snapInterval)
                return;

            while (snapTimer >= snapInterval && currentPathIndex < path.Count)
            {
                Node<Vector2Int> targetNode = path[currentPathIndex];
                Vector2Int coord = targetNode.GetCoordinate();
                Vector3 targetPos = new Vector3(coord.x, coord.y, minerTransform.position.z);

                minerTransform.position = targetPos;

                if (startNode != null)
                {
                    startNode.SetCoordinate(coord);
                }
                currentPathIndex++;
                snapTimer -= snapInterval;
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

                Debug.Log("Reached Home");
                OnFlag?.Invoke(Miner.Flags.OnTargetReach);
            }
        });

        return behaviourActions;

    }


}
public class MinerDepositingState : State
{
    public override BehaviourActions GetOnEnterBehaviours(params object[] parameters) {
        Debug.Log("Depositing gold...");

        BehaviourActions behaviourActions = new BehaviourActions();
        return behaviourActions;
    }

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
            miner.SetTargetToClosestMine();
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

        int hunger = 0;

        BehaviourActions behaviourActions = new BehaviourActions();
        behaviourActions.AddMultiThreadableBehaviour(0, () =>
        {
            Debug.Log("Mining...");
            if (hunger >= 3)
            {
                if (goldMine.RetrieveFood(1) > 0)
                    hunger = 0;
            }
            else { 
                int minedAmount = goldMine.Mine(miningRate);
                inv.inventory+= minedAmount;
                hunger++;        
            }
        });

        behaviourActions.SetTransitionBehaviour(() =>
        {
            Debug.Log("Gold in inventory: " + inv.inventory);
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



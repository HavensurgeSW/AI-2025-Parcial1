using System.Collections.Generic;
using UnityEngine;


public class CaravanMovingState : State
{
    //Pathfinding 
    private GraphView GV;
    private Traveler traveler = new Traveler();    
    private Node<Vector2Int> startNode = new Node<Vector2Int>();
    private Node<Vector2Int> destination = new Node<Vector2Int>();
    List<Node<Vector2Int>> path = new List<Node<Vector2Int>>();

    //Game movement
    private Transform caravanTransform;
    private int currentPathIndex;
    // Snapping configuration
    private float snapInterval = 0.5f;
    private float snapTimer = 0f;

    public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
    {
        Debug.Log("Entering CaravanMovingState");
        startNode = parameters[0] as Node<Vector2Int>;
        destination = parameters[1] as Node<Vector2Int>;
        GV = parameters[2] as GraphView;
        traveler.pathfinder = new AStarCaravan<Node<Vector2Int>>();

        // Try to resolve the actual node from the graph (same coordinate, real instance)
        Node<Vector2Int> graphNode = null;
        if (GV != null && GV.graph != null && GV.graph.nodes != null)
        {
            graphNode = GV.graph.nodes.Find(n => n.GetCoordinate().Equals(startNode.GetCoordinate()));
        }

        bool foundActiveMine = false;

        if (GV != null && GV.mineManager != null)
        {
            var nearest = GV.mineManager.FindNearestActive(startNode.GetCoordinate());
            if (nearest != null)
            {
                destination.SetCoordinate(nearest.Position);
                foundActiveMine = true;
            }
        }


        if (!foundActiveMine && graphNode != null)
        {
            if (traveler.TryGetNearestMine(graphNode, out Vector2Int mineCoord))
            {
                var mine = GV?.mineManager?.GetMineAt(mineCoord);
                if (mine != null && mine.HasActiveMiners)
                {
                    destination.SetCoordinate(mineCoord);
                    foundActiveMine = true;
                }
            }
        }

        if (!foundActiveMine)
        {
            Debug.Log("CaravanMovingState: no active mine found -> will not start moving.");
            path = new List<Node<Vector2Int>>();
            currentPathIndex = 0;
            BehaviourActions behaviourActionsNoMove = new BehaviourActions();
            behaviourActionsNoMove.AddMainThreadableBehaviour(0, () => { });
            return behaviourActionsNoMove;
        }

        path = traveler.FindPath(startNode, destination, GV) ?? new List<Node<Vector2Int>>();
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
        caravanTransform = parameters[0] as Transform;
        float deltaTime = (float)parameters[1];   

        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {
            snapTimer += deltaTime;

            if (snapTimer < snapInterval)
                return;

            while (snapTimer >= snapInterval && currentPathIndex < path.Count)
            {
                Node<Vector2Int> targetNode = path[currentPathIndex];
                Vector2Int coord = targetNode.GetCoordinate();
                Vector3 targetPos = new Vector3(coord.x * GV.TileSpacing, coord.y * GV.TileSpacing, caravanTransform.position.z);

                caravanTransform.position = targetPos;

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
                OnFlag?.Invoke(Caravan.Flags.OnTargetReach);
                return;
            }

            if (currentPathIndex >= path.Count)
            {
                if (startNode != null && path.Count > 0)
                {
                    startNode.SetCoordinate(path[path.Count - 1].GetCoordinate());
                }
                OnFlag?.Invoke(Caravan.Flags.OnTargetReach);
            }
        });
        return behaviourActions;
    }
}

public class CaravanDepositingState : State
{
    public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
    {
        GoldMine goldMine = parameters[0] as GoldMine;
        InventoryData inv = parameters[1] as InventoryData;

        BehaviourActions behaviourActions = new BehaviourActions();


        behaviourActions.AddMultiThreadableBehaviour(0, () =>
        {
            goldMine.foodStored += inv.inventory;
            inv.inventory = 0;
        });

        behaviourActions.SetTransitionBehaviour(() =>
        {          
             OnFlag?.Invoke(Caravan.Flags.OnInventoryEmpty);            
        });

        return behaviourActions;
    }
}

public class CaravanMovingToTownState : State
{
    //Pathfinding 
    private GraphView GV;
    private Traveler traveler = new Traveler();
    private Node<Vector2Int> startNode = new Node<Vector2Int>();
    private Node<Vector2Int> destination = new Node<Vector2Int>();
    List<Node<Vector2Int>> path = new List<Node<Vector2Int>>();

    //Game movement
    private Transform caravanTransform;
    private int currentPathIndex;
    // Snapping configuration
    private float snapInterval = 0.5f;
    private float snapTimer = 0f;

    public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
    {
        startNode = parameters[0] as Node<Vector2Int>;
        destination = parameters[1] as Node<Vector2Int>;
        GV = parameters[2] as GraphView;
        traveler.pathfinder = new AStarCaravan<Node<Vector2Int>>();

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
        caravanTransform = parameters[0] as Transform;
        float deltaTime = (float)parameters[1];
        bool wasAlarmed = (bool)parameters[2];

        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {

            snapTimer += deltaTime;

            if (snapTimer < snapInterval)
                return;

            while (snapTimer >= snapInterval && currentPathIndex < path.Count)
            {
                Node<Vector2Int> targetNode = path[currentPathIndex];
                Vector2Int coord = targetNode.GetCoordinate();
                Vector3 targetPos = new Vector3(coord.x * GV.TileSpacing, coord.y * GV.TileSpacing, caravanTransform.position.z);

                caravanTransform.position = targetPos;

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
                OnFlag?.Invoke(Caravan.Flags.OnTargetReach);
                return;
            }

            if (currentPathIndex >= path.Count)
            {
                if (startNode != null && path.Count > 0)
                {
                    startNode.SetCoordinate(path[path.Count - 1].GetCoordinate());
                }
                if(!wasAlarmed)
                    OnFlag?.Invoke(Miner.Flags.OnTargetReach);
            }
        });

        return behaviourActions;

    }
}

public class CaravanRestockingState : State
{
    public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
    {
        InventoryData inv = parameters[0] as InventoryData;

        BehaviourActions behaviourActions = new BehaviourActions();


        behaviourActions.AddMultiThreadableBehaviour(0, () =>
        {
            inv.inventory = inv.maxInventory;
        });

        behaviourActions.SetTransitionBehaviour(() =>
        {
            OnFlag?.Invoke(Caravan.Flags.OnInventoryFull);
        });

        return behaviourActions;
    }
}
public class CaravanIdleState : State
{
    public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
    {



        BehaviourActions behaviourActions = new BehaviourActions();


        behaviourActions.AddMainThreadableBehaviour(0, () =>
        {

        });

        behaviourActions.SetTransitionBehaviour(() =>
        {
            //OnFlag?.Invoke(Caravan.Flags.OnSpawned);
        });

        return behaviourActions;
    }
}



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

        if (graphNode != null)
        {
            if (traveler.TryGetNearestMine(graphNode, out Vector2Int mineCoord))
            {
                destination.SetCoordinate(mineCoord);
            }
        }
        else
        {
            // If we couldn't find the graph node instance, try the simple fallback path
            if (GV != null && GV.mineManager != null)
            {
                var nearestMine = GV.mineManager.FindNearest(startNode.GetCoordinate());
                if (nearestMine != null)
                    destination.SetCoordinate(nearestMine.Position);
            }
        }

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
                // Ensure final graphPos coordinate reflects the path's last node
                //El ultimo nodo del path es el 
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
        int currentStorage = (int)parameters[1];

        BehaviourActions behaviourActions = new BehaviourActions();


        behaviourActions.AddMultiThreadableBehaviour(0, () =>
        {
            goldMine.foodStored += currentStorage;
            Debug.Log($"Deposited {currentStorage} food to the Mine. Total food: {goldMine.foodStored}");
        });

        behaviourActions.SetTransitionBehaviour(() =>
        {
            OnFlag?.Invoke(Caravan.Flags.OnInventoryEmpty);
            Debug.Log("Transitioning from Depositing to MoveToTown");
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
                Vector3 targetPos = new Vector3(coord.x * GV.TileSpacing, coord.y * GV.TileSpacing, minerTransform.position.z);

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
                OnFlag?.Invoke(Caravan.Flags.OnTargetReach);
                return;
            }

            if (currentPathIndex >= path.Count)
            {
                if (startNode != null && path.Count > 0)
                {
                    startNode.SetCoordinate(path[path.Count - 1].GetCoordinate());
                }

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
        int currentStorage = (int)parameters[0];
        int storageSize = (int)parameters[1];

        BehaviourActions behaviourActions = new BehaviourActions();


        behaviourActions.AddMultiThreadableBehaviour(0, () =>
        {
            currentStorage = storageSize;
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
            OnFlag?.Invoke(Caravan.Flags.OnSpawned);
        });

        return behaviourActions;
    }
}



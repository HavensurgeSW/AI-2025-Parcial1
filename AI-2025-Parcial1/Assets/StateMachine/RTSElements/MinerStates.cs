using System.Collections.Generic;
using UnityEngine;
using KarplusParcial1.FSM.Core;
using KarplusParcial1.Graph;
using KarplusParcial1.Graph.Core;
using KarplusParcial1.RTSElements;
using KarplusParcial1.Pathfinding;

namespace KarplusParcial1.FSM.States
{
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
            traveler.pathfinder = new AStarPathfinder<Node<Vector2Int>>();

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
                if (GV != null && GV.mineManager != null)
                {
                    var nearestMine = GV.mineManager.FindNearest(startNode.GetCoordinate());
                    if (nearestMine != null)
                        destination.SetCoordinate(nearestMine.Position);
                }
            }

            path = traveler.FindPath(startNode, destination, GV);
            if (path == null) path = new List<Node<Vector2Int>>();
            currentPathIndex = 0;
            snapTimer = 0f;

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


            behaviourActions.AddMainThreadableBehaviour(0, () =>
            {
                if (minerTransform == null) return;
                if (GV == null) return;
                if (path == null || path.Count == 0) return;

                snapTimer += deltaTime;

                if (snapTimer < snapInterval)
                    return;

                while (snapTimer >= snapInterval && currentPathIndex < path.Count)
                {
                    Node<Vector2Int> targetNode = path[currentPathIndex];
                    if (targetNode == null) break;

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
                    OnFlag?.Invoke(Miner.Flags.OnTargetReach);
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

        public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            BehaviourActions behaviourActions = new BehaviourActions();
            behaviourActions.AddMainThreadableBehaviour(0, () =>
            {
                path?.Clear();
                currentPathIndex = 0;
                snapTimer = 0f;
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
            startNode = parameters[0] as Node<Vector2Int>;
            destination = parameters[1] as Node<Vector2Int>;
            GV = parameters[2] as GraphView;

            path = traveler.FindPath(startNode, destination, GV);
            //si el path devuelve null, inicio la variable para que no explote todo
            if (path == null) path = new List<Node<Vector2Int>>();

            currentPathIndex = 0;
            snapTimer = 0f;
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
            bool wasAlarmed = (bool)parameters[2];

            behaviourActions.AddMainThreadableBehaviour(0, () =>
            {
                if (minerTransform == null) return;
                if (GV == null) return;
                if (path == null || path.Count == 0) return;

                snapTimer += deltaTime;

                if (snapTimer < snapInterval)
                    return;

                while (snapTimer >= snapInterval && currentPathIndex < path.Count)
                {
                    Node<Vector2Int> targetNode = path[currentPathIndex];
                    if (targetNode == null) break;

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
                    OnFlag?.Invoke(Miner.Flags.OnTargetReach);
                    return;
                }

                if (currentPathIndex >= path.Count)
                {

                    if (startNode != null && path.Count > 0)
                    {
                        startNode.SetCoordinate(path[path.Count - 1].GetCoordinate());
                    }
                    if (!wasAlarmed)
                        OnFlag?.Invoke(Miner.Flags.OnTargetReach);
                }
            });

            return behaviourActions;

        }

        public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            BehaviourActions behaviourActions = new BehaviourActions();
            behaviourActions.AddMainThreadableBehaviour(0, () =>
            {
                path?.Clear();
                currentPathIndex = 0;
                snapTimer = 0f;
            });
            return behaviourActions;
        }


    }
    public class MinerDepositingState : State
    {
        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {


            BehaviourActions behaviourActions = new BehaviourActions();
            return behaviourActions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Townhall townhall = parameters[0] as Townhall;
            InventoryData inventory = parameters[1] as InventoryData;

            BehaviourActions behaviourActions = new BehaviourActions();

            behaviourActions.AddMainThreadableBehaviour(0, () =>
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
        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            GoldMine goldMine = parameters[0] as GoldMine;

            BehaviourActions behaviourActions = new BehaviourActions();
            behaviourActions.AddMultiThreadableBehaviour(0, () =>
            {
                goldMine.AddMiner();
            });
            return behaviourActions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            GoldMine goldMine = parameters[0] as GoldMine;
            int miningRate = (int)parameters[1];
            InventoryData inv = parameters[2] as InventoryData;
            Miner miner = parameters[3] as Miner;
            


            BehaviourActions behaviourActions = new BehaviourActions();
            behaviourActions.AddMultiThreadableBehaviour(0, () =>
            {

                if (inv.hunger >= 3)
                {
                    if (goldMine != null && goldMine.RetrieveFood(1) > 0)
                    {
                        inv.hunger = 0;

                    }
                }
                else
                {
                    int minedAmount = 0;
                    if (goldMine != null)
                        minedAmount = goldMine.Mine(miningRate);
                    inv.inventory += minedAmount;
                    inv.hunger++;
                }

            });

            behaviourActions.SetTransitionBehaviour(() =>
            {
                if (inv.inventory >= inv.maxInventory)
                {
                    OnFlag?.Invoke(Miner.Flags.OnInventoryFull);
                }
                else if (goldMine == null || goldMine.isDepleted)
                {
                    if (miner != null)
                    {
                        miner.SetTargetToClosestMine();
                    }
                    OnFlag?.Invoke(Miner.Flags.OnMineDepleted);
                }
            });

            return behaviourActions;
        }
    }
    public class MinerIdle : State
    {
        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {


            BehaviourActions behaviourActions = new BehaviourActions();


            behaviourActions.AddMainThreadableBehaviour(0, () =>
            {

            });

            behaviourActions.SetTransitionBehaviour(() =>
            {
                //OnFlag?.Invoke(Miner.Flags.OnSpawned);            
            });

            return behaviourActions;
        }
    }
}



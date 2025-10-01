using UnityEngine;

public class Caravan : MonoBehaviour
{

    public Townhall townhall = new Townhall();
    InventoryData inventory = new InventoryData();

    public GraphView GV;
    Node<Vector2Int> graphPos = new Node<Vector2Int>();
    Node<Vector2Int> targetPos = new Node<Vector2Int>();
    Node<Vector2Int> home = new Node<Vector2Int>();

    public enum State
    {
        MoveToTarget,
        MoveToTown,
        Depositing,
        Restocking,
        Idle
    }

    public enum Flags
    {
        OnInventoryEmpty,
        OnInventoryFull,
        OnTargetReach,
        OnReachedTown,
        OnSpawned,
        OnMineDepleted,
        AlarmRaised,
        AlarmCleared
    }

    public FSM<State, Flags> caravanFsm;
    private State previousState;
    private bool wasAlarmed = false;

    public void Start()
    {
        home.SetCoordinate(townhall.Position);
        graphPos.SetCoordinate(new Vector2Int(0, 0));
        caravanFsm = new FSM<State, Flags>(State.Idle);
        inventory.maxInventory = 10;
        inventory.inventory = inventory.maxInventory;


        caravanFsm.AddState<CaravanIdleState>(State.Idle);
        caravanFsm.AddState<CaravanMovingState>(State.MoveToTarget, onTickParameters: () => new object[] { this.transform, Time.deltaTime }, onEnterParameters: () => new object[] { graphPos, targetPos, GV });
        caravanFsm.AddState<CaravanMovingToTownState>(State.MoveToTown, onTickParameters: () => new object[] { this.transform, Time.deltaTime, wasAlarmed }, onEnterParameters: () => new object[] { graphPos, home, GV });
        caravanFsm.AddState<CaravanRestockingState>(State.Restocking, onTickParameters: () => new object[] {inventory});
        caravanFsm.AddState<CaravanDepositingState>(State.Depositing, onTickParameters: () => new object[] { GV.mineManager.GetMineAt(new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y)), inventory });

        caravanFsm.SetTransition(State.Idle, Flags.OnSpawned, State.MoveToTarget);
        caravanFsm.SetTransition(State.MoveToTarget, Flags.OnTargetReach, State.Depositing);
        caravanFsm.SetTransition(State.Depositing, Flags.OnInventoryEmpty, State.MoveToTown);
        caravanFsm.SetTransition(State.MoveToTown, Flags.OnTargetReach, State.Restocking);
        caravanFsm.SetTransition(State.Restocking, Flags.OnInventoryFull, State.MoveToTarget);

        AlarmManager.OnAlarmRaised += HandleAlarmRaised;
        AlarmManager.OnAlarmCleared += HandleAlarmCleared;
        
        GV.mineManager.MineActivated += HandleMineActivated;
        GV.mineManager.MineDeactivatedByActivity += HandleMineDeactivated;
        
    }

    private void OnDestroy()
    {
        AlarmManager.OnAlarmRaised -= HandleAlarmRaised;
        AlarmManager.OnAlarmCleared -= HandleAlarmCleared;

        if (GV != null && GV.mineManager != null)
        {
            GV.mineManager.MineActivated -= HandleMineActivated;
            GV.mineManager.MineDeactivatedByActivity -= HandleMineDeactivated;
        }
    }

    private void HandleAlarmRaised()
    {
        wasAlarmed = true;            
        caravanFsm.ForceState(State.MoveToTown);        
    }

    private void HandleAlarmCleared()
    {
        if (wasAlarmed)
        {
            wasAlarmed = false;
            caravanFsm.ForceState(State.MoveToTarget);
        }
    }

    private void Update()
    {
        caravanFsm.Tick();
    }


    public void SetTargetToClosestMine()
    {
        if (GV == null)
        {
            GV = FindFirstObjectByType<GraphView>();
            if (GV == null)
            {
                Debug.LogWarning("SetTargetToClosestMine: GraphView is null. Can't find nearest mine.");
                return;
            }
        }

        if (GV.mineManager == null)
        {
            Debug.LogWarning("SetTargetToClosestMine: mineManager is null. Can't find nearest mine.");
            return;
        }

        Vector2Int origin = new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y);
        GoldMine nearest = GV.mineManager.FindNearestActive(origin);
        if (nearest == null)
        {
            Debug.LogWarning($"SetTargetToClosestMine: no active nearest mine found from {origin}.");
            return;
        }

        targetPos.SetCoordinate(nearest.Position);
    }

    private void HandleMineActivated(GoldMine activated)
    {
        Debug.Log("Caravan detected mine activation.");
        caravanFsm.ForceSetState(State.MoveToTarget);

    }

    private void HandleMineDeactivated(GoldMine deactivated)
    {
        if (GV == null || GV.mineManager == null) return;

        var targetCoord = new Vector2Int(targetPos.GetCoordinate().x, targetPos.GetCoordinate().y);
        var currentTarget = GV.mineManager.GetMineAt(targetCoord);

        // Si el target actual es el que se desactivó, buscar el otro mas cercano
        if (currentTarget == null || currentTarget == deactivated)
        {
            SetTargetToClosestMine();
        }
    }
}

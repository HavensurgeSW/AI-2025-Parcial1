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
        caravanFsm.AddState<CaravanMovingToTownState>(State.MoveToTown, onTickParameters: () => new object[] { this.transform, Time.deltaTime }, onEnterParameters: () => new object[] { graphPos, home, GV });
        caravanFsm.AddState<CaravanRestockingState>(State.Restocking, onTickParameters: () => new object[] {inventory});
        caravanFsm.AddState<CaravanDepositingState>(State.Depositing, onTickParameters: () => new object[] { GV.mineManager.GetMineAt(new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y)), inventory });

        caravanFsm.SetTransition(State.Idle, Flags.OnSpawned, State.MoveToTarget);
        caravanFsm.SetTransition(State.MoveToTarget, Flags.OnTargetReach, State.Depositing);
        caravanFsm.SetTransition(State.Depositing, Flags.OnInventoryEmpty, State.MoveToTown);
        caravanFsm.SetTransition(State.MoveToTown, Flags.OnTargetReach, State.Restocking);
        caravanFsm.SetTransition(State.Restocking, Flags.OnInventoryFull, State.MoveToTarget);

        // Subscribe to alarm events
        AlarmManager.OnAlarmRaised += HandleAlarmRaised;
        AlarmManager.OnAlarmCleared += HandleAlarmCleared;
    }

    private void OnDestroy()
    {
        AlarmManager.OnAlarmRaised -= HandleAlarmRaised;
        AlarmManager.OnAlarmCleared -= HandleAlarmCleared;
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
            caravanFsm.ForceState(previousState);
        }
    }

    private void Update()
    {
        caravanFsm.Tick();
    }
    public void SetTargetToClosestMine()
    {
        targetPos.SetCoordinate(GV.mineManager.FindNearest(new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y)).Position);
    }
}

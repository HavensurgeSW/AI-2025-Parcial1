using UnityEngine;

public class Caravan : MonoBehaviour
{
    int storageSize = 10;
    int currentStorage = 0;

    public Townhall townhall = new Townhall();
    InventoryData inventoryData = new InventoryData();

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
        OnMineDepleted
    }

    public FSM<State, Flags> caravanFsm;
    public void Start()
    {
        home.SetCoordinate(townhall.Position);
        graphPos.SetCoordinate(new Vector2Int(0, 0));
        caravanFsm = new FSM<State, Flags>(State.Idle);
        currentStorage = 10;


        caravanFsm.AddState<CaravanIdleState>(State.Idle);
        caravanFsm.AddState<CaravanMovingState>(State.MoveToTarget, onTickParameters: () => new object[] { this.transform, Time.deltaTime }, onEnterParameters: () => new object[] { graphPos, targetPos, GV });
        caravanFsm.AddState<CaravanMovingToTownState>(State.MoveToTown, onTickParameters: () => new object[] { this.transform, Time.deltaTime }, onEnterParameters: () => new object[] { graphPos, home, GV });
        caravanFsm.AddState<CaravanRestockingState>(State.Restocking, onTickParameters: () => new object[] {currentStorage, storageSize });
        caravanFsm.AddState<CaravanDepositingState>(State.Depositing, onTickParameters: () => new object[] { GV.mineManager.GetMineAt(new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y)), currentStorage });

        caravanFsm.SetTransition(State.Idle, Flags.OnSpawned, State.MoveToTarget);
        caravanFsm.SetTransition(State.MoveToTarget, Flags.OnTargetReach, State.Depositing);
        caravanFsm.SetTransition(State.Depositing, Flags.OnInventoryEmpty, State.MoveToTown);
        ////caravanFsm.SetTransition(State.Mining, Flags.OnMineDepleted, State.MoveToTarget);
        caravanFsm.SetTransition(State.MoveToTown, Flags.OnTargetReach, State.Restocking);
        caravanFsm.SetTransition(State.Restocking, Flags.OnInventoryFull, State.MoveToTarget);
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

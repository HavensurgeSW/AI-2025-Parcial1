using System;
using System.Runtime.CompilerServices;
using UnityEngine;


class Miner : MonoBehaviour
{    
    int miningRate = 10;
    [SerializeField] GoldMine goldMine;
    [SerializeField] Townhall townhall;
    InventoryData inventoryData = new InventoryData();

    Node<Vector2Int> graphPos = new Node<Vector2Int>();
    [SerializeField] GraphView GV;

    public enum State
    {
        MoveToTarget,
        MoveToTown,
        Depositing,
        Mining,
        Idle
    }

    public enum Flags
    {
        OnInventoryEmpty,
        OnInventoryFull,
        OnTargetReach,
        OnFuckYou
    }


    public FSM<State, Flags> minerFsm;

    public void Start()
    {
        graphPos.SetCoordinate(new Vector2Int(0,0));
        minerFsm = new FSM<State, Flags>(State.Idle);
        //goldMine = GV.mineManager.GetMineAt(new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y));

        minerFsm.AddState<MinerMovingState>(State.MoveToTarget, onTickParameters: () => new object[] {this.transform, Time.deltaTime}, onEnterParameters:()=>new object[] {graphPos, GV});
        minerFsm.AddState<MinerIdle>(State.Idle);
        minerFsm.AddState<MinerMiningState>(State.Mining, onTickParameters: () => new object[] { GV.mineManager.GetMineAt(new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y)), miningRate, inventoryData, graphPos.GetCoordinate().x, graphPos.GetCoordinate().y}, null, onExitParameters: () => new object[] { this});
        minerFsm.AddState<MinerMoveToTown>(State.MoveToTown, onTickParameters: () => new object[] { });
        minerFsm.AddState<MinerDepositingState>(State.Depositing, onTickParameters: () => new object[] { townhall, inventoryData}, null, onExitParameters: () => new object[] { this });

        minerFsm.SetTransition(State.Idle, Flags.OnFuckYou, State.MoveToTarget);
        minerFsm.SetTransition(State.MoveToTarget, Flags.OnTargetReach, State.Mining);
        minerFsm.SetTransition(State.Mining, Flags.OnInventoryFull, State.MoveToTown);
        //minerFsm.SetTransition(State.MoveToTown, Flags.OnTargetReach, State.Depositing);
        //minerFsm.SetTransition(State.Depositing, Flags.OnInventoryEmpty, State.MoveToTarget);
    }

    private void Update()
    {
        minerFsm.Tick();
    }
}

class InventoryData {
    public int inventory;
    public int maxInventory;
    public InventoryData() {
        inventory = 0;
        maxInventory = 100;
    }
}

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
    [SerializeField] GraphView graphView;
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

        minerFsm.AddState<MinerMovingState>(State.MoveToTarget, onTickParameters: () => new object[] {this.transform, Time.deltaTime}, onEnterParameters:()=>new object[] {graphPos, graphView});
        minerFsm.AddState<MinerIdle>(State.Idle);
        minerFsm.AddState<MinerMoveToTown>(State.MoveToTown, onTickParameters: () => new object[] { });
        minerFsm.AddState<MinerDepositingState>(State.Depositing, onTickParameters: () => new object[] { townhall, inventoryData}, null, onExitParameters: () => new object[] { this });
        minerFsm.AddState<MinerMiningState>(State.Mining, onTickParameters: () => new object[] { goldMine, miningRate, inventoryData}, null, onExitParameters: () => new object[] { this});

        minerFsm.SetTransition(State.MoveToTarget, Flags.OnTargetReach, State.Mining);
        minerFsm.SetTransition(State.Idle, Flags.OnFuckYou, State.MoveToTarget);
        //minerFsm.SetTransition(State.Mining, Flags.OnInventoryFull, State.MoveToTown);
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

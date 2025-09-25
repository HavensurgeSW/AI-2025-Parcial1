using System;
using System.Runtime.CompilerServices;
using UnityEngine;


class Miner : MonoBehaviour
{    
    int miningRate = 1;

    GoldMine goldMine;
    Townhall townhall = new Townhall(new Vector2Int(0,0));
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
        Mining,
        Idle
    }

    public enum Flags
    {
        OnInventoryEmpty,
        OnInventoryFull,
        OnTargetReach,
        OnReachedTown,
        OnFuckYou
    }


    public FSM<State, Flags> minerFsm;

    public void Start()
    {
        home.SetCoordinate(townhall.Position);
        graphPos.SetCoordinate(new Vector2Int(0,0));
        minerFsm = new FSM<State, Flags>(State.Idle);
        

        minerFsm.AddState<MinerIdle>(State.Idle);
        minerFsm.AddState<MinerMovingState>(State.MoveToTarget, onTickParameters: () => new object[] {this.transform, Time.deltaTime}, onEnterParameters:()=>new object[] {graphPos, targetPos, GV});
        minerFsm.AddState<MinerMoveToTown>(State.MoveToTown, onTickParameters: () => new object[] {this.transform, Time.deltaTime}, onEnterParameters:()=>new object[] {graphPos, home, GV});
        minerFsm.AddState<MinerMiningState>(State.Mining, onTickParameters: () => new object[] { GV.mineManager.GetMineAt(new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y)), miningRate, inventoryData});
        minerFsm.AddState<MinerDepositingState>(State.Depositing, onTickParameters: () => new object[] { townhall, inventoryData}, null, onExitParameters: () => new object[] { this });

        minerFsm.SetTransition(State.Idle, Flags.OnFuckYou, State.MoveToTarget);
        minerFsm.SetTransition(State.MoveToTarget, Flags.OnTargetReach, State.Mining);
        minerFsm.SetTransition(State.Mining, Flags.OnInventoryFull, State.MoveToTown);
        minerFsm.SetTransition(State.MoveToTown, Flags.OnTargetReach, State.Depositing);
        //minerFsm.SetTransition(State.Depositing, Flags.OnInventoryEmpty, State.MoveToTarget);
    }

    private void Update()
    {
        minerFsm.Tick();
    }
    public void SetTargetToClosestMine() { 
        targetPos.SetCoordinate(GV.mineManager.FindNearest(new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y)).Position);
    }
}

class InventoryData {
    public int inventory;
    public int maxInventory;
    public int food;
    public InventoryData() {
        inventory = 0;
        maxInventory = 15;
        food = 10;
    }
}

using System;
using System.Runtime.CompilerServices;
using UnityEngine;


class Miner : MonoBehaviour
{
    
    int miningRate = 10;
    [SerializeField] GoldMine goldMine;
    [SerializeField] Townhall townhall;
    InventoryData inventoryData = new InventoryData();

    public float speed;
    public float reachDistance;
    public Transform mineLocation;
    public Transform townLocation;
    public Transform moveTarget;
    public enum State
    {
        MoveToTarget,
        MoveToTown,
        Depositing,
        Mining
    }

    public enum Flags
    {
        OnInventoryEmpty,
        OnInventoryFull,
        OnTargetReach,
    }


    public FSM<State, Flags> minerFsm;

    public void Start()
    {
        minerFsm = new FSM<State, Flags>(State.MoveToTarget);

        minerFsm.AddState<MinerMovingState>(State.MoveToTarget, onTickParameters: () => new object[] { moveTarget, transform, speed, reachDistance, Time.deltaTime});
        minerFsm.AddState<MinerMoveToTown>(State.MoveToTown, onTickParameters: () => new object[] { townLocation, transform, speed, reachDistance, Time.deltaTime });
        minerFsm.AddState<MinerDepositingState>(State.Depositing, onTickParameters: () => new object[] { townhall, inventoryData}, null, onExitParameters: () => new object[] { this });
        minerFsm.AddState<MinerMiningState>(State.Mining, onTickParameters: () => new object[] { goldMine, miningRate, inventoryData}, null, onExitParameters: () => new object[] { this});

        minerFsm.SetTransition(State.MoveToTarget, Flags.OnTargetReach, State.Mining);
        minerFsm.SetTransition(State.Mining, Flags.OnInventoryFull, State.MoveToTown);
        minerFsm.SetTransition(State.MoveToTown, Flags.OnTargetReach, State.Depositing);
        minerFsm.SetTransition(State.Depositing, Flags.OnInventoryEmpty, State.MoveToTarget);
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

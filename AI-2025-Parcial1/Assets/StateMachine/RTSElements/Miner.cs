using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using KarplusParcial1.Graph.Core;
using KarplusParcial1.Graph;
using KarplusParcial1.FSM.States;
using KarplusParcial1.Management;

namespace KarplusParcial1.RTSElements
{
    class Miner : MonoBehaviour
    {
        int miningRate = 1;

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
            Mining,
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


        public FSM<State, Flags> minerFsm;
        private State previousState;
        private bool wasAlarmed = false;

        public void Start()
        {
            var homePos = townhall != null ? townhall.Position : new Vector2Int(0, 0);
            home.SetCoordinate(homePos);

            graphPos.SetCoordinate(new Vector2Int(0, 0));
            minerFsm = new FSM<State, Flags>(State.MoveToTown);

            minerFsm.AddState<MinerIdle>(State.Idle);
            minerFsm.AddState<MinerMovingState>(State.MoveToTarget, onTickParameters: () => new object[] { this.transform, Time.deltaTime }, onEnterParameters: () => new object[] { graphPos, targetPos, GV });
            minerFsm.AddState<MinerMoveToTown>(State.MoveToTown, onTickParameters: () => new object[] { this.transform, Time.deltaTime, wasAlarmed }, onEnterParameters: () => new object[] { graphPos, home, GV });

            minerFsm.AddState<MinerMiningState>(State.Mining, onTickParameters: () => new object[] {
            (GV != null && GV.mineManager != null) ? GV.mineManager.GetMineAt(new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y)) : null,
            miningRate, inventoryData, this}, onEnterParameters: () => new object[] { GV.mineManager.GetMineAt(new Vector2Int(graphPos.GetCoordinate().x, graphPos.GetCoordinate().y)) });

            minerFsm.AddState<MinerDepositingState>(State.Depositing, onTickParameters: () => new object[] { townhall, inventoryData }, null, onExitParameters: () => new object[] { this });

            minerFsm.SetTransition(State.Idle, Flags.OnSpawned, State.MoveToTarget);
            minerFsm.SetTransition(State.MoveToTarget, Flags.OnTargetReach, State.Mining);
            minerFsm.SetTransition(State.Mining, Flags.OnInventoryFull, State.MoveToTown);
            minerFsm.SetTransition(State.Mining, Flags.OnMineDepleted, State.MoveToTarget);
            minerFsm.SetTransition(State.MoveToTown, Flags.OnTargetReach, State.Depositing);
            minerFsm.SetTransition(State.Depositing, Flags.OnInventoryEmpty, State.MoveToTarget);

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
            Debug.Log("Miner alarm raised, going to town!");
            wasAlarmed = true;
            minerFsm.ForceSetState(State.MoveToTown);
        }

        private void HandleAlarmCleared()
        {
            if (wasAlarmed)
            {
                wasAlarmed = false;
                minerFsm.ForceSetState(State.MoveToTarget);
            }
        }

        private void Update()
        {
            minerFsm.Tick();
        }


        public void SetTargetToClosestMine()
        {
            var origin = graphPos.GetCoordinate();
            var nearest = GV.mineManager.FindNearest(new Vector2Int(origin.x, origin.y));
            if (nearest == null)
            {
                return;
            }

            targetPos.SetCoordinate(nearest.Position);
        }
    }

    class InventoryData
    {
        public int inventory;
        public int maxInventory;
        public int hunger;
        public InventoryData()
        {
            inventory = 0;
            maxInventory = 15;
            hunger = 0;
        }
    }
}

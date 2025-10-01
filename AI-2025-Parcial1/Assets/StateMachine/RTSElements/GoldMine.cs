using System;
using UnityEngine;

namespace KarplusParcial1.RTSElements
{
    public class GoldMine
    {
        public int maxGold;
        public int currentGold;
        public int foodStored;

        public Vector2Int Position { get; }
        public event Action<GoldMine> OnDepleted;

        private int activeMinerCount;
        public bool HasActiveMiners => activeMinerCount > 0;
        public event Action<GoldMine> OnActivated;
        public event Action<GoldMine> OnDeactivatedByActivity;

        public GoldMine()
        {
            maxGold = 10;
            currentGold = maxGold;
            foodStored = 5;
            activeMinerCount = 0;
        }
        public GoldMine(int maxGold, Vector2Int position)
        {
            this.maxGold = maxGold;
            this.currentGold = maxGold;
            this.Position = position;
            this.foodStored = 5;
            this.activeMinerCount = 0;
        }
        public int Mine(int amount)
        {
            int mined = Mathf.Min(amount, currentGold);
            currentGold -= mined;

            if (currentGold <= 0)
            {
                activeMinerCount = 0;
                OnDepleted?.Invoke(this);
            }
            return mined;
        }
        public int RetrieveFood(int amount)
        {
            int retrieved = Mathf.Min(amount, foodStored);
            foodStored -= retrieved;
            return retrieved;
        }
        public void CallExist()
        {
            Debug.Log("Mine exists");
        }

        public bool isDepleted => currentGold <= 0;

        public void AddMiner()
        {
            activeMinerCount++;
            if (activeMinerCount == 1)
            {
                OnActivated?.Invoke(this);
            }
        }

        public void RemoveMiner()
        {
            if (activeMinerCount <= 0) return;
            activeMinerCount--;

            if (activeMinerCount == 0)
            {
                OnDeactivatedByActivity?.Invoke(this);
            }
        }

        public void ClearMiners()
        {
            if (activeMinerCount <= 0) return;
            activeMinerCount = 0;
            OnDeactivatedByActivity?.Invoke(this);
        }
    }
}
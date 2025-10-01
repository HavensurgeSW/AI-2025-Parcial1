using System;
using System.Collections.Generic;
using UnityEngine;
using KarplusParcial1.RTSElements;

namespace KarplusParcial1.Management
{
    public class GoldMineManager
    {
        public List<GoldMine> mines = new List<GoldMine>();
        private readonly Dictionary<Vector2Int, GoldMine> byPosition = new Dictionary<Vector2Int, GoldMine>();

        public IReadOnlyList<GoldMine> Mines => mines;

        private readonly List<GoldMine> activeMines = new List<GoldMine>();
        public IReadOnlyList<GoldMine> ActiveMines => activeMines;

        public event Action<GoldMine> MineDepleted;
        public event Action<GoldMine> MineActivated;
        public event Action<GoldMine> MineDeactivatedByActivity;

        public void AddMine(GoldMine mine)
        {
            if (mine == null) return;
            if (!byPosition.ContainsKey(mine.Position))
            {
                mines.Add(mine);
                byPosition[mine.Position] = mine;

                mine.OnDepleted += HandleMineDepleted;
                mine.OnActivated += HandleMineActivated;
                mine.OnDeactivatedByActivity += HandleMineDeactivatedByActivity;

                if (mine.HasActiveMiners && !activeMines.Contains(mine))
                {
                    activeMines.Add(mine);
                }
            }
        }

        private void HandleMineActivated(GoldMine mine)
        {
            if (!activeMines.Contains(mine))
            {
                activeMines.Add(mine);
            }
            MineActivated?.Invoke(mine);
        }

        private void HandleMineDeactivatedByActivity(GoldMine mine)
        {
            activeMines.Remove(mine);
            MineDeactivatedByActivity?.Invoke(mine);
        }

        private void HandleMineDepleted(GoldMine mine)
        {
            activeMines.Remove(mine);
            MineDepleted?.Invoke(mine);
            MineDeactivatedByActivity?.Invoke(mine);
        }

        public void CreateMines(int count, int maxGold, Vector2Int areaSize)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2Int pos;
                do
                {
                    pos = new Vector2Int(UnityEngine.Random.Range(0, areaSize.x), UnityEngine.Random.Range(0, areaSize.y));
                } while (byPosition.ContainsKey(pos));
                var mine = new GoldMine(maxGold, pos);
                AddMine(mine);
            }
        }

        public bool RemoveMineAt(Vector2Int pos)
        {
            if (byPosition.TryGetValue(pos, out var mine))
            {
                mine.OnDepleted -= HandleMineDepleted;
                mine.OnActivated -= HandleMineActivated;
                mine.OnDeactivatedByActivity -= HandleMineDeactivatedByActivity;

                byPosition.Remove(pos);
                mines.Remove(mine);
                activeMines.Remove(mine);
                return true;
            }
            return false;
        }

        public GoldMine GetMineAt(Vector2Int pos)
        {
            byPosition.TryGetValue(pos, out var mine);
            return mine;
        }
        public GoldMine FindNearestActive(Vector2Int origin)
        {
            GoldMine best = null;
            int bestDist = int.MaxValue;
            foreach (var m in activeMines)
            {
                if (m == null) continue;
                if (m.isDepleted) continue;
                int dist = Mathf.Abs(m.Position.x - origin.x) + Mathf.Abs(m.Position.y - origin.y);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = m;
                }
            }
            return best;
        }

        public GoldMine FindNearest(Vector2Int origin)
        {
            GoldMine best = null;
            int bestDist = int.MaxValue;
            foreach (var m in mines)
            {
                if (m.isDepleted) continue;
                int dist = Mathf.Abs(m.Position.x - origin.x) + Mathf.Abs(m.Position.y - origin.y);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = m;
                }
            }
            return best;
        }
        public void CallExist()
        {
            Debug.Log("GM exists");
        }

    }
}
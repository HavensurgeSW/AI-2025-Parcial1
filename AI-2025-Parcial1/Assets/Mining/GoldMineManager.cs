using System;
using System.Collections.Generic;
using UnityEngine;

public class GoldMineManager
{
    public  List<GoldMine> mines = new List<GoldMine>();
    private readonly Dictionary<Vector2Int, GoldMine> byPosition = new Dictionary<Vector2Int, GoldMine>();

    public IReadOnlyList<GoldMine> Mines => mines;


    public event Action<GoldMine> MineDepleted;

    public void AddMine(GoldMine mine)
    {
        if (mine == null) return;
        if (!byPosition.ContainsKey(mine.Position))
        {
            mines.Add(mine);
            byPosition[mine.Position] = mine;
            mine.OnDepleted += HandleMineDepleted;
        }
    }

    private void HandleMineDepleted(GoldMine mine)
    {
        MineDepleted?.Invoke(mine);
    }

    public void CreateMines(int count, int maxGold, Vector2Int areaSize)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2Int pos;
            do
            {
                pos = new Vector2Int(UnityEngine.Random.Range(0, areaSize.x), UnityEngine.Random.Range(0, areaSize.y));
            } while (byPosition.ContainsKey(pos)); // avoid duplicates
            var mine = new GoldMine(maxGold, pos);
            AddMine(mine);
        }
    }

    public bool RemoveMineAt(Vector2Int pos)
    {
        if (byPosition.TryGetValue(pos, out var mine))
        {
            // unsubscribe before removal
            mine.OnDepleted -= HandleMineDepleted;
            byPosition.Remove(pos);
            mines.Remove(mine);
            return true;
        }
        return false;
    }

    public GoldMine GetMineAt(Vector2Int pos)
    {
        byPosition.TryGetValue(pos, out var mine);
        return mine;
    }

    // Simple nearest (Manhattan). For wrap-around/grid you can adapt to toroidal distance.
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
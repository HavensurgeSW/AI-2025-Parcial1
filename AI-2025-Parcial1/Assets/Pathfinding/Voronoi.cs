using System;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi
{
    public Dictionary<Vector2Int, Vector2Int> nearestMineLookup = new Dictionary<Vector2Int, Vector2Int>();
    public Dictionary<Vector2Int, Color> mineColors = new Dictionary<Vector2Int, Color>();

    private readonly GoldMineManager mineManager;
    public bool wrapWorld;
    public Vector2Int mapSize = new Vector2Int(0, 0);

    public Voronoi(GoldMineManager mineManager, bool wrapWorld = false, Vector2Int mapSize = default)
    {
        this.mineManager = mineManager;
        this.wrapWorld = wrapWorld;
        this.mapSize = mapSize;
    }

    private int SquaredDistance(Vector2Int a, Vector2Int b)
    {
        if (!wrapWorld || mapSize.x <= 0 || mapSize.y <= 0)
        {
            int xx = a.x - b.x;
            int xy = a.y - b.y;
            return xx * xx + xy * xy;
        }

        int dx = Math.Abs(a.x - b.x);
        int dy = Math.Abs(a.y - b.y);

        if (mapSize.x > 0) dx = Math.Min(dx, mapSize.x - dx);
        if (mapSize.y > 0) dy = Math.Min(dy, mapSize.y - dy);

        return dx * dx + dy * dy;
    }


    public void ComputeAndColor(Vector2IntGraph<Node<Vector2Int>> graph, Transform parent)
    {
        nearestMineLookup.Clear();
        mineColors.Clear();

        if (mineManager == null || mineManager.mines == null || mineManager.mines.Count == 0)
            return;

        foreach (var m in mineManager.mines)
        {
            if (m == null) continue;
            int key = (m.Position.x * 73856093) ^ (m.Position.y * 19349663);
            int nonNeg = key & 0x7FFFFFFF;
            float hue = (nonNeg % 360) / 360f;
            mineColors[m.Position] = Color.HSVToRGB(hue, 0.6f, 0.95f);
        }

        int mineCount = mineManager.mines.Count;
        for (int i = 0; i < graph.nodes.Count; i++)
        {
            var node = graph.nodes[i];
            Vector2Int coord = node.GetCoordinate();

            if (node.IsBlocked())
            {               
                node.ClearNearestMine();

                var blockedChild = parent.Find($"Tile_{coord.x}_{coord.y}");
                if (blockedChild != null)
                    blockedChild.GetComponent<SpriteRenderer>().color = Color.red;
                continue;
            }

            Vector2Int chosenMinePos = default;
            if (mineCount == 1)
            {
                chosenMinePos = mineManager.mines[0].Position;
            }
            else if (mineCount == 2)
            {
                var a = mineManager.mines[0].Position;
                var b = mineManager.mines[1].Position;

                int da2 = SquaredDistance(coord, a);
                int db2 = SquaredDistance(coord, b);

                chosenMinePos = da2 <= db2 ? a : b;
            }
            else
            {
                int bestDist = int.MaxValue;
                foreach (var m in mineManager.mines)
                {
                    if (m == null) continue;
                    int d2 = SquaredDistance(coord, m.Position);
                    if (d2 < bestDist)
                    {
                        bestDist = d2;
                        chosenMinePos = m.Position;
                    }
                }
            }

            // store mapping and update the node attribute
            nearestMineLookup[coord] = chosenMinePos;
            node.SetNearestMine(chosenMinePos);

            // color the tile: mines themselves remain yellow
            var child = parent.Find($"Tile_{coord.x}_{coord.y}");
            if (child == null) continue;
            var sr = child.GetComponent<SpriteRenderer>();
            if (mineManager.GetMineAt(coord) != null)
            {
                sr.color = Color.yellow;
            }
            else
            {
                if (mineColors.TryGetValue(chosenMinePos, out Color c))
                    sr.color = c;
                else
                    sr.color = Color.green;
            }
        }
    }

    public static GameObject SpawnBisectorBetweenMines(Transform parent, GoldMineManager MM, int indexA, int indexB, float spacing, Vector2Int mapDimensions, float thickness = 0.10f)
    {
        if (MM == null || MM.mines == null) return null;
        if (indexA < 0 || indexA >= MM.mines.Count || indexB < 0 || indexB >= MM.mines.Count) return null;

        var mineA = MM.mines[indexA];
        var mineB = MM.mines[indexB];
        if (mineA == null || mineB == null) return null;

        Vector3 worldA = new Vector3(mineA.Position.x * spacing, mineA.Position.y * spacing, 0f);
        Vector3 worldB = new Vector3(mineB.Position.x * spacing, mineB.Position.y * spacing, 0f);

        Vector3 mid = (worldA + worldB) * 0.5f;
        Vector3 dir = worldB - worldA;
        float dist = dir.magnitude;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        float mapMaxDim = Mathf.Max(mapDimensions.x * spacing, mapDimensions.y * spacing);
        float length = mapMaxDim * 2.5f;
        length = Mathf.Max(length, dist * 1.2f);

        GameObject bisector = new GameObject($"Bisector_{indexA}_{indexB}");
        bisector.transform.SetParent(parent, true);

        var sr = bisector.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.color = new Color(0.1f, 0.9f, 0.9f, 0.8f);
        sr.sortingOrder = 200;

        bisector.transform.position = new Vector3(mid.x, mid.y, 0f);
        bisector.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float worldThickness = thickness * spacing;
        bisector.transform.localScale = new Vector3(length, worldThickness, 1f);

        return bisector;
    }
}
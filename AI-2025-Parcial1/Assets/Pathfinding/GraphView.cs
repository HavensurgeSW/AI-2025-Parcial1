using System;
using System.Collections.Generic;
using UnityEngine;
using KarplusParcial1.Management;
using KarplusParcial1.Graph.Core;
using KarplusParcial1.Graph.VoronoiAlgorithm;
using KarplusParcial1.Pathfinding;

namespace KarplusParcial1.Graph
{
    public class GraphView : MonoBehaviour
    {
        public Vector2IntGraph<Node<Vector2Int>> graph;
        public GoldMineManager mineManager;

        [SerializeField] private GameObject tilePrefab;
        float tileSpacing = 1.0f;

        public Dictionary<Vector2Int, Vector2Int> nearestMineLookup = new Dictionary<Vector2Int, Vector2Int>();
        public bool wrapWorld = false;
        public Vector2Int mapSize = new Vector2Int(0, 0);

        // Para evitar tener que hacer finds, me guardo los SpriteRenderers
        private Dictionary<Vector2Int, SpriteRenderer> tileRenderers = new Dictionary<Vector2Int, SpriteRenderer>(capacity: 0);

        public float TileSpacing { get { return tileSpacing; } }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (graph == null || graph.nodes == null) return;

            foreach (Node<Vector2Int> node in graph.nodes)
            {
                if (mineManager != null && mineManager.GetMineAt(node.GetCoordinate()) != null)
                {
                    Gizmos.color = Color.yellow;
                }
                else if (node.IsBlocked())
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.gray;

                Vector2Int coord = node.GetCoordinate();
                Gizmos.DrawWireSphere(new Vector3(coord.x * tileSpacing, coord.y * tileSpacing, 1), 0.1f);
            }

            if (mineManager != null)
            {
                var sites = new List<Vector2Int>();
                foreach (var m in mineManager.Mines)
                    if (m != null && !m.isDepleted) sites.Add(m.Position);

                Voronoi.DrawBisectorsGizmos(sites, mapSize, wrapWorld, TileSpacing, Color.cyan);
            }
        }

        public void InstantiateTiles()
        {
            if (graph == null || graph.nodes == null) return;
            if (tilePrefab == null) { Debug.LogWarning("GraphView.InstantiateTiles: tilePrefab is null."); return; }

            tileRenderers.Clear();
            tileRenderers = new Dictionary<Vector2Int, SpriteRenderer>(graph.nodes.Count);

            foreach (var node in graph.nodes)
            {
                Vector2Int coord = node.GetCoordinate();
                Vector3 pos = new Vector3(coord.x * tileSpacing, coord.y * tileSpacing, 1);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, this.transform);
                tile.name = $"Tile_{coord.x}_{coord.y}";

                var sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    tileRenderers[coord] = sr;
                }
            }
        }

        public void ColorMines()
        {
            if (graph == null || graph.nodes == null) return;
            if (mineManager == null) return;

            foreach (var node in graph.nodes)
            {
                Vector2Int coord = node.GetCoordinate();
                if (tileRenderers.TryGetValue(coord, out var sr) && sr != null)
                {
                    if (mineManager.GetMineAt(coord) != null) sr.color = Color.yellow;
                }
                else
                {
                    var child = transform.Find($"Tile_{coord.x}_{coord.y}");
                    if (child == null) continue;
                    var sr2 = child.GetComponent<SpriteRenderer>(); if (sr2 == null) continue;
                    if (mineManager.GetMineAt(coord) != null) sr2.color = Color.yellow;
                }
            }
        }

        public void ColorWithVoronoi()
        {
            if (graph == null || graph.nodes == null) return;

            if (mineManager == null)
            {
                nearestMineLookup.Clear();
                return;
            }

            // Collect non-depleted mine sites
            var sites = new List<Vector2Int>();
            foreach (var m in mineManager.Mines)
            {
                if (m == null) continue;
                if (m.isDepleted) continue;
                sites.Add(m.Position);
            }

            if (sites.Count == 0)
            {
                nearestMineLookup.Clear();
                ColorMines();
                return;
            }

            // Simple brute-force nearest assignment performed here (keeps Voronoi class focused on bisector visualization).
            nearestMineLookup.Clear();

            int width = Math.Max(1, mapSize.x);
            int height = Math.Max(1, mapSize.y);

            // Build blocked map for quick checks
            var coordToBlocked = new Dictionary<Vector2Int, bool>(graph.nodes.Count);
            foreach (var n in graph.nodes) coordToBlocked[n.GetCoordinate()] = n.IsBlocked();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var coord = new Vector2Int(x, y);
                    if (coordToBlocked.TryGetValue(coord, out bool blocked) && blocked)
                        continue;

                    long bestDistSq = long.MaxValue;
                    Vector2Int bestSite = new Vector2Int(-1, -1);

                    for (int i = 0; i < sites.Count; i++)
                    {
                        var s = sites[i];
                        int dx = WrappedDelta(x, s.x, width, wrapWorld);
                        int dy = WrappedDelta(y, s.y, height, wrapWorld);
                        long distSq = (long)dx * dx + (long)dy * dy;
                        if (distSq < bestDistSq)
                        {
                            bestDistSq = distSq;
                            bestSite = s;
                        }
                    }

                    if (bestSite.x != -1)
                        nearestMineLookup[coord] = bestSite;
                }
            }

            // IMPORTANT: update Node objects so other systems that query nodes get the same nearest mine
            foreach (var node in graph.nodes)
            {
                var coord = node.GetCoordinate();
                if (nearestMineLookup.TryGetValue(coord, out Vector2Int owner))
                {
                    node.SetNearestMine(owner);
                }
                else
                {
                    node.ClearNearestMine();
                }
            }

            // Generate deterministic colors per mine
            var mineColors = new Dictionary<Vector2Int, Color>();
            foreach (var s in sites)
            {
                int key = (s.x * 73856093) ^ (s.y * 19349663);
                int nonNeg = key & 0x7FFFFFFF;
                float hue = (nonNeg % 360) / 360f;
                mineColors[s] = Color.HSVToRGB(hue, 0.6f, 0.95f);
            }

            // Paint tiles using cached SpriteRenderers
            foreach (var node in graph.nodes)
            {
                Vector2Int coord = node.GetCoordinate();
                SpriteRenderer sr = null;
                tileRenderers.TryGetValue(coord, out sr);

                if (sr == null)
                {
                    var child = transform.Find($"Tile_{coord.x}_{coord.y}");
                    if (child != null) sr = child.GetComponent<SpriteRenderer>();
                }
                if (sr == null) continue;

                // Mines override color
                if (mineManager.GetMineAt(coord) != null)
                {
                    sr.color = Color.yellow;
                    continue;
                }

                // Blocked tiles are red
                if (node.IsBlocked())
                {
                    sr.color = Color.red;
                    continue;
                }

                // Use lookup color if available
                if (nearestMineLookup.TryGetValue(coord, out Vector2Int ownerPos) && mineColors.TryGetValue(ownerPos, out Color c))
                {
                    sr.color = c;
                    continue;
                }

                // Fallback
                sr.color = Color.green;
            }
        }

        public void OverlayRoads()
        {
            if (graph == null || graph.nodes == null) return;

            foreach (var node in graph.nodes)
            {
                if (!node.IsRoad()) continue;
                Vector2Int coord = node.GetCoordinate();
                if (tileRenderers.TryGetValue(coord, out var sr) && sr != null)
                {
                    sr.color = Color.grey;
                }
                else
                {
                    var child = transform.Find($"Tile_{coord.x}_{coord.y}");
                    if (child == null) continue;
                    var sr2 = child.GetComponent<SpriteRenderer>(); if (sr2 == null) continue;
                    sr2.color = Color.grey;
                }
            }
        }

        public void CallExist() { Debug.Log("GV exists"); }
        public void AssignMineManager(GoldMineManager gmm) { mineManager = gmm; }
        public void AssignTile(GameObject go) { tilePrefab = go; }
        public void SetSpacing(float s) { tileSpacing = s; }

        public Node<Vector2Int> GetNodeAt(Vector2Int coord)
        {
            if (graph == null || graph.nodes == null) return null;
            return graph.nodes.Find(n => n.GetCoordinate().Equals(coord));
        }

        public bool TryGetNearestMineAt(Vector2Int coord, out Vector2Int mineCoordinate)
        {
            mineCoordinate = default;
            if (nearestMineLookup != null && nearestMineLookup.TryGetValue(coord, out Vector2Int lookup))
            {
                mineCoordinate = lookup;
                return true;
            }

            if (mineManager != null)
            {
                var m = mineManager.FindNearest(coord);
                if (m != null)
                {
                    mineCoordinate = m.Position;
                    return true;
                }
            }

            return false;
        }

        private static int WrappedDelta(int a, int b, int size, bool wrap)
        {
            int d = a - b;
            int abs = d >= 0 ? d : -d;
            if (!wrap || size <= 0) return abs;
            int alt = size - abs;
            return Math.Min(abs, alt);
        }
    }
}

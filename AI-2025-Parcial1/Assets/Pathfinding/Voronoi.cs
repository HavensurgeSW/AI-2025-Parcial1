using System;
using System.Collections.Generic;
using UnityEngine;
using KarplusParcial1.Graph.Core;

namespace KarplusParcial1.Graph.VoronoiAlgorithm
{   
    public static class Voronoi
    {
        public struct LineSegment
        {
            public Vector2 a;
            public Vector2 b;
            public LineSegment(Vector2 a, Vector2 b) { this.a = a; this.b = b; }
        }
         

        public static Dictionary<Vector2Int, Vector2Int> ComputeNearestLookupBruteForce(
            Vector2IntGraph<Node<Vector2Int>> graph,
            IList<Vector2Int> sites,
            Vector2Int mapSize,
            bool wrapWorld)
        {
            var result = new Dictionary<Vector2Int, Vector2Int>();
            if (graph == null || graph.nodes == null || sites == null || sites.Count == 0)
                return result;

            int width = Math.Max(1, mapSize.x);
            int height = Math.Max(1, mapSize.y);

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
                        result[coord] = bestSite;
                }
            }

            return result;
        }
        public static List<LineSegment> GetBisectorSegments(
            IList<Vector2Int> sites,
            Vector2Int mapSize,
            bool wrapWorld,
            float extendMultiplier = 2f)
        {
            var segs = new List<LineSegment>();
            if (sites == null || sites.Count < 2) return segs;

            int width = Math.Max(1, mapSize.x);
            int height = Math.Max(1, mapSize.y);
            float maxExtent = Math.Max(width, height) * extendMultiplier;

            for (int i = 0; i < sites.Count; i++)
            {
                Vector2 a = new Vector2(sites[i].x, sites[i].y);
                for (int j = i + 1; j < sites.Count; j++)
                {
                    Vector2 bOrig = new Vector2(sites[j].x, sites[j].y);

                    if (wrapWorld)
                    {
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            for (int oy = -1; oy <= 1; oy++)
                            {
                                Vector2 b = bOrig + new Vector2(ox * width, oy * height);
                                AddBisectorForPair(a, b, maxExtent, segs);
                            }
                        }
                    }
                    else
                    {
                        AddBisectorForPair(a, bOrig, maxExtent, segs);
                    }
                }
            }

            return segs;
        }

        private static void AddBisectorForPair(Vector2 a, Vector2 b, float maxExtent, List<LineSegment> segs)
        {
            Vector2 v = b - a;
            if (v.sqrMagnitude < 1e-6f) return;
            Vector2 mid = (a + b) * 0.5f;
            Vector2 dir = new Vector2(-v.y, v.x);
            float mag = dir.magnitude;
            if (mag < 1e-6f) return;
            dir /= mag;
            Vector2 p1 = mid + dir * maxExtent;
            Vector2 p2 = mid - dir * maxExtent;
            segs.Add(new LineSegment(p1, p2));
        }

        public static void DrawBisectorsGizmos(
            IList<Vector2Int> sites,
            Vector2Int mapSize,
            bool wrapWorld,
            float tileSpacing,
            Color color)
        {
            if (sites == null || sites.Count < 2) return;
            var segs = GetBisectorSegments(sites, mapSize, wrapWorld, extendMultiplier: 2f);
            Gizmos.color = color;
            foreach (var s in segs)
            {
                Vector3 a = new Vector3(s.a.x * tileSpacing, s.a.y * tileSpacing, 0f);
                Vector3 b = new Vector3(s.b.x * tileSpacing, s.b.y * tileSpacing, 0f);
                Gizmos.DrawLine(a, b);
            }
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
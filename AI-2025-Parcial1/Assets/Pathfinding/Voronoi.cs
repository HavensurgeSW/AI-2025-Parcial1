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

        public static Dictionary<Vector2Int, Vector2Int> ComputeNearestLookupByClipping(
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

            var blocked = new HashSet<Vector2Int>(graph.nodes.Count);
            foreach (var n in graph.nodes) if (n.IsBlocked()) blocked.Add(n.GetCoordinate());

            // Initial clipping rectangle (covering whole map)
            List<Vector2> initialRect = new List<Vector2>(4)
            {
                new Vector2(0f, 0f),
                new Vector2(width, 0f),
                new Vector2(width, height),
                new Vector2(0f, height)
            };

            const float EPS = 1e-6f;

            for (int i = 0; i < sites.Count; i++)
            {
                var s = sites[i];
                Vector2 a = new Vector2(s.x, s.y);

                List<Vector2> poly = new List<Vector2>(initialRect);

                for (int j = 0; j < sites.Count; j++)
                {
                    if (j == i) continue;
                    Vector2 bOrig = new Vector2(sites[j].x, sites[j].y);

                    if (wrapWorld)
                    {
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            for (int oy = -1; oy <= 1; oy++)
                            {
                                Vector2 b = bOrig + new Vector2(ox * width, oy * height);
                                ClipPolygonWithBisector(ref poly, a, b, EPS);
                                if (poly.Count == 0) break;
                            }
                            if (poly.Count == 0) break;
                        }
                    }
                    else
                    {
                        ClipPolygonWithBisector(ref poly, a, bOrig, EPS);
                    }

                    if (poly.Count == 0) break;
                }

                if (poly.Count == 0) continue;

                float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
                foreach (var p in poly)
                {
                    if (p.x < minX) minX = p.x;
                    if (p.x > maxX) maxX = p.x;
                    if (p.y < minY) minY = p.y;
                    if (p.y > maxY) maxY = p.y;
                }

                int ix0 = Mathf.Clamp((int)Mathf.Floor(minX), 0, width - 1);
                int ix1 = Mathf.Clamp((int)Mathf.Ceil(maxX), 0, width - 1);
                int iy0 = Mathf.Clamp((int)Mathf.Floor(minY), 0, height - 1);
                int iy1 = Mathf.Clamp((int)Mathf.Ceil(maxY), 0, height - 1);

                for (int y = iy0; y <= iy1; y++)
                {
                    for (int x = ix0; x <= ix1; x++)
                    {
                        var coord = new Vector2Int(x, y);
                        if (blocked.Contains(coord)) continue;
                        Vector2 pt = new Vector2(x, y);
                        if (IsPointInConvexPolygon(pt, poly, EPS))
                        {                           
                            result[coord] = s;
                        }
                    }
                }
            }

            return result;
        }

      
        private static void ClipPolygonWithBisector(ref List<Vector2> poly, Vector2 a, Vector2 b, float eps)
        {
            if (poly == null || poly.Count == 0) return;

            Vector2 n = b - a; 
            float d = (Vector2.Dot(b, b) - Vector2.Dot(a, a)) * 0.5f;

            List<Vector2> output = new List<Vector2>(poly.Count);
            int count = poly.Count;
            for (int i = 0; i < count; i++)
            {
                Vector2 curr = poly[i];
                Vector2 next = poly[(i + 1) % count];

                bool currInside = Vector2.Dot(curr, n) <= d + eps;
                bool nextInside = Vector2.Dot(next, n) <= d + eps;

                if (currInside && nextInside)
                {
                  
                    output.Add(next);
                }
                else if (currInside && !nextInside)
                {
                   
                    if (TryIntersectSegmentWithLine(curr, next, n, d, out Vector2 inter))
                    {
                        output.Add(inter);
                    }
                }
                else if (!currInside && nextInside)
                {
                    
                    if (TryIntersectSegmentWithLine(curr, next, n, d, out Vector2 inter))
                    {
                        output.Add(inter);
                    }
                    output.Add(next);
                }
            }

            poly = output;
        }


        private static bool TryIntersectSegmentWithLine(Vector2 p0, Vector2 p1, Vector2 n, float d, out Vector2 intersection)
        {
            Vector2 dir = p1 - p0;
            float denom = Vector2.Dot(dir, n);
            if (Mathf.Abs(denom) < 1e-9f)
            {
                intersection = Vector2.zero;
                return false;
            }
            float t = (d - Vector2.Dot(p0, n)) / denom;
            intersection = p0 + dir * t;
            return true;
        }

        private static bool IsPointInConvexPolygon(Vector2 p, List<Vector2> poly, float eps)
        {
            int n = poly.Count;
            if (n == 0) return false;
            float sign = 0f;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = poly[i];
                Vector2 b = poly[(i + 1) % n];
                Vector2 edge = b - a;
                Vector2 vp = p - a;
                float cross = edge.x * vp.y - edge.y * vp.x;
                if (Mathf.Abs(cross) <= eps) continue;
                if (sign == 0f) sign = Mathf.Sign(cross);
                else if (Mathf.Sign(cross) != sign) return false;
            }
            return true;
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
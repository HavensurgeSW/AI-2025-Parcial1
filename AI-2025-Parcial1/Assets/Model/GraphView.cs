using System;
using System.Collections.Generic;
using UnityEngine;
using KarplusParcial1.Management;
using KarplusParcial1.Graph.Core;
using KarplusParcial1.Graph.VoronoiAlgorithm;

namespace KarplusParcial1.Graph
{
    public class GraphView
    {
        public GoldMineManager mineManager = new GoldMineManager();     
        float tileSpacing = 1.0f;

        public Dictionary<Vector2Int, Vector2Int> nearestMineLookup = new Dictionary<Vector2Int, Vector2Int>();
        public bool wrapWorld = false;
        public Vector2Int mapSize = new Vector2Int(0, 0);

        // Para evitar tener que hacer finds, me guardo los SpriteRenderers
        private Dictionary<Vector2Int, SpriteRenderer> tileRenderers = new Dictionary<Vector2Int, SpriteRenderer>(capacity: 0);

        public float TileSpacing { get { return tileSpacing; } }

        public Vector2IntGraph<Node<Vector2Int>> graph;

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

        public Node<Vector2Int> GetNodeAt(Vector2Int coord)
        {
            if (graph == null || graph.nodes == null) return null;
            return graph.nodes.Find(n => n.GetCoordinate().Equals(coord));
        }

        public void AssignMineManager(GoldMineManager gmm) {
            mineManager = gmm;
        }

    }
}
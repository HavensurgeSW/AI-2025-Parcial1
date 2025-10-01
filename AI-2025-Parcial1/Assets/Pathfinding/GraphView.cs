using System;
using System.Collections.Generic;
using UnityEngine;
using KarplusParcial1.Management;
using KarplusParcial1.Graph.Core;
using KarplusParcial1.Graph.VoronoiAlgorithm;

namespace KarplusParcial1.Graph
{
    public class GraphView : MonoBehaviour
    {
        public Vector2IntGraph<Node<Vector2Int>> graph;
        public GoldMineManager mineManager;

        [SerializeField] private GameObject tilePrefab;
        float tileSpacing = 1.0f;

        //diccionario de nodos apuntando a su mina mas cercana
        public Dictionary<Vector2Int, Vector2Int> nearestMineLookup = new Dictionary<Vector2Int, Vector2Int>();
        public bool wrapWorld = false;
        public Vector2Int mapSize = new Vector2Int(0, 0);

        public float TileSpacing { get { return tileSpacing; } }


        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
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
        }
        public void InstantiateTiles()
        {
            foreach (var node in graph.nodes)
            {
                Vector2Int coord = node.GetCoordinate();
                Vector3 pos = new Vector3(coord.x * tileSpacing, coord.y * tileSpacing, 1);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, this.transform);

                tile.name = $"Tile_{coord.x}_{coord.y}";
            }
        }

        public void ColorMines()
        {
            foreach (var node in graph.nodes)
            {
                Vector2Int coord = node.GetCoordinate();


                var child = transform.Find($"Tile_{coord.x}_{coord.y}");
                if (child == null) continue;
                var sr = child.GetComponent<SpriteRenderer>();

                if (mineManager.GetMineAt(coord) != null)
                    sr.color = Color.yellow;
            }
        }

        public void ColorWithVoronoi()
        {
            Voronoi vor = new Voronoi(mineManager, wrapWorld, mapSize);
            vor.ComputeAndColor(graph, this.transform);

            nearestMineLookup.Clear();
            foreach (var kv in vor.nearestMineLookup)
                nearestMineLookup[kv.Key] = kv.Value;
        }
        public void OverlayRoads()
        {
            if (graph == null || graph.nodes == null)
                return;

            foreach (var node in graph.nodes)
            {
                if (!node.IsRoad())
                    continue;

                Vector2Int coord = node.GetCoordinate();
                var child = transform.Find($"Tile_{coord.x}_{coord.y}");
                if (child == null)
                    continue;

                var sr = child.GetComponent<SpriteRenderer>();
                if (sr == null)
                    continue;

                sr.color = Color.grey;
            }
        }


        public void CallExist()
        {
            Debug.Log("GV exists");
        }

        public void AssignMineManager(GoldMineManager gmm)
        {
            mineManager = gmm;
        }
        public void AssignTile(GameObject go)
        {
            tilePrefab = go;
        }
        public void SetSpacing(float s)
        {
            tileSpacing = s;
        }

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
    }
}

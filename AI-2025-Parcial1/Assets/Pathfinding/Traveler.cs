using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TreeEditor.TreeEditorHelper;
using KarplusParcial1.Graph.Core;
using KarplusParcial1.Graph;


namespace KarplusParcial1.Pathfinding
{
    public class Traveler
    {
        public Pathfinder<Node<Vector2Int>> pathfinder;
        private Node<Vector2Int> startNode;
        private Node<Vector2Int> destinationNode;
        public Traveler()
        {
            pathfinder = new AStarPathfinder<Node<Vector2Int>>();
            startNode = new Node<Vector2Int>();
            destinationNode = new Node<Vector2Int>();
        }

        public List<Node<Vector2Int>> FindPath(Node<Vector2Int> start, Node<Vector2Int> destination, GraphView gv)
        {
            return pathfinder.FindPath(start, destination, gv.graph.nodes);
        }
        public bool TryGetNearestMine(Node<Vector2Int> node, out Vector2Int mineCoordinate)
        {
            mineCoordinate = new Vector2Int();
            if (node == null) return false;
            if (node.HasNearestMine())
            {
                mineCoordinate = node.GetNearestMine();
                return true;
            }
            return false;
        }

        public bool TryGetNearestMine(GraphView gv, Vector2Int coord, out Vector2Int mineCoordinate)
        {
            mineCoordinate = default;
            if (gv == null) return false;

            if (gv.nearestMineLookup != null && gv.nearestMineLookup.TryGetValue(coord, out Vector2Int lookup))
            {
                mineCoordinate = lookup;
                return true;
            }
            if (gv.TryGetNearestMineAt(coord, out Vector2Int fromGV))
            {
                mineCoordinate = fromGV;
                return true;
            }

            return false;
        }
    }
}

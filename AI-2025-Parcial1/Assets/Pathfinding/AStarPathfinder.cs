using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using KarplusParcial1.Graph.Core;   

namespace KarplusParcial1.Pathfinding
{
    public class AStarPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode
    {

        private List<NodeType> graphNodes;

        private int gridMinX, gridMinY, gridWidth, gridHeight;

        protected override int Distance(NodeType A, NodeType B)
        {
            if (A is INode<Vector2Int> a && B is INode<Vector2Int> b)
            {
                Vector2Int ca = a.GetCoordinate();
                Vector2Int cb = b.GetCoordinate();

                if (gridWidth > 0 && gridHeight > 0)
                {
                    int dxRaw = Mathf.Abs(ca.x - cb.x);
                    int dyRaw = Mathf.Abs(ca.y - cb.y);

                    int dx = Mathf.Min(dxRaw, gridWidth - dxRaw);
                    int dy = Mathf.Min(dyRaw, gridHeight - dyRaw);

                    return dx + dy;
                }
                //Manhattan
                return Mathf.Abs(ca.x - cb.x) + Mathf.Abs(ca.y - cb.y);
            }

            return 0;
        }

        protected override ICollection<NodeType> GetNeighbors(NodeType node)
        {
            var neighbors = new List<NodeType>();

            if (node == null || graphNodes == null)
                return neighbors;


            if (node is INode<Vector2Int> nodeWithCoord)
            {
                Vector2Int coord = nodeWithCoord.GetCoordinate();

                // 4 direcciones cardinales
                Vector2Int[] deltas = new[]
                {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };

                foreach (var d in deltas)
                {
                    Vector2Int neighborCoord = WrapCoordinate(new Vector2Int(coord.x + d.x, coord.y + d.y));
                    var neighborNode = FindNodeByCoordinate(neighborCoord);
                    if (neighborNode != null)
                        neighbors.Add(neighborNode);
                }
            }

            return neighbors;
        }

        protected override bool IsBlocked(NodeType node)
        {
            return node.IsBlocked();
        }

        protected override int MoveToNeighborCost(NodeType A, NodeType B)
        {
            if (A is INode<Vector2Int> a && B is INode<Vector2Int> b)
            {
                Vector2Int coordA = a.GetCoordinate();
                Vector2Int coordB = b.GetCoordinate();

                if (gridWidth > 0 && gridHeight > 0)
                {
                    int deltaxRaw = Mathf.Abs(coordA.x - coordB.x);
                    int deltayRaw = Mathf.Abs(coordA.y - coordB.y);
                    int dx = Mathf.Min(deltaxRaw, gridWidth - deltaxRaw);
                    int dy = Mathf.Min(deltayRaw, gridHeight - deltayRaw);

                    if (dx + dy == 1) return 1;
                }
            }
            return 1;
        }

        protected override bool NodesEquals(NodeType A, NodeType B)
        {
            if (ReferenceEquals(A, B))
                return true;

            if (A == null || B == null)
                return false;

            if (A is INode<Vector2Int> a && B is INode<Vector2Int> b)
                return a.GetCoordinate() == b.GetCoordinate();

            return EqualityComparer<NodeType>.Default.Equals(A, B);
        }

        public override List<NodeType> FindPath(NodeType startNode, NodeType destinationNode, ICollection<NodeType> graph)
        {
            graphNodes = new List<NodeType>(graph);

            NodeType start = FindOrAddGraphNode(graphNodes, startNode);
            NodeType goal = FindOrAddGraphNode(graphNodes, destinationNode);

            ComputeGridBounds();

            if (NodesEquals(start, goal))
            {
                graphNodes = null;
                return new List<NodeType> { start };
            }

            var openList = new List<NodeType>();
            var closedList = new HashSet<NodeType>();

            var gScore = new Dictionary<NodeType, int>();
            var fScore = new Dictionary<NodeType, int>();
            var parents = new Dictionary<NodeType, NodeType>();

            foreach (var n in graphNodes)
            {
                gScore[n] = int.MaxValue;
                fScore[n] = int.MaxValue;
            }

            gScore[start] = 0;
            fScore[start] = Distance(start, goal);

            openList.Add(start);

            while (openList.Count > 0)
            {

                NodeType current = openList[0];
                int currentIndex = 0;
                for (int i = 1; i < openList.Count; i++)
                {
                    var cand = openList[i];
                    int candF = fScore.ContainsKey(cand) ? fScore[cand] : int.MaxValue;
                    int curF = fScore.ContainsKey(current) ? fScore[current] : int.MaxValue;
                    if (candF < curF)
                    {
                        current = cand;
                        currentIndex = i;
                    }
                }

                openList.RemoveAt(currentIndex);
                closedList.Add(current);

                if (NodesEquals(current, goal))
                {
                    var result = GeneratePath(start, goal, parents);
                    graphNodes = null;
                    return result;
                }

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (neighbor == null)
                        continue;

                    if (!graphNodes.Contains(neighbor))
                        continue;

                    if (IsBlocked(neighbor) || closedList.Contains(neighbor))
                        continue;

                    int currentG = gScore.ContainsKey(current) ? gScore[current] : int.MaxValue;
                    int stepCost = MoveToNeighborCost(current, neighbor);
                    int tentativeG = (currentG == int.MaxValue) ? int.MaxValue : currentG + stepCost;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        parents[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = (tentativeG == int.MaxValue) ? int.MaxValue : tentativeG + Distance(neighbor, goal);

                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                    }
                }
            }

            // No path found
            graphNodes = null;
            return null;
        }

        private NodeType FindOrAddGraphNode(List<NodeType> nodes, NodeType probe)
        {
            if (probe == null)
                return default;

            if (probe is INode<Vector2Int> probeWithCoord)
            {
                Vector2Int coord = probeWithCoord.GetCoordinate();
                foreach (var n in nodes)
                {
                    if (n is INode<Vector2Int> nWithCoord && nWithCoord.GetCoordinate() == coord)
                        return n;
                }
                nodes.Add(probe);
                return probe;
            }

            foreach (var n in nodes)
            {
                if (EqualityComparer<NodeType>.Default.Equals(n, probe))
                    return n;
            }
            nodes.Add(probe);
            return probe;
        }

        private List<NodeType> GeneratePath(NodeType start, NodeType goal, Dictionary<NodeType, NodeType> parents)
        {
            var path = new List<NodeType>();
            var current = goal;

            while (current != null && !NodesEquals(current, start))
            {
                path.Add(current);
                if (!parents.TryGetValue(current, out current))
                {
                    return null;
                }
            }

            if (current != null)
                path.Add(current);

            path.Reverse();
            return path;
        }

        private void ComputeGridBounds()
        {
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            bool found = false;

            foreach (var n in graphNodes)
            {
                if (n is INode<Vector2Int> withCoord)
                {
                    Vector2Int c = withCoord.GetCoordinate();
                    if (c.x < minX) minX = c.x;
                    if (c.x > maxX) maxX = c.x;
                    if (c.y < minY) minY = c.y;
                    if (c.y > maxY) maxY = c.y;
                    found = true;
                }
            }

            if (!found)
            {
                gridMinX = gridMinY = gridWidth = gridHeight = 0;
                return;
            }

            gridMinX = minX;
            gridMinY = minY;
            gridWidth = maxX - minX + 1;
            gridHeight = maxY - minY + 1;

            if (gridWidth < 0) gridWidth = 0;
            if (gridHeight < 0) gridHeight = 0;
        }

        private Vector2Int WrapCoordinate(Vector2Int input)
        {
            int wrappedX = (input.x - gridMinX) % gridWidth;
            if (wrappedX < 0) wrappedX += gridWidth;
            wrappedX += gridMinX;

            int wrappedY = (input.y - gridMinY) % gridHeight;
            if (wrappedY < 0) wrappedY += gridHeight;
            wrappedY += gridMinY;

            return new Vector2Int(wrappedX, wrappedY);
        }

        private NodeType FindNodeByCoordinate(Vector2Int coord)
        {
            foreach (var n in graphNodes)
            {
                if (n is INode<Vector2Int> withCoord && withCoord.GetCoordinate() == coord)
                    return n;
            }
            return default;
        }
    }
}

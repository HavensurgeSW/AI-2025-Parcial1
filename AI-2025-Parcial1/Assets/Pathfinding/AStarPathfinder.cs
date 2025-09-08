using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode
{
    // cached reference to the graph during a FindPath call
    private List<NodeType> graphNodes;

    protected override int Distance(NodeType A, NodeType B)
    {
        // Heuristic: Manhattan distance when nodes expose Vector2Int coordinates,
        // otherwise fall back to 0 (admissible).
        if (A is INode<Vector2Int> a && B is INode<Vector2Int> b)
        {
            Vector2Int ca = a.GetCoordinate();
            Vector2Int cb = b.GetCoordinate();
            return Mathf.Abs(ca.x - cb.x) + Mathf.Abs(ca.y - cb.y);
        }

        return 0;
    }

    protected override ICollection<NodeType> GetNeighbors(NodeType node)
    {
        var neighbors = new List<NodeType>();

        if (node == null || graphNodes == null)
            return neighbors;

        // If nodes expose Vector2Int coordinates, use 4-connected grid neighbors (Manhattan == 1)
        if (node is INode<Vector2Int> nodeWithCoord)
        {
            Vector2Int coord = nodeWithCoord.GetCoordinate();

            foreach (var candidate in graphNodes)
            {
                if (candidate == null || NodesEquals(candidate, node))
                    continue;

                if (!(candidate is INode<Vector2Int> candWithCoord))
                    continue;

                Vector2Int c = candWithCoord.GetCoordinate();
                int dx = Mathf.Abs(c.x - coord.x);
                int dy = Mathf.Abs(c.y - coord.y);

                // 4-connected neighbors
                if (dx + dy == 1)
                    neighbors.Add(candidate);
            }
        }

        // If node doesn't expose coordinates, return empty set (caller can override by subclassing)
        return neighbors;
    }

    protected override bool IsBlocked(NodeType node)
    {
        return node.IsBlocked();
    }

    protected override int MoveToNeighborCost(NodeType A, NodeType B)
    {
        // If nodes have Vector2Int coords, cost 1 for orthogonal, 14 for diagonal.
        if (A is INode<Vector2Int> a && B is INode<Vector2Int> b)
        {
            Vector2Int ca = a.GetCoordinate();
            Vector2Int cb = b.GetCoordinate();
            int dx = Mathf.Abs(ca.x - cb.x);
            int dy = Mathf.Abs(ca.y - cb.y);

            if (dx + dy == 1) return 1;
            if (dx == 1 && dy == 1) return 14;
        }

        // Fallback single-step cost
        return 1;
    }

    protected override bool NodesEquals(NodeType A, NodeType B)
    {
        if (ReferenceEquals(A, B))
            return true;

        if (A == null || B == null)
            return false;

        // If both nodes expose Vector2Int coordinates, compare coordinates.
        if (A is INode<Vector2Int> a && B is INode<Vector2Int> b)
        {
            return a.GetCoordinate() == b.GetCoordinate();
        }

        // Fall back to default equality comparer (may be reference equality if not overridden)
        return EqualityComparer<NodeType>.Default.Equals(A, B);
    }

    public override List<NodeType> FindPath(NodeType startNode, NodeType destinationNode, ICollection<NodeType> graph)
    {
        if (graph == null || startNode == null || destinationNode == null)
            return null;

        // Build a mutable list of nodes (we may add start/destination if they are not present)
        graphNodes = new List<NodeType>(graph);

        // Resolve start/destination to nodes from the graph if possible (match by coordinate).
        NodeType start = FindOrAddGraphNode(graphNodes, startNode);
        NodeType goal = FindOrAddGraphNode(graphNodes, destinationNode);

        // Quick check: start == goal
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

        // Initialize scores
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
            // pick node with lowest fScore
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

        // If node exposes Vector2Int coordinates, try to find a node in the graph with the same coords.
        if (probe is INode<Vector2Int> probeWithCoord)
        {
            Vector2Int coord = probeWithCoord.GetCoordinate();
            foreach (var n in nodes)
            {
                if (n is INode<Vector2Int> nWithCoord && nWithCoord.GetCoordinate() == coord)
                    return n;
            }

            // Not found in graph: add the probe node so algorithm can start/finish at its coordinate.
            nodes.Add(probe);
            return probe;
        }

        // If not coordinate-based, prefer exact reference found in graph
        foreach (var n in nodes)
        {
            if (EqualityComparer<NodeType>.Default.Equals(n, probe))
                return n;
        }

        // Not present: add it
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
                // parent missing -> cannot reconstruct full path
                return null;
            }
        }

        if (current != null)
            path.Add(current);

        path.Reverse();
        return path;
    }
}

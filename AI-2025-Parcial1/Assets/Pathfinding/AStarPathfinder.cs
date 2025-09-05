using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode
{
    private ICollection<NodeType> currentGraph;
    protected override int Distance(NodeType A, NodeType B)
    {
        throw new System.NotImplementedException();
    }

    protected override ICollection<NodeType> GetNeighbors(NodeType node)
    {
        throw new System.NotImplementedException();
    }

    protected override bool IsBlocked(NodeType node)
    {
        return node.IsBlocked();
    }

    protected override int MoveToNeighborCost(NodeType A, NodeType B)
    {
        throw new System.NotImplementedException();
    }

    protected override bool NodesEquals(NodeType A, NodeType B)
    {
        throw new System.NotImplementedException();
    }

    public override List<NodeType> FindPath(NodeType startNode, NodeType destinationNode, ICollection<NodeType> graph)
    {
        throw new System.NotImplementedException();
    }

    private List<NodeType> GeneratePath(NodeType start, NodeType goal, Dictionary<NodeType, NodeType> parents)
    {
        List<NodeType> path = new List<NodeType>();
        NodeType current = goal;

        while (!NodesEquals(current, start))
        {
            path.Add(current);
            current = parents[current];
        }
        path.Add(start);
        path.Reverse();

        return path;
    }
}

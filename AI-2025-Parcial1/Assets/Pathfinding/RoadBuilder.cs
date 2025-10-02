using System.Collections.Generic;
using UnityEngine;
using KarplusParcial1.Graph;
using KarplusParcial1.Graph.Core;

namespace KarplusParcial1.Pathfinding
{
    public static class RoadBuilder
    {

        public static void BuildRoads(GraphView gv, Vector2Int? townhallCoord = null)
        {

            Vector2Int startCoord = townhallCoord ?? new Vector2Int(0, 0);
            var startNode = gv.GetNodeAt(startCoord);
            if (startNode == null)
            {
                Debug.LogWarning($"RoadBuilder: no start node at {startCoord}");
                return;
            }

            foreach (var mine in gv.mineManager.Mines)
            {
                if (mine == null) continue;
                Vector2Int goalCoord = mine.Position;
                var goalNode = gv.GetNodeAt(goalCoord);
                if (goalNode == null)
                {
                    Debug.LogWarning($"RoadBuilder: no node for mine at {goalCoord}, skipping.");
                    continue;
                }
                var queue = new Queue<Node<Vector2Int>>();
                var parents = new Dictionary<Node<Vector2Int>, Node<Vector2Int>>();
                var visited = new HashSet<Node<Vector2Int>>();

                queue.Enqueue(startNode);
                visited.Add(startNode);

                bool found = false;

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    if (current == null) continue;

                    if (current.GetCoordinate() == goalCoord)
                    {
                        found = true;
                        break;
                    }

                    var coord = current.GetCoordinate();
                    var deltas = new[]
                    {
                    new Vector2Int(1,0),
                    new Vector2Int(-1,0),
                    new Vector2Int(0,1),
                    new Vector2Int(0,-1)
                };

                    foreach (var d in deltas)
                    {
                        var neighCoord = new Vector2Int(coord.x + d.x, coord.y + d.y);
                    
                        var neighbor = gv.GetNodeAt(neighCoord);
                        if (neighbor == null) continue;
                        if (neighbor.IsBlocked()) continue;
                        if (visited.Contains(neighbor)) continue;

                        visited.Add(neighbor);
                        parents[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }

                if (!found)
                {
                    Debug.LogWarning($"RoadBuilder: no path found from {startCoord} to mine at {goalCoord}");
                    continue;
                }

                var pathNodes = new List<Node<Vector2Int>>();
                Node<Vector2Int> cursor = goalNode;
                while (cursor != null && cursor != startNode)
                {
                    pathNodes.Add(cursor);
                    if (!parents.TryGetValue(cursor, out cursor))
                    {
                        cursor = null;
                    }
                }
                if (cursor == startNode)
                    pathNodes.Add(startNode);

                pathNodes.Reverse();
                foreach (Node<Vector2Int> n in pathNodes)
                {
                    if (n != null) {
                        n.SetRoad(true);
                    }
                }
            }
        }
    }
}
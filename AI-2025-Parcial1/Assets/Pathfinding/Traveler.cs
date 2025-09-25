using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TreeEditor.TreeEditorHelper;

public class Traveler
{
    private AStarPathfinder<Node<Vector2Int>> pathfinder;
    private Node<Vector2Int> startNode;
    private Node<Vector2Int> destinationNode;
    public Traveler()
    {
        pathfinder = new AStarPathfinder<Node<Vector2Int>>();
        startNode = new Node<Vector2Int>();
        destinationNode = new Node<Vector2Int>();
        {
            //startNode = new Node<Vector2Int>();
            //startNode.SetCoordinate(new Vector2Int(0,0));        
            //destinationNode = new Node<Vector2Int>();
            //GoldMine nearestMine = GV.mineManager.FindNearest(startNode.GetCoordinate());
            //destinationNode.SetCoordinate(new Vector2Int(nearestMine.Position.x, nearestMine.Position.y));
            //List<Node<Vector2Int>> path = pathfinder.FindPath(startNode, destinationNode, GV.graph.nodes);

            //StartCoroutine(Move(path));
        }
    }

    public List<Node<Vector2Int>> FindPath(Node<Vector2Int> start, Node<Vector2Int> destination, GraphView gv) {
        return pathfinder.FindPath(start, destination, gv.graph.nodes);
    }

    public IEnumerator Move(List<Node<Vector2Int>> path) 
    {
        //LEGACY, Ya no se usa.
        foreach (Node<Vector2Int> node in path)
        {
            //transform.position = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
            Debug.Log("Moved to: "+ node.GetCoordinate().x+" "+ node.GetCoordinate().y);
            yield return new WaitForSeconds(1.0f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TreeEditor.TreeEditorHelper;

public class Traveler
{
    public GraphView GV;
    private AStarPathfinder<Node<Vector2Int>> pathfinder;


    private Node<Vector2Int> startNode; 
    private Node<Vector2Int> destinationNode;
    
    

    Traveler()
    {
        pathfinder = new AStarPathfinder<Node<Vector2Int>>();   
        


        //startNode = new Node<Vector2Int>();
        //startNode.SetCoordinate(new Vector2Int(0,0));        
        //destinationNode = new Node<Vector2Int>();
        //GoldMine nearestMine = GV.mineManager.FindNearest(startNode.GetCoordinate());
        //destinationNode.SetCoordinate(new Vector2Int(nearestMine.Position.x, nearestMine.Position.y));
        //List<Node<Vector2Int>> path = pathfinder.FindPath(startNode, destinationNode, GV.graph.nodes);

        //StartCoroutine(Move(path));
    }

    public List<Node<Vector2Int>> FindPath(Node<Vector2Int> start, Node<Vector2Int> destination, GraphView gv) {
        return pathfinder.FindPath(startNode, destinationNode, gv.graph.nodes);
    }

    public IEnumerator Move(List<Node<Vector2Int>> path) 
    {
        foreach (Node<Vector2Int> node in path)
        {
            //transform.position = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
            Debug.Log("Moved to: "+ node.GetCoordinate().x+" "+ node.GetCoordinate().y);
            yield return new WaitForSeconds(1.0f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traveler : MonoBehaviour
{
    public GraphView GV;
    private AStarPathfinder<Node<Vector2Int>> Pathfinder;

    private Node<Vector2Int> startNode; 
    private Node<Vector2Int> destinationNode;
    
    

    void Start()
    {
        Debug.Log("Spawning traveler");
        GV.CallExist();
        GV.mineManager.CallExist();
        Pathfinder = new AStarPathfinder<Node<Vector2Int>>();       

        startNode = new Node<Vector2Int>();
        //startNode.SetCoordinate(new Vector2Int(Random.Range(0, 10), Random.Range(0, 10)));        
        startNode.SetCoordinate(new Vector2Int(0,0));        
        Debug.Log("Start: " + startNode.GetCoordinate());

        destinationNode = new Node<Vector2Int>();
        GoldMine nearestMine = GV.mineManager.FindNearest(startNode.GetCoordinate());
        destinationNode.SetCoordinate(new Vector2Int(nearestMine.Position.x, nearestMine.Position.y));        
        Debug.Log("Destination: " + destinationNode.GetCoordinate());

        List<Node<Vector2Int>> path = Pathfinder.FindPath(startNode, destinationNode, GV.graph.nodes);

        StartCoroutine(Move(path));
    }

    public IEnumerator Move(List<Node<Vector2Int>> path) 
    {
        foreach (Node<Vector2Int> node in path)
        {
            transform.position = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
            Debug.Log("Moved to: "+ node.GetCoordinate().x+" "+ node.GetCoordinate().y);
            yield return new WaitForSeconds(1.0f);
        }
    }
}

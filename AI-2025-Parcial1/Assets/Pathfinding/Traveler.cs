using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traveler : MonoBehaviour
{
    public GraphView graphView;
    private AStarPathfinder<Node<Vector2Int>> Pathfinder;

    private Node<Vector2Int> startNode; 
    private Node<Vector2Int> destinationNode;

    void Start()
    {


        Pathfinder = new AStarPathfinder<Node<Vector2Int>>();

        startNode = new Node<Vector2Int>();
        startNode.SetCoordinate(new Vector2Int(Random.Range(0, 10), Random.Range(0, 10)));
        Debug.Log("Start: " + startNode.GetCoordinate());

        destinationNode = new Node<Vector2Int>();
        destinationNode.SetCoordinate(new Vector2Int(Random.Range(0, 10), Random.Range(0, 10)));
        Debug.Log("Destination: " + destinationNode.GetCoordinate());

        //List<Node<Vector2Int>> path = Pathfinder.FindPath(startNode, destinationNode, grapfView.grapf.nodes);
        List<Node<Vector2Int>> path = Pathfinder.FindPath(startNode, destinationNode, graphView.graph.nodes);
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

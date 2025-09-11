using UnityEngine;

public class GraphView : MonoBehaviour
{
    public Vector2IntGraph<Node<Vector2Int>> graph;
    public GoldMineManager mineManager; // optional, set in inspector or left null

    [SerializeField]private Vector2Int mapDimensions = new Vector2Int(10,10);

    void Awake()
    {
        graph = new Vector2IntGraph<Node<Vector2Int>>(mapDimensions.x, mapDimensions.y);
        if (mineManager == null)
        {
            // create a default manager so GetMineAt calls are safe in Play mode
            mineManager = new GoldMineManager();
            mineManager.CreateMines(3, 1000, new Vector2Int(mapDimensions.x, mapDimensions.y));
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        foreach (Node<Vector2Int> node in graph.nodes)
        {
            // if there's a mine on this node, paint it yellow
            if (mineManager != null && mineManager.GetMineAt(node.GetCoordinate()) != null)
            {
                Gizmos.color = Color.yellow;
            }
            else if (node.IsBlocked())
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;
            
            Gizmos.DrawWireSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), 0.1f);
        }
    }
}

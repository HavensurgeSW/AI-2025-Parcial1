using UnityEngine;

public class GraphView : MonoBehaviour
{
    public Vector2IntGraph<Node<Vector2Int>> graph;
    public GoldMineManager mineManager; // optional, set in inspector or left null
    public Townhall townhall; // optional, set in inspector or left null

    [SerializeField]private Vector2Int mapDimensions = new Vector2Int(10,10);
    [SerializeField]private GameObject tilePrefab;
    //void Awake()
    //{
    //    graph = new Vector2IntGraph<Node<Vector2Int>>(mapDimensions.x, mapDimensions.y);
    //    if (mineManager == null)
    //    {
    //        // create a default manager so GetMineAt calls are safe in Play mode
    //        mineManager = new GoldMineManager();
    //        Debug.Log("Created MineManager");
    //        mineManager.CreateMines(1, 1000, new Vector2Int(mapDimensions.x, mapDimensions.y));
    //        mineManager.mines[0].CallExist();
    //    }
    //    InstantiateTiles();

    //}

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
            
            Gizmos.DrawWireSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y, 1), 0.1f);
        }
    }
    public void InstantiateTiles()
    {
        foreach (var node in graph.nodes)
        {
            Vector2Int coord = node.GetCoordinate();
            GameObject tile = Instantiate(tilePrefab, new Vector3(coord.x, coord.y, 1), Quaternion.identity, this.transform);

          
            var sr = tilePrefab.GetComponent<SpriteRenderer>();
            if (node.IsBlocked())
                sr.color = Color.red;
            else if (mineManager.GetMineAt(coord) != null)
                sr.color = Color.yellow;
            else
                sr.color = Color.green;
        }
    }


    public void CallExist() { 
        Debug.Log("GV exists");
    }
}

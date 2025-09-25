using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector2IntGraph<Node<Vector2Int>> graph;
    public GraphView GV;

    public GoldMineManager MM;
    public Townhall TH;
    
    [SerializeField] Vector2Int mapDimensions = new Vector2Int(10, 10);

    [SerializeField] GameObject miner;
    [SerializeField] GameObject tilePrefab;

    private void Awake()
    {
        graph = new Vector2IntGraph<Node<Vector2Int>>(mapDimensions.x, mapDimensions.y);
        TH = new Townhall(new Vector2Int(0, 0));
        MM = new GoldMineManager();
        MM.CreateMines(1, 1000, new Vector2Int(mapDimensions.x, mapDimensions.y));
        GV.graph = graph;
        GV.AssignMineManager(MM);
        GV.AssignTile(tilePrefab);
        GV.InstantiateTiles();

        Instantiate(miner, new Vector3(0, 0, 0), Quaternion.identity);
        miner.GetComponent<Miner>().GV = GV;
    }

    
}

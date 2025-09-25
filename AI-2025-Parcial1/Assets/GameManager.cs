using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector2IntGraph<Node<Vector2Int>> graph;
    public GraphView GV;

    public GoldMineManager MM;
    public Townhall TH;
    
    [SerializeField] private Vector2Int mapDimensions = new Vector2Int(10, 10);

    private void Awake()
    {
        graph = new Vector2IntGraph<Node<Vector2Int>>(mapDimensions.x, mapDimensions.y);
        TH = new Townhall(new Vector2Int(0, 0));
        MM = new GoldMineManager();
        MM.CreateMines(1, 1000, new Vector2Int(mapDimensions.x, mapDimensions.y));
        GV.InstantiateTiles();
    }

    
}

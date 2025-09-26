using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Vector2IntGraph<Node<Vector2Int>> graph;
    public GraphView GV;

    public GoldMineManager MM;
    public Townhall TH;
    
    [SerializeField] Vector2Int mapDimensions = new Vector2Int(10, 10);

    [SerializeField] GameObject miner;
    [SerializeField] GameObject tilePrefab;

    //UI Inputs
    [SerializeField] TMP_InputField mapX;
    [SerializeField] TMP_InputField mapY;
    [SerializeField] TMP_InputField mineAmount;
    [SerializeField] private Slider tileSpacingSlider;

    private void Awake()
    {
        int x, y, mines;
        if (!int.TryParse(mapX.text, out x))
            x = 10;
        if (!int.TryParse(mapY.text, out y))
            y = 10;
        if (!int.TryParse(mineAmount.text, out mines))
            mines = 1;

        mapDimensions = new Vector2Int(x, y);   

        graph = new Vector2IntGraph<Node<Vector2Int>>(mapDimensions.x, mapDimensions.y);
        TH = new Townhall(new Vector2Int(0, 0));
        MM = new GoldMineManager();
        MM.CreateMines(mines, 1000, new Vector2Int(mapDimensions.x, mapDimensions.y));
        GV.graph = graph;
        GV.AssignMineManager(MM);
        GV.AssignTile(tilePrefab);
        GV.InstantiateTiles();
        GV.ColorWithTerrain();

        Instantiate(miner, new Vector3(0, 0, 0), Quaternion.identity);
        miner.GetComponent<Miner>().GV = GV;
        miner.GetComponent<Miner>().townhall = TH;
        AdjustCameraToGrid();
    }

    private void AdjustCameraToGrid()
    {
        Camera cam = Camera.main;   
        float centerX = (mapDimensions.x - 1) / 2f;
        float centerY = (mapDimensions.y - 1) / 2f;
        cam.transform.position = new Vector3(centerX, centerY, cam.transform.position.z);
    
        float aspect = cam.aspect;
        float sizeX = mapDimensions.x / (2f * aspect);
        float sizeY = mapDimensions.y / 2f;
        cam.orthographicSize = Mathf.Max(sizeX, sizeY) + 1f;
    }


}

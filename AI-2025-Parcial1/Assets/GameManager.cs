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
    [SerializeField] Slider tileSpacingSlider;

    private void Awake()
    {
        int x, y, mines;
        if (!int.TryParse(mapX.text, out x))
            x = 10;
        if (!int.TryParse(mapY.text, out y))
            y = 10;
        if (!int.TryParse(mineAmount.text, out mines))
            mines = 5;      


        mapDimensions = new Vector2Int(x, y);   

        graph = new Vector2IntGraph<Node<Vector2Int>>(mapDimensions.x, mapDimensions.y);
        TH = new Townhall(new Vector2Int(0, 0));
        MM = new GoldMineManager();
        MM.CreateMines(mines, 1000, new Vector2Int(mapDimensions.x, mapDimensions.y));
        GV.graph = graph;
        GV.SetSpacing(tileSpacingSlider.value);
        GV.AssignMineManager(MM);
        GV.AssignTile(tilePrefab);
        GV.InstantiateTiles();
        GV.ColorWithTerrain();
        GV.ColorWithVoronoi();

        if (MM != null && MM.mines != null && MM.mines.Count >= 2)
        {
            SpawnBisectorBetweenMines(0, 1);
            SpawnBisectorBetweenMines(1, 2);
            SpawnBisectorBetweenMines(0, 2);
            SpawnBisectorBetweenMines(0, 3);
            SpawnBisectorBetweenMines(1, 3);
            SpawnBisectorBetweenMines(2, 3);
            SpawnBisectorBetweenMines(3, 4);
            SpawnBisectorBetweenMines(4, 0);
            SpawnBisectorBetweenMines(4, 1);
            SpawnBisectorBetweenMines(4, 2);
        }

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

    public void SpawnBisectorBetweenMines(int indexA, int indexB, float thickness = 0.10f)
    {
        if (MM == null || MM.mines == null) return;
        if (indexA < 0 || indexA >= MM.mines.Count || indexB < 0 || indexB >= MM.mines.Count) return;

        var mineA = MM.mines[indexA];
        var mineB = MM.mines[indexB];
        if (mineA == null || mineB == null) return;

        float spacing = 1.0f;
        if (GV != null) spacing = GV.TileSpacing;

        Vector3 worldA = new Vector3(mineA.Position.x * spacing, mineA.Position.y * spacing, 0f);
        Vector3 worldB = new Vector3(mineB.Position.x * spacing, mineB.Position.y * spacing, 0f);

        Vector3 mid = (worldA + worldB) * 0.5f;
        Vector3 dir = worldB - worldA;
        float dist = dir.magnitude;
        if (dist <= Mathf.Epsilon)
        {
            Debug.LogWarning("SpawnBisectorBetweenMines: mines are at the same position");
            return;
        }

        // Bisector direction is perpendicular to dir
        // Angle for the bisector (in degrees)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;

        // Compute length so the bisector spans the map bounds (safe margin)
        float mapMaxDim = Mathf.Max(mapDimensions.x * spacing, mapDimensions.y * spacing);
        float length = mapMaxDim * 2.5f; // multiplier ensures full coverage even when rotated
        // Optionally ensure length is at least slightly larger than the distance between mines
        length = Mathf.Max(length, dist * 1.2f);

        GameObject bisector = new GameObject($"Bisector_{indexA}_{indexB}");
        bisector.transform.SetParent(transform, true);

        var sr = bisector.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.color = new Color(0.1f, 0.9f, 0.9f, 0.8f);
        sr.sortingOrder = 200; // above tiles and markers

        // Position, rotation and scale
        bisector.transform.position = new Vector3(mid.x, mid.y, 0f);
        bisector.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float worldThickness = thickness * spacing;
        bisector.transform.localScale = new Vector3(length, worldThickness, 1f);
    }
}

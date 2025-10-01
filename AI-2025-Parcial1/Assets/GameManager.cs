using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;

public class GameManager : MonoBehaviour
{
    public Vector2IntGraph<Node<Vector2Int>> graph;
    public GraphView GV;

    public GoldMineManager MM;
    public Townhall TH;

    [SerializeField] Vector2Int mapDimensions = new Vector2Int(10, 10);

    [SerializeField] GameObject miner;
    [SerializeField] GameObject caravan;
    [SerializeField] GameObject tilePrefab;

    //UI Inputs
    [SerializeField] TMP_InputField mapX;
    [SerializeField] TMP_InputField mapY;
    [SerializeField] TMP_InputField mineAmount;
    [SerializeField] Slider tileSpacingSlider;

    // Flag set from MineDepleted event handler (may be raised from background thread).
    private volatile bool voronoiNeedsUpdate = false;

    private void Awake()
    {
        int x, y, mines;
        if (!int.TryParse(mapX.text, out x))
            x = 10;
        if (!int.TryParse(mapY.text, out y))
            y = 10;
        if (!int.TryParse(mineAmount.text, out mines))
            mines = 3;


        mapDimensions = new Vector2Int(x, y);

        graph = new Vector2IntGraph<Node<Vector2Int>>(mapDimensions.x, mapDimensions.y);
        TH = new Townhall(new Vector2Int(0, 0));
        MM = new GoldMineManager();
        MM.CreateMines(mines, 50, new Vector2Int(mapDimensions.x, mapDimensions.y));

        if (MM != null)
            MM.MineDepleted += OnMineDepleted;

        GV.graph = graph;
        GV.SetSpacing(tileSpacingSlider.value);
        GV.AssignMineManager(MM);
        GV.AssignTile(tilePrefab);
        GV.wrapWorld = true;
        GV.mapSize = mapDimensions;
        GV.InstantiateTiles();
        //GV.ColorWithTerrain();
        GV.ColorWithVoronoi();

        if (MM != null && MM.mines != null && MM.mines.Count >= 2)
        {
            //Voronoi.SpawnBisectorBetweenMines(transform, MM, 0, 1, GV.TileSpacing, mapDimensions);
            //Voronoi.SpawnBisectorBetweenMines(transform, MM, 1, 2, GV.TileSpacing, mapDimensions);
            //Voronoi.SpawnBisectorBetweenMines(transform, MM, 0, 2, GV.TileSpacing, mapDimensions);
            //Voronoi.SpawnBisectorBetweenMines(transform, MM, 0, 3, GV.TileSpacing, mapDimensions);
            //Voronoi.SpawnBisectorBetweenMines(transform, MM, 1, 3, GV.TileSpacing, mapDimensions);
            //Voronoi.SpawnBisectorBetweenMines(transform, MM, 2, 3, GV.TileSpacing, mapDimensions);
        }

        AdjustCameraToGrid();
    }

    private void Update()
    {
        if (voronoiNeedsUpdate)
        {
            voronoiNeedsUpdate = false;
            if (GV != null)
                GV.ColorWithVoronoi();
        }
    }

    private void OnDestroy()
    {
        if (MM != null)
            MM.MineDepleted -= OnMineDepleted;
    }

    private void OnMineDepleted(GoldMine mine)
    {
        voronoiNeedsUpdate = true;
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

    public void SpawnMiner() {
        Instantiate(miner, new Vector3(0, 0, 0), Quaternion.identity);
        miner.GetComponent<Miner>().GV = GV;
        miner.GetComponent<Miner>().townhall = TH;
    }

    public void SpawnCaravan() {
        Instantiate(caravan, new Vector3(0, 0, 0), Quaternion.identity);        
        caravan.GetComponent<Caravan>().GV = GV;
        caravan.GetComponent<Caravan>().townhall = TH;
    }


    //DEBUG METHOD
    public void SupplyFoodToMines() { 
        foreach (GoldMine mine in MM.Mines) {
            mine.foodStored = 10;
        }
    }
}

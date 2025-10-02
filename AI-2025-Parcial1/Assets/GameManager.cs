using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KarplusParcial1.Graph;
using KarplusParcial1.RTSElements;
using KarplusParcial1.Pathfinding;
using KarplusParcial1.Graph.Core;


namespace KarplusParcial1.Management
{
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
        [SerializeField] TMP_Text scoreTracker;

        private volatile bool voronoiNeedsUpdate = false;

        private void Awake()
        {
            int x, y, mines;
            if (!int.TryParse(mapX.text, out x))
                x = 20;
            if (!int.TryParse(mapY.text, out y))
                y = 20;
            if (!int.TryParse(mineAmount.text, out mines))
                mines = 4;


            mapDimensions = new Vector2Int(x, y);

            graph = new Vector2IntGraph<Node<Vector2Int>>(mapDimensions.x, mapDimensions.y);
            TH = new Townhall(new Vector2Int(0, 0));
            MM = new GoldMineManager();
            MM.CreateMines(mines, 60, new Vector2Int(mapDimensions.x, mapDimensions.y));

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
            RoadBuilder.BuildRoads(GV, new Vector2Int(0, 0));
            GV.OverlayRoads();
            GV.ColorMines();


            AdjustCameraToGrid();

            SpawnCaravan();
            SpawnMiner();
        }

        private void Update()
        {
            if (voronoiNeedsUpdate)
            {
                voronoiNeedsUpdate = false;
                if (GV != null)
                    GV.ColorWithVoronoi();
            }
            scoreTracker.text = "Gold stored: " + TH.goldStored;
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

        public void SpawnMiner()
        {
            GameObject instance = Instantiate(miner, new Vector3(0, 0, 0), Quaternion.identity);
            var m = instance.GetComponent<Miner>();
            if (m != null)
            {
                m.GV = GV;
                m.townhall = TH;
            }
        }

        public void SpawnCaravan()
        {
            GameObject instance = Instantiate(caravan, new Vector3(0, 0, 0), Quaternion.identity);
            var c = instance.GetComponent<Caravan>();
            if (c != null)
            {
                c.GV = GV;
                c.townhall = TH;
            }
        }


        //DEBUG METHOD
        public void SupplyFoodToMines()
        {
            foreach (GoldMine mine in MM.Mines)
            {
                mine.foodStored = 10;
            }
        }
    }
}

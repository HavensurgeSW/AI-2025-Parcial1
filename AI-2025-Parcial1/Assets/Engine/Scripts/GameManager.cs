using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KarplusParcial1.Graph;
using KarplusParcial1.RTSElements;
using KarplusParcial1.Pathfinding;
using KarplusParcial1.Graph.Core;
using System.Collections.Generic;
using KarplusParcial1.Graph.VoronoiAlgorithm;


namespace KarplusParcial1.Management
{
    public class GameManager : MonoBehaviour
    {
        public Vector2IntGraph<Node<Vector2Int>> graph;
        public GraphView GV;

        public GoldMineManager MM;
        public Townhall TH;
        public AlarmManager AM;

        //EX GRAPH VIEW STUFF
            // Para evitar tener que hacer finds, me guardo los SpriteRenderers
            private Dictionary<Vector2Int, SpriteRenderer> tileRenderers = new Dictionary<Vector2Int, SpriteRenderer>(capacity: 0);
            public Dictionary<Vector2Int, Vector2Int> nearestMineLookup = new Dictionary<Vector2Int, Vector2Int>();
            float tileSpacing = 1.0f;
            public bool wrapWorld = true;


        public float TileSpacing { get { return tileSpacing; } }
        

        [SerializeField] Vector2Int mapDimensions = new Vector2Int(10, 10);

        [SerializeField] GameObject minerPrefab;
        [SerializeField] GameObject caravanPrefab;
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
            AM = new AlarmManager();
            GV = new GraphView();
            GV.AssignMineManager(MM);
            MM.CreateMines(mines, 60, new Vector2Int(mapDimensions.x, mapDimensions.y));

            if (MM != null)
                MM.MineDepleted += OnMineDepleted;

            GV.graph = graph;
            tileSpacing = tileSpacingSlider.value;
            GV.mapSize = mapDimensions;
            //GV.ColorWithTerrain();
            InstantiateTiles();
            ColorWithVoronoi();
            RoadBuilder.BuildRoads(GV, new Vector2Int(0, 0));
            OverlayRoads();
            ColorMines(); 

            AdjustCameraToGrid();

            SpawnMiner();
            //SpawnCaravan();
        }

        private void Update()
        {
            if (voronoiNeedsUpdate)
            {
                voronoiNeedsUpdate = false;
                if (GV != null)
                {
                    ColorWithVoronoi();
                    OverlayRoads();
                    ColorMines();
                }
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
            GameObject instance = Instantiate(minerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            var m = instance.GetComponent<MinerView>();
            if (m != null)
            {
                m.miner.GV = GV;
                m.miner.GV.mineManager = MM;
                m.miner.townhall = TH;
            }
        }

        public void SpawnCaravan()
        {
            GameObject instance = Instantiate(caravanPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            var c = instance.GetComponent<CaravanView>();
            if (c != null)
            {
                c.caravan.GV = GV;
                c.caravan.GV.mineManager = MM;
                c.caravan.townhall = TH;
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
    

        public void InstantiateTiles()
        {
            if (graph == null || graph.nodes == null) return;
            if (tilePrefab == null) { Debug.LogWarning("GraphView.InstantiateTiles: tilePrefab is null."); return; }

            tileRenderers.Clear();
            tileRenderers = new Dictionary<Vector2Int, SpriteRenderer>(graph.nodes.Count);

            foreach (var node in graph.nodes)
            {
                Vector2Int coord = node.GetCoordinate();
                Vector3 pos = new Vector3(coord.x * tileSpacing, coord.y * tileSpacing, 1);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, this.transform);
                tile.name = $"Tile_{coord.x}_{coord.y}";

                var sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    tileRenderers[coord] = sr;
                }
            }
        }
        public void ColorMines()
        {
            if (graph == null || graph.nodes == null) return;
            if (MM == null) return;

            foreach (var node in graph.nodes)
            {
                Vector2Int coord = node.GetCoordinate();
                if (tileRenderers.TryGetValue(coord, out var sr) && sr != null)
                {
                    if (MM.GetMineAt(coord) != null) sr.color = Color.yellow;
                }
                else
                {
                    var child = transform.Find($"Tile_{coord.x}_{coord.y}");
                    if (child == null) continue;
                    var sr2 = child.GetComponent<SpriteRenderer>(); if (sr2 == null) continue;
                    if (MM.GetMineAt(coord) != null) sr2.color = Color.yellow;
                }
            }
        }
        public void ColorWithVoronoi()
        {
            if (graph == null || graph.nodes == null) return;

            if (MM == null)
            {
                nearestMineLookup.Clear();
                return;
            }


            var sites = new List<Vector2Int>();
            foreach (var m in MM.Mines)
            {
                if (m == null) continue;
                if (m.isDepleted) continue;
                sites.Add(m.Position);
            }

            if (sites.Count == 0)
            {
                nearestMineLookup.Clear();
                ColorMines();
                return;
            }

            nearestMineLookup = Voronoi.ComputeNearestLookupByClipping(graph, sites, mapDimensions, wrapWorld);

            int width = Math.Max(1, mapDimensions.x);
            int height = Math.Max(1, mapDimensions.y);
            var blocked = new HashSet<Vector2Int>(graph.nodes.Count);
            foreach (var n in graph.nodes) if (n.IsBlocked()) blocked.Add(n.GetCoordinate());

            foreach (var node in graph.nodes)
            {
                var coord = node.GetCoordinate();
                if (blocked.Contains(coord)) continue;
                if (nearestMineLookup.ContainsKey(coord)) continue;

                long bestDistSq = long.MaxValue;
                Vector2Int bestSite = new Vector2Int(-1, -1);
                for (int i = 0; i < sites.Count; i++)
                {
                    var s = sites[i];
                    int dx = WrappedDelta(coord.x, s.x, width, wrapWorld);
                    int dy = WrappedDelta(coord.y, s.y, height, wrapWorld);
                    long distSq = (long)dx * dx + (long)dy * dy;
                    if (distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        bestSite = s;
                    }
                }

                if (bestSite.x != -1)
                    nearestMineLookup[coord] = bestSite;
            }

            foreach (var node in graph.nodes)
            {
                var coord = node.GetCoordinate();
                if (nearestMineLookup.TryGetValue(coord, out Vector2Int owner))
                {
                    node.SetNearestMine(owner);
                }
                else
                {
                    node.ClearNearestMine();
                }
            }


            var mineColors = new Dictionary<Vector2Int, Color>();
            foreach (var s in sites)
            {
                int key = (s.x * 73856093) ^ (s.y * 19349663);
                int nonNeg = key & 0x7FFFFFFF;
                float hue = (nonNeg % 360) / 360f;
                mineColors[s] = Color.HSVToRGB(hue, 0.6f, 0.95f);
            }

            foreach (var node in graph.nodes)
            {
                Vector2Int coord = node.GetCoordinate();
                SpriteRenderer sr = null;
                tileRenderers.TryGetValue(coord, out sr);

                if (sr == null)
                {
                    var child = transform.Find($"Tile_{coord.x}_{coord.y}");
                    if (child != null) sr = child.GetComponent<SpriteRenderer>();
                }
                if (sr == null) continue;


                if (MM.GetMineAt(coord) != null)
                {
                    sr.color = Color.yellow;
                    continue;
                }

                if (node.IsBlocked())
                {
                    sr.color = Color.red;
                    continue;
                }

                if (nearestMineLookup.TryGetValue(coord, out Vector2Int ownerPos) && mineColors.TryGetValue(ownerPos, out Color c))
                {
                    sr.color = c;
                    continue;
                }

                sr.color = Color.green;
            }
        }
        public void OverlayRoads()
        {
            if (graph == null || graph.nodes == null) return;

            foreach (var node in graph.nodes)
            {
                if (!node.IsRoad()) continue;
                Vector2Int coord = node.GetCoordinate();
                if (tileRenderers.TryGetValue(coord, out var sr) && sr != null)
                {
                    sr.color = Color.grey;
                }
                else
                {
                    var child = transform.Find($"Tile_{coord.x}_{coord.y}");
                    if (child == null) continue;
                    var sr2 = child.GetComponent<SpriteRenderer>(); if (sr2 == null) continue;
                    sr2.color = Color.grey;
                }
            }
        }
        public void SetSpacing(float s) { tileSpacing = s; }             

        private static int WrappedDelta(int a, int b, int size, bool wrap)
        {
            int d = a - b;
            int abs = d >= 0 ? d : -d;
            if (!wrap || size <= 0) return abs;
            int alt = size - abs;
            return Math.Min(abs, alt);
        }
    }


}

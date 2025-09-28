using System.Collections.Generic;
        using UnityEngine;

public class GraphView : MonoBehaviour
{
    public Vector2IntGraph<Node<Vector2Int>> graph;
    public GoldMineManager mineManager;

    [SerializeField] private GameObject tilePrefab;
    float tileSpacing = 1.0f;

    // Map from node coordinate -> closest mine coordinate (set by ColorWithVoronoi)
    public Dictionary<Vector2Int, Vector2Int> nearestMineLookup = new Dictionary<Vector2Int, Vector2Int>();

    public float TileSpacing { get { return tileSpacing; } }


    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        foreach (Node<Vector2Int> node in graph.nodes)
        {
            if (mineManager != null && mineManager.GetMineAt(node.GetCoordinate()) != null)
            {
                Gizmos.color = Color.yellow;
            }
            else if (node.IsBlocked())
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.gray;

            Vector2Int coord = node.GetCoordinate();
            Gizmos.DrawWireSphere(new Vector3(coord.x * tileSpacing, coord.y * tileSpacing, 1), 0.1f);
        }
    }
    public void InstantiateTiles()
    {
        foreach (var node in graph.nodes)
        {
            Vector2Int coord = node.GetCoordinate();
            Vector3 pos = new Vector3(coord.x * tileSpacing, coord.y * tileSpacing, 1);
            GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, this.transform);

            // give tiles stable names so we can find them later
            tile.name = $"Tile_{coord.x}_{coord.y}";

            var sr = tile.GetComponent<SpriteRenderer>();
            if (node.IsBlocked())
                sr.color = Color.red;
            else if (mineManager.GetMineAt(coord) != null)
                sr.color = Color.yellow;
            else
                sr.color = Color.green;
        }
    }

    public void ColorWithTerrain()
    {
        foreach (var node in graph.nodes)
        {
            Vector2Int coord = node.GetCoordinate();

            // find instantiated tile by name (InstantiateTiles sets the name)
            var child = transform.Find($"Tile_{coord.x}_{coord.y}");
            if (child == null) continue;
            var sr = child.GetComponent<SpriteRenderer>();

            if (node.IsBlocked())
                sr.color = Color.red;
            else if (mineManager.GetMineAt(coord) != null)
                sr.color = Color.yellow;
            else
                sr.color = Color.green;
        }
    }

    // Assigns nearest mine coordinate for each node and colors tiles by mine.
    // Uses the bisector logic implicitly: if exactly two mines exist, we
    // compare distance to the two mines (equivalent to splitting by their perpendicular bisector).
    // For more than two mines, each node is assigned to its nearest mine (Euclidean).
    public void ColorWithVoronoi()
    {
        nearestMineLookup.Clear();
        if (mineManager == null || mineManager.mines == null || mineManager.mines.Count == 0)
            return;

        // build deterministic color per mine
        var mineColors = new Dictionary<Vector2Int, Color>();
        foreach (var m in mineManager.mines)
        {
            if (m == null) continue;
            int key = (m.Position.x * 73856093) ^ (m.Position.y * 19349663);
            float hue = (Mathf.Abs(key) % 360) / 360f;
            mineColors[m.Position] = Color.HSVToRGB(hue, 0.6f, 0.95f);
        }

        int mineCount = mineManager.mines.Count;
        for (int i = 0; i < graph.nodes.Count; i++)
        {
            var node = graph.nodes[i];
            Vector2Int coord = node.GetCoordinate();

            // skip blocked nodes from being assigned a mine (optional)
            if (node.IsBlocked())
            {
                // color blocked tile red
                var blockedChild = transform.Find($"Tile_{coord.x}_{coord.y}");
                if (blockedChild != null)
                    blockedChild.GetComponent<SpriteRenderer>().color = Color.red;
                continue;
            }

            // If exactly two mines, use direct comparison between the two (bisector)
            Vector2Int chosenMinePos = default;
            if (mineCount == 1)
            {
                chosenMinePos = mineManager.mines[0].Position;
            }
            else if (mineCount == 2)
            {
                var a = mineManager.mines[0].Position;
                var b = mineManager.mines[1].Position;

                // squared distances
                int dax = coord.x - a.x; int day = coord.y - a.y;
                int dbx = coord.x - b.x; int dby = coord.y - b.y;
                int da2 = dax * dax + day * day;
                int db2 = dbx * dbx + dby * dby;

                chosenMinePos = da2 <= db2 ? a : b;
            }
            else
            {
                // more than two mines: find nearest by Euclidean squared distance
                int bestDist = int.MaxValue;
                foreach (var m in mineManager.mines)
                {
                    if (m == null) continue;
                    int dx = coord.x - m.Position.x;
                    int dy = coord.y - m.Position.y;
                    int d2 = dx * dx + dy * dy;
                    if (d2 < bestDist)
                    {
                        bestDist = d2;
                        chosenMinePos = m.Position;
                    }
                }
            }

            // store mapping
            nearestMineLookup[coord] = chosenMinePos;

            // color the tile: mines themselves remain yellow
            var child = transform.Find($"Tile_{coord.x}_{coord.y}");
            if (child == null) continue;
            var sr = child.GetComponent<SpriteRenderer>();
            if (mineManager.GetMineAt(coord) != null)
            {
                sr.color = Color.yellow;
            }
            else
            {
                // use mine color
                if (mineColors.TryGetValue(chosenMinePos, out Color c))
                    sr.color = c;
                else
                    sr.color = Color.green;
            }
        }
    }


    public void CallExist()
    {
        Debug.Log("GV exists");
    }

    public void AssignMineManager(GoldMineManager gmm)
    {
        mineManager = gmm;
    }
    public void AssignTile(GameObject go)
    {
        tilePrefab = go;
    }
    public void SetSpacing(float s)
    {
        tileSpacing = s;
    }
}

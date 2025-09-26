using UnityEngine;

public class GraphView : MonoBehaviour
{
    public Vector2IntGraph<Node<Vector2Int>> graph;
    public GoldMineManager mineManager;
    
    [SerializeField]private GameObject tilePrefab;
    float tileSpacing = 1.0f;

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

            var sr = tilePrefab.GetComponent<SpriteRenderer>();
            if (node.IsBlocked())
                sr.color = Color.red;
            else if (mineManager.GetMineAt(coord) != null)
                sr.color = Color.yellow;
            else
                sr.color = Color.green;
        }
    }

    public void ColorWithTerrain() {
        foreach (var node in graph.nodes)
        {
            Vector2Int coord = node.GetCoordinate();            

            var sr = tilePrefab.GetComponent<SpriteRenderer>();
            if (node.IsBlocked())
                sr.color = Color.red;
            else if (mineManager.GetMineAt(coord) != null)
                sr.color = Color.yellow;
            else
                sr.color = Color.green;
        }
    }
    public void ColorWithVoronoi() { 
        
    }


    public void CallExist() { 
        Debug.Log("GV exists");
    }

    public void AssignMineManager(GoldMineManager gmm) { 
        mineManager = gmm;
    }
    public void AssignTile(GameObject go) { 
        tilePrefab = go;
    }
    public void SetSpacing(float s) {
        tileSpacing = s;
    }
}

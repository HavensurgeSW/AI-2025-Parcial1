using UnityEngine;

public class GraphView : MonoBehaviour
{
    public Vector2IntGraph<Node<Vector2Int>> graph;
    public GoldMineManager mineManager;
    
    [SerializeField]private GameObject tilePrefab; 

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        foreach (Node<Vector2Int> node in graph.nodes)
        {
            //Si hay una mina, pinta el Gizmo amarillo
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
}

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    public GridManager gridManager;
    public Tilemap tilemap;
    public TileBase pathTile;
    public TileBase startTile;
    public TileBase endTile;
    public GameObject playerPrefab;

    private GameObject playerInstance;
    private Vector3Int startPos;
    private Vector3Int endPos;
    private bool hasStart = false;
    private bool hasEnd = false;
    private bool debugView = false;
    private List<Node> lastPath = new List<Node>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            debugView = !debugView;
            DebugView(debugView);
        }

        if (debugView)
        {
            if (Input.GetMouseButtonDown(0)) 
            {
                Vector3Int clickedTile = GetTileUnderMouse();
                if (clickedTile != Vector3Int.zero)
                {
                    Node node = gridManager.GetNode(clickedTile);
                    if (node != null && node.tileCost == 1)
                    {
                        SetStartPoint(clickedTile);
                    }
                }
            }

            if (Input.GetMouseButtonDown(1)) 
            {
                Vector3Int clickedTile = GetTileUnderMouse();
                if (clickedTile != Vector3Int.zero)
                {
                    Node node = gridManager.GetNode(clickedTile);
                    if (node != null && node.tileCost == 1)
                    {
                        SetEndPoint(clickedTile);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            FindPath();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (lastPath == null || lastPath.Count == 0)
            {
                Debug.Log("No path found. Press 'F' first.");
                return;
            }
            SpawnAndMovePlayer();
        }
    }

    Vector3Int GetTileUnderMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

        if (gridManager.GetNode(cellPosition) != null)
        {
            return cellPosition;
        }
        return Vector3Int.zero;
    }

    void SetStartPoint(Vector3Int position)
    {
        if (hasStart)
        {
            tilemap.SetTile(startPos, gridManager.walkableTile);
        }

        startPos = position;
        hasStart = true;
        tilemap.SetTile(startPos, startTile);
    }

    void SetEndPoint(Vector3Int position)
    {
        if (hasEnd)
        {
            tilemap.SetTile(endPos, gridManager.walkableTile);
        }

        endPos = position;
        hasEnd = true;
        tilemap.SetTile(endPos, endTile);
    }

    public void FindPath()
    {
        if (!hasStart || !hasEnd)
        {
            Debug.Log("Start or End point is not set!");
            return;
        }

        Node startNode = gridManager.GetNode(startPos);
        Node endNode = gridManager.GetNode(endPos);

        if (startNode == null || endNode == null || !startNode.isWalkable || !endNode.isWalkable)
        {
            Debug.Log("Invalid start or end node.");
            return;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);
        startNode.gCost = 0;
        startNode.hCost = GetHeuristic(startNode.position, endNode.position);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                    (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == endNode)
            {
                lastPath = RetracePath(startNode, endNode);
                return;
            }

            foreach (Node neighbor in gridManager.GetNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor) || !neighbor.isWalkable) continue;

                int newGCost = currentNode.gCost + neighbor.tileCost; 

                if (newGCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = GetHeuristic(neighbor.position, endNode.position);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
    }



    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            tilemap.SetTile(currentNode.position, pathTile);

            Debug.Log($"Tile {currentNode.position}: Cost so far = {currentNode.gCost}");

            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    int GetHeuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    void SpawnAndMovePlayer()
    {
        if (lastPath == null || lastPath.Count == 0)
        {
            Debug.LogError("No valid path found! Make sure you press 'F' to generate a path first.");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is not assigned! Please assign it in the Inspector.");
            return;
        }

        if (playerInstance == null)
        {
            Vector3 worldStart = tilemap.GetCellCenterWorld(startPos);
            worldStart.z = -1f;

            playerInstance = Instantiate(playerPrefab, worldStart, Quaternion.identity);

            PlayerMovement playerMovement = playerInstance.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.tilemap = tilemap;
            }
        }

        PlayerMovement movementScript = playerInstance.GetComponent<PlayerMovement>();
        if (movementScript == null)
        {
            Debug.LogError("PlayerMovement script is missing on the Player Prefab!");
            return;
        }

        movementScript.SetPath(lastPath);
        movementScript.StartMoving();
    }

    void DebugView(bool enabled)
    {
        ClearDebugElements(); 

        if (!enabled) return; 

        List<Node> allNodes = gridManager.GetAllNodes(); 

        Debug.Log($"Debug Mode ON: Displaying debug for {allNodes.Count} tiles.");

        foreach (Node node in allNodes)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(node.position);
            DrawTileBorder(worldPos);
            ShowTileCost(worldPos, node.position); 
        }
    }

    void DrawTileBorder(Vector3 worldPos)
    {
        GameObject border = new GameObject($"TileBorder_{worldPos}");
        border.tag = "Debug"; 

        LineRenderer lineRenderer = border.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.yellow;
        lineRenderer.positionCount = 5;
        lineRenderer.sortingOrder = 11; 

        float size = 0.5f;
        Vector3[] points = new Vector3[]
        {
        new Vector3(worldPos.x - size, worldPos.y - size, -0.1f),
        new Vector3(worldPos.x + size, worldPos.y - size, -0.1f),
        new Vector3(worldPos.x + size, worldPos.y + size, -0.1f),
        new Vector3(worldPos.x - size, worldPos.y + size, -0.1f),
        new Vector3(worldPos.x - size, worldPos.y - size, -0.1f)
        };

        lineRenderer.SetPositions(points);

        Debug.Log($"Created TileBorder at {worldPos}"); 
    }

    void ShowTileCost(Vector3 worldPos, Vector3Int tilePos)
    {
        Node node = gridManager.GetNode(tilePos); 

        if (node == null) return; 

        GameObject textObj = new GameObject($"TileCost_{worldPos}");
        textObj.tag = "Debug";
        textObj.transform.position = new Vector3(worldPos.x, worldPos.y, -0.5f);

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        int tileCost = node.tileCost; 
        textMesh.text = tileCost.ToString(); 
        textMesh.fontSize = 10;
        textMesh.characterSize = 0.2f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        textMesh.color = (tileCost == 0) ? Color.red : Color.black;

        if (textObj.GetComponent<MeshRenderer>() == null)
        {
            textObj.AddComponent<MeshRenderer>().sortingOrder = 12;
        }

        Debug.Log($"TileCost Displayed at {tilePos}: {tileCost}"); 
    }





    void ClearDebugElements()
    {
        GameObject[] debugObjects = GameObject.FindGameObjectsWithTag("Debug");
        foreach (GameObject obj in debugObjects)
        {
            Destroy(obj);
        }

        Debug.Log($"Cleared {debugObjects.Length} debug elements.");
    }

}


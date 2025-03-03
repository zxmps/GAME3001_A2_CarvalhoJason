using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase walkableTile;
    public TileBase obstacleTile;
    public int width = 10;
    public int height = 10;
    private Dictionary<Vector3Int, Node> grid = new Dictionary<Vector3Int, Node>();

    void Start()
    {
        GenerateGrid();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateGrid();
        }
    }

    void GenerateGrid()
    {
        tilemap.ClearAllTiles(); //  Clears the old grid

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                bool isWalkable = (Random.value > 0.2f); // 80% walkable, 20% obstacles

                int tileCost = isWalkable ? 1 : 0; //  Passable = 1, Impassable = 0

                Node node = new Node(position, isWalkable, tileCost);
                grid[position] = node;

                tilemap.SetTile(position, isWalkable ? walkableTile : obstacleTile);

                Debug.Log($"Generated Tile at {position} - Walkable: {isWalkable}, Cost: {tileCost}");
            }
        }
    }




    public Node GetNode(Vector3Int position)
    {
        return grid.ContainsKey(position) ? grid[position] : null;
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborPos = node.position + dir;
            if (grid.ContainsKey(neighborPos) && grid[neighborPos].isWalkable)
            {
                neighbors.Add(grid[neighborPos]);
            }
        }
        return neighbors;
    }

    public List<Node> GetAllNodes()
    {
        return new List<Node>(grid.Values);
    }
}

public class Node
{
    public Vector3Int position;
    public bool isWalkable;
    public int tileCost; //  Stores tile cost (1 for walkable, 0 for obstacles)
    public int gCost;
    public int hCost;
    public Node parent;

    public int FCost => gCost + hCost;

    public Node(Vector3Int pos, bool walkable, int cost)
    {
        position = pos;
        isWalkable = walkable;
        tileCost = cost; //  Assign cost when creating the node
    }
}

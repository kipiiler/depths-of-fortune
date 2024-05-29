using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{

    private const int MAX_TRIED = 1000;
    private int width;
    private int height;
    private Vector2 MapStartCoordinate;
    private Vector2 Start;
    private Vector2 End;

    private string[,] map; // 2D array of strings

    private Cell[,] grid;

    private List<Tile> tiles;

    // Path building tiles
    private List<Tile> NorthToSouthTiles;
    private List<Tile> EastToWestTiles;
    private List<Tile> NorthToEastTiles;
    private List<Tile> EastToSouthTiles;
    private List<Tile> SouthToWestTiles;
    private List<Tile> WestToNorthTiles;

    private GameObject[,] tileObjects;

    private Dictionary<string, Dictionary<Tile.EdgeDirection, List<Tile>>> tileRules;
    private Dictionary<string, double> tileWeights;

    private RandomWalkStrategy randomWalkStrategy;

    public MapGenerator(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.Start = new Vector2(0, 0);
        this.End = new Vector2(width - 1, height - 1);
        this.map = new string[width, height];
        this.grid = new Cell[width, height];
        this.randomWalkStrategy = new DFSRandomWalkStrategy(Start, End, map, 0.5f);
        this.tileRules = new Dictionary<string, Dictionary<Tile.EdgeDirection, List<Tile>>>();
        this.tileWeights = new Dictionary<string, double>();
        this.MapStartCoordinate = new Vector2(0, 0);

        // Path building tiles
        this.NorthToEastTiles = new List<Tile>();
        this.EastToSouthTiles = new List<Tile>();
        this.SouthToWestTiles = new List<Tile>();
        this.WestToNorthTiles = new List<Tile>();
        this.NorthToSouthTiles = new List<Tile>();
        this.EastToWestTiles = new List<Tile>();

        // reference to the game objects
        this.tileObjects = new GameObject[width, height];
    }

    public void SetEastToWestTiles(List<Tile> tiles)
    {
        this.EastToWestTiles = tiles;
    }

    public void SetNorthToSouthTiles(List<Tile> tiles)
    {
        this.NorthToSouthTiles = tiles;
    }

    public void SetNorthToEastTiles(List<Tile> tiles)
    {
        this.NorthToEastTiles = tiles;
    }

    public void SetEastToSouthTiles(List<Tile> tiles)
    {
        this.EastToSouthTiles = tiles;
    }


    public void SetSouthToWestTiles(List<Tile> tiles)
    {
        this.SouthToWestTiles = tiles;
    }


    public void SetWestToNorthTiles(List<Tile> tiles)
    {
        this.WestToNorthTiles = tiles;
    }

    public void SetTiles(List<Tile> tiles)
    {
        this.tiles = tiles;
    }

    public void SetMapStartCoordinate(Vector2 coordinate)
    {
        this.MapStartCoordinate = coordinate;
    }

    public void SetRandomWalkStrategy(RandomWalkStrategy strategy)
    {
        this.randomWalkStrategy = strategy;
    }

    public void GenerateMap()
    {
        intitMap();
        List<Vector2> path = RandomWalkFromStartToEnd();
        bool isPathCollapsed = false;
        int count = 0;
        while (!isPathCollapsed && count < MAX_TRIED)
        {
            try
            {
                CollapsePath(path);
                isPathCollapsed = true;
            }
            catch (Exception e)
            {
                path = RandomWalkFromStartToEnd();
                Debug.Log("Attempt " + count + " failed, trying again..." + '\n');
                ClearMap();
                isPathCollapsed = false;
            }
            count++;
        }
        if (count == MAX_TRIED)
        {
            Debug.LogError("Unable to guarantees path is collapsed. Please edit your tile set.");
        }

        Debug.Log("Path Collapsed in " + count + " tries");


        bool isNotCollapseAll = true;
        while (isNotCollapseAll)
        {
            isNotCollapseAll = waveFunctionCollapse();
        }

        Debug.Log("Wave Function Collapse Done");


        DrawMap();
    }

    public string[,] GetMap()
    {
        return map;
    }

    private void intitMap()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                map[i, j] = "*";
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                grid[i, j] = new Cell(this.tiles);
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (i > 0)
                {
                    grid[i, j].AddNeighbor(Tile.EdgeDirection.WEST, grid[i - 1, j]);
                }
                if (i < width - 1)
                {
                    grid[i, j].AddNeighbor(Tile.EdgeDirection.EAST, grid[i + 1, j]);
                }
                if (j > 0)
                {
                    grid[i, j].AddNeighbor(Tile.EdgeDirection.SOUTH, grid[i, j - 1]);
                }
                if (j < height - 1)
                {
                    grid[i, j].AddNeighbor(Tile.EdgeDirection.NORTH, grid[i, j + 1]);
                }
            }
        }

        GenerateTileRules();
        GenerateTileWeights();
    }

    public void GenerateTileRules()
    {
        foreach (Tile tile in tiles)
        {
            tileRules[tile.nameID] = new Dictionary<Tile.EdgeDirection, List<Tile>>();
            tile.GenerateRules(tiles);
            tileRules[tile.nameID] = tile.rules;
        }

        string result = "";

        foreach (KeyValuePair<string, Dictionary<Tile.EdgeDirection, List<Tile>>> entry in tileRules)
        {
            result += entry.Key + ":\n";
            foreach (KeyValuePair<Tile.EdgeDirection, List<Tile>> rule in entry.Value)
            {
                result += rule.Key + ": ";
                foreach (Tile tile in rule.Value)
                {
                    result += tile.nameID + ", ";
                }
                result += "\n";
            }
        }
    }

    public void GenerateTileWeights()
    {
        tileWeights = new Dictionary<string, double>();
        double totalWeight = 0;
        foreach (Tile tile in tiles)
        {
            totalWeight += tile.weight;
        }

        foreach (Tile tile in tiles)
        {
            tileWeights[tile.nameID] = tile.weight / totalWeight;
        }

        string result = "";

        foreach (KeyValuePair<string, double> entry in tileWeights)
        {
            result += entry.Key + ": " + entry.Value + "\n";
        }
        Debug.Log(result);
    }

    private List<Vector2> findSmallestEntropyCells()
    {
        List<Vector2> smallestEntropyCells = new List<Vector2>();
        double minEntropy = double.MaxValue;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j].getEntropy() < minEntropy && grid[i, j].collapsed == false)
                {
                    minEntropy = grid[i, j].getEntropy();
                    smallestEntropyCells.Clear();
                    smallestEntropyCells.Add(new Vector2(i, j));
                }
                else if (grid[i, j].getEntropy() == minEntropy && grid[i, j].collapsed == false)
                {
                    smallestEntropyCells.Add(new Vector2(i, j));
                }
            }
        }
        return smallestEntropyCells;
    }


    public void CollapsePath(List<Vector2> path)
    {
        if (path.Count < 3)
        {
            throw new System.Exception("Path is too short");
        }

        Vector2 startPos = path[0];
        Vector2 nextPos = path[1];

        if (nextPos.x - startPos.x == 1)
        {
            CollapseCell((int)startPos.x, (int)startPos.y, "Start");
        }
        else if (nextPos.x - startPos.x == -1)
        {
            CollapseCell((int)startPos.x, (int)startPos.y, "Start Rotate 2");
        }
        else if (nextPos.y - startPos.y == 1)
        {
            CollapseCell((int)startPos.x, (int)startPos.y, "Start Rotate 3");
        }
        else
        {
            CollapseCell((int)startPos.x, (int)startPos.y, "Start Rotate 1");
        }


        Vector2 endPos = path[path.Count - 1];
        Vector2 prevPos = path[path.Count - 2];
        if (endPos.x - prevPos.x == 1)
        {
            CollapseCell((int)endPos.x, (int)endPos.y, "End Rotate 2");
        }
        else if (endPos.x - prevPos.x == -1)
        {
            CollapseCell((int)endPos.x, (int)endPos.y, "End");
        }
        else if (endPos.y - prevPos.y == 1)
        {
            CollapseCell((int)endPos.x, (int)endPos.y, "End Rotate 1");
        }
        else
        {
            CollapseCell((int)endPos.x, (int)endPos.y, "End Rotate 3");
        }

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2 currentPos = path[i];
            Vector2 prev = path[i - 1];
            Vector2 next = path[i + 1];

            int xDifNext = (int)next.x - (int)currentPos.x;
            int yDifNext = (int)next.y - (int)currentPos.y;

            int xDifPrev = (int)currentPos.x - (int)prev.x;
            int yDifPrev = (int)currentPos.y - (int)prev.y;

            if (xDifNext == 1 && yDifNext == 0 && xDifPrev == 1 && yDifPrev == 0 ||
                xDifNext == -1 && yDifNext == 0 && xDifPrev == -1 && yDifPrev == 0)
            {
                // East to West
                List<string> options = TileListToStringList(EastToWestTiles);
                CollapseCell((int)currentPos.x, (int)currentPos.y, options);
            }

            if (xDifNext == 0 && yDifNext == 1 && xDifPrev == 0 && yDifPrev == 1 ||
                xDifNext == 0 && yDifNext == -1 && xDifPrev == 0 && yDifPrev == -1)
            {
                // North to South
                List<string> options = TileListToStringList(NorthToSouthTiles);
                CollapseCell((int)currentPos.x, (int)currentPos.y, options);
            }

            // If path from prev -> current -> next or next -> current -> prev is a northeast corner
            if ((xDifNext == 1 && yDifNext == 0 && xDifPrev == 0 && yDifPrev == -1) ||
                (xDifNext == 0 && yDifNext == 1 && xDifPrev == -1 && yDifPrev == 0))
            {
                // Debug.Log("Corner: Northeast");
                List<string> options = TileListToStringList(NorthToEastTiles);
                CollapseCell((int)currentPos.x, (int)currentPos.y, options);
            }

            // If path from prev -> current -> next or next -> current -> prev is a southeast corner
            if ((xDifNext == 1 && yDifNext == 0 && xDifPrev == 0 && yDifPrev == 1) ||
                (xDifNext == 0 && yDifNext == -1 && xDifPrev == -1 && yDifPrev == 0))
            {
                // Debug.Log("Corner: Southeast");
                List<string> options = TileListToStringList(EastToSouthTiles);
                CollapseCell((int)currentPos.x, (int)currentPos.y, options);
            }

            // If path from prev -> current -> next or next -> current -> prev is a southwest corner
            if ((xDifNext == 0 && yDifNext == -1 && xDifPrev == 1 && yDifPrev == 0) ||
                (xDifNext == -1 && yDifNext == 0 && xDifPrev == 0 && yDifPrev == 1))
            {
                // Debug.Log("Corner: Southwest");
                List<string> options = TileListToStringList(SouthToWestTiles);
                CollapseCell((int)currentPos.x, (int)currentPos.y, options);
            }

            // If path from prev -> current -> next or next -> current -> prev is a northwest corner
            if ((xDifNext == 0 && yDifNext == 1 && xDifPrev == 1 && yDifPrev == 0) ||
                (xDifNext == -1 && yDifNext == 0 && xDifPrev == 0 && yDifPrev == -1))
            {
                // Debug.Log("Corner: Northwest");
                List<string> options = TileListToStringList(WestToNorthTiles);
                CollapseCell((int)currentPos.x, (int)currentPos.y, options);
            }
        }
    }

    public void CollapseCell(int x, int y, string tileName)
    {
        grid[x, y].Collapse(tileName);
        ConstrainsWaveFunction(new Vector2(x, y));
    }

    public void CollapseCell(int x, int y, List<string> options)
    {
        grid[x, y].Collapse(options, this.tileWeights);
        ConstrainsWaveFunction(new Vector2(x, y));
    }

    public bool waveFunctionCollapse()
    {
        List<Vector2> smallestEntropyCells = findSmallestEntropyCells();
        if (smallestEntropyCells.Count == 0)
        {
            return false;
        }

        Vector2 cell = smallestEntropyCells[UnityEngine.Random.Range(0, smallestEntropyCells.Count)];
        int x = (int)cell.x;
        int y = (int)cell.y;
        grid[x, y].Collapse(this.tileWeights);

        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(cell);
        ConstrainsWaveFunction(cell);
        return true;
    }

    private List<string> TileListToStringList(List<Tile> tiles)
    {
        List<string> result = new List<string>();
        foreach (Tile tile in tiles)
        {
            result.Add(tile.nameID);
        }
        return result;
    }

    /*Broad cast collapse to all affected cell on board*/
    private void ConstrainsWaveFunction(Vector2 cell)
    {
        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(cell);

        while (stack.Count > 0)
        {
            Vector2 current = stack.Pop();
            List<Tile.EdgeDirection> directions = grid[(int)current.x, (int)current.y].GetDirection();
            for (int i = 0; i < directions.Count; i++)
            {
                Tile.EdgeDirection direction = directions[i];
                Cell neighbor = grid[(int)current.x, (int)current.y].GetNeighbor(direction);
                if (neighbor.collapsed == false)
                {
                    bool isReduced = neighbor.ConstrainsOption(grid[(int)current.x, (int)current.y].possibleOptions, direction, tileRules);
                    if (isReduced)
                        stack.Push(new Vector2((int)current.x, (int)current.y));
                }
            }
        }
    }

    public void DrawTile(string tileName, int x, int y)
    {
        Tile tile = tiles.Find(t => t.nameID == tileName);
        GameObject go = tile.DrawTile(new Vector3(MapStartCoordinate.x + x * Map.MODULE_WIDTH + Map.MODULE_OFFSET, Map.MAP_BASE_Y, MapStartCoordinate.y + y * Map.MODULE_WIDTH + Map.MODULE_OFFSET));
        tileObjects[x, y] = go;
    }

    public void DrawMap()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j].collapsed)
                {
                    DrawTile(grid[i, j].chooseOption, i, j);
                }
            }
        }
    }

    private void ClearMap()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                map[i, j] = "*";
                if (tileObjects[i, j] != null)
                {
                    GameObject.Destroy(tileObjects[i, j]);
                }
            }
        }
    }

    private List<Vector2> RandomWalkFromStartToEnd()
    {
        List<Vector2> path = randomWalkStrategy.PerformanceRandomWalk();
        return path;
    }
}

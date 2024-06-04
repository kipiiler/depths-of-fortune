using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO:
// 1. Using abstraction to mark which tile can be use as special tile: empty, start, end
// 2. Using abstraction to mark which tile can be use as path building tile: NorthToSouth, EastToWest, NorthToEast, EastToSouth, SouthToWest, WestToNorth
// 3. Using abstraction rule to define which tile connector is connecting to empty
// 4. After the path is collapsed, keep trying collapsing to make sure every tile is collapsed. If not, re-generate the path (try catch for no possible tile option)
// 5. Marking a border around the map with dead end tile
// 6. Fix current encapsulation issue (public to private)
public class MapGenerator
{

    private const int MAX_TRIED = 5;
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

    public bool GenerateMap(int attempts)
    {
        ClearMap();
        bool failed = true;
        int count = 0;
        while (failed && count < attempts)
        {
            try
            {
                GenerateMap();
                failed = false;
            }
            catch
            {
                Debug.Log("Failed to generate map, trying again");
                ClearMap();
                count++;
            }
        }

        return !failed;
    }

    private void GenerateMap()
    {
        initMap();
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
            catch
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
            Debug.LogError("Unable to guarantees path is collapsed");
            throw new Exception("just retry map generation");
        }

        bool isNotCollapseAll = true;
        while (isNotCollapseAll)
        {
            isNotCollapseAll = waveFunctionCollapse();
        }

        DrawMap();
    }

    public string[,] GetMap()
    {
        return map;
    }

    private void initMap()
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
                List<string> options = TileListToStringList(NorthToEastTiles);
                CollapseCell((int)currentPos.x, (int)currentPos.y, options);
            }

            // If path from prev -> current -> next or next -> current -> prev is a southeast corner
            if ((xDifNext == 1 && yDifNext == 0 && xDifPrev == 0 && yDifPrev == 1) ||
                (xDifNext == 0 && yDifNext == -1 && xDifPrev == -1 && yDifPrev == 0))
            {
                List<string> options = TileListToStringList(EastToSouthTiles);
                CollapseCell((int)currentPos.x, (int)currentPos.y, options);
            }

            // If path from prev -> current -> next or next -> current -> prev is a southwest corner
            if ((xDifNext == 0 && yDifNext == -1 && xDifPrev == 1 && yDifPrev == 0) ||
                (xDifNext == -1 && yDifNext == 0 && xDifPrev == 0 && yDifPrev == 1))
            {
                List<string> options = TileListToStringList(SouthToWestTiles);
                CollapseCell((int)currentPos.x, (int)currentPos.y, options);
            }

            // If path from prev -> current -> next or next -> current -> prev is a northwest corner
            if ((xDifNext == 0 && yDifNext == 1 && xDifPrev == 1 && yDifPrev == 0) ||
                (xDifNext == -1 && yDifNext == 0 && xDifPrev == 0 && yDifPrev == -1))
            {
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

    private bool isTwoCellConnected(string name1, string name2, Tile.EdgeDirection direction)
    {
        // no need to check if one of them is empty
        if (name1 == "Empty" || name2 == "Empty")
        {
            return false;
        }
        Tile tile1 = tiles.Find(t => t.nameID == name1);
        Tile tile2 = tiles.Find(t => t.nameID == name2);
        if (tile1.Edges[(int)direction] == "0")
        {
            return false;
        }
        return tile1.rules[direction].Contains(tile2);
    }

    // Get direction from source to destination tile
    private Tile.EdgeDirection GetDirectionFromTile(Vector2 source, Vector2 destination)
    {
        if (source.x == destination.x)
        {
            if (source.y < destination.y)
            {
                return Tile.EdgeDirection.NORTH;
            }
            else
            {
                return Tile.EdgeDirection.SOUTH;
            }
        }
        else
        {
            if (source.x < destination.x)
            {
                return Tile.EdgeDirection.EAST;
            }
            else
            {
                return Tile.EdgeDirection.WEST;
            }
        }
    }

    // @Warning: This function is only worked after all tile collapsed
    public List<Map.Segment> GetAdjacentMapSegmentList()
    {

        List<Map.Segment> segmentList = new List<Map.Segment>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j].chooseOption == "Empty") continue;
                Map.Segment segment;
                if (grid[i, j].chooseOption == "Start")
                {
                    segment = new Map.Segment(i, j, 0.0f, Map.Segment.Type.START);
                }
                else if (grid[i, j].chooseOption == "End")
                {
                    segment = new Map.Segment(i, j, 0.0f, Map.Segment.Type.END);

                }
                else
                {
                    segment = new Map.Segment(i, j, 0.0f, Map.Segment.Type.NORMAL);
                }
                segmentList.Add(segment);
            }
        }

        // Adding neighbor
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<Vector2> neighbors = new List<Vector2>();
                if (i > 0 && grid[i - 1, j].chooseOption != "Empty")
                {
                    neighbors.Add(new Vector2(i - 1, j));
                }
                if (i < width - 1 && grid[i + 1, j].chooseOption != "Empty")
                {
                    neighbors.Add(new Vector2(i + 1, j));
                }
                if (j > 0 && grid[i, j - 1].chooseOption != "Empty")
                {
                    neighbors.Add(new Vector2(i, j - 1));
                }
                if (j < height - 1 && grid[i, j + 1].chooseOption != "Empty")
                {
                    neighbors.Add(new Vector2(i, j + 1));
                }
                foreach (Vector2 neighbor in neighbors)
                {
                    string name1 = grid[i, j].chooseOption;
                    string name2 = grid[(int)neighbor.x, (int)neighbor.y].chooseOption;
                    if (isTwoCellConnected(name1, name2, GetDirectionFromTile(new Vector2(i, j), neighbor)))
                    {
                        Map.Segment segment1 = segmentList.Find(s => s.x == i && s.z == j);
                        Map.Segment segment2 = segmentList.Find(s => s.x == (int)neighbor.x && s.z == (int)neighbor.y);
                        segment1.BindAdjacent(segment2);
                    }
                }
            }
        }

        return segmentList;
    }
}

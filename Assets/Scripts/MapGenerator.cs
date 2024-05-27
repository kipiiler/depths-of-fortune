using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    private int width;
    private int height;

    private Vector2 Start;
    private Vector2 End;

    private string[,] map; // 2D array of strings

    private Cell[,] grid;

    private List<Tile> tiles;
    private Dictionary<string, Dictionary<Tile.EdgeDirection, List<Tile>>> tileRules;

    private RandomWalkStrategy randomWalkStrategy;

    public MapGenerator(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.Start = new Vector2(0, 0);
        this.End = new Vector2(width - 1, height - 1);
        this.map = new string[width, height];
        this.grid = new Cell[width, height];
        this.randomWalkStrategy = new DFSRandomWalkStrategy(Start, End, map, 0.6f);
        this.tileRules = new Dictionary<string, Dictionary<Tile.EdgeDirection, List<Tile>>>();
    }

    public void SetTiles(List<Tile> tiles)
    {
        this.tiles = tiles;
    }

    public List<Vector2> GenerateMap()
    {
        intitMap();
        // return RandomWalkFromStartToEnd();
        return new List<Vector2>();
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

    public void CollapseCell(int x, int y)
    {
        grid[x, y].Collapse();
        DrawTile(grid[x, y].chooseOption, x, y);
        List<Tile.EdgeDirection> directions = grid[x, y].GetDirection();
        for (int i = 0; i < directions.Count; i++)
        {
            Tile.EdgeDirection direction = directions[i];
            Cell neighbor = grid[x, y].GetNeighbor(direction);
            // neighbor.ConstrainsOption(grid[x, y].possibleOptions, direction, tileRules);
        }
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
        grid[x, y].Collapse();
        DrawTile(grid[x, y].chooseOption, x, y);

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
        return true;
    }

    public void DrawTile(string tileName, int x, int y)
    {
        Tile tile = tiles.Find(t => t.nameID == tileName);
        tile.DrawTile(new Vector3(x * Map.MODULE_WIDTH, 0, y * Map.MODULE_WIDTH));
    }

    private List<Vector2> RandomWalkFromStartToEnd()
    {
        List<Vector2> path = randomWalkStrategy.PerformanceRandomWalk();
        return path;
    }
}

public abstract class RandomWalkStrategy
{
    protected Vector2 start;
    protected Vector2 end;
    protected string[,] map;
    protected int width;
    protected int height;

    public RandomWalkStrategy(Vector2 start, Vector2 end, string[,] map)
    {
        this.start = start;
        this.end = end;
        this.map = map;
        this.width = map.GetLength(0);
        this.height = map.GetLength(1);

    }

    public abstract List<Vector2> PerformanceRandomWalk();
}

public class DFSRandomWalkStrategy : RandomWalkStrategy
{
    private static System.Random random;
    private double temperature;
    private const int MAX_TEMPERATURE = 1000;


    public DFSRandomWalkStrategy(Vector2 start, Vector2 end, string[,] map) : base(start, end, map)
    {
        random = new System.Random();
        this.temperature = 0.5;
    }

    public DFSRandomWalkStrategy(Vector2 start, Vector2 end, string[,] map, double temperature) : base(start, end, map)
    {
        random = new System.Random();
        this.temperature = temperature;
    }

    private static void ShuffleNeighbors(List<Vector2> neighbors)
    {

        int n = neighbors.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + random.Next(n - i);
            (neighbors[r], neighbors[i]) = (neighbors[i], neighbors[r]);
        }
    }

    public override List<Vector2> PerformanceRandomWalk()
    {
        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(start);
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>(); // To store the path
        cameFrom[start] = start;

        while (stack.Count > 0)
        {
            Vector2 cur = stack.Pop();
            map[(int)cur.x, (int)cur.y] = "a";
            if (cur == end)
            {
                break;
            }
            List<Vector2> neighbors = new List<Vector2>();
            if (cur.x > 0 && random.Next(MAX_TEMPERATURE) < MAX_TEMPERATURE * temperature)
            {
                neighbors.Add(new Vector2(cur.x - 1, cur.y));
            }
            if (cur.x < width - 1)
            {
                neighbors.Add(new Vector2(cur.x + 1, cur.y));
            }
            if (cur.y > 0 && random.Next(MAX_TEMPERATURE) < MAX_TEMPERATURE * temperature)
            {
                neighbors.Add(new Vector2(cur.x, cur.y - 1));
            }
            if (cur.y < height - 1)
            {
                neighbors.Add(new Vector2(cur.x, cur.y + 1));
            }
            ShuffleNeighbors(neighbors);
            foreach (Vector2 neighbor in neighbors)
            {
                if (map[(int)neighbor.x, (int)neighbor.y] == "*")
                {
                    stack.Push(neighbor);
                    cameFrom[neighbor] = cur;
                }
            }
        }

        // Reconstruct the path from end to start
        List<Vector2> path = new List<Vector2>();
        if (cameFrom.ContainsKey(end))
        {
            Vector2 current = end;
            while (current != start)
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Add(start);
            path.Reverse();
        }

        return path;
    }
}

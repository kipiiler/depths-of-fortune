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

    private RandomWalkStrategy randomWalkStrategy;

    public MapGenerator(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.Start = new Vector2(0, 0);
        this.End = new Vector2(width - 1, height - 1);
        this.map = new string[width, height];
        this.randomWalkStrategy = new DFSRandomWalkStrategy(Start, End, map, 0.2f);
    }

    public List<Vector2> GenerateMap()
    {
        intitMap();
        return RandomWalkFromStartToEnd();
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
        List<Vector2> path = new List<Vector2>();
        while (stack.Count > 0)
        {
            Vector2 cur = stack.Pop();
            map[(int)cur.x, (int)cur.y] = "a";
            path.Add(cur);
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
                }
            }
        }
        return path;
    }
}

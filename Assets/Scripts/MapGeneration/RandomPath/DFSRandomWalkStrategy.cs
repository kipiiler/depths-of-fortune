using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        string result = "";
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                result += map[i, j];
            }
            result += "\n";
        }
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


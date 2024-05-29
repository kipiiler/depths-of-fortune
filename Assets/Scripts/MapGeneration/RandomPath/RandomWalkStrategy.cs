using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class Map
{
    [NonSerialized]
    public static GameObject monster = null;
    [NonSerialized]
    public static GameObject player = null;

    public const int MAP_WIDTH = 10;
    public const int MAP_HEIGHT = 10;

    [NonSerialized]
    public static int level = 0;
    [NonSerialized]
    public static int treasure = 0;
    [NonSerialized]
    private static float startTime;

    [NonSerialized]
    public static MapGenerator mapGenerator = new MapGenerator(MAP_WIDTH, MAP_HEIGHT);

    public const float MODULE_WIDTH = 20;
    public const float MAP_BASE_Y = 0;
    public const float MAP_FLOOR_HEIGHT = 0.5f;

    [NonSerialized]
    public const float MODULE_OFFSET = MODULE_WIDTH / 2;

    // all the segments in the map
    [NonSerialized]
    public static List<Segment> mapSegments;

    // unity postions to spawn player and monster
    [NonSerialized]
    public static Vector3 playerOrigin;
    [NonSerialized]
    public static Vector3 monsterOrigin;
    [NonSerialized]
    public static Quaternion playerOriginRotation;

    [NonSerialized]
    public static GameObject playerPrefab;
    [NonSerialized]
    public static GameObject monsterPrefab;

    static Map()
    {
        startTime = Time.realtimeSinceStartup;
    }

    public static void AdvanceLevel()
    {
        GenerateMap();

        Sounds.Remove(monster);
        UnityEngine.Object.Destroy(player);
        UnityEngine.Object.Destroy(monster);

        player = UnityEngine.Object.Instantiate(playerPrefab, playerOrigin, playerOriginRotation);
        monster = UnityEngine.Object.Instantiate(monsterPrefab, monsterOrigin, Quaternion.identity);
        Sounds.Add(monster);

        level++;
        player.GetComponentInChildren<FirstPersonController>().ResetMapReveal();
    }

    public static void GenerateMap()
    {
        mapGenerator.GenerateMap(10);
        List<Segment> segments = mapGenerator.GetAdjacentMapSegmentList();
        setMap(segments);
    }

    public static int CalculateScore()
    {
        return (level - 1) * 1000 + treasure * 250 + (int)(Time.realtimeSinceStartup - startTime);
    }

    /**
     * Sets the map using the provided list of segments
     */
    public static void setMap(List<Segment> segments)
    {
        // find player origin and load new map
        mapSegments = segments;
        playerOrigin = new Vector3(MODULE_OFFSET, MAP_BASE_Y + MAP_FLOOR_HEIGHT, MODULE_OFFSET);
        playerOriginRotation = (FindSegment(playerOrigin).Adjacent[0].x != 1) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 90, 0);

        // spawn monster at furthest point from player
        Vector3 far = playerOrigin;
        float dist = 0;
        foreach (Segment s in mapSegments)
        {
            if (s.type == Segment.Type.END) continue;
            Vector3 pos = s.GetUnityPosition();
            float newDist = Vector3.Distance(playerOrigin, pos);
            if (newDist > dist)
            {
                far = pos;
                dist = newDist;
            }
        }
        monsterOrigin = far;
        monsterOrigin.y += MAP_FLOOR_HEIGHT;
    }

    /**
     * Finds the segment positioned at the unity position pos
     * throws ArgumentException if pos is not in a segment
     */
    public static Segment FindSegment(Vector3 pos)
    {
        int x = (int)(pos.x / MODULE_WIDTH);
        int z = (int)(pos.z / MODULE_WIDTH);
        foreach (Segment s in mapSegments)
        {
            if (s.x == x && s.z == z) return s;
        }
        throw new ArgumentException("Position is not in a segment");
    }

    /**
     * Finds the shortest path between the two segments,
     * popping elements from the returned queue will trace the path to follow
     */
    public static Stack<Segment> FindPath(Segment origin, Segment destination)
    {
        Queue<Segment> queued = new Queue<Segment>();
        queued.Enqueue(origin);
        Segment[][] prev = new Segment[MAP_WIDTH][];
        for (int i = 0; i < MAP_WIDTH; i++) prev[i] = new Segment[MAP_HEIGHT];

        while (queued.Count != 0)
        {
            Segment cur = queued.Dequeue();

            if (cur == destination)
            {
                // path found, create return queue
                Stack<Segment> path = new Stack<Segment>();
                while (cur != origin)
                {
                    path.Push(cur);
                    cur = prev[cur.x][cur.z];
                }
                return path;
            }

            // run another round of BFS
            foreach (Segment s in cur.Adjacent)
            {
                if (prev[s.x][s.z] == null)
                {
                    prev[s.x][s.z] = cur;
                    queued.Enqueue(s);
                }
            }
        }
        throw new InvalidOperationException("No path exists");
    }

    /**
     * Class for map segments
     */
    public class Segment
    {
        // adjacent segments
        private List<Segment> adjacent;
        public List<Segment> Adjacent
        {
            get { return adjacent; }
        }

        // integer grid mapping position of this
        public readonly int x, z;

        public readonly Type type;

        /**
         * Creates a new map segment
         * trapIntensity is a deterministic measure of trap deadliness at this tile
         *  trapIntensity is bounded [0, 1], where 0 is easiest (torch) and 1 is a dangerous trap
         * posX and posY are the indices of this in the segment grid
         * throws ArgumentException if posX, posY, trapIntensity are outside of their bounds
         */
        public Segment(int posX, int posZ, float trapIntensity, Type type)
        {
            if (posX < 0 || posX > MAP_WIDTH || posZ < 0 || posZ > MAP_HEIGHT || trapIntensity < 0 || trapIntensity > 1.0)
                throw new ArgumentException("Invalid segment args");
            adjacent = new List<Segment>();
            x = posX;
            z = posZ;
            this.type = type;
        }

        public enum Type
        {
            START, END, NORMAL
        }

        /**
         * Returns the unity position of this
         */
        public Vector3 GetUnityPosition()
        {
            return new Vector3(x * MODULE_WIDTH + MODULE_OFFSET, MAP_BASE_Y, z * MODULE_WIDTH + MODULE_OFFSET);
        }

        /**
         * Connects this segment to the adjacent segment
         * throws ArgumentException if adj cannot be connected to this
         * Only unconnected, adjacent segments may be connected to each other
         */
        public void BindAdjacent(Segment adj)
        {
            if (Math.Abs(x - adj.x) + Math.Abs(z - adj.z) != 1)
            {
                Debug.Log("Binding " + x + " " + z + " to " + adj.x + " " + adj.z);
                throw new ArgumentException("Cannot bind nonadjacent segments");
            }
            if (!adj.adjacent.Contains(this))
            {
                adjacent.Add(adj);
                adj.adjacent.Add(this);
            }
        }
    }
}

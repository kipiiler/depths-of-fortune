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
    public static MapGenerator mapGenerator = new MapGenerator(Map.MAP_WIDTH, Map.MAP_HEIGHT);

    public const float MODULE_WIDTH = 20;
    public const float MAP_BASE_Y = 0;
    public const float MAP_FLOOR_HEIGHT = 0.5f;

    [NonSerialized]
    public const float MODULE_OFFSET = MODULE_WIDTH / 2;

    private static readonly UnityEngine.Object start;
    private static readonly UnityEngine.Object end;
    private static readonly UnityEngine.Object[] deadEnds;
    private static readonly UnityEngine.Object[] corners;
    private static readonly UnityEngine.Object[] straights;
    private static readonly UnityEngine.Object[] threeWays;
    private static readonly UnityEngine.Object[] fourWays;

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

    /**
     * Creates a new map class
     */
    static Map()
    {
        // shouldn't all this be in the mapgenerator constructor?
        List<Tile> allTiles = Tile.CreateListFromPath("Assets/Scripts/MapGeneration/Config/TileData.json");
        List<Tile> NorthToSouthTiles = Tile.CreateListFromPath("Assets/Scripts/MapGeneration/Config/NorthToSouthTileData.json");
        List<Tile> EastToWestTiles = Tile.CreateListFromPath("Assets/Scripts/MapGeneration/Config/EastToWestTileData.json");
        List<Tile> NorthToEastTiles = Tile.CreateListFromPath("Assets/Scripts/MapGeneration/Config/NorthToEastTileData.json");
        List<Tile> EastToSouthTiles = Tile.CreateListFromPath("Assets/Scripts/MapGeneration/Config/EastToSouthTileData.json");
        List<Tile> SouthToWestTiles = Tile.CreateListFromPath("Assets/Scripts/MapGeneration/Config/SouthToWestTileData.json");
        List<Tile> WestToNorthTiles = Tile.CreateListFromPath("Assets/Scripts/MapGeneration/Config/WestToNorthTileData.json");

        mapGenerator.SetTiles(allTiles);
        mapGenerator.SetEastToWestTiles(EastToWestTiles);
        mapGenerator.SetNorthToSouthTiles(NorthToSouthTiles);
        mapGenerator.SetNorthToEastTiles(NorthToEastTiles);
        mapGenerator.SetEastToSouthTiles(EastToSouthTiles);
        mapGenerator.SetSouthToWestTiles(SouthToWestTiles);
        mapGenerator.SetWestToNorthTiles(WestToNorthTiles);

        start = Resources.Load("start");
        end = Resources.Load("end");
        deadEnds = Resources.LoadAll("deadends", typeof(GameObject));
        corners = Resources.LoadAll("corners", typeof(GameObject));
        straights = Resources.LoadAll("straights", typeof(GameObject));
        threeWays = Resources.LoadAll("threeways", typeof(GameObject));
        fourWays = Resources.LoadAll("fourways", typeof(GameObject));
        mapSegments = new List<Segment>();
        playerOrigin = Vector3.zero;
        monsterOrigin = Vector3.zero;
    }

    public static void AdvanceLevel()
    {
        GenerateMap();
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = playerOrigin;
        player.transform.rotation = playerOriginRotation;
        monster.transform.position = monsterOrigin;
        player.GetComponent<CharacterController>().enabled = true;
        level++;
    }

    public static void GenerateMap()
    {
        mapGenerator.GenerateMap(10);
        List<Segment> segments = mapGenerator.GetAdjacentMapSegmentList();
        setMap(segments);
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

        private GameObject instance;

        // severity of trap to be placed here (0 is torch, 1 is max)
        private float trapIntensity;

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
            this.trapIntensity = trapIntensity;
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

        /**
         * Compiles this using the connected segments and displays it to unity
         * Once built, do not use bind()
         * throws InvalidOperationException if this has no adjacent segments
         */
        internal void Build()
        {
            if (this.type != Type.NORMAL && this.adjacent.Count != 1) throw new InvalidOperationException("Start and End must have only one connection");

            // determine necessary connections
            bool up = false, down = false, left = false, right = false;
            foreach (Segment con in adjacent)
            {
                if (con.x == x)
                {
                    if (con.z == z + 1)
                    {
                        up = true;
                    }
                    else
                    {
                        // con.z == z - 1 (implied)
                        down = true;
                    }
                }
                else if (con.x == x + 1)
                {
                    right = true;
                }
                else
                {
                    // con.x == x - 1 (implied)
                    left = true;
                }
            }

            // determine type and rotation
            float rotation;
            UnityEngine.Object[] segmentSource;
            switch (this.adjacent.Count)
            {
                case 0:
                    throw new InvalidOperationException("Unconnected segment");
                case 1:
                    segmentSource = Map.deadEnds;
                    if (up) rotation = 270;
                    else if (down) rotation = 90;
                    else if (left) rotation = 180;
                    else rotation = 0;
                    break;
                case 2:
                    if (up && down)
                    {
                        rotation = 90;
                        segmentSource = Map.straights;
                    }
                    else if (left && right)
                    {
                        rotation = 0;
                        segmentSource = Map.straights;
                    }
                    else if (up && right)
                    {
                        rotation = 0;
                        segmentSource = Map.corners;
                    }
                    else if (right && down)
                    {
                        rotation = 90;
                        segmentSource = Map.corners;
                    }
                    else if (down && left)
                    {
                        rotation = 180;
                        segmentSource = Map.corners;
                    }
                    else if (left && up)
                    {
                        rotation = 270;
                        segmentSource = Map.corners;
                    }
                    else
                    {
                        throw new InvalidProgramException("Missed corner case");
                    }
                    break;
                case 3:
                    segmentSource = Map.threeWays;
                    if (!up) rotation = 180;
                    else if (!down) rotation = 0;
                    else if (!left) rotation = 90;
                    else rotation = 270;
                    break;
                case 4:
                    segmentSource = Map.fourWays;
                    rotation = 0;
                    break;
                default:
                    // should be unreachable due to bind() restrictions
                    throw new InvalidProgramException("Invalid segment connection number");
            }

            // determine the exact type to spawn
            UnityEngine.Object original;
            if (this.type == Type.START)
            {
                original = Map.start;
            }
            else if (this.type == Type.END)
            {
                original = Map.end;
            }
            else
            {
                // select based on trapIntensity
                int sel = (int)Math.Round(trapIntensity * (segmentSource.Length - 1));
                original = segmentSource[sel];
            }

            // spawn the gameobject
            instance = (GameObject)UnityEngine.Object.Instantiate(original, this.GetUnityPosition(), Quaternion.Euler(0, rotation, 0));
        }

        /**
         * Removes this from display
         */
        internal void Release()
        {
            UnityEngine.Object.Destroy(instance);
            instance = null;
        }
    }
}

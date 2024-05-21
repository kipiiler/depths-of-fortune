using System;
using System.Collections.Generic;
using UnityEngine;

public static class Map
{
    public const int MAP_WIDTH = 10;
    public const int MAP_HEIGHT = 10;

    public const float MODULE_WIDTH = 50;
    public const float MAP_BASE_Y = 0;
    public const float MAP_FLOOR_HEIGHT = 0.5f;
    private const float MODULE_OFFSET = MODULE_WIDTH / 2;

    private static readonly UnityEngine.Object start;
    private static readonly UnityEngine.Object end;
    private static readonly UnityEngine.Object[] deadEnds;
    private static readonly UnityEngine.Object[] corners;
    private static readonly UnityEngine.Object[] straights;
    private static readonly UnityEngine.Object[] threeWays;
    private static readonly UnityEngine.Object[] fourWays;

    // all the segments in the map
    public static List<Segment> mapSegments;

    // unity postions to spawn player and monster
    public static Vector3 playerOrigin;
    public static Vector3 monsterOrigin;

    /**
     * Creates a new map class
     */
    static Map()
    {
        start =  Resources.Load("start");
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

    /**
     * Sets the map using the provided list of segments
     */
    public static void setMap(List<Segment> segments)
    {
        // destroy old map
        foreach (Segment s in mapSegments) s.Release();

        // find player origin and load new map
        mapSegments = segments;
        foreach (Segment s in mapSegments)
        {
            s.Build();
            if (s.type == Segment.Type.START) playerOrigin = s.GetUnityPosition();
        }
        playerOrigin.y += MAP_FLOOR_HEIGHT;

        // spawn monster at furthest point from player
        Vector3 far = playerOrigin;
        float dist = 0;
        foreach (Segment s in mapSegments)
        {
            if (s.type == Segment.Type.END) continue;
            Vector3 pos = s.GetUnityPosition();
            float newDist = Vector3.Distance(far, pos);
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
        // TODO
        return null;
    }

    /**
     * Finds the shortest path between the two segments,
     * popping elements from the returned queue will trace the path to follow
     */
    public static Queue<Segment> FindPath(Segment origin, Segment destination)
    {
        // TODO
        return null;
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
                throw new ArgumentException("Cannot bind nonadjacent segments");
            if (adj.adjacent.Contains(this))
                throw new ArgumentException("Cannot rebind segments");
            adjacent.Add(adj);
            adj.adjacent.Add(this);
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
                if (con.x == x) {
                    if (con.z == z + 1) {
                        up = true;
                    } else {
                        // con.z == z - 1 (implied)
                        down = true;
                    }
                } else if (con.x == x + 1) {
                    right = true;
                } else {
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
                    if (up && down) {
                        rotation = 90;
                        segmentSource = Map.straights;
                    } else if (left && right) {
                        rotation = 0;
                        segmentSource = Map.straights;
                    } else if (up && right) {
                        rotation = 0;
                        segmentSource = Map.corners;
                    } else if (right && down) {
                        rotation = 90;
                        segmentSource = Map.corners;
                    } else if (down && left) {
                        rotation = 180;
                        segmentSource = Map.corners;
                    } else if (left && up) {
                        rotation = 270;
                        segmentSource = Map.corners;
                    } else {
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
            if (this.type == Type.START) {
                original = Map.start;
            } else if (this.type == Type.END) {
                original = Map.end;
            } else {
                // select based on trapIntensity
                int sel = (int) Math.Round(trapIntensity * (segmentSource.Length - 1));
                original = segmentSource[sel];
            }

            // spawn the gameobject
            instance = (GameObject) UnityEngine.Object.Instantiate(original, this.GetUnityPosition(), Quaternion.Euler(0, rotation, 0));
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

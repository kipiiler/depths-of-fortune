using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Map
{
    public const int MAP_WIDTH = 0;
    public const int MAP_HEIGHT = 0;

    public const float MODULE_WIDTH = 0;
    public const float MAP_BASE_Y = 0;
    private const float MODULE_OFFSET = MODULE_WIDTH / 2;

    // all the segments in the map
    public List<Segment> mapSegments;

    // unity postions to spawn player and monster
    public Vector3 playerOrigin;
    public Vector3 monsterOrigin;

    /**
     * Creates a new map class
     * Do not call to change map, use setMap() instead!
     */
    public Map()
    {
        // TODO load assets
    }

    /**
     * Sets the map using the provided list of segments
     */
    public void setMap(List<Segment> segments)
    {
        // TODO destroy old map, find player and monster origins, load new map
    }

    /**
     * Finds the segment positioned at the unity position pos
     * throws ArgumentException if pos is not in a segment
     */
    public Segment FindSegment(Vector3 pos)
    {
        // TODO
        return null;
    }

    /**
     * Finds the shortest path between the two segments,
     * popping elements from the returned queue will trace the path to follow
     */
    public Queue<Segment> FindPath(Segment origin, Segment destination)
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
        public List<Segment> adjacent
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
            x = posX;
            z = posZ;
            this.type = type;
            this.trapIntensity = trapIntensity;
        }

        public enum Type
        {
            START, END, NORMAL
        }

        private enum SegmentType
        {
            DEAD_END, STRAIGHT, CORNER, TRI_JUNCTION, QUAD_JUNCTION
        }

        /**
         * Returns the unity position of this
         */
        public Vector3 GetUnityPosition()
        {
            return new Vector3(x * MODULE_WIDTH + MODULE_OFFSET, MAP_BASE_Y, z + MODULE_WIDTH + MODULE_OFFSET);
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
            SegmentType type;
            float rotation;
            switch (this.adjacent.Count)
            {
                case 0:
                    throw new InvalidOperationException("Unconnected segment");
                case 1:
                    type = SegmentType.DEAD_END;
                    if (up) rotation = 0;
                    else if (down) rotation = 180;
                    else if (left) rotation = 270;
                    else rotation = 90;
                    break;
                case 2:
                    if (up && down) {
                        rotation = 90;
                        type = SegmentType.STRAIGHT;
                    } else if (left && right) {
                        rotation = 0;
                        type = SegmentType.STRAIGHT;
                    } else if (up && right) {
                        rotation = 0;
                        type = SegmentType.CORNER;
                    } else if (right && down) {
                        rotation = 90;
                        type = SegmentType.CORNER;
                    } else if (down && left) {
                        rotation = 180;
                        type = SegmentType.CORNER;
                    } else if (left && up) {
                        rotation = 270;
                        type = SegmentType.CORNER;
                    } else {
                        throw new InvalidProgramException("Missed corner case");
                    }
                    break;
                case 3:
                    type = SegmentType.TRI_JUNCTION;
                    if (!up) rotation = 180;
                    else if (!down) rotation = 0;
                    else if (!left) rotation = 90;
                    else rotation = 270;
                    break;
                case 4:
                    type = SegmentType.QUAD_JUNCTION;
                    rotation = 0;
                    break;
                default:
                    // should be unreachable due to bind() restrictions
                    throw new InvalidProgramException("Invalid segment connection number");
            }

            // TODO spawn the gameobject
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

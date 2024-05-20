using System.Collections.Generic;
using UnityEngine;

public class Map
{
    // all the segments in the map
    public List<Segment> mapSegments;

    // unity postions to spawn player and monster
    public Vector3 playerOrigin;
    public Vector3 monsterOrigin;

    public Map()
    {
        // TODO generate map somehow or get called by MapFactory(?)
    }

    // TODO some functions to build and release all the map segments

    /**
     * Finds the segment positioned at the unity position pos
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
        private int x, y;

        // rotation of this
        private Rotation rotation;

        // tile type of this
        private Type type;

        // severity of trap to be placed here (0 is torch, 1 is max)
        private float trapIntensity;

        /**
         * Creates a new map segment
         * trapIntensity is a deterministic measure of trap deadliness at this tile
         *  trapIntensity is bounded [0, 1], where 0 is easiest (torch) and 1 is a dangerous trap
         */
        public Segment(float trapIntensity)
        {
            this.trapIntensity = trapIntensity;
        }

        private enum Rotation
        {
            UP, DOWN, LEFT, RIGHT
        }

        private enum Type
        {
            START, END, STRAIGHT, TURN, DEAD_END, TRI_JUNCTION, QUAD_JUNCTION
        }

        /**
         * Returns the unity position of this
         */
        public Vector3 GetUnityPosition()
        {
            // TODO
            return Vector3.zero;
        }

        /**
         * Connects this segment to the adjacent segment
         * throws ArgumentException if adj cannot be connected to this
         */
        public void BindAdjacent(Segment adj)
        {
            // TODO
            adjacent.Add(adj);
            adj.adjacent.Add(this);
        }

        /**
         * Compiles this using the connected segments and displays it to unity
         * Once built, do not use bind()
         */
        internal void Build()
        {
            // TODO
        }

        /**
         * Removes this from display
         */
        internal void Release()
        {
            // TODO
        }
    }
}

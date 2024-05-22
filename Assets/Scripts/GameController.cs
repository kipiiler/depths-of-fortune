using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Prefabs to instantiate!
    public GameObject player;
    public GameObject monster;

    // Start is called before the first frame update
    void Start()
    {
        // Temporary map generation script
        // TODO replace with map factory generation
        List<Map.Segment> segments = new List<Map.Segment>();
        segments.Add(new Map.Segment(0, 0, 0, Map.Segment.Type.START));
        segments.Add(new Map.Segment(0, 1, 0, Map.Segment.Type.NORMAL));
        segments[0].BindAdjacent(segments[1]);
        segments.Add(new Map.Segment(1, 1, 0, Map.Segment.Type.NORMAL));
        segments[1].BindAdjacent(segments[2]);
        segments.Add(new Map.Segment(1, 0, 0, Map.Segment.Type.NORMAL));
        segments[2].BindAdjacent(segments[3]);
        segments.Add(new Map.Segment(1, 2, 0, Map.Segment.Type.NORMAL));
        segments[2].BindAdjacent(segments[4]);
        segments.Add(new Map.Segment(1, 3, 0, Map.Segment.Type.NORMAL));
        segments[4].BindAdjacent(segments[5]);
        segments.Add(new Map.Segment(2, 3, 0, Map.Segment.Type.END));
        segments[5].BindAdjacent(segments[6]);
        segments.Add(new Map.Segment(0, 3, 0, Map.Segment.Type.NORMAL));
        segments[5].BindAdjacent(segments[7]);
        segments.Add(new Map.Segment(1, 4, 0, Map.Segment.Type.NORMAL));
        segments[5].BindAdjacent(segments[8]);
        segments.Add(new Map.Segment(0, 4, 0, Map.Segment.Type.NORMAL));
        segments[7].BindAdjacent(segments[9]);
        segments[8].BindAdjacent(segments[9]);
        segments.Add(new Map.Segment(0, 5, 0, Map.Segment.Type.NORMAL));
        segments[9].BindAdjacent(segments[10]);
        Map.setMap(segments);

        UnityEngine.Object.Instantiate(player, Map.playerOrigin, Quaternion.identity);
        UnityEngine.Object.Instantiate(monster, Map.monsterOrigin, Quaternion.identity);

        Queue<Map.Segment> path = Map.FindPath(Map.FindSegment(Map.playerOrigin), Map.FindSegment(Map.monsterOrigin));
        while (path.Count > 0)
        {
            Debug.Log(path.Peek().x.ToString() + ", " + path.Dequeue().z.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

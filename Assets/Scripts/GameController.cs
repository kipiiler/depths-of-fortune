using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Prefabs to instantiate!
    public GameObject playerPrefab;
    public GameObject monsterPrefab;

    private GameObject monster;

    // Monster to player path
    Stack<Map.Segment> path;

    // Start is called before the first frame update
    void Start()
    {
        // Temporary map generation script
        // TODO replace with map factory generation
        List<Map.Segment> segments = new List<Map.Segment>();
        segments.Add(new Map.Segment(0, 0, 0, Map.Segment.Type.START));
        segments.Add(new Map.Segment(0, 1, 1f, Map.Segment.Type.NORMAL));
        segments[0].BindAdjacent(segments[1]);
        segments.Add(new Map.Segment(1, 1, 0, Map.Segment.Type.NORMAL));
        segments[1].BindAdjacent(segments[2]);
        segments.Add(new Map.Segment(1, 0, 0, Map.Segment.Type.NORMAL));
        segments[2].BindAdjacent(segments[3]);
        segments.Add(new Map.Segment(1, 2, 0, Map.Segment.Type.NORMAL));
        segments[2].BindAdjacent(segments[4]);
        segments.Add(new Map.Segment(1, 3, 1, Map.Segment.Type.NORMAL));
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

        UnityEngine.Object.Instantiate(playerPrefab, Map.playerOrigin, Quaternion.identity);
        monster = UnityEngine.Object.Instantiate(monsterPrefab, Map.monsterOrigin, Quaternion.identity);

        path = Map.FindPath(Map.FindSegment(Map.monsterOrigin), Map.FindSegment(Map.playerOrigin));
    }

    // Update is called once per frame
    void Update()
    {
        // Provide the monster new setpoints to the player
        if (path.Count > 1) {
            // Check if monster has hit the setpoint
            if (Vector3.Distance(monster.transform.position, path.Peek().GetUnityPosition()) < 0.4) {
                Debug.Log("Monster reached the setpoint, queue-ing a new one");
                path.Pop();
            }

            monster.GetComponent<MonsterBehavior>().UpdateSetpoint(path.Peek().GetUnityPosition());
        }
    }
}

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
        MapGenerator mapGenerator = new MapGenerator(Map.MAP_WIDTH, Map.MAP_HEIGHT);
        List<Vector2> paths = mapGenerator.GenerateMap();
        string[,] map = mapGenerator.GetMap();
        // print this out in grid form
        string mapString = "";
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                mapString += map[i, j];
            }
            mapString += "\n";
        }
        Debug.Log(mapString);
        // Temporary map generation script
        // TODO replace with map factory generation
        List<Map.Segment> segments = new List<Map.Segment>();
        for (int i = 0; i < paths.Count; i++)
        {
            if (i == 0)
            {
                segments.Add(new Map.Segment((int)paths[i].x, (int)paths[i].y, 0, Map.Segment.Type.START));
                continue;
            }
            if (i == paths.Count - 1)
            {
                segments.Add(new Map.Segment((int)paths[i].x, (int)paths[i].y, 0, Map.Segment.Type.END));
                segments[i - 1].BindAdjacent(segments[i]);
                continue;
            }
            segments.Add(new Map.Segment((int)paths[i].x, (int)paths[i].y, 0, Map.Segment.Type.NORMAL));
            segments[i - 1].BindAdjacent(segments[i]);
        }
        Map.setMap(segments);


        UnityEngine.Object.Instantiate(playerPrefab, Map.playerOrigin, Quaternion.identity);
        monster = UnityEngine.Object.Instantiate(monsterPrefab, Map.monsterOrigin, Quaternion.identity);

        path = Map.FindPath(Map.FindSegment(Map.monsterOrigin), Map.FindSegment(Map.playerOrigin));
    }

    // Update is called once per frame
    void Update()
    {
        // Provide the monster new setpoints to the player
        if (path.Count > 1)
        {
            // Check if monster has hit the setpoint
            if (Vector3.Distance(monster.transform.position, path.Peek().GetUnityPosition()) < 0.4)
            {
                Debug.Log("Monster reached the setpoint, queue-ing a new one");
                path.Pop();
            }

            monster.GetComponent<MonsterBehavior>().UpdateSetpoint(path.Peek().GetUnityPosition());
        }
    }
}

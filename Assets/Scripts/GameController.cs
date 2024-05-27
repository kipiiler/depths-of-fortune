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
        // List<Vector2> paths = mapGenerator.GenerateMap();
        // string[,] map = mapGenerator.GetMap();
        // // print this out in grid form
        // string mapString = "";
        // for (int i = 0; i < map.GetLength(0); i++)
        // {
        //     for (int j = 0; j < map.GetLength(1); j++)
        //     {
        //         mapString += map[i, j];
        //     }
        //     mapString += "\n";
        // }
        // Debug.Log(mapString);
        // Temporary map generation script
        // TODO replace with map factory generation
        // List<Map.Segment> segments = new List<Map.Segment>();
        // for (int i = 0; i < paths.Count; i++)
        // {
        //     if (i == 0)
        //     {
        //         segments.Add(new Map.Segment((int)paths[i].x, (int)paths[i].y, 0, Map.Segment.Type.START));
        //         continue;
        //     }
        //     if (i == paths.Count - 1)
        //     {
        //         segments.Add(new Map.Segment((int)paths[i].x, (int)paths[i].y, 0, Map.Segment.Type.END));
        //         segments[i - 1].BindAdjacent(segments[i]);
        //         continue;
        //     }
        //     segments.Add(new Map.Segment((int)paths[i].x, (int)paths[i].y, 0, Map.Segment.Type.NORMAL));
        //     segments[i - 1].BindAdjacent(segments[i]);
        // }
        // Map.setMap(segments);




        // Tile tile = new Tile("start", new string[] { "0", "1", "0", "0" }, "Start");
        Tile tile1 = new Tile("empty", new string[] { "0", "0", "0", "0" }, "Empty");
        Tile tile2 = new Tile("threeways/threeway1", new string[] { "1", "1", "0", "1" }, "Threeway original");
        Tile tile3 = new Tile("corners/corner1", new string[] { "1", "1", "0", "0" }, "Corner original");
        Tile tiler4 = tile3.Rotate(1, "Corner Rotate 1");
        Tile tiler5 = tile3.Rotate(2, "Corner Rotate 2");
        Tile tiler6 = tile3.Rotate(3, "Corner Rotate 3");
        Tile tile4 = tile2.Rotate(1, "Rotate 1");
        Tile tile5 = tile2.Rotate(2, "Rotate 2");
        Tile tile6 = tile2.Rotate(3, "Rotate 3");
        Tile tile7 = new Tile("fourways/fourway2", new string[] { "1", "1", "1", "1" }, "4some");
        mapGenerator.SetTiles(new List<Tile> { tile7, tile1, tile3, tile4, tile5, tile6, tile2, tiler4, tiler5, tiler6 });
        List<Vector2> paths = mapGenerator.GenerateMap();
        // // mapGenerator.CollapseCell(0, 0);
        bool running = true;
        while (running)
        {
            running = mapGenerator.waveFunctionCollapse();
        }
        // mapGenerator.GenerateTileRules();

        // Tile[] tiles = new Tile[] { tile, tile4, tile3, tile5, tile6 };
        // tile.GenerateRules(tiles);
        // tile.PrintRules();

        // tile.DrawTile(new Vector3(0, 0, 0));
        // tile2.DrawTile(new Vector3(Map.MODULE_WIDTH, 0, 0));

        // print edge of tile3
        // string result = "";
        // result += tiler4.nameID + " edges: ";
        // foreach (string tile in tiler4.Edges)
        // {
        //     result += tile + ", ";
        // }
        // Debug.Log(result);

        // tile3.DrawTile(new Vector3(Map.MODULE_WIDTH * 2, 0, 0));
        // tiler4.DrawTile(new Vector3(Map.MODULE_WIDTH * 3, 0, 0));
        // tiler5.DrawTile(new Vector3(Map.MODULE_WIDTH * 4, 0, 0));
        // tiler6.DrawTile(new Vector3(Map.MODULE_WIDTH * 5, 0, 0));

        // UnityEngine.Object.Instantiate(playerPrefab, Map.playerOrigin, Quaternion.identity);
        // monster = UnityEngine.Object.Instantiate(monsterPrefab, Map.monsterOrigin, Quaternion.identity);

        // path = Map.FindPath(Map.FindSegment(Map.monsterOrigin), Map.FindSegment(Map.playerOrigin));
    }

    // Update is called once per frame
    void Update()
    {
        // // Provide the monster new setpoints to the player
        // if (path.Count > 1)
        // {
        //     // Check if monster has hit the setpoint
        //     if (Vector3.Distance(monster.transform.position, path.Peek().GetUnityPosition()) < 0.4)
        //     {
        //         Debug.Log("Monster reached the setpoint, queue-ing a new one");
        //         path.Pop();
        //     }

        //     monster.GetComponent<MonsterBehavior>().UpdateSetpoint(path.Peek().GetUnityPosition());
        // }
    }
}

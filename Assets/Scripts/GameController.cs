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
        // Temporary map generation script
        // TODO replace with map factory generation

        Tile startTile = new Tile("start", new string[] { "0", "1", "0", "0" }, "Start");
        Tile startTile1 = startTile.Rotate(1, "Start Rotate 1");
        Tile startTile2 = startTile.Rotate(2, "Start Rotate 2");
        Tile startTile3 = startTile.Rotate(3, "Start Rotate 3");

        Tile endTile = new Tile("end", new string[] { "0", "1", "0", "0" }, "End");
        Tile endTile1 = endTile.Rotate(1, "End Rotate 1");
        Tile endTile2 = endTile.Rotate(2, "End Rotate 2");
        Tile endTile3 = endTile.Rotate(3, "End Rotate 3");


        Tile tile1 = new Tile("empty", new string[] { "0", "0", "0", "0" }, "Empty");

        Tile straightTile = new Tile("straights/straight1", new string[] { "0", "1", "0", "1" }, "Straight original");
        Tile straightTile1 = straightTile.Rotate(1, "Straight Rotate 1");

        Tile tile3 = new Tile("corners/corner1", new string[] { "1", "1", "0", "0" }, "Corner original");
        Tile tiler4 = tile3.Rotate(1, "Corner Rotate 1");
        Tile tiler5 = tile3.Rotate(2, "Corner Rotate 2");
        Tile tiler6 = tile3.Rotate(3, "Corner Rotate 3");

        Tile threeTile = new Tile("threeways/threeway1", new string[] { "1", "1", "0", "1" }, "Threeway original");
        Tile tile4 = threeTile.Rotate(1, "Threeway Rotate 1");
        Tile tile5 = threeTile.Rotate(2, "Threeway Rotate 2");
        Tile tile6 = threeTile.Rotate(3, "Threeway Rotate 3");

        Tile tile7 = new Tile("fourways/fourway2", new string[] { "1", "1", "1", "1" }, "4some");
        Tile tile8 = new Tile("fourways/fourway1", new string[] { "1", "1", "1", "1" }, "4some1");
        mapGenerator.SetTiles(new List<Tile> { straightTile, straightTile1, startTile, startTile1, startTile2, startTile3, endTile, endTile1, endTile2, endTile3, tile8, tile7, tile1, tile3, tile4, tile5, tile6, threeTile, tiler4, tiler5, tiler6 });
        List<Tile> EastToWestTiles = new List<Tile> { straightTile, threeTile, tile5, tile7, tile8 };
        List<Tile> NorthToSouthTiles = new List<Tile> { straightTile1, tile4, tile6, tile7, tile8 };
        List<Tile> NorthToEastTiles = new List<Tile> { tile3, tile8 };
        List<Tile> EastToSouthTiles = new List<Tile> { tiler4, tile8 };
        List<Tile> SouthToWestTiles = new List<Tile> { tiler5, tile8 };
        List<Tile> WestToNorthTiles = new List<Tile> { tiler6, tile8 };
        mapGenerator.SetEastToWestTiles(EastToWestTiles);
        mapGenerator.SetNorthToSouthTiles(NorthToSouthTiles);
        mapGenerator.SetNorthToEastTiles(NorthToEastTiles);
        mapGenerator.SetEastToSouthTiles(EastToSouthTiles);
        mapGenerator.SetSouthToWestTiles(SouthToWestTiles);
        mapGenerator.SetWestToNorthTiles(WestToNorthTiles);

        mapGenerator.GenerateMap();

        UnityEngine.Object.Instantiate(playerPrefab, new Vector3(Map.MODULE_OFFSET, Map.MAP_FLOOR_HEIGHT, Map.MODULE_OFFSET), Quaternion.identity);
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

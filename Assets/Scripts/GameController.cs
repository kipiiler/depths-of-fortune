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

        mapGenerator.GenerateMap(2);

        List<Map.Segment> segments = mapGenerator.GetAdjacentMapSegmentList();
        Map.setMap(segments);

        UnityEngine.Object.Instantiate(playerPrefab, new Vector3(Map.MODULE_OFFSET, Map.MAP_FLOOR_HEIGHT, Map.MODULE_OFFSET), Quaternion.identity);
        monster = UnityEngine.Object.Instantiate(monsterPrefab, Map.monsterOrigin, Quaternion.identity);

        path = Map.FindPath(Map.FindSegment(Map.monsterOrigin), Map.FindSegment(Map.playerOrigin));

        // Add monster as hearer
        Sounds.Add(monster);
    }

    // Update is called once per frame
    void Update()
    {
        // // Provide the monster new setpoints to the player
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

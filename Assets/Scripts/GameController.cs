using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Prefabs to instantiate!
    public GameObject playerPrefab;
    public GameObject monsterPrefab;

    private GameObject player;
    private GameObject monster;
    private GameObject mapObject;

    private bool toggleMap = false;

    // Start is called before the first frame update
    void Start()
    {
        // find the map object
        mapObject = GameObject.Find("MapContainer");
        if (mapObject != null)
        {
            mapObject.SetActive(toggleMap);
        }
        else
        {
            Debug.LogError("MapContainer object not found");
        }

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

        player = UnityEngine.Object.Instantiate(playerPrefab, new Vector3(Map.MODULE_OFFSET, Map.MAP_FLOOR_HEIGHT, Map.MODULE_OFFSET), Quaternion.identity);
        monster = UnityEngine.Object.Instantiate(monsterPrefab, Map.monsterOrigin, Quaternion.identity);


        // Add monster as hearer
        Sounds.Add(monster);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("M key was pressed");
            toggleMap = !toggleMap;
            mapObject.SetActive(toggleMap);
        }
        if (monster.GetComponent<MonsterBehavior>().CurrentState == MonsterBehavior.MonsterState.Aggressive)
        {
            monster.GetComponent<MonsterBehavior>().playerPosition = player.transform.position;
        }
    }
}

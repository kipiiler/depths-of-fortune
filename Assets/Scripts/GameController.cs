using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Prefabs to instantiate!
    public GameObject playerPrefab;
    public GameObject monsterPrefab;

    private GameObject mapObject;

    private bool toggleMap = false;

    // Start is called before the first frame update
    void Start()
    {
        mapObject = GameObject.Find("MapContainer");
        if (mapObject != null)
        {
            mapObject.SetActive(toggleMap);
        }
        else
        {
            Debug.LogError("MapContainer object not found");
        }

        Map.playerPrefab = playerPrefab;
        Map.monsterPrefab = monsterPrefab;

        Map.AdvanceLevel();

        Map.monster.GetComponent<MonsterBehavior>().player = Map.player.GetComponent<FirstPersonController>();

        Sounds.Add(Map.monster);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            toggleMap = !toggleMap;
            mapObject.SetActive(toggleMap);
        }
        
        Map.monster.GetComponent<MonsterBehavior>().playerPosition = Map.player.transform.position;
    }
}

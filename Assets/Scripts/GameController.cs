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

        Map.monster = Instantiate(monsterPrefab);
        Map.player = Instantiate(playerPrefab);
        Map.AdvanceLevel();
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
        if (Map.monster.GetComponent<MonsterBehavior>().CurrentState == MonsterBehavior.MonsterState.Aggressive)
        {
            Map.monster.GetComponent<MonsterBehavior>().playerPosition = Map.player.transform.position;
        }
    }
}

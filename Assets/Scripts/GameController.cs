using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Prefabs to instantiate!
    public GameObject playerPrefab;
    public GameObject monsterPrefab;

    // Start is called before the first frame update
    void Start()
    {
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
            Map.monster.GetComponent<MonsterBehavior>().playerPosition = player.transform.position;
        }
    }
}

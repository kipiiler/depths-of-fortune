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
}

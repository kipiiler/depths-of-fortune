using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Prefabs to instantiate!
    public GameObject playerPrefab;
    public GameObject monsterPrefab;

    // Monster to player path
    Stack<Map.Segment> path = new Stack<Map.Segment>();

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
        // // Provide the monster new setpoints to the player
        if (path.Count > 1)
        {
            // Check if monster has hit the setpoint
            if (Vector3.Distance(Map.monster.transform.position, path.Peek().GetUnityPosition()) < 0.4)
            {
                Debug.Log("Monster reached the setpoint, queue-ing a new one");
                path.Pop();
            }

            Map.monster.GetComponent<MonsterBehavior>().UpdateSetpoint(path.Peek().GetUnityPosition());
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSound : MonoBehaviour
{
    private MonsterBehavior monsterBehavior;
    public AudioSource screamSound;
    public AudioSource attackSound;

    private MonsterBehavior.MonsterState prevState;

    // Start is called before the first frame update
    void Start()
    {
        monsterBehavior = gameObject.GetComponent<MonsterBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        if (prevState != MonsterBehavior.MonsterState.Suspicious && monsterBehavior.CurrentState == MonsterBehavior.MonsterState.Suspicious)
        {
            screamSound.Play();
        } else if (prevState != MonsterBehavior.MonsterState.Aggressive && monsterBehavior.CurrentState == MonsterBehavior.MonsterState.Aggressive)
        {
            attackSound.Play();
        }

        prevState = monsterBehavior.CurrentState;
    }
}

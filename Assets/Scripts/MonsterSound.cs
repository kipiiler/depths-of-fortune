using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSound : MonoBehaviour
{
    private MonsterBehavior monsterBehavior;
    public AudioSource screamSound;
    public AudioSource attackSound;
    public AudioSource hurtSound;
    public AudioSource screechSound;
    private bool hasScreeched;

    private MonsterBehavior.MonsterState prevState;
    private bool prevStunned;

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
        }
        else if (prevState != MonsterBehavior.MonsterState.Aggressive && monsterBehavior.CurrentState == MonsterBehavior.MonsterState.Aggressive)
        {
            attackSound.Play();
        }

        if (!prevStunned && monsterBehavior.isStunned)
        {
            hurtSound.Play();
        }

        if (!hasScreeched && monsterBehavior.CurrentState == MonsterBehavior.MonsterState.Aggressive && Vector3.Distance(transform.position, monsterBehavior.playerPosition) < 30f)
        {
            screechSound.Play();
            hasScreeched = true;
        }

        if (Vector3.Distance(transform.position, monsterBehavior.playerPosition) > 30f)
        {
            hasScreeched = false;
        }

        prevState = monsterBehavior.CurrentState;
        prevStunned = monsterBehavior.isStunned;
    }
}

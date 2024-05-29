using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    private GameObject player;

    private FirstPersonController controller;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        controller = player.GetComponent<FirstPersonController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Monster" && controller.isAttacking)
        {
            // other.GetComponent<Animator>().SetTrigger("Hit");
            Debug.Log("hit");
        }
    }
}

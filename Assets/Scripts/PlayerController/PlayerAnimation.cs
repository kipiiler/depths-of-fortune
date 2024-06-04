using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator anim;
    private FirstPersonController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<FirstPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Walking", controller.isWalking);
        anim.SetBool("Sprinting", controller.isSprinting);
        anim.SetBool("Grounded", controller.isGrounded);
        anim.SetFloat("vy", controller.velocity.y);
        anim.SetBool("Crouched", controller.isCrouched);
        anim.SetBool("HasTorch", controller.hasTorch);
        anim.SetBool("Attack", controller.isAttacking);
        if (controller.isDead)
        {
            anim.SetTrigger("death");
        }
    }
}

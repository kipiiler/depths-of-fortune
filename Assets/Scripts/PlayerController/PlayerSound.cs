using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    private FirstPersonController controller;
    public GameObject walkFootstep;
    public GameObject runFootstep;
    public GameObject attackSound;
    public GameObject landSound;

    private AudioSource landingSource;

    private bool prevIsGrounded;
    
    static float SOUND_FREQUENCY = 1f;
    float lastSoundTimeElapsed;
    
    private void Awake()
    {
        walkFootstep.SetActive(false);
        runFootstep.SetActive(false);
        attackSound.SetActive(false);
        landingSource = landSound.GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<FirstPersonController>();
        lastSoundTimeElapsed = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!prevIsGrounded && controller.isGrounded)
        {
            landingSource.Play();
            Sound lSound = new Sound(transform.position, 40f);
            Sounds.MakeSound(lSound);
        }

        prevIsGrounded = controller.isGrounded;

        if (controller.isGrounded)
        {
            if (controller.isCrouched)
            {
                walkFootstep.SetActive(false);
                runFootstep.SetActive(false);
            }
            else if (controller.isSprinting)
            {
                walkFootstep.SetActive(false);
                runFootstep.SetActive(true);

                if (lastSoundTimeElapsed <= 0)
                {
                    Sound runSound = new Sound(transform.position, 25f);
                    Sounds.MakeSound(runSound);
                    lastSoundTimeElapsed = SOUND_FREQUENCY;
                }
            }
            else if (controller.isWalking)
            {
                walkFootstep.SetActive(true);
                runFootstep.SetActive(false);

                if (lastSoundTimeElapsed <= 0)
                {
                    Sound walkSound = new Sound(transform.position, 10f);
                    Sounds.MakeSound(walkSound);
                    lastSoundTimeElapsed = SOUND_FREQUENCY;
                }
            }
            else
            {
                walkFootstep.SetActive(false);
                runFootstep.SetActive(false);
            }
        }
        else
        {
            walkFootstep.SetActive(false);
            runFootstep.SetActive(false);
        }

        if (controller.isAttacking)
        {
            attackSound.SetActive(true);
            Sound atkSound = new Sound(transform.position, 40f);
            Sounds.MakeSound(atkSound);
        } else
        {
            attackSound.SetActive(false);
        }

        lastSoundTimeElapsed -= Time.deltaTime;
    }
}

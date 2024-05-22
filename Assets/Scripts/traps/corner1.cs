using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class corner1 : MonoBehaviour
{
    public AudioSource sound;

    private void OnTriggerEnter(Collider other)
    {
        if (sound.isPlaying) return;
        sound.Play();
    }
}

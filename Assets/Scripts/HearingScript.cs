using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HearingScript : MonoBehaviour, IHear
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RespondToSound(Sound sound)
    {
        Debug.Log("I hear sound from " + sound.pos + " with intensity " + sound.intensity);
    }
}

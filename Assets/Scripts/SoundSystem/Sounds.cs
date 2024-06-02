using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Sounds
{

    // This method should be called whenever a sound is played. All receivers need to implement IHear.
    public static void MakeSound(Sound sound)
    {
        foreach (IHear hearer in GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IHear>()) {
            hearer.RespondToSound(sound);
        }
    }
}

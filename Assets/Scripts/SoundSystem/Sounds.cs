using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Sounds
{
    private static List<GameObject> hearers = new List<GameObject>();

    public static void Add(GameObject hearer)
    {
        hearers.Add(hearer);
    }

    public static void Remove(GameObject hearer)
    {
        hearers.Remove(hearer);
    }

    // This method should be called whenever a sound is played. All receivers need to implement IHear.
    public static void MakeSound(Sound sound)
    {
        for (int i = 0; i < hearers.Count; i++)
        {
            if (hearers[i].TryGetComponent(out IHear hearer))
            {
                hearer.RespondToSound(sound);
            }
        }

        /*
        Collider[] col = Physics.OverlapSphere(sound.pos, sound.range);

        for (int i = 0; i < col.Length; i++)
            if (col[i].TryGetComponent(out IHear hearer))
                hearer.RespondToSound(sound);
        */
    }
}

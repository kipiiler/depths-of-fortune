using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootGrabbable : MonoBehaviour, IGrabbable
{
    public const float spawn_chance = 0.3f;

    public void Start()
    {
        if (Random.value > spawn_chance)
        {
            Destroy(this.gameObject);
        }
    }

    public GameObject Grab(Transform grabPoint)
    {
        Sounds.MakeSound(new Sound(grabPoint.position, 12f));
        return null;
    }
}

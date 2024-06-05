using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    private GameObject player;

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
    }

    void LateUpdate()
    {

        if (player == null)
        {
            return;
        }

        Vector3 newPosition = player.transform.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;

        transform.rotation = Quaternion.Euler(90f, player.transform.eulerAngles.y, 0f);
    }
}

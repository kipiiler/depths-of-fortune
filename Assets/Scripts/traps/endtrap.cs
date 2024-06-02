using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class endtrap : MonoBehaviour
{
    private Image blackoutScreen;

    private Collider player;

    private float curAlpha = 0;
    private float targetAlpha = 0;
    private bool change = false;

    public float FadeRate = 1f;

    private void Start()
    {
        blackoutScreen = FindObjectOfType<Image>();
    }

    private void Update()
    {
        if (curAlpha != targetAlpha)
        {
            curAlpha = Mathf.MoveTowards(curAlpha, targetAlpha, Time.deltaTime);
            Color color = blackoutScreen.color;
            color.a = curAlpha;
            blackoutScreen.color = color;
            if (change && curAlpha == targetAlpha)
            {
                change = false;
                player.gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                this.gameObject.GetComponent<Collider>().enabled = false;
                Map.AdvanceLevel();
                targetAlpha = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other;
            change = true;
            targetAlpha = 1f;
        }
    }
}

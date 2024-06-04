using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class endtrap : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            this.gameObject.GetComponent<Collider>().enabled = false;
            other.gameObject.GetComponentInChildren<FirstPersonController>().SetBlackout(() => {
                Map.AdvanceLevel();
            });
        }
    }
}

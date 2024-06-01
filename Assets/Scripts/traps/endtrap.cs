using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class endtrap : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            this.gameObject.GetComponent<Collider>().enabled = false;
            Map.AdvanceLevel();
        }
    }
}

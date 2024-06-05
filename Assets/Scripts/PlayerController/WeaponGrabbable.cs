using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGrabbable : MonoBehaviour, IGrabbable
{
    public GameObject torchWeaponPrefab;

    public GameObject Grab(Transform grabPoint)
    {
        GameObject newObj = (GameObject)Instantiate(torchWeaponPrefab, grabPoint);
        newObj.transform.localPosition = new Vector3(-0.062f, 0.088f, 0.027f);

        Vector3 v = new Vector3(0f, 0f, -76.714f);
        newObj.transform.localRotation = Quaternion.Euler(v);
        newObj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        return newObj;
    }

}

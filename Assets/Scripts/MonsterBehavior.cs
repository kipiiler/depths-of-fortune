using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehavior : MonoBehaviour
{
    // public Transform Player;
    int MoveSpeed = 4;
    int MaxDist = 10;
    int MinDist = 5;
    

    Vector3 setpoint;
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        setpoint = new Vector3(0, 0, 0);
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(setpoint);
        transform.position += transform.forward * MoveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, setpoint) < MoveSpeed) {
            anim.SetTrigger("StartWalk");
        }
    }

    public void UpdateSetpoint(Vector3 newSetpoint) {
        if (setpoint != newSetpoint) {
            Debug.Log("Adding new setpoint");
            anim.SetTrigger("StartWalk");
            setpoint = new Vector3(newSetpoint.x, newSetpoint.y, newSetpoint.z);
        }
    }
}

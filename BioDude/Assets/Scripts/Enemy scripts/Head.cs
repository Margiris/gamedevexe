using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour {

    public float rotationSpeed = 1f;
    private bool canRotate = true;
    public bool isRotated = false; // is head finished rotating to its target angle
    private float angle;
    //public bool isRotating = false;

    public float targetAngle;
    //private float prevRotation;
    //public float currentrotation;


    // Use this for initialization
    void Start () {
        //currentrotation = transform.rotation.z;
        //prevRotation = currentrotation;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //currentrotation = transform.rotation.z;
        //isRotating = (prevRotation != currentrotation);
        //prevRotation = currentrotation;

        angle = Quaternion.Angle(transform.rotation, Quaternion.Euler(0, 0, targetAngle)); 
        if (canRotate && !isRotated)
        {
            isRotated = (Quaternion.Angle(transform.rotation, Quaternion.Euler(0, 0, targetAngle)) == 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, targetAngle), rotationSpeed);
        }
    }

    public void SetTargetAngle(float angle)
    {
        targetAngle = angle;
        isRotated = (Quaternion.Angle(transform.rotation, Quaternion.Euler(0, 0, targetAngle)) == 0);
    }
}

using System;
using UnityEngine;

namespace Enemy_scripts
{
    public class Head : MonoBehaviour
    {
        public float rotationSpeed = 1f;
        public bool isRotated; // is head finished rotating to its target angle
        public float targetAngle;

        // Update is called once per frame
        private void Update()
        {
            Quaternion.Angle(transform.rotation, Quaternion.Euler(0, 0, targetAngle));
            if (isRotated) return;
            isRotated = (Math.Abs(Quaternion.Angle(transform.rotation, Quaternion.Euler(0, 0, targetAngle))) <= 0);
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, targetAngle), rotationSpeed);
        }

        public void SetTargetAngle(float angle)
        {
            targetAngle = angle;
            isRotated = (Math.Abs(Quaternion.Angle(transform.rotation, Quaternion.Euler(0, 0, targetAngle))) <= 0);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Richochet : MonoBehaviour {

    private LayerMask wallMask;
    private Rigidbody2D rigidbody;

    public void Awake()
    {
        wallMask = LayerMask.GetMask("Collider");
        rigidbody = transform.GetComponent<Rigidbody2D>();
    }
   
    private Vector3 oldVelocity;

    void FixedUpdate()
    {
        // because we want the velocity after physics, we put this in fixed update
        oldVelocity = rigidbody.velocity;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Bouncy")
        {
            // get the point of contact
            ContactPoint2D contact = collision.contacts[0];

            // reflect our old velocity off the contact point's normal vector
            Vector3 reflectedVelocity = Vector3.Reflect(oldVelocity, contact.normal);

            // assign the reflected velocity back to the rigidbody
            rigidbody.velocity = reflectedVelocity;
            // rotate the object by the same ammount we changed its velocity
            Quaternion rotation = Quaternion.FromToRotation(oldVelocity, reflectedVelocity);
            transform.rotation = rotation * transform.rotation;
        }
        
    }
}

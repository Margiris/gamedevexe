using UnityEngine;

namespace Items_scripts
{
    public class Ricochet : MonoBehaviour
    {
        private Rigidbody2D rigidBody;

        public void Awake()
        {
            rigidBody = transform.GetComponent<Rigidbody2D>();
        }

        private Vector3 oldVelocity;

        private void FixedUpdate()
        {
            // because we want the velocity after physics, we put this in fixed update
            oldVelocity = rigidBody.velocity;
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.transform.CompareTag("Bouncy")) return;
            // get the point of contact
            var contact = collision.contacts[0];

            // reflect our old velocity off the contact point's normal vector
            var reflectedVelocity = Vector3.Reflect(oldVelocity, contact.normal);

            // assign the reflected velocity back to the rigidBody
            rigidBody.velocity = reflectedVelocity;
            // rotate the object by the same amount we changed its velocity
            var rotation = Quaternion.FromToRotation(oldVelocity, reflectedVelocity);
            transform.rotation = rotation * transform.rotation;
        }
    }
}
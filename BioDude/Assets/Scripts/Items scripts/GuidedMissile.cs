using UnityEngine;

namespace Items_scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class GuidedMissile : Explosive
    {
        [HideInInspector] public float speed;
        [HideInInspector] public float rotSpeed;
        [HideInInspector] public float radius;
        [HideInInspector] public float force;
        [HideInInspector] public ParticleSystem[] emitters;

        public GameObject explosionEffect;
        public float damage = 40f;

        private Rigidbody2D body;

        // Use this for initialization
        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
            // ReSharper disable once MultipleResolveCandidatesInText
            Invoke("Explode", 4f);
            emitters = transform.GetComponentsInChildren<ParticleSystem>();
        }

        // ReSharper disable in file ParameterHidesMember
        public void Instantiate(float speed, float rotationSpeed, float radius, float force)
        {
            this.speed = speed;
            rotSpeed = rotationSpeed;
            this.radius = radius;
            this.force = force;
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
//            var mousePos = Input.mousePosition;
//            mousePos.z = -10; // select distance = 10 units from the camera
            // ReSharper disable once Unity.InefficientCameraMainUsage
            // ReSharper disable once PossibleNullReferenceException
            var direction = (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition) - body.position;

            direction.Normalize();
            var rotateAmount = Vector3.Cross(direction, transform.up).z;

            body.angularVelocity = -rotateAmount * rotSpeed;

            body.velocity = transform.up * speed;
        }

        private void OnCollisionEnter2D()
        {
            Explode();
        }

        public override void Explode()
        {
            foreach (var t in emitters)
            {
                // This splits the particle off so it doesn't get deleted with the parent
                t.transform.parent = null;
                // this stops the particle from creating more bits
                t.Stop();
            }


            Instantiate(explosionEffect, transform.position, transform.rotation);

            var nearbyObjects = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (var obj in nearbyObjects)
            {
                var rb = obj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    AddExplosionForce(rb, force, transform.position, radius, damage);
                }
            }

            Destroy(gameObject);
        }
    }
}
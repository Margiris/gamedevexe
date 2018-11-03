using UnityEngine;

namespace Items_scripts.Weapon_Scripts
{
    public class FragGrenade : Explosive
    {
        public float delay = 2f;
        private float countdown;

        public GameObject explosionEffect;

        private bool exploded;
        public float radius = 3f;
        public float force = 500f;
        public float damage = 1f;

        // Use this for initialization
        private void Start()
        {
            countdown = delay;
            GetComponent<Rigidbody2D>();
            //started = true;
        }


        // Update is called once per frame
        private void FixedUpdate()
        {
            if (!started)
            {
                Throw(throwForce);
                started = true;
            }

            countdown -= Time.deltaTime;
            if (!(countdown <= 0f) || exploded) return;
            Explode();
            exploded = true;
        }

        public override void Explode()
        {
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
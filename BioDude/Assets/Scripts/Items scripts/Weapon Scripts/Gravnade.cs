using UnityEngine;

namespace Items_scripts.Weapon_Scripts
{
    public class Gravnade : Explosive
    {
        public float delay = 2f;
        private float countdown;

        public GameObject blackHoleEffect;

        private bool exploded;

        // Use this for initialization
        private void Start()
        {
            countdown = delay;
            GetComponent<Rigidbody2D>();
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
            //gravnade effect
            Instantiate(blackHoleEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
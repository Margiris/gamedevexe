using UnityEngine;


// NOTE: Children order is important fro this script to work

namespace Enemy_scripts
{
    public class MeleeTank : Tank
    {
        // Use this for initialization
        private void Start()
        {
            Instantiate();
        }

        // Update is called once per frame
        private void Update()
        {
            CheckVision();
            SetAlertionIndicator();
            BehaviourIfCantSeePlayer();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;
            animator.SetBool("TargetInRange", true);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;
            animator.SetBool("TargetInRange", false);
        }

        protected override void Die()
        {
            base.Die();
            // enemy death: smokes and stopped movement
            Destroy(gameObject); //changed to game object because "this" destroys only MeleeTank script and the tank still chases player
        }
    }
}
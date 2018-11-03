using UnityEngine;

namespace Items_scripts.Weapon_Scripts
{
    public class GravHole : MonoBehaviour
    {
        private float duration;
        public ParticleSystem emitter;
        private PointEffector2D gravityEffector;
        private WindZone windZone;

        private float timeSpent;

        // Use this for initialization
        private void Start()
        {
            gravityEffector = transform.GetComponentInChildren<PointEffector2D>();
            duration = transform.GetComponentInParent<ParticleSystem>().main.duration;
            windZone = transform.GetComponentInChildren<WindZone>();
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            timeSpent += Time.fixedDeltaTime;
            if (timeSpent > duration - 2 && timeSpent < duration)
            {
                gravityEffector.forceMagnitude += 150 * Time.fixedDeltaTime;
            }

            if (!(timeSpent >= duration - 1) || !(timeSpent <= duration) || !(windZone.radius > 5)) return;
            windZone.radius = 0.5f;
            emitter.transform.parent = null;
        }
    }
}
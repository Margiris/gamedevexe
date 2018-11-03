using UnityEngine;

namespace Items_scripts
{
    public class Explosive : MonoBehaviour
    {
        public bool started;
        public float throwForce = 100f;
        public int AmmoType;

        public virtual void Explode()
        {
        }

        protected static void AddExplosionForce(Rigidbody2D body, float expForce, Vector3 expPosition, float expRadius,
            float damage)
        {
            var dir = (body.transform.position - expPosition);
            var calc = 1 - (dir.magnitude / expRadius);
            if (calc <= 0)
            {
                calc = 0;
            }

            body.AddForce(dir.normalized * expForce * calc);
            var charObj = body.gameObject.GetComponent<Character>();
            if (charObj != null)
            {
                body.gameObject.GetComponent<Character>().Damage(damage * calc);
            }
        }

        public virtual void Throw(float force)
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) return;
            Debug.Log("base grenade");
            var mousePos = Input.mousePosition;
            var dirForce = Vector3.Distance(transform.position, mousePos);
            dirForce *= 0.07f;
            dirForce *= dirForce;

            Debug.Log("dir: " + dirForce);
            Debug.Log("force: " + transform.up * force * dirForce * 0.005f);
            rb.AddForce(transform.up * force * dirForce * 0.005f);
        }
    }
}
using UnityEngine;

namespace Items_scripts
{
    public class Bullet : MonoBehaviour
    {
        private float damage;
        public ParticleSystem impactConcrete;
        public ParticleSystem impactMetal;
        public ParticleSystem impactFlesh;

        //bool bulletFired = false;  //////////////////////////////

        /// <summary>
        /// give bullet speed and time after which it should destroy itself
        /// </summary>
        /// <param name="destroyAfter">time in second after how long destroy this bullet</param>
        /// <param name="speed">speed to give to bullet</param>
        /// <param name="damage"></param>
        // ReSharper disable once ParameterHidesMember
        public void Instantiate(float destroyAfter, float speed, float damage)
        {
            Destroy(gameObject, destroyAfter);
            GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0, speed));
            this.damage = damage;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //checking if collided with a character
            var charObj = collision.gameObject.GetComponent<Character>();

            //getting contact points and setting rotation to the contact normal
            var contacts = new ContactPoint2D[2];
            var contactCount = collision.GetContacts(contacts);

            if (contactCount > 0)
            {
                Vector3 contactPos = contacts[0].point;
                var rot = Quaternion.FromToRotation(transform.forward, contacts[0].normal);

                if (charObj != null)
                {
                    charObj.Damage(damage);
                    if (charObj.CompareTag("Player"))
                    {
                        var emitter = Instantiate(impactFlesh, contactPos, rot);
                        // This splits the particle off so it doesn't get deleted with the parent
                        emitter.transform.parent = null;
                        //Debug.Log("player blood");
                    }
                    else if (charObj.CompareTag("Enemy"))
                    {
                        var emitter = Instantiate(impactMetal, contactPos, rot);
                        // This splits the particle off so it doesn't get deleted with the parent
                        emitter.transform.parent = null;
                        //Debug.Log("enemy metal");
                    }
                }
                else if (!collision.transform.CompareTag("Bouncy"))
                {
                    var emitter = Instantiate(impactConcrete, contactPos, rot);
                    // This splits the particle off so it doesn't get deleted with the parent
                    emitter.transform.parent = null;
                }
            }

            if (!collision.transform.CompareTag("Bouncy"))
                Destroy(gameObject);
        }

/*
    IEnumerator Fire()
    {
        yield return new WaitForFixedUpdate();

        GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0, weapon.projectileSpeed));
    }*/
    }
}
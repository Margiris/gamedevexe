using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 0;
    public ParticleSystem impactConcrete;
    public ParticleSystem impactMetal;
    public ParticleSystem impactFlesh;
    public Vector2 creationLocation;
    public int OwnerID = -1;
    public float speed;

    //bool bulletFired = false;  //////////////////////////////

    /// <summary>
    /// give bullet speed and time after which it should destroy itself
    /// </summary>
    /// <param name="destroyAfter">time in second after how long destroy this bullet</param>
    /// <param name="speed">speed to give to bullet</param>
    public void Instantiate(float destroyAfter, float speed, float damage, int ownerId = -1)
    {
        OwnerID = ownerId;
        Destroy(gameObject, destroyAfter);
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, speed / 400);
        //GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0, speed));
        this.damage = damage;
        creationLocation = transform.position;
    }

	private void OnCollisionEnter2D(Collision2D collision)
    {
        //checking if collided with a character
        Character charObj = collision.gameObject.GetComponent<Character>();
        if(charObj == null)
        {
            charObj = collision.transform.parent.gameObject.GetComponent<Character>();
        }
        
        //getting contact points and setting rotation to the contact normal
        ContactPoint2D[] contacts = new ContactPoint2D[2];
        int contactCount = collision.GetContacts(contacts);

        if (contactCount>0)
        {
            Vector3 contactPos = contacts[0].point;
            Quaternion rot = Quaternion.FromToRotation(transform.forward, contacts[0].normal);

            if (charObj != null)
            {
                if (charObj.tag == "Player")
                {
                    charObj.Damage(damage);
                    ParticleSystem emitter = Instantiate(impactFlesh, contactPos, rot);
                    // This splits the particle off so it doesn't get deleted with the parent
                    emitter.transform.parent = null;
                    //Debug.Log("player blood");

                }
                else if (charObj.tag == "Enemy")
                {
                    if (OwnerID != -1) // player shot this bullet at sensitive enemy
                    {
                        Tank tankObj = charObj.gameObject.GetComponent<Tank>();
                        if(tankObj != null)
                        {
                            tankObj.DamageAlerting(damage, OwnerID, creationLocation);
                        }
                        else
                        {
                            charObj.Damage(damage);
                        }
                    }
                    else
                    {
                        charObj.Damage(damage);
                    }
                    ParticleSystem emitter = Instantiate(impactMetal, contactPos, rot);
                    // This splits the particle off so it doesn't get deleted with the parent
                    emitter.transform.parent = null;
                    //Debug.Log("enemy metal");
                }

            }
            else if (collision.transform.tag != "Bouncy")
            {
                ParticleSystem emitter = Instantiate(impactConcrete, contactPos, rot);
                // This splits the particle off so it doesn't get deleted with the parent
                emitter.transform.parent = null;
            }
        }
        if (collision.transform.tag != "Bouncy")
            Destroy(gameObject);
    }
/*
    IEnumerator Fire()
    {
        yield return new WaitForFixedUpdate();

        GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0, weapon.projectileSpeed));
    }*/
}

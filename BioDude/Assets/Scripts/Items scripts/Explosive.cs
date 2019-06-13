using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Explosive : NetworkBehaviour {
    public bool started = false;
    public float throwForce = 100f;
    public int AmmoType;
    protected Vector2 creationLocation;
    public int ownerId = -1;
    [Command]
    public virtual void CmdExplode()
    {
    }

    public static void AddExplosionForce(Rigidbody2D body, float expForce, Vector3 expPosition, float expRadius, float damage, int ownerId = -1, Vector2 creationLocation = new Vector2())
    {
        var dir = (body.transform.position - expPosition);
        float calc = 1 - (dir.magnitude / expRadius);
        if (calc <= 0)
        {
            calc = 0;
        }

        body.AddForce(dir.normalized * expForce * calc);
        Character charObj = body.gameObject.GetComponent<Character>();
        if (charObj != null)
        {
            if (ownerId != -1) // player shot this explosive at sensitive enemy
            {
                Tank tankObj = charObj.gameObject.GetComponent<Tank>();
                if (tankObj != null)
                {
                    tankObj.CmdDamageAlerting(damage * calc, ownerId, creationLocation);
                }
                else
                {
                    charObj.Damage(damage * calc);
                }
            }
            else
            {
                charObj.Damage(damage * calc);
            }
        }
    }

    public virtual void Throw(float force)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Debug.Log("base grenade");
            Vector3 mousePos = Input.mousePosition;
            float dirForce = Vector3.Distance(transform.position, mousePos);
            dirForce *= 0.07f;
            dirForce *= dirForce;

            Debug.Log("dir: " + dirForce);
            Debug.Log("force: " + transform.up * force * dirForce * 0.005f);
            rb.AddForce(transform.up * force * dirForce * 0.005f);
        }
    }
}

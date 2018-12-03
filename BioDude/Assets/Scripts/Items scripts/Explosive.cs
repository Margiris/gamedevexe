using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour {
    public bool started = false;
    public float throwForce = 100f;
    public int AmmoType;
    public virtual void Explode()
    {
    }

    public static void AddExplosionForce(Rigidbody2D body, float expForce, Vector3 expPosition, float expRadius, float damage)
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
            body.gameObject.GetComponent<Character>().Damage(damage * calc);
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

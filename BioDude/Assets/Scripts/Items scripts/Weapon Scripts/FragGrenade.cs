using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FragGrenade : Explosive
{

    public float delay = 2f;
    float countdown;

    public GameObject explosionEffect;

    bool exploded = false;
    public float radius = 3f;
    public float force = 500f;
    public float damage = 1f;
    private Rigidbody2D rb;

    // Use this for initialization
    void Start()
    {
        countdown = delay;
        rb = GetComponent<Rigidbody2D>();
        //started = true;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if(!started)
        {
            Throw(throwForce);
            started = true;
        }
        countdown -= Time.deltaTime;
        if (countdown <= 0f && !exploded)
        {
            CmdExplode();
            exploded = true;
        }
    }

    [Command]
    public override void CmdExplode()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation);

        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D obj in nearbyObjects)
        {
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                AddExplosionForce(rb, force, transform.position, radius, damage);
            }
        }

        NetworkServer.Destroy(gameObject);
    }
}
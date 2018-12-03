using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GuidedMisile : Explosive {
    [HideInInspector]
    public float speed = 0;
    [HideInInspector]
    public float rotSpeed = 0;
    [HideInInspector]
    public float radius = 0;
    [HideInInspector]
    public float force = 0;
    [HideInInspector]
    public ParticleSystem[] emitters;

    public GameObject explosionEffect;
    public float damage = 40f;

    private Rigidbody2D body;
	// Use this for initialization
	void Start () {
        body = GetComponent<Rigidbody2D>();
        Invoke("Explode", 4f);
        emitters = transform.GetComponentsInChildren<ParticleSystem>();
    }

    public void Instantiate(float speed, float rotationSpeed, float radius, float force)
    {
        this.speed = speed;
        this.rotSpeed = rotationSpeed;
        this.radius = radius;
        this.force = force;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -10; // select distance = 10 units from the camera
        Vector2 direction = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - body.position;

        direction.Normalize();
        float rotateAmount = Vector3.Cross(direction, transform.up).z;

        body.angularVelocity = -rotateAmount * rotSpeed;

        body.velocity = transform.up * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Explode();
    }
    public override void Explode()
    {
        for (int i = 0; i < emitters.Length; i++)
        {
            // This splits the particle off so it doesn't get deleted with the parent
            emitters[i].transform.parent = null;
            // this stops the particle from creating more bits
            emitters[i].Stop();
        }


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
        Destroy(gameObject);
    }
}

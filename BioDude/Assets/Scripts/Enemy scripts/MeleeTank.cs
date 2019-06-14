using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;
using UnityEngine.Networking;

// NOTE: Children order is important fro this script to work

public class MeleeTank : Tank
{
    public int Meleedamage = 1;
    private player target;

    //private:

    private void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isServer)
        {
            if (collision.tag == "Player")
            {
                targetInAttackRange = true;
                animator.SetBool("TargetInRange", true);
                target = collision.transform.parent.GetComponent<player>();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isServer)
        {
            if (collision.tag == "Player")
            {
                targetInAttackRange = false;
                animator.SetBool("TargetInRange", false);
            }
        }
    }

    private void PerformAttack() // a smarter way of inflicting damage is required - this is nether secure nor efficient 
    {
        if (isServer)
        {
            if (targetInAttackRange)
            {
                target.Damage(Meleedamage);
            }
        }
    }

    protected override void Die()
    {
        base.Die();
        // enemy death: smokes and stoped movement
        NetworkServer.Destroy(gameObject);    //changed to gameobject because "this" destroys only MeleeTank script and the tank still chases player
    }
    
    
}

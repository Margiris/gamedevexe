using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;


// NOTE: Children order is important fro this script to work

public class MeleeTank : Tank
{
    public int Meleedamage = 1;

    //private:
    // Use this for initialization
    void Start()
    {
        Instantiate();
    }

    // Update is called once per frame
    void Update()
    {
        CheckVision();
        SetAlertionIndicator();
        BechaviourIfCantSeePlayer();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            targetInAttackRange = true;
            animator.SetBool("TargetInRange", true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            targetInAttackRange = false;
            animator.SetBool("TargetInRange", false);
        }
    }

    private void PerformAttack() // a smarter way of inflicting damage is required - this is nether secure nor efficient 
    {
        if (targetInAttackRange)
        {
            player.GetComponent<player>().Damage(Meleedamage);
        }
    }

    protected override void Die()
    {
        base.Die();
        // enemy death: smokes and stoped movement
        Destroy(gameObject);    //changed to gameobject because "this" destroys only MeleeTank script and the tank still chases player
    }
    
    
}

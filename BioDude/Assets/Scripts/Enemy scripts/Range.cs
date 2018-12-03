using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range : MonoBehaviour {

//unused     private BasicEnemy parent;

    private void Start()
    {
//unused         parent = GetComponentInParent<BasicEnemy>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            
        }
    }
}

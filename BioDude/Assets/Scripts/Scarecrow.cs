using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scarecrow : Character {

    protected EnemyHPBar HpBar;

    protected override void Die()
    {
        Destroy(gameObject);
    }

    // Use this for initialization
    void Start () {

        HpBar = transform.GetComponent<EnemyHPBar>();
        HpBar.Initiate();
        healthCurrent = healthMax;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public override void Damage(float amount)
    {
        if (isServer)
        {
            base.Damage(amount);
            HpBar.RpcSetHealth(GetHealth());
        }
    }

}

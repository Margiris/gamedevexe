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

        HpBar = transform.Find("EnemyCanvas").GetComponent<EnemyHPBar>();
        HpBar.Initiate();
        healthCurrent = healthMax;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public override void Damage(float amount)
    {
        base.Damage(amount);
        HpBar.SetHealth(GetHealth());
    }

}

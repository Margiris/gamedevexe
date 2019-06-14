using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

    [Command]
    public override void CmdDamage(float amount)
    {
        if (isServer)
        {
            base.CmdDamage(amount);
            HpBar.RpcSetHealth(GetHealth());
        }
    }

}

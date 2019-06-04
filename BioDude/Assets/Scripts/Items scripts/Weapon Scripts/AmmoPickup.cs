using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour {

	public string ammoName;
	public int ammoAmount;
	private WeaponManager weaponManager;

	// Use this for initialization
	void Start () {  
	}
	
	private void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag == "Player")
		{
			other.gameObject.GetComponent<WeaponManager>().AddAmmoByName(ammoName, ammoAmount);
            Destroy(gameObject);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour {

    public Weapon weapon;
    private WeaponManager weaponManager;

    // Use this for initialization
    void Start()
    {
        weaponManager = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            weaponManager.DiscoverWeaponByName(weapon.name);
            Destroy(gameObject);
        }
    }
}

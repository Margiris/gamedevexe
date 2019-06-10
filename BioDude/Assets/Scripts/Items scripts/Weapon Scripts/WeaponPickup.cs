using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour {

    public Weapon weapon;
    private WeaponManager weaponManager;

    // Use this for initialization
    void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.transform.parent.gameObject.GetComponent<WeaponManager>().DiscoverWeaponByName(weapon.name);
            Destroy(gameObject);
        }
    }
}

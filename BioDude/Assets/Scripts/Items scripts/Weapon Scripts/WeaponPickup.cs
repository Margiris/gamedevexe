using Player_scripts;
using UnityEngine;

namespace Items_scripts.Weapon_Scripts
{
    public class WeaponPickup : MonoBehaviour
    {
        public Weapon weapon;
        private WeaponManager weaponManager;

        // Use this for initialization
        private void Start()
        {
            weaponManager = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponManager>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            weaponManager.DiscoverWeaponByName(weapon.name);
            Destroy(gameObject);
        }
    }
}
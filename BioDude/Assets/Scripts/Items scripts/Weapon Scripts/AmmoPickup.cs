using Player_scripts;
using UnityEngine;

namespace Items_scripts.Weapon_Scripts
{
    public class AmmoPickup : MonoBehaviour
    {
        public string ammoName;
        public int ammoAmount;
        private WeaponManager weaponManager;

        // Use this for initialization
        private void Start()
        {
            weaponManager = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponManager>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            weaponManager.AddAmmoByName(ammoName, ammoAmount);

            Destroy(gameObject);
        }
    }
}
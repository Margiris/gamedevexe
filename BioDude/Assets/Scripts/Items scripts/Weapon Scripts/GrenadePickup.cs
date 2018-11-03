using Player_scripts;
using UnityEngine;

namespace Items_scripts.Weapon_Scripts
{
    public class GrenadePickup : MonoBehaviour
    {
        public int explosiveID;
        public int count = 2;
        private WeaponManager weaponManager;

        // Use this for initialization
        private void Start()
        {
            weaponManager = GameObject.FindGameObjectWithTag("PlayerWeaponSlot").GetComponent<WeaponManager>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            weaponManager.AddExplosivesByIndex(explosiveID, count);
            Destroy(gameObject);
        }
    }
}
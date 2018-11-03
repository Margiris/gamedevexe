using UnityEngine;
using UnityEngine.Serialization;

namespace Items_scripts.Weapon_Scripts
{
    public abstract class Weapon : Item
    {
        /*public enum WeaponType //deprecated or not used yet
    {
        Melee,
        Pistol,
        RocketLauncher
    }*/

        public AudioClip weaponSound;
        public AudioClip emptySound;
        public AudioClip reloadSound;
        public GameObject projectile;
        public GameObject cartridgeCase;
        public GameObject tip;
        public float projectileSpeed;

        // ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("timeUntilSelfDestrucion")]
        public float timeUntilSelfDestruction;

        public float cooldown;

        //public WeaponType weaponType;
        public int clipSize; // how many shots weapon can hold inside
        public int currentClipAmmo; // how many shots are left in weapon
        public float reloadTime;
        public float accuracy;
        public float damage;

        // ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("allertingRadius")]
        public float alertingRadius;

        public float cameraRecoil;
        public int ammoType; // index of ammo array in player weaponManager
        public bool isDiscovered;
    }
}
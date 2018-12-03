using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
    /*public enum WeaponType //deprecated or not used yet
    {
        Melee,
        Pistol,
        RocketLauncher
    }*/

    public AudioClip weaponSound;
    public AudioClip shellSound;
    public AudioClip emptySound;
    public AudioClip reloadSound;
    public GameObject projectile;
    public GameObject cartridgeCase;
    public GameObject tip;
    public float projectileSpeed;
    public float timeUntilSelfDestrucion;
    public float cooldown;
    //public WeaponType weaponType;
    public int clipSize; // how many shots weapon can hold inside
    public int currentClipAmmo; // how many shots are left in weapon
    public float reloadTime;
    public float accuracy = 0;
    public float damage;
    public float allertingRadius;
    public float cameraRecoil;
    public int ammoType; // index of ammo array in player weaponManager
    public bool isDiscovered = false;
}

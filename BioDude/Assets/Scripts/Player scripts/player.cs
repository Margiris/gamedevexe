using System;
using GUI_scripts;
using Items_scripts.Weapon_Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable CommentTypo

namespace Player_scripts
{
    public class player : Character
    {
        public float speed { get; set; } // The speed that the player will move at.

        private Vector2 movement; // The vector to store the direction of the player's movement.
        private Animator anim; // Reference to the animator component.
        private Rigidbody2D playerRigidBody; // Reference to the player's rigid body.
        private bool ableToMove = true;
        private AudioSource audioSource;
        public AudioClip[] painSounds;
        public AudioClip deathSound;

        private PauseMenu PausemenuCanvas;
        private float rot_z;
        private WeaponManager weaponManager;

        private void Awake()
        {
            Initiate();
            // Set up references.
            audioSource = gameObject.GetComponents<AudioSource>()[1];
            PausemenuCanvas = GameObject.Find("Pausemenu Canvas").GetComponent<PauseMenu>();
            anim = GetComponentInChildren<Animator>();
            playerRigidBody = GetComponent<Rigidbody2D>();
            weaponManager = transform.GetComponent<WeaponManager>();
            speed = 150;
        }

        protected override void Initiate()
        {
            healthMax = 100;

            if (PlayerPrefs.HasKey("PlayerHP"))
            {
                healthCurrent = PlayerPrefs.GetFloat("PlayerHP");
            }
            else
            {
                base.Initiate();
            }
        }

        private void Update()
        {
            if (!ableToMove) return;
            Controls();
            Turning(); // Turn the player to face the mouse cursor.
        }

        private void FixedUpdate()
        {
            if (ableToMove)
            {
                var h = Input.GetAxisRaw("Horizontal");
                var v = Input.GetAxisRaw("Vertical");

                Move(h, v);
                Animating(h,
                    v); // Animate the player. //BULLSHIT kam nurodyti h ir v jei jau bus issaugota i movement vectoriu tik atsargiai kad nepakelti auksciau nes tada nebus
            }
        }

        private void
            Controls() //BULLSHIT reikia susitvarkyti ir apgalvoti ar viskas bus ok jei vienu framu pasileistu visos komandos nes nenaudojami else if - galbut reikia debouncing arba kintamuju delayinti veiksma kitam framui  
        {
            if (Input.GetButtonDown("Fire"))
                weaponManager.Shoot();
            else if (Input.GetButton("Fire"))
                weaponManager.AutomaticShoot();

            if (Input.GetButtonDown("ThrowGrenade"))
                weaponManager.UseExplosive();
            if (Input.GetButtonDown("Reload"))
                Reload();
            if (Input.GetButtonDown("SwitchWeaponRight"))
                weaponManager.SwitchWeaponRight();
            if (Input.GetButtonDown("SwitchWeaponLeft"))
                weaponManager.SwitchWeaponLeft();
            if (Input.GetButtonDown("SwitchGrenadeRight"))
                weaponManager.SwitchExplosiveRight();
            if (Input.GetButtonDown("SwitchGrenadeLeft"))
                weaponManager.SwitchExplosiveLeft();
            if (Input.GetButtonDown("SelectKnife"))
                weaponManager.SelectWeaponByIndex(-1);
        }

        private void Move(float h, float v)
        {
            //playerRigidbody.velocity = new Vector2(h * speed, v * speed);  //option1
            movement = new Vector2(h, v); //option2
            playerRigidBody.AddForce(movement * speed);
        }

        private void Turning()
        {
            // ReSharper disable once PossibleNullReferenceException
            var diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            diff.Normalize();

            Camera.main.ScreenToWorldPoint(transform.position);

            rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        }

        private void Animating(float h, float v)
        {
            // Create a boolean that is true if either of the input axes is non-zero.
            var walking = Math.Abs(h) >= 0 || Math.Abs(v) >= 0;

            // Tell the animator whether or not the player is walking.
            anim.SetBool("IsMoving", walking);
            //Vector2 rot = transform.rotation;
            var relative = transform.InverseTransformVector(movement);
            anim.SetFloat("XSpeed", relative.x);
            anim.SetFloat("YSpeed", relative.y);
        }

        // OVERRIDEN METHODS:

        protected override void Die()
        {
            ableToMove = false;
            Destroy(gameObject.GetComponent<Rigidbody2D>());
            Destroy(gameObject.GetComponent<CircleCollider2D>());
            audioSource.clip = deathSound;
            audioSource.Play();
            //^^^ pakeist i player death animation

            StartCoroutine(PausemenuCanvas.PlayerDeath());
        }

        public override void Damage(float amount)
        {
            healthCurrent -= amount;
            if (healthCurrent <= 0)
            {
                healthCurrent = 0;
                Die();
            }
            else
            {
                var indexSound = Random.Range(0, painSounds.Length);
                audioSource.clip = painSounds[indexSound];
                audioSource.Play();
            }
        }


        // OTHER METHODS:

        public float GetDirectionAngle()
        {
            return rot_z;
        }

        private void Reload()
        {
            weaponManager.Reload();
        }

        public void SetAbleToMove(bool value)
        {
            ableToMove = value;
        }


        public void SavePlayerStats()
        {
            PlayerPrefs.SetFloat("PlayerHP", healthCurrent);
            Debug.Log("saved player hp");
            if (weaponManager != null)
            {
                Debug.Log("saving ammo info");
                //Saving info
                string name1;
                int ammoCount;
                if (weaponManager.fireArmAmmo != null)
                {
                    foreach (var t in weaponManager.weaponArray)
                    {
                        weaponManager.fireArmAmmo[t.GetComponent<Weapon>().ammoType].amount
                            += t.GetComponent<Weapon>().currentClipAmmo;
                    }

                    for (var i = 0; i < weaponManager.fireArmAmmo.Length; i++)
                    {
                        name1 = weaponManager.fireArmAmmo[i].name;
                        ammoCount = weaponManager.fireArmAmmo[i].amount;
                        if (!string.IsNullOrEmpty(name1) && ammoCount >= 0)
                        {
                            //Debug.Log("saved " + ammoCount + name);
                            PlayerPrefs.SetInt(name1 + "Ammo", ammoCount);
                        }
                    }
                }
                else
                    Debug.Log("failed to save weapon ammo info");

                if (weaponManager.explosiveAmmo != null)
                    for (var i = 0; i < weaponManager.explosiveAmmo.Length; i++)
                    {
                        name1 = weaponManager.explosiveAmmo[i].name;
                        ammoCount = weaponManager.explosiveAmmo[i].amount;
                        if (!string.IsNullOrEmpty(name1) && ammoCount >= 0)
                        {
                            //Debug.Log("saved " + ammoCount + name);
                            PlayerPrefs.SetInt(name1 + "Ammo", ammoCount);
                        }
                    }
                else
                    Debug.Log("failed to save grenade ammo info");

                if (weaponManager.weaponArray != null)
                    foreach (var t in weaponManager.weaponArray)
                    {
                        var weapon = t.GetComponent<Weapon>();
                        name1 = weapon.name;
                        PlayerPrefs.SetInt(name1 + "Discovered", weapon.isDiscovered ? 1 : 0);
                    }
                else
                    Debug.Log("failed to save weapon discovery info");
            }

            PlayerPrefs.Save();
        }
    }
}
using System.Collections;
using System.Linq;
using GUI_scripts;
using GUI_scripts.Achievement;
using Items_scripts;
using Items_scripts.Weapon_Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Player_scripts
{
    public class WeaponManager : MonoBehaviour
    {
        /// <summary>
        /// this script should be replaced on player itself!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        public struct Ammo
        {
            public string name;
            public int amount; // how many shots player has
            public int maxAmount;

            /// <summary>
            /// adds ammo to inventory
            /// </summary>
            /// <param name="amount1">amount of ammo to add</param>
            /// <returns>returns amount of ammo added</returns>
            public int AddAmmo(int amount1)
            {
                if (amount1 > 0)
                {
                    var desiredAmount = amount1 + amount;
                    if (desiredAmount > maxAmount)
                    {
                        var added = amount1 - desiredAmount + maxAmount;
                        amount = maxAmount;
                        return added;
                    }

                    amount = desiredAmount;
                    return amount1;
                }

                Debug.Log("You can't add negative amount of ammo or add 0. Use TakeAmmo instead");
                return 0;
            }

            /// <summary>
            /// takes ammo from inventory
            /// </summary>
            /// <param name="amountRequested">amount of ammo requested</param>
            /// <returns>returns amount of ammo taken from inventory</returns>
            public int TakeAmmo(int amountRequested)
            {
                if (amountRequested > 0)
                {
                    var ammoTaken = Mathf.Min(amount, amountRequested);
                    amount -= ammoTaken;
                    return ammoTaken;
                }

                Debug.Log("You can't take negative amount of ammo or take 0. Use AddAmmo instead");
                return 0;
            }
        }

        public Ammo[] fireArmAmmo;
        public Ammo[] explosiveAmmo;

        public GameObject rightHandSlot;
        public GameObject leftHandSlot;

        public GameObject[] explosiveArray; // add all types of grenades to array in inspector
        public GameObject activeGrenade;

        public GameObject[] weaponArray; // add all types of weapons to array in inspector
        private Weapon aWeaponScript;

        public GameObject knife;
        private Knife knifeScript;

        private int awAmmoType;
        private int aeAmmoType;

        private GameObject[] weaponSlots;
        private RawImage[] weaponSlotReds;
        private GameObject[] explosiveSlots;
        private RawImage[] explosiveSlotReds;

        public int lastSelectedFireArm;
        public int lastSelectedExplosive;

        public ParticleSystem impactConcrete;
        public ParticleSystem impactMetal;

        private GameObject activeWeaponRTip;
        private GameObject activeWeaponLTip;
        private bool cooldownEnded = true;

        private bool isReloading;

        //private SpriteRenderer spriteRenderer;
        private Alerting playerAlerting;
        private Animator playerAnimator;
        private int selectedFireArm = -1;
        private int selectedExplosive = -1;
        private Transform projectiles;

        private GUIManager guiManager;
        private CameraScript mainCameraScript;
        private AudioSource weaponAudioSource;
        private AudioSource reloadAudioSource;
        private AudioSource emptyAudioSource;
        private AchievementManager notifications;

        private RawImage ammoImage;
        private RawImage explosiveImage;
        private RawImage weaponSlotImage;

        private readonly int[] AutomaticWeapons = {2};


        // Use this for initialization
        private void Start()
        {
            //rightHandSlot = transform.Find("hand_R").GetChild(0).gameObject;
            activeWeaponRTip = rightHandSlot.transform.GetChild(0).gameObject;
            //leftHandSlot = transform.Find("hand_L").GetChild(0).gameObject;
            activeWeaponLTip = leftHandSlot.transform.GetChild(0).gameObject;


            // static information about offence weapons 
            fireArmAmmo = new Ammo[4];
            explosiveAmmo = new Ammo[2];
            LoadFromPrefs();

            notifications = GameObject.Find("AchievementManager").GetComponent<AchievementManager>();
            knifeScript = knife.GetComponent<Knife>();
            projectiles = GameObject.Find("Projectiles").transform;
            mainCameraScript = GameObject.Find("Main Camera").GetComponent<CameraScript>();
            guiManager = GameObject.Find("GUI").GetComponent<GUIManager>();
            playerAnimator = GetComponentInChildren<Animator>();
            playerAlerting = GetComponent<Alerting>();
            weaponAudioSource = GetComponent<AudioSource>();
            reloadAudioSource = GetComponents<AudioSource>()[2];
            emptyAudioSource = GetComponents<AudioSource>()[3];
            ammoImage = GameObject.Find("PlayerAmmoText").GetComponentInChildren<RawImage>();
            explosiveImage = GameObject.Find("PlayerExplosiveText").GetComponentInChildren<RawImage>();

            // get weapon slots
            weaponSlots = new GameObject[weaponArray.Length + 1]; // knife at last position
            weaponSlotReds = new RawImage[weaponArray.Length + 1]; // knife at last position
            for (var i = 0; i < weaponArray.Length; i++)
            {
                weaponSlots[i] =
                    GameObject.Find("WeaponSelectionSlot0" +
                                    (i + 1)); // sets the last selected firearm as a knife (also, the selected weapon should be set as knife too, when the knife is implemented)
                weaponSlotReds[i] = weaponSlots[i].transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
            }

            weaponSlots[weaponArray.Length] = GameObject.Find("WeaponSelectionSlot06");
            // get explosive slots
            explosiveSlots = new GameObject[explosiveArray.Length];
            explosiveSlotReds = new RawImage[explosiveArray.Length];
            for (var i = 0; i < explosiveArray.Length; i++)
            {
                explosiveSlots[i] =
                    GameObject.Find("ExplosiveSelectionSlot0" +
                                    (i + 1)); // sets the last selected explosion as a standard grenade
                explosiveSlotReds[i] = explosiveSlots[i].transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
            }

            //GetAllWeapons();
            // fill weapons with bullets and display discovered on start
            for (var i = 0; i < weaponArray.Length; i++)
            {
                var weapon = weaponArray[i].GetComponent<Weapon>();
                if (weapon.isDiscovered)
                {
                    weapon.currentClipAmmo =
                        fireArmAmmo[weapon.ammoType].TakeAmmo(weapon.clipSize); // load with bullets from pool
                    DisplayDiscoveredWeapon(i);
                }
                else
                {
                    weapon.currentClipAmmo = 0;
                }
            }

            SwitchWeaponRight();
            SwitchExplosiveRight();

            UpdateWeaponGUI();
            UpdateExplosiveGUI();
            UpdateBulletGUI();
        }

        // Automatic Reloading

        private void ReloadOnPickup(string ammoName)
        {
            for (var i = 0; i < fireArmAmmo.Length; i++)
                if (fireArmAmmo[i].name == ammoName)
                {
                    ReloadOnPickup(i);
                    return;
                }
        }

        private void ReloadOnPickup(int ammoIDx)
        {
            if (selectedFireArm != -1 && weaponArray[selectedFireArm].GetComponent<Weapon>().ammoType == ammoIDx)
            {
                AutoReload(selectedFireArm);
            }

            for (var i = 0; i < weaponArray.Length; i++)
            {
                if (weaponArray[i].GetComponent<Weapon>().ammoType == ammoIDx && i != selectedFireArm)
                    AutoReload(i);
            }
        }

        private void AutoReload(int weaponIDx)
        {
            var weapon = weaponArray[weaponIDx].GetComponent<Weapon>();
            if (weapon.isDiscovered)
            {
                weapon.currentClipAmmo +=
                    fireArmAmmo[weapon.ammoType].TakeAmmo(weapon.clipSize - weapon.currentClipAmmo);
            }
        }

        //

        public void UpdateWeaponGUI() // update gui
        {
            for (var i = 0; i < weaponArray.Length; i++)
            {
                var weapon = weaponArray[i].GetComponent<Weapon>();
                if (!weapon.isDiscovered)
                {
                    //we haven't gained this weapon yet
                }
                else if (weapon.currentClipAmmo + fireArmAmmo[weapon.ammoType].amount > 0)
                {
                    //we have weapon which is in index i
                    weaponSlotReds[i].enabled = false;
                }
                else
                {
                    //we don't have weapon which is in index i
                    weaponSlotReds[i].enabled = true;
                }
            }
        }

        public void UpdateBulletGUI()
        {
            if (selectedFireArm == -1)
            {
                //display no ammo - infinity or whatever when knife should be selected
                guiManager.SetBulletGUI("∞", "∞");
            }
            else
            {
                Debug.Log(awAmmoType);

                Debug.Log(fireArmAmmo[awAmmoType].amount.ToString());
                guiManager.SetBulletGUI(aWeaponScript.currentClipAmmo.ToString(),
                    fireArmAmmo[awAmmoType].amount.ToString());
                // show how many bullets left 
                // weapon sprite to display next to bullets numbers: weaponArray[selectedFireArm].GetComponent<SpriteRenderer>().sprite
                // bullets in gun: aWeaponScript.currentClipAmmo
                // bullets in inventory (pool): fireArmAmmo[awAmmoType].amount
            }
        }

        public void UpdateExplosiveGUI()
        {
            if (selectedExplosive == -1)
            {
                guiManager.SetExplosiveGUI(0);
                for (var i = 0; i < explosiveArray.Length; i++)
                {
                    explosiveSlotReds[i].GetComponent<RawImage>().enabled = explosiveAmmo[i].amount <= 0;
                }

                //display no explosives no image or whatever
            }
            else
            {
                // show how many explosives left 
                // explosive sprite to display next to explosive numbers: explosiveArray[selectedExplosive].GetComponent<SpriteRenderer>().sprite
                // explosives left: explosiveAmmo[aeAmmoType].amount
                guiManager.SetExplosiveGUI(explosiveAmmo[aeAmmoType].amount);
                for (var i = 0; i < explosiveArray.Length; i++)
                {
                    explosiveSlotReds[i].GetComponent<RawImage>().enabled = explosiveAmmo[i].amount <= 0;
                }
            }
        }

        public void LoadFromPrefs()
        {
            fireArmAmmo = new[]
            {
                new Ammo // pistol, double pistol ammo
                {
                    amount = 0,
                    maxAmount = 120,
                    name = "Pistol"
                },
                new Ammo // rocket launcher ammo
                {
                    amount = 0,
                    maxAmount = 10,
                    name = "RocketLauncher"
                },
                new Ammo // assault rifle ammo
                {
                    amount = 0,
                    maxAmount = 180,
                    name = "AssaultRifle"
                },
                new Ammo // shotgun ammo
                {
                    amount = 0,
                    maxAmount = 80,
                    name = "Shotgun"
                }
            };

            explosiveAmmo = new[]
            {
                new Ammo // simple grenade ammo
                {
                    amount = 5,
                    maxAmount = 8,
                    name = "fragGrenade"
                },
                new Ammo // simple grenade ammo
                {
                    amount = 3,
                    maxAmount = 8,
                    name = "gravnade"
                }
            };

            //checking if keys for ammo exist and then assigning new ammo values
            for (var i = 0; i < fireArmAmmo.Length; i++)
            {
                if (PlayerPrefs.HasKey(fireArmAmmo[i].name + "Ammo"))
                {
                    //Debug.Log("loaded " + fireArmAmmo[i].name + PlayerPrefs.GetInt(fireArmAmmo[i].name + "Ammo"));
                    fireArmAmmo[i].amount = PlayerPrefs.GetInt(fireArmAmmo[i].name + "Ammo");
                }
            }

            for (var i = 0; i < explosiveAmmo.Length; i++)
            {
                if (PlayerPrefs.HasKey(explosiveAmmo[i].name + "Ammo"))
                {
                    explosiveAmmo[i].amount = PlayerPrefs.GetInt(explosiveAmmo[i].name + "Ammo");
                }
            }

            foreach (var t in weaponArray)
            {
                if (PlayerPrefs.HasKey(t.name + "Discovered"))
                {
                    Debug.Log(t.name + "had a key");
                    t.GetComponent<Weapon>().isDiscovered =
                        PlayerPrefs.GetInt(t.GetComponent<Weapon>().name + "Discovered") == 1;
                    Debug.Log(t.GetComponent<Weapon>().isDiscovered);
                }
                else
                    t.GetComponent<Weapon>().isDiscovered = false;
            }
        }

        private void UpdateWeapon()
        {
            playerAnimator.SetInteger("Weapon", selectedFireArm);
            if (selectedFireArm == -1) // selected knife
            {
                knifeScript.Equip(leftHandSlot);
                rightHandSlot.GetComponent<SpriteRenderer>().sprite = null;
                ammoImage.texture = Resources.Load<Texture>("KnifeImage");
            }
            else
            {
                aWeaponScript = weaponArray[selectedFireArm].GetComponent<Weapon>();
                activeWeaponRTip.transform.localPosition = aWeaponScript.tip.transform.localPosition;
                aWeaponScript.Equip(rightHandSlot);
                awAmmoType = aWeaponScript.ammoType;
                leftHandSlot.GetComponent<SpriteRenderer>().sprite = null;
                weaponAudioSource.clip = aWeaponScript.weaponSound;
                if (aWeaponScript.reloadSound != null)
                    reloadAudioSource.clip = aWeaponScript.reloadSound;
                if (aWeaponScript.emptySound != null)
                    emptyAudioSource.clip = aWeaponScript.emptySound;

                if (selectedFireArm == 4) //dual welded
                {
                    aWeaponScript.Equip(leftHandSlot);
                    activeWeaponLTip.transform.localPosition = aWeaponScript.tip.transform.localPosition;
                }

                switch (awAmmoType)
                {
                    case 0:
                        ammoImage.texture = Resources.Load<Texture>("PistolAmmoClip");
                        break;
                    case 1:
                        // ReSharper disable once StringLiteralTypo
                        ammoImage.texture = Resources.Load<Texture>("Misile");
                        break;
                    case 2:
                        ammoImage.texture = Resources.Load<Texture>("AssaultRifleAmmoClip");
                        break;
                    case 3:
                        ammoImage.texture = Resources.Load<Texture>("ShotgunAmmoClip");
                        break;
                }
            }

            // highlighting selected weapon slot
            SetWeaponSlotHighlight(lastSelectedFireArm, false);
            SetWeaponSlotHighlight(selectedFireArm, true);
            UpdateBulletGUI();
        }

        /// <summary>
        /// Highlights or remove highlight on weapon Slot
        /// </summary>
        /// <param name="idx">index of weapon</param>
        /// <param name="isActive">set weapon as highlighted or not</param>
        private void SetWeaponSlotHighlight(int idx, bool isActive)
        {
            if (idx == -1)
                idx = weaponArray.Length;
            weaponSlots[idx].GetComponent<RawImage>().texture =
                Resources.Load<Texture>((isActive ? "WeaponSlotActive" : "WeaponSlot"));
        }

        private void DisplayDiscoveredWeapon(int idx)
        {
            weaponSlots[idx].transform.GetChild(0).GetComponent<Image>().sprite =
                Resources.Load<Sprite>(weaponArray[idx].GetComponent<Weapon>().ItemName + "Image");
        }

        private void UpdateExplosive()
        {
            if (selectedExplosive == -1)
            {
                //None explosives left

                if (lastSelectedExplosive != -1)
                    explosiveSlots[lastSelectedExplosive].GetComponent<RawImage>().texture =
                        Resources.Load<Texture>("WeaponSlot");
            }
            else
            {
                // setting selection indication
                if (lastSelectedExplosive != -1)
                    explosiveSlots[lastSelectedExplosive].GetComponent<RawImage>().texture =
                        Resources.Load<Texture>("WeaponSlot");
                explosiveSlots[selectedExplosive].GetComponent<RawImage>().texture =
                    Resources.Load<Texture>("WeaponSlotActive");

                activeGrenade = explosiveArray[selectedExplosive];
                aeAmmoType = activeGrenade.GetComponent<Explosive>().AmmoType;

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (selectedExplosive == 0)
                    explosiveImage.texture = Resources.Load<Texture>("GrenadeNew");
                else if (selectedExplosive == 1) explosiveImage.texture = Resources.Load<Texture>("GravnadeNew");
            }

            UpdateExplosiveGUI();
        }

        public void Reload()
        {
            if (isReloading || selectedFireArm == -1) return;
            if (aWeaponScript.currentClipAmmo != aWeaponScript.clipSize && fireArmAmmo[awAmmoType].amount > 0)
                StartCoroutine(ReloadCor());
        }

        private IEnumerator ReloadCor()
        {
            reloadAudioSource.Play();

            isReloading = true;
            playerAnimator.SetFloat("reloadSpeed", 1 / aWeaponScript.reloadTime);
            playerAnimator.SetTrigger("playerReload");

            yield return new WaitForSeconds(aWeaponScript.reloadTime);

            var takenAmmo = fireArmAmmo[awAmmoType].TakeAmmo(aWeaponScript.clipSize - aWeaponScript.currentClipAmmo);
            aWeaponScript.currentClipAmmo += takenAmmo;
            UpdateBulletGUI();
            UpdateWeaponGUI();
            isReloading = false;
        }

        public void AutomaticShoot()
        {
            if (AutomaticWeapons.Any(t => t == selectedFireArm))
            {
                Shoot();
            }
        }

        public void Shoot()
        {
            if (selectedFireArm == -1) // knife attack
            {
                if (!cooldownEnded) return;
                playerAnimator.SetTrigger("Shoot");
                cooldownEnded = false;
                StartCoroutine("Cooldown");
                //mainCameraScript.AddOffset(knifeScript.cameraRecoil);
            }
            else
            {
                if (!cooldownEnded || isReloading) return;
                if (aWeaponScript.currentClipAmmo > 0)
                {
                    cooldownEnded = false;
                    aWeaponScript.currentClipAmmo--;
                    StartCoroutine("Cooldown");
                    mainCameraScript.AddOffset(aWeaponScript.cameraRecoil);
                    playerAlerting.AlertSurroundings(aWeaponScript.alertingRadius);
                    weaponAudioSource.Play();

                    switch (selectedFireArm)
                    {
                        case 0:
                            ShootPistol();
                            break;
                        case 1:
                            ShootRocket();
                            break;
                        case 2:
                            ShootAssaultRifle();
                            break;
                        case 3:
                            playerAnimator.SetTrigger("Shoot");
                            ShootShotgun();
                            break;
                        case 4:
                            ShootDualPistol();
                            break;
                        // ReSharper disable once RedundantEmptySwitchSection
                        default:
                            break;
                    }

                    if (aWeaponScript.currentClipAmmo == 0)
                    {
                        Reload();
                        if (fireArmAmmo[awAmmoType].amount == 0)
                            UpdateWeaponGUI();
                    }

                    UpdateBulletGUI();
                }
                else
                {
                    emptyAudioSource.Play();
                    Reload();
                }
            }
        }

        private void ShootPistol()
        {
            var weaponScript = weaponArray[selectedFireArm].GetComponent<Weapon>();
            EjectWeaponCartridgeCasing(weaponScript);

            var bulletAngle = Random.Range(-weaponScript.accuracy, weaponScript.accuracy);
            var newBullet = Instantiate(aWeaponScript.projectile, activeWeaponRTip.transform.position,
                Quaternion.Euler(0f, 0f, activeWeaponRTip.transform.rotation.eulerAngles.z + bulletAngle), projectiles);
            newBullet.GetComponent<Bullet>().Instantiate(aWeaponScript.timeUntilSelfDestruction,
                aWeaponScript.projectileSpeed, aWeaponScript.damage);
        }

        private void ShootRocket()
        {
            var newRocket = Instantiate(aWeaponScript.projectile, activeWeaponRTip.transform.position,
                rightHandSlot.transform.rotation);
            var rocketLauncher = weaponArray[selectedFireArm].GetComponent<RocketLauncher>();
            newRocket.GetComponent<GuidedMissile>().Instantiate(rocketLauncher.projectileSpeed,
                rocketLauncher.rotationSpeed, rocketLauncher.radius, rocketLauncher.force);
        }

        private void ShootAssaultRifle()
        {
            var weaponScript = weaponArray[selectedFireArm].GetComponent<Weapon>();
            EjectWeaponCartridgeCasing(weaponScript);
            var bulletAngle = Random.Range(-weaponScript.accuracy, weaponScript.accuracy);
            var newBullet = Instantiate(aWeaponScript.projectile, activeWeaponRTip.transform.position,
                Quaternion.Euler(0f, 0f, activeWeaponRTip.transform.rotation.eulerAngles.z + bulletAngle), projectiles);
            newBullet.GetComponent<Bullet>().Instantiate(aWeaponScript.timeUntilSelfDestruction,
                aWeaponScript.projectileSpeed, aWeaponScript.damage);
        }

        private void ShootShotgun()
        {
            var shotgunScript = weaponArray[selectedFireArm].GetComponent<Shotgun>();
            EjectWeaponCartridgeCasing(shotgunScript);
            for (var i = 0; i < shotgunScript.bulletCount; i++)
            {
                var bulletAngle = Random.Range(-shotgunScript.accuracy, shotgunScript.accuracy);
                var newBullet = Instantiate(aWeaponScript.projectile, activeWeaponRTip.transform.position,
                    Quaternion.Euler(0f, 0f, activeWeaponRTip.transform.rotation.eulerAngles.z + bulletAngle),
                    projectiles);
                newBullet.GetComponent<Bullet>().Instantiate(aWeaponScript.timeUntilSelfDestruction,
                    aWeaponScript.projectileSpeed, aWeaponScript.damage);
            }
        }

        private void ShootDualPistol()
        {
            var weaponScript = weaponArray[selectedFireArm].GetComponent<Weapon>();
            EjectWeaponCartridgeCasing(weaponScript);
            EjectWeaponCartridgeCasing(weaponScript, "l");
            var bulletAngle = Random.Range(-weaponScript.accuracy, weaponScript.accuracy);
            var newBullet = Instantiate(aWeaponScript.projectile, activeWeaponRTip.transform.position,
                Quaternion.Euler(0f, 0f, activeWeaponRTip.transform.rotation.eulerAngles.z + bulletAngle), projectiles);
            newBullet.GetComponent<Bullet>().Instantiate(aWeaponScript.timeUntilSelfDestruction,
                aWeaponScript.projectileSpeed, aWeaponScript.damage);
            if (aWeaponScript.currentClipAmmo <= 1) return;
            var newBullet2 = Instantiate(aWeaponScript.projectile, activeWeaponLTip.transform.position,
                Quaternion.Euler(0f, 0f, activeWeaponLTip.transform.rotation.eulerAngles.z + bulletAngle),
                projectiles);
            newBullet2.GetComponent<Bullet>().Instantiate(aWeaponScript.timeUntilSelfDestruction,
                aWeaponScript.projectileSpeed, aWeaponScript.damage);
            aWeaponScript.currentClipAmmo--;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="direction">r - right, l - left</param>
        public void EjectWeaponCartridgeCasing(Weapon weapon, string direction = "r")
        {
            const float ejectionForce = 500;
            switch (direction)
            {
                case "r":
                {
                    var cartridgeCasing = Instantiate(weapon.cartridgeCase, rightHandSlot.transform.position,
                        rightHandSlot.transform.rotation);
                    var rb = cartridgeCasing.GetComponent<Rigidbody2D>();
                    rb.AddForce(transform.right * ejectionForce);
                    break;
                }
                case "l":
                {
                    var cartridgeCasing = Instantiate(weapon.cartridgeCase, leftHandSlot.transform.position,
                        leftHandSlot.transform.rotation);
                    var rb = cartridgeCasing.GetComponent<Rigidbody2D>();
                    rb.AddForce(-transform.right * ejectionForce);
                    break;
                }
                // ReSharper disable once RedundantEmptySwitchSection
                default:
                    break;
            }
        }


        private void OnTriggerEnter2D(Collider2D collision) // knife attack
        {
            var charObj = collision.gameObject.GetComponent<Character>();
            /*
        if (charObj != null && collision.tag != "Player")
        {
            mainCameraScript.AddOffset(knifeScript.cameraRecoil);
            charObj.Damage(knifeScript.damage);
        }
        */

            //getting contact points and setting rotation to the contact normal
            var contacts = new ContactPoint2D[2];
            var contactCount = collision.GetContacts(contacts);

            if (contactCount <= 0) return;
            Vector3 contactPos = contacts[0].point;
            var rot = Quaternion.FromToRotation(transform.forward, contacts[0].normal);

            //mainCameraScript.AddOffset(knifeScript.cameraRecoil);
            if (charObj != null && !collision.CompareTag("Player"))
            {
                charObj.Damage(knifeScript.damage);
                if (!charObj.CompareTag("Enemy")) return;
                var emitter = Instantiate(impactMetal, contactPos, rot);
                // This splits the particle off so it doesn't get deleted with the parent
                emitter.transform.parent = null;
                //Debug.Log("enemy metal");
            }
            else
            {
                // Debug.Log("not an enemy");
                var emitter = Instantiate(impactConcrete, contactPos, rot);
                // This splits the particle off so it doesn't get deleted with the parent
                emitter.transform.parent = null;
            }
        }

        //for explosives throwing
        public void UseExplosive()
        {
            if (isReloading) return;
            if (selectedExplosive == -1)
            {
                //no explosives left
            }
            else
            {
                if (explosiveAmmo[selectedExplosive].amount <= 0) return;
                TakeExplosivesByIndex(selectedExplosive, 1);
                var instantiatePos = transform.position;
                Debug.Log("throw");
                if (explosiveArray[selectedExplosive] != null)
                {
                    Instantiate(activeGrenade, instantiatePos, transform.rotation);
                }
            }
        }

        private IEnumerator Cooldown()
        {
            yield return new WaitForSeconds(selectedFireArm == -1 ? knifeScript.cooldown : aWeaponScript.cooldown);
            cooldownEnded = true;
        }

        // API:

        // API: Explosives manipulation

        public void SwitchExplosiveLeft()
        {
            lastSelectedExplosive = selectedExplosive;
            var i = 0;
            for (; i < explosiveArray.Length; i++) // find explosive with ammo
            {
                selectedExplosive--;
                if (selectedExplosive < 0)
                    selectedExplosive = explosiveArray.Length - 1;
                // if any ammo left:
                if (explosiveAmmo[explosiveArray[selectedExplosive].GetComponent<Explosive>().AmmoType].amount > 0)
                    break;
            }

            if (i == explosiveArray.Length)
                selectedExplosive = -1;
            UpdateExplosive();
        }

        public void SwitchExplosiveRight()
        {
            lastSelectedExplosive = selectedExplosive;
            var i = 0;
            for (; i < explosiveArray.Length; i++) // find explosive with ammo
            {
                selectedExplosive++;
                if (selectedExplosive >= explosiveArray.Length)
                    selectedExplosive = 0;
                // if any ammo left:
                if (explosiveAmmo[explosiveArray[selectedExplosive].GetComponent<Explosive>().AmmoType].amount > 0)
                    break;
            }

            if (i == explosiveArray.Length)
                selectedExplosive = -1;
            UpdateExplosive();
        }

        public void SelectExplosiveByIndex(int index)
        {
            lastSelectedExplosive = selectedExplosive;
            if (index < 0 || index >= explosiveArray.Length) return;
            selectedExplosive = index;
            UpdateExplosive();
        }

        // API: Weapon manipulations

        public void SwitchWeaponLeft()
        {
            if (isReloading) return;
            lastSelectedFireArm = selectedFireArm;
            var i = 0;
            for (; i < weaponArray.Length; i++) // find weapon with ammo
            {
                selectedFireArm--;
                if (selectedFireArm < 0)
                    selectedFireArm = weaponArray.Length - 1;
                // if any ammo left:
                var weapon = weaponArray[selectedFireArm].GetComponent<Weapon>();
                if (fireArmAmmo[weapon.ammoType].amount + weapon.currentClipAmmo > 0 && weapon.isDiscovered)
                    break;
            }

            if (i == weaponArray.Length)
                selectedFireArm = -1;
            UpdateWeapon();
        }

        public void SwitchWeaponRight()
        {
            if (isReloading) return;
            lastSelectedFireArm = selectedFireArm;
            var i = 0;
            for (; i < weaponArray.Length; i++) // find weapon with ammo
            {
                selectedFireArm++;
                if (selectedFireArm >= weaponArray.Length)
                    selectedFireArm = 0;
                // if any ammo left:
                var weapon = weaponArray[selectedFireArm].GetComponent<Weapon>();
                if (fireArmAmmo[weapon.ammoType].amount + weapon.currentClipAmmo > 0 && weapon.isDiscovered)
                    break;
            }

            if (i == weaponArray.Length)
                selectedFireArm = -1;
            UpdateWeapon();
        }

        public void SelectWeaponByIndex(int index)
        {
            if (isReloading) return;
            lastSelectedFireArm = selectedFireArm;
            if (index == -1)
            {
                selectedFireArm = -1;
                UpdateWeapon();
            }
            else if (index >= 0 && index < weaponArray.Length)
            {
                var weapon = weaponArray[index].GetComponent<Weapon>();
                if (fireArmAmmo[weapon.ammoType].amount + weapon.currentClipAmmo <= 0 || !weapon.isDiscovered) return;
                selectedFireArm = index;
                UpdateWeapon();
            }
        }

        public void DiscoverWeaponByIndex(int idx)
        {
            var weapon = weaponArray[idx].GetComponent<Weapon>();
            if (!weapon.isDiscovered) // not yet
            {
                weapon.isDiscovered = true;
                notifications.Notify(weapon.name + " discovered!");
                DisplayDiscoveredWeapon(idx);
            }

            AddAmmoByWeaponIndex(idx, weapon.clipSize * 2);
        }

        public void DiscoverWeaponByName(string name1)
        {
            for (var i = 0; i < weaponArray.Length; i++)
            {
                if (weaponArray[i].name == name1)
                {
                    DiscoverWeaponByIndex(i);
                    break;
                }

                Debug.Log(name1 + " !=" + weaponArray[i].name);
            }
        }

        public void ForgetAllWeapons()
        {
            foreach (var gun in weaponArray)
            {
                gun.GetComponent<Weapon>().isDiscovered = false;
            }
        }

        // API: Ammo manipulations
        /// <summary>
        /// adds ammo by ammo name
        /// </summary>
        /// <param name="name1">name of ammo to add</param>
        /// <param name="amount">amount of ammo to add</param>
        /// <returns>return amount added or -1 if such ammo type doesn't exist</returns>
        public void AddAmmoByName(string name1, int amount)
        {
            for (var i = 0; i < fireArmAmmo.Length; i++)
            {
                if (fireArmAmmo[i].name != name1) continue;
                var added = fireArmAmmo[i].AddAmmo(amount);
                ReloadOnPickup(fireArmAmmo[i].name);
                UpdateWeaponGUI();
                UpdateBulletGUI();
                notifications.Notify(added + " " + fireArmAmmo[i].name + " ammo added");
                return;
            }

            for (var i = 0; i < explosiveAmmo.Length; i++)
                if (explosiveAmmo[i].name == name1)
                {
                    var added = explosiveAmmo[i].AddAmmo(amount);
                    UpdateWeaponGUI();
                    UpdateExplosiveGUI();
                    notifications.Notify(added + " " + explosiveAmmo[i].name + " added");
                    return;
                }
        }

//        /// <summary>
//        /// takes ammo by ammo name
//        /// </summary>
//        /// <param name="name">name of ammo to take</param>
//        /// <param name="amount">amount of ammo to take</param>
//        /// <returns>return amount taken or -1 if such ammo type doesn't exist</returns>
//        public int TakeAmmoByName(string name, int amount)
//        {
//            for (var i = 0; i < fireArmAmmo.Length; i++)
//                if (fireArmAmmo[i].name == name)
//                {
//                    var taken = fireArmAmmo[i].TakeAmmo(amount);
//                    UpdateWeaponGUI();
//                    UpdateBulletGUI();
//                    return taken;
//                }
//
//            for (var i = 0; i < explosiveAmmo.Length; i++)
//                if (explosiveAmmo[i].name == name)
//                {
//                    var taken = explosiveAmmo[i].TakeAmmo(amount);
//                    UpdateWeaponGUI();
//                    UpdateBulletGUI();
//                    return taken;
//                }
//
//            return -1;
//        }

//        /// <summary>
//        /// adds ammo by ammo index
//        /// </summary>
//        /// <param name="index">index of ammo to add</param>
//        /// <param name="amount">amount of ammo to add</param>
//        /// <returns>return amount added or -1 if such ammo doesn't exist</returns>
//        public int AddAmmoByAmmoIndex(int index, int amount)
//        {
//            if (index < 0 || index >= fireArmAmmo.Length) return -1;
//            var added = fireArmAmmo[index].AddAmmo(amount);
//            ReloadOnPickup(index);
//            UpdateWeaponGUI();
//            UpdateBulletGUI();
//            notifications.Notify(added + " " + fireArmAmmo[index].name + " ammo added");
//            return added;
//        }

//        /// <summary>
//        /// takes ammo by ammo index
//        /// </summary>
//        /// <param name="index">index of ammo to take</param>
//        /// <param name="amount">amount of ammo to take</param>
//        /// <returns>return amount taken or -1 if such ammo doesn't exist</returns>
//        public int TakeAmmoByAmmoIndex(int index, int amount)
//        {
//            if (index < 0 || index >= fireArmAmmo.Length) return -1;
//            var taken = fireArmAmmo[index].TakeAmmo(amount);
//            UpdateWeaponGUI();
//            UpdateBulletGUI();
//            return taken;
//        }

        /// <summary>
        /// takes ammo by weapon index
        /// </summary>
        /// <param name="index">index of weapon to add ammo</param>
        /// <param name="amount">amount of ammo to add</param>
        /// <returns>return amount added or -1 if such ammo doesn't exist</returns>
        private void AddAmmoByWeaponIndex(int index, int amount)
        {
            if (index < 0 || index >= weaponArray.Length) return;
            var ammoType = weaponArray[index].GetComponent<Weapon>().ammoType;
            var added = fireArmAmmo[ammoType].AddAmmo(amount);
            ReloadOnPickup(ammoType);
            UpdateWeaponGUI();
            if (index == selectedFireArm)
                UpdateBulletGUI();
            notifications.Notify(added + " " + fireArmAmmo[ammoType].name + " ammo added");
        }

//        /// <summary>
//        /// takes ammo by weapon index
//        /// </summary>
//        /// <param name="index">index of weapon to take ammo</param>
//        /// <param name="amount">amount of ammo to take</param>
//        /// <returns>return amount taken or -1 if such ammo doesn't exist</returns>
//        public int TakeAmmoByWeaponIndex(int index, int amount)
//        {
//            if (index < 0 || index >= weaponArray.Length) return -1;
//            var taken = fireArmAmmo[weaponArray[index].GetComponent<Weapon>().ammoType].TakeAmmo(amount);
//            UpdateWeaponGUI();
//            if (index == selectedFireArm)
//                UpdateBulletGUI();
//            return taken;
//        }

        /// <summary>
        /// adds explosives by explosives index
        /// </summary>
        /// <param name="index">index of explosives to add</param>
        /// <param name="amount">amount of explosives to add</param>
        /// <returns>return amount added or -1 if such explosives doesn't exist</returns>
        public void AddExplosivesByIndex(int index, int amount)
        {
            if (index < 0 || index >= explosiveAmmo.Length) return;
            var added = explosiveAmmo[index].AddAmmo(amount);
            UpdateExplosiveGUI();
            notifications.Notify(added + " " + explosiveAmmo[index].name + " added");
        }

        /// <summary>
        /// takes explosives by explosives index
        /// </summary>
        /// <param name="index">index of explosives to take</param>
        /// <param name="amount">amount of explosives to take</param>
        /// <returns>return amount taken or -1 if such explosives doesn't exist</returns>
        private void TakeExplosivesByIndex(int index, int amount)
        {
            if (index < 0 || index >= explosiveAmmo.Length) return;
            explosiveAmmo[index].TakeAmmo(amount);
            UpdateExplosiveGUI();
        }

        //methods for testing (cheats)
        public void GetAllWeapons()
        {
            foreach (var weapon in weaponArray)
            {
                weapon.GetComponent<Weapon>().isDiscovered = true;
                weapon.GetComponent<Weapon>().currentClipAmmo = weapon.GetComponent<Weapon>().clipSize;
            }

            for (var i = 0; i < fireArmAmmo.Length; i++)
            {
                fireArmAmmo[i].amount = fireArmAmmo[i].maxAmount;
            }
        }
    }
}
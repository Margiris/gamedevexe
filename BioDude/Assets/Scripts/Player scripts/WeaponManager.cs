using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        /// <param name="amount">amount of ammo to add</param>
        /// <returns>returns amount of ammo added</returns>
        public int AddAmmo(int amount)
        {
            if (amount > 0)
            {
                int desiredAmount = amount + this.amount;
                if(desiredAmount > maxAmount)
                {
                    int added = amount - desiredAmount + maxAmount;
                    this.amount = maxAmount;
                    return added;
                }
                else
                {
                    this.amount = desiredAmount;
                    return amount;
                }
            }
            else
                Debug.Log("You can't add negative amount of ammo or add 0. Use TakeAmmo instead");
            return 0;
        }

        /// <summary>
        /// takes ammo from invetory
        /// </summary>
        /// <param name="amountRequested">amount of ammo requested</param>
        /// <returns>returns amount of ammo taken from inventory</returns>
        public int TakeAmmo(int amountRequested)
        {
            if (amountRequested > 0)
            {
                int ammoTaken = Mathf.Min(amount, amountRequested);
                amount -= ammoTaken;
                return ammoTaken;
            }
            else
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
    public ParticleSystem impactFlesh;

    private GameObject activeWeaponRTip;
    private GameObject activeWeaponLTip;
    private bool cooldownEnded = true;
    private bool isReloading = false;
    //private SpriteRenderer spriteRenderer;
    private Allerting playerAlerting;
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

    private int[] AutomaticWeapons = { 2 };


    // Use this for initialization
    void Start ()
    {
        //rightHandSlot = transform.Find("hand_R").GetChild(0).gameObject;
        activeWeaponRTip = rightHandSlot.transform.GetChild(0).gameObject;
        //leftHandSlot = transform.Find("hand_L").GetChild(0).gameObject;
        activeWeaponLTip = leftHandSlot.transform.GetChild(0).gameObject;


        // static information about ofence weapons 
        fireArmAmmo = new Ammo[4];
        explosiveAmmo = new Ammo[2];
        LoadFromPrefs();
        ///

        notifications = GameObject.Find("AchievementManager").GetComponent<AchievementManager>();
        knifeScript = knife.GetComponent<Knife>();
        projectiles = GameObject.Find("Projectiles").transform;
        mainCameraScript = GameObject.Find("Main Camera").GetComponent<CameraScript>();
        guiManager = GameObject.Find("GUI").GetComponent<GUIManager>();
        playerAnimator = GetComponentInChildren<Animator>();
        playerAlerting = GetComponent<Allerting>();
        weaponAudioSource = GetComponent<AudioSource>();
        reloadAudioSource = GetComponents<AudioSource>()[2];
        emptyAudioSource = GetComponents<AudioSource>()[3];
        ammoImage = GameObject.Find("PlayerAmmoText").GetComponentInChildren<RawImage>();
        explosiveImage = GameObject.Find("PlayerExplosiveText").GetComponentInChildren<RawImage>();

        // get weapon slots
        weaponSlots = new GameObject[weaponArray.Length + 1]; // knife at last position
        weaponSlotReds = new RawImage[weaponArray.Length + 1]; // knife at last position
        for (int i = 0; i < weaponArray.Length; i++)
        {
            weaponSlots[i] = GameObject.Find("WeaponSelectionSlot0" + (i + 1).ToString());  // sets the last selected firearm as a knife (also, the selected weapon should be set as knife too, when the knife is implemented)
            weaponSlotReds[i] = weaponSlots[i].transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
        }
        weaponSlots[weaponArray.Length] = GameObject.Find("WeaponSelectionSlot06");
        // get explosive slots
        explosiveSlots = new GameObject[explosiveArray.Length];
        explosiveSlotReds = new RawImage[explosiveArray.Length];
        for (int i = 0; i < explosiveArray.Length; i++)
        {
            explosiveSlots[i] = GameObject.Find("ExplosiveSelectionSlot0" + (i + 1).ToString());  // sets the last selected explosion as a standard grenade
            explosiveSlotReds[i] = explosiveSlots[i].transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
        }
        //GetAllWeapons();
        // fill weapons with bullets and display discovered on start
        for(int i = 0; i < weaponArray.Length; i++)
        {
            Weapon weapon = weaponArray[i].GetComponent<Weapon>();
            if (weapon.isDiscovered)
            {
                weapon.currentClipAmmo = fireArmAmmo[weapon.ammoType].TakeAmmo(weapon.clipSize); // load with bullets from pool
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
        for(int i = 0; i < fireArmAmmo.Length; i++)
            if(fireArmAmmo[i].name == ammoName)
            {
                ReloadOnPickup(i);
                return;
            }
    }

    private void ReloadOnPickup(int ammoidx)
    {
        if(selectedFireArm != -1 && weaponArray[selectedFireArm].GetComponent<Weapon>().ammoType == ammoidx)
        {
            AutoReload(selectedFireArm);
        }
        for (int i = 0; i < weaponArray.Length; i++)
        {
            if (weaponArray[i].GetComponent<Weapon>().ammoType == ammoidx && i != selectedFireArm)
                AutoReload(i);
        }
    }

    private void AutoReload(int weaponidx)
    {
        Weapon weapon = weaponArray[weaponidx].GetComponent<Weapon>();
        if (weapon.isDiscovered)
        {
            weapon.currentClipAmmo += fireArmAmmo[weapon.ammoType].TakeAmmo(weapon.clipSize - weapon.currentClipAmmo);
        }
    }

    //

    public void UpdateWeaponGUI() // update gui
    {
        for (int i = 0; i < weaponArray.Length; i++)
        {
            Weapon weapon = weaponArray[i].GetComponent<Weapon>();
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
        if(selectedFireArm == -1)
        {
            //display no ammo - infinity or whatevs when knife should be selected
            guiManager.SetBulletGUI("∞", "∞");
        }
        else
        {
            Debug.Log(awAmmoType);

            Debug.Log(fireArmAmmo[awAmmoType].amount.ToString());
            guiManager.SetBulletGUI(aWeaponScript.currentClipAmmo.ToString(), fireArmAmmo[awAmmoType].amount.ToString());
            // show how many bullets left 
            // weapon sprite to display next to bullets nums: weaponArray[selectedFireArm].GetComponent<SpriteRenderer>().sprite
            // bullets in gun: aWeaponScript.currentClipAmmo
            // bullets in inventory (pool): fireArmAmmo[awAmmoType].amount
        }
    }

    public void UpdateExplosiveGUI()
    {
        if (selectedExplosive == -1)
        {
            guiManager.SetExplosiveGUI(0);
            for (int i = 0; i < explosiveArray.Length; i++)
            {
                explosiveSlotReds[i].GetComponent<RawImage>().enabled = explosiveAmmo[i].amount <= 0;
            }
            //display no explosives no image or whatevs
        }
        else
        {
            // show how many explosives left 
            // explosive sprite to display next to explosive nums: explosiveArray[selectedExplosive].GetComponent<SpriteRenderer>().sprite
            // explosives left: explosiveAmmo[aeAmmoType].amount
            guiManager.SetExplosiveGUI(explosiveAmmo[aeAmmoType].amount);
            for(int i = 0; i < explosiveArray.Length; i++)
            {
                explosiveSlotReds[i].GetComponent<RawImage>().enabled = explosiveAmmo[i].amount <= 0;
            }
        }
    }

    public void LoadFromPrefs()
    {
        fireArmAmmo = new Ammo[]
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

        explosiveAmmo = new Ammo[]
        {
            new Ammo // simple grande ammo
            {
                amount = 5,
                maxAmount = 8,
                name = "fragGrenade"
            },
            new Ammo // simple grande ammo
            {
                amount = 3,
                maxAmount = 8,
                name = "gravnade"
            }
        };
        
        //checking if keys for ammo exist and then assigning new ammo values
        for (int i = 0; i < fireArmAmmo.Length; i++)
        {
            if (PlayerPrefs.HasKey(fireArmAmmo[i].name + "Ammo"))
            {
                //Debug.Log("loaded " + fireArmAmmo[i].name + PlayerPrefs.GetInt(fireArmAmmo[i].name + "Ammo"));
                fireArmAmmo[i].amount = PlayerPrefs.GetInt(fireArmAmmo[i].name + "Ammo");
            }
        }

        for (int i = 0; i < explosiveAmmo.Length; i++)
        {
            if (PlayerPrefs.HasKey(explosiveAmmo[i].name + "Ammo"))
            {
                explosiveAmmo[i].amount = PlayerPrefs.GetInt(explosiveAmmo[i].name + "Ammo");
            }
        }

        for (int i = 0; i < weaponArray.Length; i++)
        {
            if(PlayerPrefs.HasKey(weaponArray[i].name + "Discovered"))
            {
                Debug.Log(weaponArray[i].name + "had a key");
                weaponArray[i].GetComponent<Weapon>().isDiscovered = PlayerPrefs.GetInt(weaponArray[i].GetComponent<Weapon>().name + "Discovered") == 1 ? true : false;
                Debug.Log(weaponArray[i].GetComponent<Weapon>().isDiscovered);
            }
            else
                weaponArray[i].GetComponent<Weapon>().isDiscovered = false;
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
            if(aWeaponScript.reloadSound != null)
                reloadAudioSource.clip = aWeaponScript.reloadSound;
            if (aWeaponScript.emptySound != null)
                emptyAudioSource.clip = aWeaponScript.emptySound;

            if (selectedFireArm == 4) //dual vielded
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
        // highligting selected weapon slot
        SetWeaponSlotHighlight(lastSelectedFireArm, false);
        SetWeaponSlotHighlight(selectedFireArm, true);
        UpdateBulletGUI();
    }

    /// <summary>
    /// Highligts or remove higlight on weapon Slot
    /// </summary>
    /// <param name="idx">index of weapon</param>
    /// <param name="isActive">set wepon as highligted or not</param>
    private void SetWeaponSlotHighlight(int idx, bool isActive)
    {
        if (idx == -1)
            idx = weaponArray.Length;
        weaponSlots[idx].GetComponent<RawImage>().texture = Resources.Load<Texture>((isActive ? "WeaponSlotActive" : "WeaponSlot"));
    }

    private void DisplayDiscoveredWeapon(int idx)
    {
        weaponSlots[idx].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(weaponArray[idx].GetComponent<Weapon>().ItemName + "Image");
    }

    private void UpdateExplosive()
    {
        if (selectedExplosive == -1)
        {
            //None explosives left

            if (lastSelectedExplosive != -1)
                explosiveSlots[lastSelectedExplosive].GetComponent<RawImage>().texture = Resources.Load<Texture>("WeaponSlot");
        }
        else
        {
            // seting selection indication
            if(lastSelectedExplosive != -1)
                explosiveSlots[lastSelectedExplosive].GetComponent<RawImage>().texture = Resources.Load<Texture>("WeaponSlot");
            explosiveSlots[selectedExplosive].GetComponent<RawImage>().texture = Resources.Load<Texture>("WeaponSlotActive");

            activeGrenade = explosiveArray[selectedExplosive];
            aeAmmoType = activeGrenade.GetComponent<Explosive>().AmmoType;

            // setting selected explosive image gui
            switch(selectedExplosive)
            {
                case 0:
                    explosiveImage.texture = Resources.Load<Texture>("GrenadeNew");
                    break;
                case 1:
                    explosiveImage.texture = Resources.Load<Texture>("GravnadeNew");
                    break;
            }
        }
        UpdateExplosiveGUI();
    }

    public void Reload()
    {
        if (!isReloading && selectedFireArm != -1)
        {
            if (aWeaponScript.currentClipAmmo != aWeaponScript.clipSize && fireArmAmmo[awAmmoType].amount > 0)
                StartCoroutine(Reloadco());
        }
    }

    private IEnumerator Reloadco()
    {
        reloadAudioSource.Play();

        isReloading = true;
        playerAnimator.SetFloat("reloadSpeed", 1 / aWeaponScript.reloadTime);
        playerAnimator.SetTrigger("playerReload");
        
        yield return new WaitForSeconds(aWeaponScript.reloadTime);

        int takenAmmo = fireArmAmmo[awAmmoType].TakeAmmo(aWeaponScript.clipSize - aWeaponScript.currentClipAmmo);
        aWeaponScript.currentClipAmmo += takenAmmo;
        UpdateBulletGUI();
        UpdateWeaponGUI();
        isReloading = false;
    }

    public void AutomaticShoot()
    {
        for(int i = 0; i < AutomaticWeapons.Length; i++)
        {
            if (AutomaticWeapons[i] == selectedFireArm)
            {
                Shoot();
                break;
            }
        }
    }

    public void Shoot()
    {
        if(selectedFireArm == -1) // knife attack
        {
            if (cooldownEnded)
            {
                playerAnimator.SetTrigger("Shoot");
                cooldownEnded = false;
                StartCoroutine("Cooldown");
                //mainCameraScript.AddOffset(knifeScript.cameraRecoil);
            }
        }
        else
        {
            if (cooldownEnded && !isReloading)
            {
                if (aWeaponScript.currentClipAmmo > 0)
                {
                    cooldownEnded = false;
                    aWeaponScript.currentClipAmmo--;
                    StartCoroutine("Cooldown");
                    mainCameraScript.AddOffset(aWeaponScript.cameraRecoil);
                    playerAlerting.AllertSurroundings(aWeaponScript.allertingRadius);
                    weaponAudioSource.Play();

                    switch (selectedFireArm) ////////////requires optimisation - maybe code firing in weapon prefabs, or leave like this
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
    }

    private void ShootPistol()
    {
        Weapon weaponScript = weaponArray[selectedFireArm].GetComponent<Weapon>();
        EjectWeaponCartridgeCasing(weaponScript);
        
        float bulletAngle = Random.Range(-weaponScript.accuracy, weaponScript.accuracy);
        GameObject newBullet = Instantiate(aWeaponScript.projectile, activeWeaponRTip.transform.position, Quaternion.Euler(0f, 0f, activeWeaponRTip.transform.rotation.eulerAngles.z + bulletAngle), projectiles);
        newBullet.GetComponent<Bullet>().Instantiate(aWeaponScript.timeUntilSelfDestrucion, aWeaponScript.projectileSpeed, aWeaponScript.damage);
    }

    private void ShootRocket()
    {
        GameObject newRocket = Instantiate(aWeaponScript.projectile, activeWeaponRTip.transform.position, rightHandSlot.transform.rotation);
        RocketLauncher rocketLauncher = weaponArray[selectedFireArm].GetComponent<RocketLauncher>();
        newRocket.GetComponent<GuidedMisile>().Instantiate(rocketLauncher.projectileSpeed, rocketLauncher.rotationSpeed, rocketLauncher.radius, rocketLauncher.force);
    }
    
    private void ShootAssaultRifle()
    {
        Weapon weaponScript = weaponArray[selectedFireArm].GetComponent<Weapon>();
        EjectWeaponCartridgeCasing(weaponScript);
        float bulletAngle = Random.Range(-weaponScript.accuracy, weaponScript.accuracy);
        GameObject newBullet = Instantiate(aWeaponScript.projectile, activeWeaponRTip.transform.position , Quaternion.Euler(0f, 0f, activeWeaponRTip.transform.rotation.eulerAngles.z + bulletAngle), projectiles);
        newBullet.GetComponent<Bullet>().Instantiate(aWeaponScript.timeUntilSelfDestrucion, aWeaponScript.projectileSpeed, aWeaponScript.damage);
    }

    private void ShootShotgun()
    {
        Shotgun shotgunscript = weaponArray[selectedFireArm].GetComponent<Shotgun>();
        EjectWeaponCartridgeCasing(shotgunscript);
        float bulletAngle;
        GameObject newBullet;
        for (int i = 0; i < shotgunscript.bulletCount; i++)
        {
            bulletAngle = Random.Range(-shotgunscript.accuracy, shotgunscript.accuracy);
            newBullet = Instantiate(aWeaponScript.projectile, activeWeaponRTip.transform.position, Quaternion.Euler(0f, 0f, activeWeaponRTip.transform.rotation.eulerAngles.z + bulletAngle), projectiles);
            newBullet.GetComponent<Bullet>().Instantiate(aWeaponScript.timeUntilSelfDestrucion, aWeaponScript.projectileSpeed, aWeaponScript.damage);
        }
    }

    private void ShootDualPistol()
    {
        Weapon weaponScript = weaponArray[selectedFireArm].GetComponent<Weapon>();
        EjectWeaponCartridgeCasing(weaponScript);
        EjectWeaponCartridgeCasing(weaponScript, "l");
        float bulletAngle = Random.Range(-weaponScript.accuracy, weaponScript.accuracy);
        GameObject newBullet = Instantiate(aWeaponScript.projectile, activeWeaponRTip.transform.position, Quaternion.Euler(0f, 0f, activeWeaponRTip.transform.rotation.eulerAngles.z + bulletAngle), projectiles);
        newBullet.GetComponent<Bullet>().Instantiate(aWeaponScript.timeUntilSelfDestrucion, aWeaponScript.projectileSpeed, aWeaponScript.damage);
        if (aWeaponScript.currentClipAmmo > 1) // if only one bullet left in clip so two guns can't fire
        {
            GameObject newBullet2 = Instantiate(aWeaponScript.projectile, activeWeaponLTip.transform.position, Quaternion.Euler(0f, 0f, activeWeaponLTip.transform.rotation.eulerAngles.z + bulletAngle), projectiles);
            newBullet2.GetComponent<Bullet>().Instantiate(aWeaponScript.timeUntilSelfDestrucion, aWeaponScript.projectileSpeed, aWeaponScript.damage);
            aWeaponScript.currentClipAmmo--;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weapon"></param>
    /// <param name="direction">r - right, l - left</param>
    public void EjectWeaponCartridgeCasing(Weapon weapon, string direction = "r")
    {
        float ejectionForce = 500;
        if (direction == "r")
        {
            GameObject cartridgeCasing = Instantiate(weapon.cartridgeCase, rightHandSlot.transform.position, rightHandSlot.transform.rotation);
            Rigidbody2D rb = cartridgeCasing.GetComponent<Rigidbody2D>();
            rb.AddForce(transform.right * ejectionForce);
        }
        else if (direction == "l")
        {
            GameObject cartridgeCasing = Instantiate(weapon.cartridgeCase, leftHandSlot.transform.position, leftHandSlot.transform.rotation);
            Rigidbody2D rb = cartridgeCasing.GetComponent<Rigidbody2D>();
            rb.AddForce(-transform.right * ejectionForce);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision) // knife attack
    {
        Character charObj = collision.gameObject.GetComponent<Character>();
        /*
        if (charObj != null && collision.tag != "Player")
        {
            mainCameraScript.AddOffset(knifeScript.cameraRecoil);
            charObj.Damage(knifeScript.damage);
        }
        */
        
        //getting contact points and setting rotation to the contact normal
        ContactPoint2D[] contacts = new ContactPoint2D[2];
        int contactCount = collision.GetContacts(contacts);

        if (contactCount > 0)
        {
            Vector3 contactPos = contacts[0].point;
            Quaternion rot = Quaternion.FromToRotation(transform.forward, contacts[0].normal);

            //mainCameraScript.AddOffset(knifeScript.cameraRecoil);
            if (charObj != null && collision.tag != "Player")
            {
                charObj.Damage(knifeScript.damage);
                if (charObj.tag == "Enemy")
                {
                    ParticleSystem emitter = Instantiate(impactMetal, contactPos, rot);
                    // This splits the particle off so it doesn't get deleted with the parent
                    emitter.transform.parent = null;
                    //Debug.Log("enemy metal");
                }
                
            }
            else
            {
                // Debug.Log("not an enemy");
                ParticleSystem emitter = Instantiate(impactConcrete, contactPos, rot);
                // This splits the particle off so it doesn't get deleted with the parent
                emitter.transform.parent = null;
            }

        }
        
     }

    //for explosives throwing
    public void UseExplosive()
    {
        if (!isReloading)
        {
            if (selectedExplosive == -1)
            {
                //no explosives left
            }
            else
            {
                if(explosiveAmmo[selectedExplosive].amount > 0)
                {
                    TakeExplosivesByIndex(selectedExplosive, 1);
                    Vector3 instantiatePos = transform.position;
                    Debug.Log("throw");
                    if (explosiveArray[selectedExplosive] != null)
                    {
                        //var nade = PrefabUtility.InstantiatePrefab(explosiveArray[selectedExplosive]) as GameObject;
                        GameObject nade = Instantiate(activeGrenade, instantiatePos, transform.rotation);
                        //nade.Throw(500);
                    }
                }
            }
        }
    }
    
    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(selectedFireArm == -1? knifeScript.cooldown : aWeaponScript.cooldown);
        cooldownEnded = true;
    }
    
    // API:

        // API: Explosives manipulation

    public void SwitchExplosiveLeft()
    {
        lastSelectedExplosive = selectedExplosive;
        int i = 0;
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
        int i = 0;
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
        if (index >= 0 && index < explosiveArray.Length)
        {
            selectedExplosive = index;
            UpdateExplosive();
        }
    }

    // API: Weapon manipulations

    public void SwitchWeaponLeft()
    {
        if (!isReloading)
        {
            lastSelectedFireArm = selectedFireArm;
            int i = 0;
            for (; i < weaponArray.Length; i++) // find weapon with ammo
            {
                selectedFireArm--;
                if (selectedFireArm < 0)
                    selectedFireArm = weaponArray.Length - 1;
                // if any ammo left:
                Weapon weapon = weaponArray[selectedFireArm].GetComponent<Weapon>();
                if (fireArmAmmo[weapon.ammoType].amount + weapon.currentClipAmmo > 0 && weapon.isDiscovered)
                    break;
            }
            if (i == weaponArray.Length)
                selectedFireArm = -1;
            UpdateWeapon();
        }
    }

    public void SwitchWeaponRight()
    {
        if (!isReloading)
        {
            lastSelectedFireArm = selectedFireArm;
            int i = 0;
            for (; i < weaponArray.Length; i++) // find weapon with ammo
            {
                selectedFireArm++;
                if (selectedFireArm >= weaponArray.Length)
                    selectedFireArm = 0;
                // if any ammo left:
                Weapon weapon = weaponArray[selectedFireArm].GetComponent<Weapon>();
                if (fireArmAmmo[weapon.ammoType].amount + weapon.currentClipAmmo > 0 && weapon.isDiscovered)
                    break;
            }
            if (i == weaponArray.Length)
                selectedFireArm = -1;
            UpdateWeapon();
        }
    }

    public void SelectWeaponByIndex(int index)
    {
        if (!isReloading)
        {
            lastSelectedFireArm = selectedFireArm;
            if(index == -1)
            {
                selectedFireArm = -1;
                UpdateWeapon();
            }
            else if (index >= 0 && index < weaponArray.Length)
            {
                Weapon weapon = weaponArray[index].GetComponent<Weapon>();
                if (fireArmAmmo[weapon.ammoType].amount + weapon.currentClipAmmo > 0 && weapon.isDiscovered)
                {
                    selectedFireArm = index;
                    UpdateWeapon();
                }
            }
        }
    }

    public void DiscoverWeaponByindex(int idx)
    {
        Weapon weapon = weaponArray[idx].GetComponent<Weapon>();
        if (!weapon.isDiscovered) // not yet
        {
            weapon.isDiscovered = true;
            notifications.Notify(weapon.name + " discovered!");
            DisplayDiscoveredWeapon(idx);
        }
        AddAmmoByWeaponIndex(idx, weapon.clipSize * 2);
    }

    public void DiscoverWeaponByName(string name)
    {
        for (int i = 0; i < weaponArray.Length; i++)
        {
            if (weaponArray[i].name == name)
            {
                DiscoverWeaponByindex(i);
                break;
            }
            else
                Debug.Log(name + " !=" + weaponArray[i].name);
        }
    }

    public void UndiscoverAllWeapons()
    {
        foreach(GameObject gun in weaponArray)
        {
            gun.GetComponent<Weapon>().isDiscovered = false;
        }
    }

    // API: Ammo manipulations
    /// <summary>
    /// adds ammo by ammo name
    /// </summary>
    /// <param name="name">name of ammo to add</param>
    /// <param name="amount">amount of ammo to add</param>
    /// <returns>return amount added or -1 if such ammo type doesn't exist</returns>
    public int AddAmmoByName(string name, int amount)
    {
        for (int i = 0; i < fireArmAmmo.Length; i++)
        {
            if (fireArmAmmo[i].name == name)
            {
                int added = fireArmAmmo[i].AddAmmo(amount);
                ReloadOnPickup(fireArmAmmo[i].name);
                UpdateWeaponGUI();
                UpdateBulletGUI();
                notifications.Notify(added.ToString() + " " + fireArmAmmo[i].name.ToString() + " ammo added");
                return added;
            }
        }
        for (int i = 0; i < explosiveAmmo.Length; i++)
            if (explosiveAmmo[i].name == name)
            {
                int added = explosiveAmmo[i].AddAmmo(amount);
                UpdateWeaponGUI();
                UpdateExplosiveGUI();
                notifications.Notify(added.ToString() + " " + explosiveAmmo[i].name.ToString() + " added");
                return added;
            }
        return -1;
    }

    /// <summary>
    /// takes ammo by ammo name
    /// </summary>
    /// <param name="name">name of ammo to take</param>
    /// <param name="amount">amount of ammo to take</param>
    /// <returns>return amount taken or -1 if such ammo type doesn't exist</returns>
    public int TakeAmmoByName(string name, int amount)
    {
        for (int i = 0; i < fireArmAmmo.Length; i++)
            if (fireArmAmmo[i].name == name)
            {
                int taken = fireArmAmmo[i].TakeAmmo(amount);
                UpdateWeaponGUI();
                UpdateBulletGUI();
                return taken;
            }
        for (int i = 0; i < explosiveAmmo.Length; i++)
            if (explosiveAmmo[i].name == name)
            {
                int taken = explosiveAmmo[i].TakeAmmo(amount);
                UpdateWeaponGUI();
                UpdateBulletGUI();
                return taken;
            }
        return -1;
    }

    /// <summary>
    /// adds ammo by ammo index
    /// </summary>
    /// <param name="index">index of ammo to add</param>
    /// <param name="amount">amount of ammo to add</param>
    /// <returns>return amount added or -1 if such ammo doesn't exist</returns>
    public int AddAmmoByAmmoIndex(int index, int amount)
    {
        if (index >= 0 && index < fireArmAmmo.Length)
        {
            int added = fireArmAmmo[index].AddAmmo(amount);
            ReloadOnPickup(index);
            UpdateWeaponGUI();
            UpdateBulletGUI();
            notifications.Notify(added.ToString() + " " + fireArmAmmo[index].name.ToString() + " ammo added");
            return added;
        }
        return -1;
    }

    /// <summary>
    /// takes ammo by ammo index
    /// </summary>
    /// <param name="index">index of ammo to take</param>
    /// <param name="amount">amount of ammo to take</param>
    /// <returns>return amount taken or -1 if such ammo doesn't exist</returns>
    public int TakeAmmoByAmmoIndex(int index, int amount)
    {
        if (index >= 0 && index < fireArmAmmo.Length)
        {
            int taken = fireArmAmmo[index].TakeAmmo(amount);
            UpdateWeaponGUI();
            UpdateBulletGUI();
            return taken;
        }
        return -1;
    }

    /// <summary>
    /// takes ammo by weapon index
    /// </summary>
    /// <param name="index">index of weapon to add ammo</param>
    /// <param name="amount">amount of ammo to add</param>
    /// <returns>return amount added or -1 if such ammo doesn't exist</returns>
    public int AddAmmoByWeaponIndex(int index, int amount)
    {
        if (index >= 0 && index < weaponArray.Length)
        {
            int ammoType = weaponArray[index].GetComponent<Weapon>().ammoType;
            int added = fireArmAmmo[ammoType].AddAmmo(amount);
            ReloadOnPickup(ammoType);
            UpdateWeaponGUI();
            if (index == selectedFireArm)
                UpdateBulletGUI();
            notifications.Notify(added.ToString() + " " + fireArmAmmo[ammoType].name.ToString() + " ammo added");
            return added;
        }
        return -1;
    }

    /// <summary>
    /// takes ammo by weapon index
    /// </summary>
    /// <param name="index">index of weapon to take ammo</param>
    /// <param name="amount">amount of ammo to take</param>
    /// <returns>return amount taken or -1 if such ammo doesn't exist</returns>
    public int TakeAmmoByWeaponIndex(int index, int amount)
    {
        if (index >= 0 && index < weaponArray.Length)
        {
            int taken = fireArmAmmo[weaponArray[index].GetComponent<Weapon>().ammoType].TakeAmmo(amount);
            UpdateWeaponGUI();
            if(index == selectedFireArm)
                UpdateBulletGUI();
            return taken;
        }
        return -1;
    }

    /// <summary>
    /// adds explosives by explosives index
    /// </summary>
    /// <param name="index">index of explosives to add</param>
    /// <param name="amount">amount of explosives to add</param>
    /// <returns>return amount added or -1 if such explosives doesn't exist</returns>
    public int AddExplosivesByIndex(int index, int amount)
    {
        if (index >= 0 && index < explosiveAmmo.Length)
        {
            int added = explosiveAmmo[index].AddAmmo(amount);
            UpdateExplosiveGUI();
            notifications.Notify(added.ToString() + " " + explosiveAmmo[index].name.ToString() + " added");
            return added;
        }
        return -1;
    }

    /// <summary>
    /// takes explosives by explosives index
    /// </summary>
    /// <param name="index">index of explosives to take</param>
    /// <param name="amount">amount of explosives to take</param>
    /// <returns>return amount taken or -1 if such explosives doesn't exist</returns>
    public int TakeExplosivesByIndex(int index, int amount)
    {
        if(index >= 0 && index < explosiveAmmo.Length)
        {
            int taken = explosiveAmmo[index].TakeAmmo(amount);
            UpdateExplosiveGUI();
            return taken;
        }
        return -1;
    }

    //methods for testing (cheats)
    public void GetAllWeapons()
    {
        foreach (GameObject weapon in weaponArray)
        {
            weapon.GetComponent<Weapon>().isDiscovered = true;
            weapon.GetComponent<Weapon>().currentClipAmmo = weapon.GetComponent<Weapon>().clipSize;
        }
        for (int i = 0; i < fireArmAmmo.Length; i++)
        {
            fireArmAmmo[i].amount = fireArmAmmo[i].maxAmount;
        }

    }
}

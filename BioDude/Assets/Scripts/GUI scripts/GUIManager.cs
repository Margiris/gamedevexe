using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{

    public Slider healthBar;
    public Text HPText;
    public player playerCharacter;
    public Text AmmoText;
    WeaponManager weaponManager;
    public Text ExplosiveText;

    // Use this for initialization
    void Start()
    {
        playerCharacter = transform.parent.GetComponent<player>();
        weaponManager = transform.parent.GetComponent<WeaponManager>();
        healthBar.maxValue = playerCharacter.healthMax;
        //AmmoText = transform.Find("PlayerAmmoText").GetComponent<Text>();
        //ExplosiveText = transform.Find("PlayerExplosiveText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = playerCharacter.GetHealth();
        HPText.text = "HP: " + playerCharacter.GetHealth() + "/" + playerCharacter.healthMax;
    }

    public void SetBulletGUI(string currentClipAmmo, string currentAmmo)
    {
        AmmoText.text = currentClipAmmo + "/" + currentAmmo;
    }

    public void SetExplosiveGUI(int currentExplosiveAmmo)
    {
        ExplosiveText.text = currentExplosiveAmmo.ToString();
    }
}
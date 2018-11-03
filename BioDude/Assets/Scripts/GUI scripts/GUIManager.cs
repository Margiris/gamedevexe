using Player_scripts;
using UnityEngine;
using UnityEngine.UI;

namespace GUI_scripts
{
    public class GUIManager : MonoBehaviour
    {
        public Slider healthBar;
        public Text HPText;
        private player playerCharacter;
        public Text AmmoText;
        public Text ExplosiveText;

        // Use this for initialization
        private void Start()
        {
            playerCharacter = GameObject.Find("player").GetComponent<player>();
            playerCharacter.GetComponent<WeaponManager>();
            healthBar.maxValue = playerCharacter.healthMax;
            //AmmoText = transform.Find("PlayerAmmoText").GetComponent<Text>();
            //ExplosiveText = transform.Find("PlayerExplosiveText").GetComponent<Text>();
        }

        // Update is called once per frame
        private void Update()
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
}
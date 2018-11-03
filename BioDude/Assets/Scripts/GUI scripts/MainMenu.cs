using UnityEngine;
using UnityEngine.UI;

namespace GUI_scripts
{
    public class MainMenu : MonoBehaviour
    {
        // Use this for initialization
        private void Start()
        {
            var continueButton = gameObject.GetComponentInChildren<LoadSceneFromMenu>().GetComponent<Button>();
            if (!PlayerPrefs.HasKey("PlayerHP"))
            {
                continueButton.interactable = false;
            }
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
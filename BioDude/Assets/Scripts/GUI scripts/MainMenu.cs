using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Button continueButton = gameObject.GetComponentInChildren<LoadSceneFromMenu>().GetComponent<Button>();
        if(!PlayerPrefs.HasKey("PlayerHP"))
        {
            continueButton.interactable = false;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void QuitGame()
    {
        Application.Quit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {

    public static bool PausemenuOpen = false;
    public static bool blockPause = false;
    public GameObject PauseMenuUI;
    public GameObject PauseMenuPanel;
    public GameObject OptionsMenuPanel;
    public GameObject GameOverMenu;
    public GameObject DeathSplashImage;
    public GameObject LevelClearedMenu;
    private LevelManager lvlManager;
    player _player;
    DialogueManager DialManager;


    private float time;
    void Start()
    {
        _player = GameObject.Find("player").GetComponent<player>();
        lvlManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        PauseMenuUI.SetActive(false);  //disabling pausemenu canvas because it should only be active when pausemenu is summoned
        DialManager = GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>();
        ResetPanels();
        blockPause = false;

    }
    public void ResetPanels()
    {
        PauseMenuPanel.SetActive(true);
        OptionsMenuPanel.SetActive(false);
        GameOverMenu.SetActive(false);
        DeathSplashImage.SetActive(false);
        LevelClearedMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        if(blockPause)
        {

        }
        else
		if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (PausemenuOpen)
                Resume();
            else if(!DialManager.IsDialogueOpen() && !blockPause)
                Pause();
        }
	}

    public void OnEnable()
    {
        ResetPanels();
    }
    public void Pause()
    {
        _player.SetAbleToMove(false);
        DialManager.SetDialogueState(false);
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        PausemenuOpen = true;
        ResetPanels();
    }

    public void Resume()
    {
        _player.SetAbleToMove(true);
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        PausemenuOpen = false;
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Resume();
    }
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(1);
        Resume();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadNextLevel()
    {
        LevelManager lvlManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        Time.timeScale = 1f;
        lvlManager.LoadNextLevel();
    }
    public void ShowNextLevelScreen()
    {
        blockPause = true;
        Pause();
        PauseMenuPanel.SetActive(false);
        LevelClearedMenu.SetActive(true);
        //if(SceneManager.GetActiveScene().buildIndex == SceneManager.GetAllScenes().Length - 1)

        //if (nextSceneIndex >= SceneManager.sceneCount)
        if (lvlManager.LastLevel)
        {
            LevelClearedMenu.transform.Find("NextLevelButton").GetComponent<Button>().interactable = false;
            LevelClearedMenu.transform.Find("LevelClearedText").GetComponent<Text>().text += "\n This is the end of the Game.\nThank you for playing!";
        }
    }

    public IEnumerator PlayerDeath()
    {
        blockPause = true;
        yield return new WaitForSeconds(1);
        PauseMenuUI.SetActive(true);
        ResetPanels();
        PauseMenuPanel.SetActive(false);
        PauseMenuUI.GetComponent<Animator>().Play("PausemenuFade");
        DeathSplashImage.SetActive(true);
        DeathSplashImage.GetComponent<Animator>().Play("DeathImageSplash");
        yield return new WaitForSeconds(3);
        //Time.timeScale = 0;   //comment/uncomment to disable/enable the flow of time
        GameOverMenu.SetActive(true);
    }


}

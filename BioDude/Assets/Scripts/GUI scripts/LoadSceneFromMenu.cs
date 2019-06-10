using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadSceneFromMenu : MonoBehaviour
{

    public GameObject[] ObjectToMove;
    
    private void Awake()
    {
        foreach (var item in ObjectToMove)
        {
            DontDestroyOnLoad(item);
        }
        Time.timeScale = 1;
    }
    public void NewGame()
    {
        Destroy(GameObject.Find("MainMenuCanvas"));

        GamePrefs.DeletePlayerProgress();
        LoadByIndex(StaticsConfig.LobbyIdx);
    }
    public void ContinueGame()
    {
        int indexToLoad = PlayerPrefs.GetInt("LastLevelCheckpoint");
        Destroy(GameObject.Find("MainMenuCanvas"));
        LoadByIndex(indexToLoad);
    }

    void LoadByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        Time.timeScale = 1f;
    }
}

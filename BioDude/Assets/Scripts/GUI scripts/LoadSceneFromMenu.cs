using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadSceneFromMenu : MonoBehaviour
{

    public GameObject[] ObjectToMove;
    
    public AchievementManager achievementManager;
    public GameObject achievementPanel;

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
        achievementManager.DestroyAllAchievements();
        Destroy(GameObject.Find("MainMenuCanvas"));

        GamePrefs.DeletePlayerProgress();
        LoadByIndex(2);
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

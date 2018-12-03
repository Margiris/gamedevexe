using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    [SerializeField]
    int EnemiesOnMapLeft = 0;
    public bool clear = false;
    [SerializeField]
    public bool LastLevel = false;
    PauseMenu Pausemenu;
    string LastLevelKeyName = "LastLevelCheckpoint";
	// Use this for initialization
    void Start()
    {
        GameObject obj = GameObject.Find("Pausemenu Canvas");
        if(obj != null)
            Pausemenu = obj.GetComponent<PauseMenu>();
        if (GameObject.Find("Enemies") != null)
            EnemiesOnMapLeft = GameObject.Find("Enemies").transform.childCount;
        if (SceneManager.GetActiveScene().buildIndex > 0 &&
            SceneManager.GetActiveScene().name != "Menu")
            SaveCurrentLevelIndex();
        if (SceneManager.GetActiveScene().buildIndex >= 4)
            LastLevel = true;
    }

    public void LevelCleared()
    {
        //play level finished screen with option to load next level
        Pausemenu.ShowNextLevelScreen();
        Debug.Log("Stage cleared");
    }

    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex +1;
        PlayerPrefs.SetInt(LastLevelKeyName, nextSceneIndex);
        SceneManager.LoadScene(nextSceneIndex);
    }
    public int GetLastLevelIndex()
    {
        if (PlayerPrefs.HasKey(LastLevelKeyName))
            return PlayerPrefs.GetInt(LastLevelKeyName);
        else
            return -1;
    }
    public void SaveCurrentLevelIndex()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt(LastLevelKeyName, currentSceneIndex);
        PlayerPrefs.Save();
    }

    public bool DoesPlayerProgressExist()
    {
        return PlayerPrefs.HasKey(LastLevelKeyName);
    }

    public void EnemyDefeated()
    {
        EnemiesOnMapLeft--;
        if (EnemiesOnMapLeft <= 0)
        {
            clear = true;
            GameObject.Find("Exit").GetComponent<LevelManagerTrigger>().OpenExit();
        }
    }
}

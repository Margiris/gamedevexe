using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class LevelManager : NetworkBehaviour {

    [SerializeField]
    int EnemiesOnMapLeft = 0;
    int playersOnMap = 0;
    public bool clear = false;
    [SerializeField]
    public bool LastLevel = false;
    PauseMenu Pausemenu;
    string LastLevelKeyName = "LastLevelCheckpoint";
    int IDxgen = 0;

    List<Gamer> players;
    List<Tank> enemies;

	// Use this for initialization
    void Start()
    {
        
        GameObject obj = GameObject.Find("Pausemenu Canvas");
        Transform enemyparent = null;
        try
        {
            enemyparent = GameObject.Find("Enemies").transform;
        }
        catch (NullReferenceException e){}

        if (obj != null)
            Pausemenu = obj.GetComponent<PauseMenu>();
        if (GameObject.Find("Enemies") != null)
            EnemiesOnMapLeft = enemyparent.childCount;
        if (SceneManager.GetActiveScene().buildIndex > 0 &&
            SceneManager.GetActiveScene().name != "Menu")
            SaveCurrentLevelIndex();
        if (SceneManager.GetActiveScene().buildIndex >= 4)
            LastLevel = true;
        enemies = new List<Tank>();
        players = new List<Gamer>();
        for (int i = 0; i < EnemiesOnMapLeft; i++)
        {
            enemies.Add(enemyparent.GetChild(i).GetComponent<Tank>());
        }
        if (EnemiesOnMapLeft <= 0)
        {
            clear = true;
            
            try
            {
                GameObject.Find("Exit").GetComponent<LevelManagerTrigger>().OpenExit();
            }
            catch (NullReferenceException e){}
        }
    }

    public void LevelCleared(player player)
    {
        //play level finished screen with option to load next level
        player.transform.parent.GetComponent<Gamer>().pausemenu.ShowNextLevelScreen();
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

    //Player management
    public int RegisterNewPlayer(Gamer player)
    {
        playersOnMap++;
        players.Add(player);
        player.setPLayerID(IDxgen++);
        UpdateEnemies();
        return players.Count - 1;
    }
    
    public void DisconnectPlayer(int playerID)
    {
        players.Remove(players.Find(e => e.getPlayerID() == playerID));
        playersOnMap--;
        UpdateEnemies();
    }

    void UpdateEnemies()
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].UpdatePLayerList(players);
        }
    }

    public List<Gamer> GetPlayersData()
    {
        if(players == null)
        {
            return new List<Gamer>();
        }
        return players;
    }
}

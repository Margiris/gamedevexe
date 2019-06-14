using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class LevelManager : NetworkBehaviour {
    
    [SyncVar]
    int EnemiesOnMapLeft = 0;
    [SyncVar]
    int playersOnMap = 0;
    [SyncVar]
    public bool clear = false;
    [SerializeField]
    public bool LastLevel = false;
    PauseMenu Pausemenu;
    string LastLevelKeyName = "LastLevelCheckpoint";
    int IDxgen = 0;
    List<GameObject> players;
    List<Tank> enemies;

	// Use this for initialization
    void Start()
    {
        Debug.Log("levelmanager start :" + isServer);
        GameObject obj = GameObject.Find("Pausemenu Canvas");
        //Transform enemyparent = null;
        // try
        //{
        //    enemyparent = GameObject.Find("Enemies").transform;
        //}
        //catch (NullReferenceException e){}

       
        if (obj != null)
            Pausemenu = obj.GetComponent<PauseMenu>();

        ScanForEnemies();
        if (SceneManager.GetActiveScene().buildIndex > 0 &&
            SceneManager.GetActiveScene().name != "Menu")
            SaveCurrentLevelIndex();
        if (SceneManager.GetActiveScene().buildIndex >= 4)
            LastLevel = true;
        players = new List<GameObject>();
        
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

    public void ScanForEnemies()
    {
        enemies = new List<Tank>();
        if (GameObject.FindGameObjectsWithTag("Enemy") != null)
        {
            GameObject[] _enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enem in _enemies)
            {
                Tank tank;
                if ((tank = enem.GetComponent<Tank>()) != null)
                {
                    enemies.Add(tank);
                }
            }
            EnemiesOnMapLeft = enemies.Count;
        }
    }

    public void LevelCleared(player player)
    {
        if (isLocalPlayer)
        {
            //play level finished screen with option to load next level
            player.transform.parent.GetComponent<Gamer>().pausemenu.ShowNextLevelScreen();
            Debug.Log("Stage cleared");
        }
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
    
    public void EnemyDefeated(GameObject deadEnemy)
    {
        enemies.Remove(deadEnemy.GetComponent<Tank>());
        EnemiesOnMapLeft--;
        Debug.Log("before scan " + EnemiesOnMapLeft + " : " + enemies.Count);
        if (EnemiesOnMapLeft <= 0)
        {
            clear = true;
            RpcOpenExit();
        }
    }

    [ClientRpc]
    public void RpcOpenExit()
    {
        Debug.Log("iskviestas open exit ar servas: " + isServer);
        GameObject.Find("Exit").GetComponent<LevelManagerTrigger>().OpenExit();
    }

    //Player management
    public void RegisterNewPlayer(GameObject player)
    {
        playersOnMap++;
        players.Add(player);
        Debug.Log("Connecting new player"+ IDxgen);
        player.GetComponent<Gamer>().setPLayerID(IDxgen++);
        UpdateEnemies();
        //return players.Count - 1;
    }

    [Command]
    public void CmdDisconnectPlayer(int playerID)
    {
        if (isServer)
        {
            players.Remove(players.Find(e => e.GetComponent<Gamer>().getPlayerID() == playerID));
            playersOnMap--;
            UpdateEnemies();
        }
    }
    
    void UpdateEnemies()
    {
        if (isServer)
        {
            Debug.Log("updating player for " + enemies.Count + " enemies");
            Debug.Log(players);
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].UpdatePLayerList();
            }
        }
    }

    public List<GameObject> GetPlayersData()
    {
        if(players == null)
        {
            return new List<GameObject>();
        }
        return players;
    }
}

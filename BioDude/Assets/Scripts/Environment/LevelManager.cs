using GUI_scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Environment
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private int EnemiesOnMapLeft;
        public bool clear;
        [SerializeField] public bool LastLevel;

        private PauseMenu PauseMenu;

        private const string LastLevelKeyName = "LastLevelCheckpoint";

        // Use this for initialization
        private void Start()
        {
            var obj = GameObject.Find("Pausemenu Canvas");
            if (obj != null)
                PauseMenu = obj.GetComponent<PauseMenu>();
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
            PauseMenu.ShowNextLevelScreen();
            Debug.Log("Stage cleared");
        }

        public void LoadNextLevel()
        {
            var nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            PlayerPrefs.SetInt(LastLevelKeyName, nextSceneIndex);
            SceneManager.LoadScene(nextSceneIndex);
        }

//        public int GetLastLevelIndex()
//        {
//            if (PlayerPrefs.HasKey(LastLevelKeyName))
//                return PlayerPrefs.GetInt(LastLevelKeyName);
//            return -1;
//        }

        public void SaveCurrentLevelIndex()
        {
            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            PlayerPrefs.SetInt(LastLevelKeyName, currentSceneIndex);
            PlayerPrefs.Save();
        }

//        public bool DoesPlayerProgressExist()
//        {
//            return PlayerPrefs.HasKey(LastLevelKeyName);
//        }

        public void EnemyDefeated()
        {
            EnemiesOnMapLeft--;
            if (EnemiesOnMapLeft > 0) return;
            clear = true;
            GameObject.Find("Exit").GetComponent<LevelManagerTrigger>().OpenExit();
        }
    }
}
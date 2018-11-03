using GUI_scripts.Achievement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GUI_scripts
{
    public class LoadSceneFromMenu : MonoBehaviour
    {
        public GameObject[] ObjectToMove;

        public AchievementManager achievementManager;

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
            var indexToLoad = PlayerPrefs.GetInt("LastLevelCheckpoint");
            Destroy(GameObject.Find("MainMenuCanvas"));
            LoadByIndex(indexToLoad);
        }

        private void LoadByIndex(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
            Time.timeScale = 1f;
        }
    }
}
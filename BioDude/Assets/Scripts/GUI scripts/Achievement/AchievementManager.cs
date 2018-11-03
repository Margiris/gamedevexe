using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GUI_scripts.Achievement
{
    public class AchievementManager : MonoBehaviour
    {
        public Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();

        public GameObject achievementPrefab;
        public Sprite[] sprites;

        // ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("activeButon")] public AchievementButton ActiveButton;
        public ScrollRect scrollRect;
        public GameObject achievementMenu;
        public GameObject backButton;
        public GameObject visualAchievement;
        public GameObject visualNotification;
        public Transform notificationPanel;
        public Sprite unlockedSprite;
        public Text textPoints;
        private static AchievementManager instance;
        private const int fadeTime = 1;

        public static AchievementManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AchievementManager>();
                }

                return instance;
            }
        }

        // Use this for initialization
        private void Start()
        {
            //Debug.Log("started achievement manager");
            notificationPanel = GameObject.Find("NotificationPanel").transform;

            //create achievements: category, title, description, points, sprite(can be added/dragged on script), (optional)script dependencies
            CreateAchievement("General", "Press W", "Press W to unlock", 5, 1, 0);
            CreateAchievement("General", "Press A", "Press A to unlock", 5, 1, 0);
            CreateAchievement("General", "Press S", "Press S to unlock", 5, 1, 0);
            CreateAchievement("General", "Press D", "Press D to unlock", 5, 1, 0);
            CreateAchievement("Other", "Get Moving", "all the movement keys", 10, 1, 0,
                new[] {"Press W", "Press A", "Press S", "Press D"});
            CreateAchievement("Other", "Press L", "Press L 3 times to unlock", 5, 1, 3);

            foreach (var achievementList in GameObject.FindGameObjectsWithTag("AchievementList"))
            {
                achievementList.SetActive(false);
            }

            ActiveButton.Click();

            achievementMenu.SetActive(false);
        }

        // Update is called once per frame
        private void Update()
        {
            //this opens achievement menu when not in main menu and P is pressed. Comment if you don't want this to work

            if (Input.GetKeyDown(KeyCode.P) && SceneManager.GetActiveScene().buildIndex != 0)
            {
                achievementMenu.SetActive(!achievementMenu.activeSelf);
                backButton.SetActive(!backButton.activeSelf);
            }
        }

        public void EarnAchievement(string title)
        {
            if (achievements[title].EarnAchievement())
            {
                var achievement = Instantiate(visualAchievement);
                SetAchievementInfo("NotificationPanel", achievement, title);
                textPoints.text = "Points: " + PlayerPrefs.GetInt("Points");
                StartCoroutine(FadeAchievement(achievement));
            }
        }

        public void Notify(string text)
        {
            var notification = Instantiate(visualNotification, notificationPanel);
            notification.transform.GetChild(0).GetComponent<Text>().text = text;
            StartCoroutine(FadeAchievement(notification));
        }

        public IEnumerator HideAchievement(GameObject achievement)
        {
            yield return new WaitForSeconds(3);
            Destroy(achievement);
        }

        public void DestroyAllAchievements()
        {
            foreach (var item in achievements)
            {
                item.Value.DestroyAchievement();
            }
        }

        public void CreateAchievement(string parent, string title, string description, int points, int spriteIndex,
            int progress, string[] dependencies = null)
        {
            var achievement = Instantiate(achievementPrefab);
            var newAchievement = achievement.AddComponent<Achievement>();
            achievements.Add(title, newAchievement);
            SetAchievementInfo(parent, achievement, title, progress);
            if (dependencies == null) return;
            foreach (var achievementTitle in dependencies)
            {
                var dependency = achievements[achievementTitle];
                dependency.Child = title;
                newAchievement.AddDependency(dependency);

                //Dependency = press space <-- Child = Press W
                //NewAchievement = Press W --> Press Space
            }
        }

        public void SetAchievementInfo(string parent, GameObject achievement, string title, int progression = 0)
        {
            achievement.transform.SetParent(GameObject.Find(parent).transform);

            achievement.transform.localScale = new Vector3(1, 1, 1);

            var progress = progression > 0
                ? " " + PlayerPrefs.GetInt("Progression" + title) + "/" + progression
                : string.Empty;

            achievement.transform.GetChild(0).GetComponent<Text>().text = title + progress;
            achievement.transform.GetChild(1).GetComponent<Text>().text = achievements[title].Description;
            achievement.transform.GetChild(2).GetComponent<Text>().text = achievements[title].Points.ToString();
            achievement.transform.GetChild(3).GetComponent<Image>().sprite = sprites[achievements[title].SpriteIndex];
        }

        public void ChangeCategory(GameObject button)
        {
            var achievementButton = button.GetComponent<AchievementButton>();

            scrollRect.content = achievementButton.achievementList.GetComponent<RectTransform>();

            achievementButton.Click();
            ActiveButton.Click();
            ActiveButton = achievementButton;
        }

        private IEnumerator FadeAchievement(GameObject achievement)
        {
            var canvasGroup = achievement.GetComponent<CanvasGroup>();

            var rate = 1.0f / fadeTime;

            var startAlpha = 0;
            var endAlpha = 1;


            for (var i = 0; i < 2; i++)
            {
                var progress = 0.0f;

                while (progress < 1.0)
                {
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);

                    progress += rate * Time.deltaTime;

                    yield return null;
                }

                yield return new WaitForSeconds(2);
                startAlpha = 1;
                endAlpha = 0;
            }

            Destroy(achievement);
        }
    }
}
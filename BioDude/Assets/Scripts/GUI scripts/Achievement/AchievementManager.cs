using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AchievementManager : MonoBehaviour
{
    public Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();

    public GameObject achievementPrefab;
    public Sprite[] sprites;
    public AchievementButton activeButon;
    public ScrollRect scrollRect;
    public GameObject achievementMenu;
    public GameObject backButton;
    public GameObject visualAchievement;
    public GameObject visualNotification;
    public Transform notificationPanel;
    public Sprite unlockedSprite;
    public Text textPoints;
    private static AchievementManager instance;
    private int fadeTime = 1;

    public static AchievementManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<AchievementManager>();
            }
            return instance;
        }

        set
        {
            instance = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        //Debug.Log("started achievement manager");
        notificationPanel = GameObject.Find("NotificationPanel").transform;
        //REMEMBER to Delete or Comment after game release
        //PlayerPrefs.DeleteAll();
        //activeButon = GameObject.Find("GeneralButton").GetComponent<AchievementButton>();

        //create achievements: category, title, description, points, sprite(can be added/dragged on scipt), (optional)script dependancies
        CreateAchievement("General", "Press W", "Press W to unlock", 5, 1, 0);
        CreateAchievement("General", "Press A", "Press A to unlock", 5, 1, 0);
        CreateAchievement("General", "Press S", "Press S to unlock", 5, 1, 0);
        CreateAchievement("General", "Press D", "Press D to unlock", 5, 1, 0);
        CreateAchievement("Other", "Get Moving", "all the movement keys", 10, 1, 0, new string[] { "Press W", "Press A", "Press S", "Press D" });
        CreateAchievement("Other", "Press L", "Press L 3 times to unlock", 5, 1, 3);
        
        foreach (GameObject achievementList in GameObject.FindGameObjectsWithTag("AchievementList"))
        {
            achievementList.SetActive(false);
        }
        activeButon.Click();

        achievementMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
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
            GameObject achievement = (GameObject)Instantiate(visualAchievement);
            SetAchievementInfo("NotificationPanel", achievement, title);
            textPoints.text = "Points: " + PlayerPrefs.GetInt("Points");
            StartCoroutine(FadeAchievement(achievement));
        }
    }

    public void Notify(string text)
    {
        GameObject notification = (GameObject)Instantiate(visualNotification, notificationPanel);
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

    public void CreateAchievement(string parent, string title, string description, int points, int spriteIndex, int progress, string[] dependencies = null)
    {
        GameObject achievement = (GameObject)Instantiate(achievementPrefab);
        Achievement newAchievement = new Achievement(title, description, points, spriteIndex, achievement, progress);
        achievements.Add(title, newAchievement);
        SetAchievementInfo(parent, achievement, title, progress);
        if (dependencies != null)
        {
            foreach (string achievementTitle in dependencies)
            {
                Achievement dependency = achievements[achievementTitle];
                dependency.Child = title;
                newAchievement.AddDependency(dependency);

                //Dependency = press space <-- Child = Press W
                //NewAchievement = Press W --> Press Space
            }
        }
    }

    public void SetAchievementInfo(string parent, GameObject achievement, string title, int progression = 0)
    {
        achievement.transform.SetParent(GameObject.Find(parent).transform);

        achievement.transform.localScale = new Vector3(1, 1, 1);

        string progress = progression > 0 ? " " + PlayerPrefs.GetInt("Progression" + title) + "/" + progression.ToString() : string.Empty;

        achievement.transform.GetChild(0).GetComponent<Text>().text = title + progress;
        achievement.transform.GetChild(1).GetComponent<Text>().text = achievements[title].Description;
        achievement.transform.GetChild(2).GetComponent<Text>().text = achievements[title].Points.ToString();
        achievement.transform.GetChild(3).GetComponent<Image>().sprite = sprites[achievements[title].SpriteIndex];
    }

    public void ChangeCategory(GameObject button)
    {
        AchievementButton achievementButton = button.GetComponent<AchievementButton>();

        scrollRect.content = achievementButton.achievementList.GetComponent<RectTransform>();

        achievementButton.Click();
        activeButon.Click();
        activeButon = achievementButton;
    }

    private IEnumerator FadeAchievement(GameObject achievement)
    {
        CanvasGroup canvasGroup = achievement.GetComponent<CanvasGroup>();

        float rate = 1.0f / fadeTime;

        int startAlpha = 0;
        int endAlpha = 1;



        for (int i = 0; i < 2; i++)
        {
            float progress = 0.0f;

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

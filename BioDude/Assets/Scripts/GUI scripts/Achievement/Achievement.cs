using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GUI_scripts.Achievement
{
    public class Achievement : MonoBehaviour
    {
        private readonly List<Achievement> dependencies = new List<Achievement>();

        private int currentProgression;
        private readonly int maxProgression;

        public Achievement(string name, string description, int points, int spriteIndex, GameObject achievementRef,
            int maxProgression)
        {
            Name = name;
            Description = description;
            Unlocked = false;
            Points = points;
            SpriteIndex = spriteIndex;
            AchievementRef = achievementRef;
            this.maxProgression = maxProgression;


            LoadAchievement();
        }

        public void AddDependency(Achievement dependency)
        {
            dependencies.Add(dependency);
        }

        public string Name { private get; set; }

        public string Description { get; set; }

        public bool Unlocked { private get; set; }

        public int Points { get; set; }

        public GameObject AchievementRef { private get; set; }

        public int SpriteIndex { get; set; }

        public string Child { private get; set; }

        public bool EarnAchievement()
        {
            if (Unlocked || dependencies.Exists(x => x.Unlocked == false) || !CheckProgress()) return false;
            AchievementRef.GetComponent<Image>().sprite = AchievementManager.Instance.unlockedSprite;
            SaveAchievement(true);

            if (Child != null)
            {
                AchievementManager.Instance.EarnAchievement(Child);
            }

            return true;
        }

        public void DestroyAchievement()
        {
            var tmpPoints = PlayerPrefs.GetInt("Points");

            PlayerPrefs.SetInt("Points", tmpPoints - Points);
            PlayerPrefs.SetInt(Name, 0);
            PlayerPrefs.SetInt("Progression" + Name, 0);
            PlayerPrefs.Save();
        }

        public void SaveAchievement(bool value)
        {
            Unlocked = value;


            if (value)
            {
                var tmpPoints = PlayerPrefs.GetInt("Points");

                PlayerPrefs.SetInt("Points", tmpPoints + Points);
                PlayerPrefs.SetInt(Name, 1);
            }
            else
            {
                PlayerPrefs.SetInt(Name, 0);
            }

            PlayerPrefs.SetInt("Progression" + Name, currentProgression);


            PlayerPrefs.Save();
            //stores achievement's status
            PlayerPrefs.SetInt(Name, value ? 1 : 0);
        }

        public void LoadAchievement()
        {
            Unlocked = PlayerPrefs.GetInt(Name) == 1;

            if (!Unlocked) return;
            AchievementManager.Instance.textPoints.text = "Points: " + PlayerPrefs.GetInt("Points");
            currentProgression = PlayerPrefs.GetInt("Progression" + Name);
            AchievementRef.GetComponent<Image>().sprite = AchievementManager.Instance.unlockedSprite;
        }

        private bool CheckProgress()
        {
            currentProgression++;
            if (maxProgression != 0)
                AchievementRef.transform.GetChild(0).GetComponent<Text>().text =
                    Name + " " + currentProgression + "/" + maxProgression;

            SaveAchievement(false);

            if (maxProgression == 0)
            {
                return true;
            }

            return currentProgression >= maxProgression;
        }
    }
}
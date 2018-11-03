using UnityEngine;

namespace GUI_scripts.Achievement
{
    public class EarnConditionsExample : MonoBehaviour
    {
        // Update is called once per frame
        private void Update()
        {
            //achievement earn conditions

            if (Input.GetKeyDown(KeyCode.W))
            {
                AchievementManager.Instance.EarnAchievement("Press W");
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                AchievementManager.Instance.EarnAchievement("Press A");
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                AchievementManager.Instance.EarnAchievement("Press S");
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                AchievementManager.Instance.EarnAchievement("Press D");
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                AchievementManager.Instance.EarnAchievement("Press L");
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                PlayerPrefs.DeleteAll();
            }
        }
    }
}
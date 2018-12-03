using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EarnConditionsExample : MonoBehaviour {
   

    /// <summary>
    /// 
    /// Achievements must be written in AchievementManager script Start() function
    /// 
    /// </summary>

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

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

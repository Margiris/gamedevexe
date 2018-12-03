using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    public AudioMixer audioMixer;

    public Dropdown resDropdown;
    private Resolution[] resolutions;
    public Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    public Slider volumeSlider;

    void Start()
    {
        resolutions = Screen.resolutions;
        resDropdown.ClearOptions();
        List<string> resOptions = new List<string>();

        int currentRes = -1;
        bool fullscreen;
        int height;
        int width;
        int qualityIndex;
        float volume;
        Debug.Log("-------------------------");


        if (PlayerPrefs.HasKey("IsFullscreen"))
        {
            fullscreen = PlayerPrefs.GetInt("IsFullscreen") == 1 ? true : false;
            Debug.Log("found fullscreen:" + fullscreen);
        }
        else
        {
            fullscreen = Screen.fullScreen;
            PlayerPrefs.SetInt("IsFullscreen", fullscreen ? 1 : 0);
        }

        if (PlayerPrefs.HasKey("ResolutionHeight"))
        {

            height = PlayerPrefs.GetInt("ResolutionHeight");
            //Debug.Log("found height:" + height);

        }
        else
        {
            height = Screen.height;
            PlayerPrefs.SetInt("ResolutionHeight", height);
        }

        if (PlayerPrefs.HasKey("ResolutionWidth"))
        {
            width = PlayerPrefs.GetInt("ResolutionWidth");
            //Debug.Log("found width:" + width);

        }
        else
        {
            width = Screen.width;
            PlayerPrefs.SetInt("ResolutionWidth", width);
        }

        if (PlayerPrefs.HasKey("QualityIndex"))
        {
            qualityIndex = PlayerPrefs.GetInt("QualityIndex");
            //Debug.Log("found quality:" + qualityIndex);

        }
        else
        {
            qualityIndex = QualitySettings.GetQualityLevel();
            PlayerPrefs.SetInt("QualityIndex", qualityIndex);
        }

        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            volume = PlayerPrefs.GetFloat("MasterVolume");
            //Debug.Log("found volume:" + volume);

        }
        else
        {
            audioMixer.GetFloat("MasterVolume", out volume);
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }


        Debug.Log(fullscreen);
        Debug.Log(height + "x" + width);
        Debug.Log(qualityIndex);
        Debug.Log(volume);
        Debug.Log("-------------------------");


        fullscreenToggle.isOn = fullscreen;

        for (int i = 0; i < resolutions.Length; i++)
        {
            //if (resolutions[i].width / resolutions[i].height == 16 / 9)
            //{
                string resOption =
                    resolutions[i].width + "x" + resolutions[i].height /* + " " +  resolutions[i].refreshRate + "Hz"*/;
                
                resOptions.Add(resOption);
                //Debug.Log(resolutions[i].width + "!=" + width + "&&" +
                //resolutions[i].height + "!=" + height + "index: " + currentRes);

                /*if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height &&
                    resolutions[i].refreshRate == Screen.currentResolution.refreshRate)*/
                if (resolutions[i].width == width &&
                    resolutions[i].height == height)
                {
                    currentRes = i;
                    //Debug.Log(resolutions[i].width +"=="+ width +"&&"+
                    //resolutions[i].height +"=="+ height + "index: " + currentRes);
                    //Debug.Log("found res index:" + currentRes);
                }
            //}
        }
        
        //resOptions.Distinct();
        
        if(currentRes<0)
        {
            currentRes = resOptions.Count - 1;
            //Debug.Log("no res index found");
        }
        resDropdown.AddOptions(resOptions);
        resDropdown.value = currentRes;
        resDropdown.RefreshShownValue();

        qualityDropdown.value = qualityIndex;
        qualityDropdown.RefreshShownValue();

        volumeSlider.value = volume;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resIndex)
    {
        Resolution res = resolutions[resIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen, res.refreshRate);
        /*PlayerPrefs.SetInt("ResolutionHeight", res.height);
        PlayerPrefs.SetInt("ResolutionWidth", res.width);
        Debug.Log(PlayerPrefs.GetInt("ResolutionHeight"));
        Debug.Log(PlayerPrefs.GetInt("ResolutionWidth"));*/

    }

    public void SetGraphicsQuality(int Qindex)
    {
        QualitySettings.SetQualityLevel(Qindex);
        //PlayerPrefs.SetInt("QualityIndex", Qindex);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
        //PlayerPrefs.SetFloat("MasterVolume", volume);
        //Debug.Log("Volume set to " + PlayerPrefs.GetFloat("MasterVolume", volume) + "(" + volume + ")");
    }
    void OnDisable()
    {
        SaveOptions();
    }

    public void SaveOptions()
    {
        Debug.Log("saved");
        PlayerPrefs.SetInt("IsFullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("ResolutionHeight", Screen.currentResolution.height);
        PlayerPrefs.SetInt("ResolutionWidth", Screen.currentResolution.height);
        PlayerPrefs.SetInt("QualityIndex", qualityDropdown.value);
        PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
    }
}

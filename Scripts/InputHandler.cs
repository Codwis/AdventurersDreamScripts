using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
    public Settings settingsFile;
    private static KeyCode rebind = KeyCode.None;

    public Text sensText;
    public Text volText;
    public Slider volSlider;
    public Slider sensSlider;

    public AudioMixer mixer;
    public Dropdown qualityDropdown;
    public Dropdown fullScreenDropdown;
    public ResolutionSetter setter;

    private CanvasGroup settingsMenu;
    public static InputHandler instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            SetupKeys();
        }

        if (!PlayerPrefs.HasKey("ScreenWidth"))
        {

            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);

            PlayerPrefs.SetInt("ScreenWidth", Screen.currentResolution.width);
            PlayerPrefs.SetInt("ScreenHeight", Screen.currentResolution.height);

            PlayerPrefs.SetInt("Fullscreen", 0);

            PlayerPrefs.SetFloat("Volume", 0);
            ChangeVolume(0);

            PlayerPrefs.SetFloat("Sensitivity", 1);
            ChangeSensitivity(1);

            PlayerPrefs.SetInt("Quality", 0);
            QualitySettings.SetQualityLevel(0);

            PlayerPrefs.Save();
        }
        else
        {
            int full = PlayerPrefs.GetInt("Fullscreen");
            FullScreenMode m;
            if (full == 0)
            {
                m = FullScreenMode.ExclusiveFullScreen;
            }
            else if (full == 1)
            {
                m = FullScreenMode.FullScreenWindow;
            }
            else
            {
                m = FullScreenMode.Windowed;
            }

            Screen.SetResolution(PlayerPrefs.GetInt("ScreenWidth"), PlayerPrefs.GetInt("ScreenHeight"), m);

            volSlider.value = PlayerPrefs.GetFloat("Volume");
            sensSlider.value = PlayerPrefs.GetFloat("Sensitivity");
            ChangeVolume(PlayerPrefs.GetFloat("Volume"));
            ChangeSensitivity(PlayerPrefs.GetFloat("Sensitivity"));

            qualityDropdown.value = PlayerPrefs.GetInt("Quality");
            QualityChange(PlayerPrefs.GetInt("Quality"));

            fullScreenDropdown.value = PlayerPrefs.GetInt("Fullscreen");
        }


        ChangeFullscreen(PlayerPrefs.GetInt("Fullscreen"));
        settingsMenu = GetComponent<CanvasGroup>();
    }


    private void SetupKeys()
    {
        foreach(Inputs input in settingsFile.preInputs)
        {
            if(!PlayerPrefs.HasKey(input.keyName))
            {
                PlayerPrefs.SetInt(input.keyName, (int)input.key);
            }
        }
    }

    public void ResetKeys()
    {
        foreach (Inputs input in settingsFile.preInputs)
        {
            PlayerPrefs.SetInt(input.keyName, (int)input.key);
        }

        CreateControlSettings.instance.ResetKeys();

        setter.resolutionThing.value = 0;
        setter.fullscreenMode.value = 0;
        Screen.SetResolution(Screen.width, Screen.height, true);

        PlayerPrefs.SetInt("ScreenWidth", Screen.currentResolution.width);
        PlayerPrefs.SetInt("ScreenHeight", Screen.currentResolution.height);

        PlayerPrefs.SetInt("Fullscreen", 0);

        PlayerPrefs.SetFloat("Volume", 0);
        ChangeVolume(0);
        volSlider.value = PlayerPrefs.GetFloat("Volume");

        PlayerPrefs.SetFloat("Sensitivity", 1);
        ChangeSensitivity(1);
        sensSlider.value = PlayerPrefs.GetFloat("Sensitivity");


        qualityDropdown.value = 0;
        QualityChange(0);
    }

    public void ShowSettings()
    {
        settingsMenu.alpha = 1;
        settingsMenu.interactable = true;
        settingsMenu.blocksRaycasts = true;
    }
    public void Apply()
    {
        settingsMenu.alpha = 0;
        settingsMenu.interactable = false;
        settingsMenu.blocksRaycasts = false;

        PlayerPrefs.Save();
    }

    private Text tempCurrentKey;
    public void Rebind(string key, in Text currentKeyText)
    {
        tempCurrentKey = currentKeyText;
        StartCoroutine(RebindKey(key));
    }

    public IEnumerator RebindKey(string key) //rebind a key
    {
        rebind = KeyCode.None; //reset this to none
        while (rebind == KeyCode.None)
        {
            foreach(KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) //Goes through all keycodes and when player presses one key it sets it as new
            {
                if(Input.GetKey(keyCode))
                {
                    tempCurrentKey.text = keyCode.ToString();
                    rebind = keyCode;
                    InputHandler.instance.settingsFile.ChangeKey(key, rebind);
                    break;
                }
            }

            yield return null;
        }
    }

    public KeyCode GetKey(string keyName)
    {
        return (KeyCode)PlayerPrefs.GetInt(keyName);
    }

    public void ChangeFullscreen(int i)
    {
        PlayerPrefs.SetInt("Fullscreen", i);

        if (i == 0)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else if(i == 1)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }

        PlayerPrefs.Save();
    }

    public void ChangeVolume(float vol)
    {
        mixer.SetFloat("Volume", vol);
        PlayerPrefs.SetFloat("Volume", vol);
        volText.text = "Volume: " + Mathf.RoundToInt(Mathf.InverseLerp(-80, 20, vol) * 100) + "%";
    }

    public void ChangeSensitivity(float vol)
    {
        PlayerPrefs.SetFloat("Sensitivity", vol);
        sensText.text = "Sensitivity: " + System.Math.Round(vol / 10 * 10, 2);
    }

    public void QualityChange(int quality)
    {
        PlayerPrefs.SetInt("Quality", quality);
        QualitySettings.SetQualityLevel(quality);
    }
}

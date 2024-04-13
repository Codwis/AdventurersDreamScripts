using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionSetter : MonoBehaviour
{
    public Dropdown resolutionThing;
    public Dropdown fullscreenMode;

    private int current = 0;
    private void Start()
    {
        GetResolutions();
        fullscreenMode.value = PlayerPrefs.GetInt("Fullscreen");
        resolutionThing.onValueChanged.AddListener(ResolutionChange);
        resolutionThing.value = resolutionThing.options.Count - 1;
        resolutionThing.value = current;
    }
    private void GetResolutions()
    {
        Dropdown.OptionData t;
        resolutionThing.ClearOptions();

        foreach(Resolution res in Screen.resolutions)
        {
            bool duplicate = false;
            foreach(Dropdown.OptionData op in resolutionThing.options)
            {
                string[] resolution = op.text.Split("x");
                if(res.width.ToString() == resolution[0] && res.height.ToString() == resolution[1])
                {
                    duplicate = true;
                    break;
                }
            }

            if (duplicate) continue;

            t = new Dropdown.OptionData();
            t.text = res.width + "x" + res.height;

            resolutionThing.options.Add(t);
        }

        resolutionThing.options.Reverse();

        int i = 0;
        foreach (Dropdown.OptionData op in resolutionThing.options)
        {
            string[] resolution = op.text.Split("x");
            if (resolution[0] == PlayerPrefs.GetInt("ScreenWidth").ToString() && resolution[1] == PlayerPrefs.GetInt("ScreenHeight").ToString())
            {
                current = i;
                break;
            }
            i++;
        }
        resolutionThing.value = current;
    }

    public void ResolutionChange(int i)
    {
        string[] res = resolutionThing.options[i].text.Split("x");
        PlayerPrefs.SetInt("ScreenWidth", int.Parse(res[0]));
        PlayerPrefs.SetInt("ScreenHeight", int.Parse(res[1]));

        int full = PlayerPrefs.GetInt("Fullscreen");
        FullScreenMode mod;

        if(full == 0)
        {
            mod = FullScreenMode.ExclusiveFullScreen;
        }
        else if(full == 1)
        {
            mod = FullScreenMode.FullScreenWindow;
        }
        else
        {
            mod = FullScreenMode.Windowed;
        }
        Screen.SetResolution(PlayerPrefs.GetInt("ScreenWidth"), PlayerPrefs.GetInt("ScreenHeight"), mod);
    }

}

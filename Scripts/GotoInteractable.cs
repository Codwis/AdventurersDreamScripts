using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoInteractable : Interactable
{
    public string sceneName = "RealMainMenu";
    public override void Interact(Transform source)
    {
        base.Interact(source);
        SceneManager.LoadScene(sceneName);
    }
}

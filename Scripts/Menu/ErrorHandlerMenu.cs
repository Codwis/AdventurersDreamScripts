using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class ErrorHandlerMenu : MonoBehaviour
{
    public Text errorText;

    public void ShowError(LobbyServiceException e)
    {
        errorText.text = e.Message;
        StartCoroutine(ResetText());
    }

    public IEnumerator ResetText()
    {
        yield return new WaitForSeconds(5);
        errorText.text = "";
    }
    public void ShowText(string toWrite)
    {
        StopAllCoroutines();
        errorText.text = toWrite;
        StartCoroutine(ResetText());
    }
}

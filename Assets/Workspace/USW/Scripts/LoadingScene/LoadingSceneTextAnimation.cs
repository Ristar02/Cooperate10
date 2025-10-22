using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneTextAnimation : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI loadingText;
    [SerializeField] float delayBetweenDots = 0.2f;

    private string[] loadingMessages = { "Now Loading", "Now Loading.", "Now Loading..", "Now Loading..." };
    private int currentMessageIndex = 0;

    private void Start()
    {
        if (loadingText == null)
        {
            enabled = false;
            return;
        }

        StartCoroutine(AnimationLoadingText());
        
        Invoke("LoadNextScene",3f);
    }

    IEnumerator AnimationLoadingText()
    {
        while (true)
        {
            loadingText.text = loadingMessages[currentMessageIndex];
            
            currentMessageIndex = (currentMessageIndex + 1) % loadingMessages.Length;
            
            yield return new WaitForSeconds(delayBetweenDots);
        }
    }

    void LoadNextScene()
    {
        //SceneManager.LoadScene("USW_Tutorial");
    }
}

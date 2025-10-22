using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialPanel : MonoBehaviour
{
    [SerializeField] private Button _nextSceneButton;

    private void Start()
    {
        _nextSceneButton.onClick.AddListener(() => SceneManager.LoadScene("CYH_Lobby"));
    }
}

#if UNITY_EDITOR
using TMPro;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class AreaBackgroundLoader : MonoBehaviour
{
    void Start()
    {
        LoadRandomAreaBackground();
    }
    /// <summary>
    /// Area1~Area7 중 하나를 랜덤으로 Additive로 로드합니다.
    /// </summary>
    public void LoadRandomAreaBackground()
    {
        int randomIndex = Random.Range(1, 8); // 1~7 (끝값 미포함)
        string sceneName = $"Area{randomIndex}";
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }
}
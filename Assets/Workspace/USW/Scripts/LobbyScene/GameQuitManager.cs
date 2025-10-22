using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;
#endif

public class GameQuitManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameQuitPanel;
    public Button yesButton;
    public Button noButton;

    void Start()
    {
        if (yesButton != null)
            yesButton.onClick.AddListener(QuitGame);
        
        if (noButton != null)
            noButton.onClick.AddListener(CloseQuitPanel);
        
        if (gameQuitPanel != null)
            gameQuitPanel.SetActive(false);
    }

    void Update()
    {
        // 안드로이드 뒤로가기 버튼 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowQuitPanel();
        }
    }

    /// <summary>
    /// 게임 종료 패널 표시
    /// </summary>
    public void ShowQuitPanel()
    {
        if (gameQuitPanel != null)
        {
            gameQuitPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// 게임 종료 패널 닫기
    /// </summary>
    public void CloseQuitPanel()
    {
        if (gameQuitPanel != null)
        {
            gameQuitPanel.SetActive(false);
            // 게임 재개
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// 실제 게임 종료
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 에디터에서 플레이 모드 중지
        EditorApplication.isPlaying = false;
#else
        // 빌드된 애플리케이션 종료
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (yesButton != null)
            yesButton.onClick.RemoveListener(QuitGame);
        
        if (noButton != null)
            noButton.onClick.RemoveListener(CloseQuitPanel);
    }
}
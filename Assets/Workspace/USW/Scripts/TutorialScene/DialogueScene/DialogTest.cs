using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogTest : MonoBehaviour
{
    [Header("Dialog Systems")]
    [SerializeField]
    private    DialogSystem   dialogSystem01;
    [SerializeField]
    private    DialogSystem   dialogSystem02;
    
    [Header("UI Elements")]
    [SerializeField]
    private    TextMeshProUGUI    textCountdown;
    
    [Header("Settings")]
    [SerializeField]
    private    string         nextSceneName = "";        // 다음 씬 이름 (비워두면 앱 종료)
    [SerializeField]
    private    bool           enableBackButton = true;   // 뒤로가기 버튼 활성화 여부
    [SerializeField]
    private    int            countdownDuration = 5;     // 카운트다운 시간

    private    bool           isTestRunning = false;     // 테스트 실행 중 여부

    private IEnumerator Start()
    {
        isTestRunning = true;
        textCountdown.gameObject.SetActive(false);

        // 첫 번째 대사 읽기 시작
        yield return StartCoroutine(RunDialogSystem(dialogSystem01, "첫 번째 대화"));

        // 대사 읽기 사이에 원하는 행동을 추가할 수 있다.
        // 캐릭터를 이동시키거나 아이템을 획득하는 연출.. 여기서는 5-4-3-2-1 카운트 다운 연출
        yield return StartCoroutine(ShowCountdown());

        // 두 번째 대사 읽기 시작
        yield return StartCoroutine(RunDialogSystem(dialogSystem02, "두 번째 대화"));

        // 종료 메시지 표시
        yield return StartCoroutine(ShowEndMessage());

        // 테스트 완료 처리
        isTestRunning = false;
        HandleTestComplete();
    }

    private void Update()
    {
        // 모바일 뒤로가기 버튼 처리 
        if (enableBackButton && Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }
    }

    private IEnumerator RunDialogSystem(DialogSystem dialogSystem, string dialogName)
    {
        if (dialogSystem == null)
        {
            Debug.LogWarning($"DialogSystem이 할당되지 않았습니다: {dialogName}");
            yield break;
        }

        Debug.Log($"{dialogName} 시작");
        
        // 대화 시스템이 완료될 때까지 대기
        yield return new WaitUntil(() => {
            bool isFinished = dialogSystem.UpdateDialog();
            return isFinished || dialogSystem.IsDialogFinished();
        });
        
        Debug.Log($"{dialogName} 완료");
    }

    private IEnumerator ShowCountdown()
    {
        textCountdown.gameObject.SetActive(true);
        int count = countdownDuration;
        
        while (count > 0)
        {
            textCountdown.text = count.ToString();
            count--;
            yield return new WaitForSeconds(1);
        }
        
        textCountdown.gameObject.SetActive(false);
    }

    private IEnumerator ShowEndMessage()
    {
        textCountdown.gameObject.SetActive(true);
        textCountdown.text = "The End";
        yield return new WaitForSeconds(2);
        textCountdown.gameObject.SetActive(false);
    }

    private void HandleTestComplete()
    {
        Debug.Log("Dialog Test 완료");
        
        // 다음 씬이 지정되어 있으면 씬 전환
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            LoadNextScene();
        }
        else
        {
            // 씬이 지정되지 않았으면 앱 종료 또는 메인 메뉴로 이동
            ExitApplication();
        }
    }

    private void HandleBackButton()
    {
        if (isTestRunning)
        {
            // 실행 중이면 종료 확인
            ShowExitConfirmation();
        }
        else
        {
            ExitApplication();
        }
    }

    private void ShowExitConfirmation()
    {
        // 간단한 종료 확인
        Debug.Log("뒤로가기 버튼 눌림 - 앱 종료");
        ExitApplication();
    }

    private void LoadNextScene()
    {
        try
        {
            SceneManager.LoadScene(nextSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"씬 로드 실패: {nextSceneName}. 오류: {e.Message}");
            ExitApplication();
        }
    }

    private void ExitApplication()
    {
        #if UNITY_EDITOR
            // 에디터 환경에서는 플레이 모드 중지
            UnityEditor.EditorApplication.ExitPlaymode();
        #else
            // 빌드된 앱에서는 앱 종료
            Application.Quit();
        #endif
    }

    // 공개 메서드 - 외부에서 테스트 상태 확인
    public bool IsTestRunning()
    {
        return isTestRunning;
    }

    // 공개 메서드 - 외부에서 카운트다운 시간 설정
    public void SetCountdownDuration(int duration)
    {
        countdownDuration = Mathf.Max(1, duration);
    }

    // 공개 메서드 - 외부에서 뒤로가기 버튼 활성화/비활성화
    public void SetBackButtonEnabled(bool enabled)
    {
        enableBackButton = enabled;
    }
}
using System;
using System.Collections;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 게임내 다양한 팝업을 통한 관리하는 클래스입니다
/// - 일반 메시지 팝업, 확인/취소 팝업, 비밀번호 변경 팝업, 메일박스 팝업등을 구현
/// - 모달 형태의 팝업 지원 (배경 어둡게, 외부 클릭 닫기)
/// </summary>
public class PopupManager : Singleton<PopupManager>
{
    [Header("배경 오버레이")]
    [SerializeField] private GameObject backgroundOverlay;
    [SerializeField] private Image overlayImage;
    [SerializeField] private CanvasGroup backgroundCanvasGroup;

    [Header("오류 메세지 확인 팝업")] 
    [SerializeField] private GameObject mainPopupPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button closeButton;
    [SerializeField] Canvas canvas;

    [Header("확인 / 취소 팝업")] 
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] TextMeshProUGUI confirmationText;
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;

    [Header("비밀번호 변경 팝업")] 
    [SerializeField] private GameObject passwordChangePanel;
    [SerializeField] TMP_InputField currentPasswordInputField;
    [SerializeField] TMP_InputField newPasswordInputField;
    [SerializeField] Button changePasswordButton;
    [SerializeField] Button cancelPasswordButton;
    
    [Header("닉네임 변경 팝업")]
    [SerializeField] private GameObject nicknameChangePanel;
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button saveNicknameButton;
    [SerializeField] private Button cancelNicknameButton;

    [Header("룸 코드 팝업")] 
    [SerializeField] private GameObject roomcodePanel;
    [SerializeField] private TMP_InputField roomcodeInputField;
    [SerializeField] private Button confirmJoinButton;
    [SerializeField] private Button cancelJoinButton;

    [Header("메일박스 팝업")]
    [SerializeField] private GameObject mailboxPanel;
    [SerializeField] private Button mailboxCloseButton;
    [SerializeField] private CanvasGroup mailboxCanvasGroup;

    // 현재 활성화된 팝업 타입
    private PopupType currentPopupType = PopupType.None;
    
    public enum PopupType
    {
        None,
        Message,
        Confirmation,
        PasswordChange,
        NicknameChange,
        RoomCode,
        Mailbox
    }

    public static PopupManager instance;

    public static PopupManager Instance
    {
        get
        {
            if (instance == null)
            {
                CreatePopupManager();
            }
            return instance;
        }
    }

    // 콜백 함수들
    private Action onYesCallback;
    private Action onNoCallback;
    private Action<string> onNicknameSaveCallBack;
    private Action<string> onRoomCodeJoinCallBack;
    private Action onMailboxCloseCallback;

    // 처리 중 플래그
    private bool isProcessingPasswordChange = false;

    void SetupButtons()
    {
        if (closeButton)
            closeButton.onClick.AddListener(ClosePopup);

        if (yesButton)
        {
            yesButton.onClick.AddListener(() =>
            {
                onYesCallback?.Invoke();
                ClosePopup();
            });
        }

        if (noButton)
        {
            noButton.onClick.AddListener(() =>
            {
                onNoCallback?.Invoke();
                ClosePopup();
            });
        }

        if (changePasswordButton)
            changePasswordButton.onClick.AddListener(OnChangePasswordClick);

        if (cancelPasswordButton)
            cancelPasswordButton.onClick.AddListener(OnCancelPasswordClick);
        
        if (saveNicknameButton)
            saveNicknameButton.onClick.AddListener(OnSaveNicknameClick);

        if (cancelNicknameButton)
            cancelNicknameButton.onClick.AddListener(OnCancleNicknameClick);
        
        if (confirmJoinButton)
            confirmJoinButton.onClick.AddListener(OnConfirmJoinClick);
            
        if (cancelJoinButton)
            cancelJoinButton.onClick.AddListener(OnCancelJoinClick);

        // 메일박스 버튼 설정
        if (mailboxCloseButton)
            mailboxCloseButton.onClick.AddListener(ClosePopup);

        // 배경 오버레이 클릭 이벤트 설정
        SetupBackgroundOverlay();

        // Enter 키 이벤트들
        SetupEnterKeyEvents();

        // 초기에 모든 패널 비활성화
        HideAllPanels();
    }

    void SetupBackgroundOverlay()
    {
        if (backgroundOverlay == null) return;

        // 배경 오버레이에 EventTrigger 추가하여 외부 클릭 감지
        EventTrigger trigger = backgroundOverlay.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = backgroundOverlay.AddComponent<EventTrigger>();

        // 클릭 이벤트 추가
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => OnBackgroundClick((PointerEventData)data));
        trigger.triggers.Add(entry);

        // 배경 이미지 설정 (반투명 검은색)
        if (overlayImage)
        {
            overlayImage.color = new Color(0, 0, 0, 0.5f);
            overlayImage.raycastTarget = true;
        }

        // CanvasGroup 설정
        if (backgroundCanvasGroup == null)
            backgroundCanvasGroup = backgroundOverlay.GetComponent<CanvasGroup>();
        
        if (backgroundCanvasGroup == null)
            backgroundCanvasGroup = backgroundOverlay.AddComponent<CanvasGroup>();
    }

    void OnBackgroundClick(PointerEventData eventData)
    {
        // 메일박스 팝업이 열려있을 때만 외부 클릭으로 닫기
        if (currentPopupType == PopupType.Mailbox)
        {
            // 클릭된 오브젝트가 메일박스 패널 내부가 아닌 경우에만 닫기
            if (!IsClickInsideMailbox(eventData))
            {
                ClosePopup();
            }
        }
    }

    bool IsClickInsideMailbox(PointerEventData eventData)
    {
        if (mailboxPanel == null || !mailboxPanel.activeInHierarchy)
            return false;

        // RectTransform을 이용하여 클릭 위치가 메일박스 내부인지 확인
        RectTransform mailboxRect = mailboxPanel.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(mailboxRect, eventData.position, eventData.pressEventCamera);
    }

    void SetupEnterKeyEvents()
    {
        if (newPasswordInputField)
        {
            newPasswordInputField.onEndEdit.AddListener(delegate
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    OnChangePasswordClick();
            });
        }
        
        if (nicknameInputField)
        {
            nicknameInputField.onEndEdit.AddListener(delegate
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    OnSaveNicknameClick();
            });
        }
        
        if (roomcodeInputField)
        {
            roomcodeInputField.onEndEdit.AddListener(delegate
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    OnConfirmJoinClick();
            });
        }
    }

    static void CreatePopupManager()
    { 
        GameObject popupPrefab = Resources.Load<GameObject>("PopupManager");
        GameObject popupInstance = Instantiate(popupPrefab);
        
        PopupManager popup = popupInstance.GetComponent<PopupManager>();
        instance = popup;
        
        DontDestroyOnLoad(popupInstance); 
        popup.canvas.sortingOrder = 777;
        
        popup.SetupButtons();
        popupInstance.SetActive(false); 
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    #region 공용 메서드

    public void ShowPopup(string message)
    {
        HideAllPanels();
        
        if (statusText != null)
            statusText.text = message;

        ShowMainPanel();
        currentPopupType = PopupType.Message;
        gameObject.SetActive(true);
        ShowBackground(false); 
    }

    public void ShowConfirmationPopup(string message, Action onYes, Action onNo = null)
    {
        onYesCallback = onYes;
        onNoCallback = onNo;

        if (confirmationText != null)
            confirmationText.text = message;

        HideAllPanels();
        ShowConfirmationPanel();
        currentPopupType = PopupType.Confirmation;
        gameObject.SetActive(true);
        ShowBackground(false); 
    }

    public void ShowPasswordChangePopup()
    {
        HideAllPanels();
        ShowPasswordChangePanel();
        currentPopupType = PopupType.PasswordChange;
        gameObject.SetActive(true);
        ShowBackground(false);

        if (currentPasswordInputField != null)
        {
            currentPasswordInputField.text = "";
            currentPasswordInputField.Select();
        }

        if (newPasswordInputField != null)
            newPasswordInputField.text = "";

        isProcessingPasswordChange = false;
    }

    /// <summary>
    /// 메일박스 팝업 표시 (모달 형태)
    /// </summary>
    /// <param name="onClose">닫힐 때 실행할 콜백</param>
    public void ShowMailboxPopup(Action onClose = null)
    {
        Debug.Log("메일함 열기.");
        onMailboxCloseCallback = onClose;
        
        HideAllPanels();
        ShowMailboxPanel();
        
        var playerDataController = FindObjectOfType<PlayerDataController>();
        var mailboxController = FindObjectOfType<PlayerMailBoxController>();
        
        var mailboxPopup = mailboxPanel.GetComponent<MailBoxPopup>();
        if (mailboxPopup != null)
        {
            mailboxPopup.Initialize(playerDataController, mailboxController);
        }
        
        currentPopupType = PopupType.Mailbox;
        gameObject.SetActive(true);
        ShowBackground(true); 
    }

    /// <summary>
    /// 닉네임 변경 팝업 표시
    /// </summary>
    public void ShowNicknameChangePopup(string currentNickname, Action<string> onSave)
    {
        onNicknameSaveCallBack = onSave;
        
        HideAllPanels();
        ShowNicknameChangePanel();
        currentPopupType = PopupType.NicknameChange;
        gameObject.SetActive(true);
        ShowBackground(false);

        if (nicknameInputField)
        {
            nicknameInputField.text = currentNickname;
            nicknameInputField.Select();
        }
    }

    /// <summary>
    /// 방 코드 입력 팝업 표시
    /// </summary>
    public void ShowRoomCodeInputPopup(Action<string> onJoin)
    {
        onRoomCodeJoinCallBack = onJoin;

        HideAllPanels();
        ShowRoomCodeInputPanel();
        currentPopupType = PopupType.RoomCode;
        gameObject.SetActive(true);
        ShowBackground(false);

        if (roomcodeInputField)
        {
            roomcodeInputField.text = "";
            roomcodeInputField.Select();
        }
    }

    #endregion

    #region Panel 표시 관련

    void ShowMainPanel()
    {
        if (mainPopupPanel)
            mainPopupPanel.SetActive(true);
    }

    void ShowConfirmationPanel()
    {
        if (confirmationPanel)
            confirmationPanel.SetActive(true);
    }

    void ShowPasswordChangePanel()
    {
        if (passwordChangePanel)
            passwordChangePanel.SetActive(true);
    }

    void ShowNicknameChangePanel()
    {
        if (nicknameChangePanel)
            nicknameChangePanel.SetActive(true);
    }

    void ShowRoomCodeInputPanel()
    {
        if (roomcodePanel)
            roomcodePanel.SetActive(true);
    }

    void ShowMailboxPanel()
    {
        if (mailboxPanel)
        {
            mailboxPanel.SetActive(true);
            
            // 메일박스 CanvasGroup 설정
            if (mailboxCanvasGroup)
            {
                mailboxCanvasGroup.alpha = 1f;
                mailboxCanvasGroup.interactable = true;
                mailboxCanvasGroup.blocksRaycasts = true;
            }
        }
    }

    /// <summary>
    /// 배경 오버레이 표시/숨김
    /// </summary>
    /// <param name="show">true면 배경 표시, false면 숨김</param>
    void ShowBackground(bool show)
    {
        if (backgroundOverlay)
        {
            backgroundOverlay.SetActive(show);
            
            if (backgroundCanvasGroup)
            {
                backgroundCanvasGroup.alpha = show ? 1f : 0f;
                backgroundCanvasGroup.interactable = show;
                backgroundCanvasGroup.blocksRaycasts = show;
            }
        }
    }

    void HideAllPanels()
    {
        if (mainPopupPanel) mainPopupPanel.SetActive(false);
        if (confirmationPanel) confirmationPanel.SetActive(false);
        if (passwordChangePanel) passwordChangePanel.SetActive(false);
        if (nicknameChangePanel) nicknameChangePanel.SetActive(false);
        if (roomcodePanel) roomcodePanel.SetActive(false);
        if (mailboxPanel) mailboxPanel.SetActive(false);
    }

    #endregion

    #region 기존 로직들 (비밀번호 변경, 닉네임 변경 등)

    void OnChangePasswordClick()
    {
        if (isProcessingPasswordChange)
        {
            Debug.Log("이미 비밀번호 변경 처리 중입니다.");
            return;
        }

        string currentPassword = currentPasswordInputField.text.Trim();
        string newPassword = newPasswordInputField.text.Trim();

        if (string.IsNullOrEmpty(currentPassword))
        {
            ShowPopup("현재 비밀번호를 입력해주세요.");
            return;
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            ShowPopup("새 비밀번호를 입력해주세요.");
            return;
        }

        if (newPassword.Length < 6)
        {
            ShowPopup("새 비밀번호는 6자 이상이어야 합니다.");
            return;
        }

        if (currentPassword == newPassword)
        {
            ShowPopup("새 비밀번호는 현재 비밀번호와 달라야 합니다.");
            return;
        }

        isProcessingPasswordChange = true;
        StartCoroutine(ProcessPasswordChange(currentPassword, newPassword));
    }

    private IEnumerator ProcessPasswordChange(string currentPassword, string newPassword)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            ShowPopup("로그인 되지 않은 유저입니다.");
            isProcessingPasswordChange = false;
            yield break;
        }

        bool reAuthSuccess = false;
        bool reAuthCompleted = false;
        string reAuthErrorMessage = "";

        Credential credential = EmailAuthProvider.GetCredential(user.Email, currentPassword);

        user.ReauthenticateAsync(credential).ContinueWithOnMainThread(reAuthTask =>
        {
            reAuthCompleted = true;
            if (reAuthTask.IsCanceled)
                reAuthErrorMessage = "취소 되었습니다.";
            else if (reAuthTask.IsFaulted)
                reAuthErrorMessage = "현재 비밀번호가 아닙니다";
            else
                reAuthSuccess = true;
        });

        float timeout = 0f;
        while (!reAuthCompleted && timeout < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            timeout += 0.1f;
        }

        if (!reAuthSuccess)
        {
            ShowPopup(string.IsNullOrEmpty(reAuthErrorMessage) ? "재인증에 실패했습니다." : reAuthErrorMessage);
            isProcessingPasswordChange = false;
            yield break;
        }

        bool changeSuccess = false;
        bool changeCompleted = false;
        string changeErrorMessage = "";

        user.UpdatePasswordAsync(newPassword).ContinueWithOnMainThread(changeTask =>
        {
            changeCompleted = true;
            if (changeTask.IsCanceled)
                changeErrorMessage = "비밀번호 변경이 취소되었습니다";
            else if (changeTask.IsFaulted)
                changeErrorMessage = "비밀번호 변경에 실패하였습니다.";
            else
                changeSuccess = true;
        });

        timeout = 0f;
        while (!changeCompleted && timeout < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            timeout += 0.1f;
        }

        if (changeSuccess)
            ShowPopup("비밀번호가 변경되었습니다.");
        else
            ShowPopup(string.IsNullOrEmpty(changeErrorMessage) ? "비밀번호 변경에 실패하였습니다" : changeErrorMessage);

        isProcessingPasswordChange = false;
    }

    void OnCancelPasswordClick()
    {
        ClosePopup();
    }

    void OnSaveNicknameClick()
    {
        string newNickname = nicknameInputField.text.Trim();

        if (string.IsNullOrEmpty(newNickname))
        {
            ShowPopup("닉네임을 입력해주세요");
            return;
        }

        if (newNickname.Length < 2 || newNickname.Length > 12)
        {
            ShowPopup("닉네임은 2~12자로 입력해주세요");
            return;
        }
        
        onNicknameSaveCallBack?.Invoke(newNickname);
        ClosePopup();
    }

    void OnCancleNicknameClick()
    {
        ClosePopup();
    }

    void OnConfirmJoinClick()
    {
        string roomCode = roomcodeInputField.text.Trim();

        if (string.IsNullOrEmpty(roomCode))
        {
            ShowPopup("방 코드를 입력해주세요");
            return;
        }

        if (roomCode.Length != 4)
        {
            ShowPopup("4자리의 방 코드를 입력하세요");
            return;
        }

        if (!int.TryParse(roomCode, out int code))
        {
            ShowPopup("숫자만 입력 가능합니다");
            return;
        }

        onRoomCodeJoinCallBack?.Invoke(roomCode);
        ClosePopup();
    }
    
    void OnCancelJoinClick()
    {
        ClosePopup();
    }

    #endregion
    
    #region 팝업 컨트롤 메서드

    public void ClosePopup()
    {
        // 닫기 콜백 실행 (메일박스용)
        if (currentPopupType == PopupType.Mailbox)
        {
            onMailboxCloseCallback?.Invoke();
        }

        HideAllPanels();
        ShowBackground(false); 
        
        // 콜백 함수 참조 정리
        onNoCallback = null;
        onYesCallback = null;
        onNicknameSaveCallBack = null;
        onRoomCodeJoinCallBack = null;
        onMailboxCloseCallback = null;
        
        isProcessingPasswordChange = false;
        currentPopupType = PopupType.None;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 메일박스 팝업 표시 퍼블릭으로 내뺴두기.
    /// </summary>
    public void ShowMailPopup()
    {
        ShowMailboxPopup();
    }

    #endregion
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopPanelSystem : MonoBehaviour
{
    [Header("Player Data Controller")] [SerializeField]
    private PlayerDataController _playerDataController;

    [Header("TopPanel UI Elements")] [SerializeField]
    private TextMeshProUGUI _playerNicknameText;

    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _diamondText;
    [SerializeField] private TextMeshProUGUI _staminaText;
    [SerializeField] private TextMeshProUGUI _staminaTimerText;

    [Header("Player Profile")] [SerializeField]
    private GameObject _playerProfilePopup;

    [SerializeField] private PlayerProfilePopup _profilePopup;
    [SerializeField] private Button _playerProfileButton;

    private void Start()
    {
        _playerProfileButton.onClick.AddListener(ShowPlayerProfilePopup);

        // PlayerDataController 이벤트 구독
        if (_playerDataController != null)
        {
            _playerDataController.OnUpdateUI += UpdateTopPanelUI;
            _playerDataController.OnStaminaRecoveryTimer += UpdateStaminaTimer;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_playerDataController != null)
        {
            _playerDataController.OnUpdateUI -= UpdateTopPanelUI;
            _playerDataController.OnStaminaRecoveryTimer -= UpdateStaminaTimer;
        }

        // 버튼 이벤트 해제
        if (_playerProfileButton != null)
        {
            _playerProfileButton.onClick.RemoveListener(ShowPlayerProfilePopup);
        }
    }

    /// <summary>
    /// Top 패널 UI 전체 업데이트
    /// </summary>
    /// <param name="playerData">플레이어 데이터</param>
    public void UpdateTopPanelUI(PlayerData playerData)
    {
        if (playerData == null) return;

        // 플레이어 정보 업데이트
        if (_playerNicknameText != null)
            _playerNicknameText.text = playerData.PlayerName;

        if (_goldText != null)
            _goldText.text = playerData.Gold.ToString("N0");

        if (_diamondText != null)
            _diamondText.text = playerData.Diamond.ToString("N0");

        if (_staminaText != null)
            _staminaText.text = $"{playerData.Stamina}/{playerData.MaxStamina}";
    }

    /// <summary>
    /// 스테미나 타이머 업데이트
    /// </summary>
    /// <param name="remainingSeconds">남은 시간</param>
    public void UpdateStaminaTimer(int remainingSeconds)
    {
        if (_staminaTimerText == null) return;

        if (remainingSeconds <= 0)
        {
            _staminaTimerText.text = "";
            return;
        }

        // 분:초 형태로 표시

        int minutes = (remainingSeconds % 3600) / 60;
        int seconds = remainingSeconds % 60;


        _staminaTimerText.text = $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// 플레이어 프로필 팝업 표시
    /// </summary>
    private void ShowPlayerProfilePopup()
    {
        if (_playerProfilePopup == null || _profilePopup == null || _playerDataController == null)
            return;

        _profilePopup.Init(_playerDataController.Data);
        _profilePopup.EnableDataBind(true);
        _playerProfilePopup.SetActive(true);
    }
}
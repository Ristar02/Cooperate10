using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailBoxPopup : MonoBehaviour
{
    private PlayerMailBoxController _controller;
    private PlayerDataController _playerDataController;

    [Header("List")]
    [SerializeField] private RectTransform _content;
    [SerializeField] private GameObject _mailItemPrefab;

    [Header("Button")]
    [SerializeField] private Button _receiveAllButton;

    [Header("Panel")]
    [SerializeField] private GameObject _emptyMailViewPanel;
    [SerializeField] private GameObject _receiveAllInfoPanel;
    [SerializeField] private GameObject _gold;
    [SerializeField] private GameObject _diamond;

    private bool _isDataBind = false;
    private bool _isReceivingAll = false;
    private bool _isInitialized = false;

    private List<MailData> _currentMails;
    private void Apply(List<MailData> mails) => Init(mails);

    /// <summary>
    /// 외부에서 컨트롤러들을 주입받아 초기화하는 메서드
    /// </summary>
    public void Initialize(PlayerDataController playerDataController, PlayerMailBoxController mailboxController)
    {
        _playerDataController = playerDataController;
        _controller = mailboxController;
        
        if (_controller != null)
        {
            // 메일 데이터 초기화
            if (_controller.Mail != null)
            {
                Init(_controller.Mail);
            }
            
            // 데이터 바인딩 활성화
            EnableDataBind(true);
        }
        
        // 플레이어 데이터 이벤트 구독
        if (_playerDataController != null)
        {
            _playerDataController.OnUpdateUI += OnPlayerDataUpdated;
        }
        
        _isInitialized = true;
    }

    private void OnPlayerDataUpdated(PlayerData data)
    {
       
    }

    private void Start()
    {
        _receiveAllButton.onClick.RemoveAllListeners();
        _receiveAllButton.onClick.AddListener(OnClickReceiveAll);
    }

    private void OnEnable()
    {
        // Initialize가 호출되지 않았다면 컨트롤러 찾기 시도
        if (!_isInitialized)
        {
            _controller = FindObjectOfType<PlayerMailBoxController>();
            _playerDataController = FindObjectOfType<PlayerDataController>();
            
            if (_controller != null)
            {
                EnableDataBind(true);
                if (_controller.Mail != null)
                {
                    Init(_controller.Mail);
                }
            }
        }
        
        if (_currentMails == null || _currentMails.Count == 0)
        {
            CheckEmptyMailBox(_currentMails);
        }
    }

    private void OnDisable()
    {
        if (_isDataBind)
        {
            _controller.OnMailboxUpdated -= Apply;
            _isDataBind = false;
        }
        
        if (_playerDataController != null)
        {
            _playerDataController.OnUpdateUI -= OnPlayerDataUpdated;
        }
    }

    /// <summary>
    /// 유저 DB 메일 데이터 리스트 -> 우편함 UI 재빌드하는 메서드
    /// </summary>
    /// <param name="mails">유저 DB 메일 데이터 리스트</param>
    public void Init(List<MailData> mails)
    {
        _currentMails = mails;

        CheckEmptyMailBox(mails);

        // 기존 메일 제거
        for (int i = _content.childCount - 1; i >= 0; i--)
        {
            Destroy(_content.GetChild(i).gameObject);
        }

        // IsReceived = false 메일을 우편함에 생성 + 바인딩
        foreach (var mail in mails)
        {
            if (mail.IsReceived == false)
            {
                GameObject mailObject = Instantiate(_mailItemPrefab, _content);

                // 비활성화로 생성 / 데이터 바인드 이후 OnEnable 실행
                mailObject.SetActive(false);

                MailItem mailItem = mailObject.GetComponent<MailItem>();
                if (mailItem != null)
                {
                    mailItem.Bind(mail, _controller);
                }

                mailObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// DB 데이터를 팝업에 실시간 연동 활성화/비활성화 하는 메서드
    /// </summary>
    public void EnableDataBind(bool enable)
    {
        if (_controller == null) return;
        if (_isDataBind == enable) return;
        
        _isDataBind = enable;

        if (_isDataBind)
            _controller.OnMailboxUpdated += Apply;
        else
            _controller.OnMailboxUpdated -= Apply;
    }

    private void CheckEmptyMailBox(List<MailData> mails)
    {
        if (mails == null || mails.Count == 0 || 
            mails.TrueForAll(m => m.IsReceived)) 
        {
            _emptyMailViewPanel.SetActive(true);
            _receiveAllButton.interactable = false;
            return;
        }
        else
        {
            _emptyMailViewPanel.SetActive(false);
            _receiveAllButton.interactable = true;
        }
    }

    /// <summary>
    /// 모두 받기 버튼 클릭 -> 보상 UI 활성화하는 메서드
    /// </summary>
    private async void OnClickReceiveAll()
    {
        if (_isReceivingAll) return;
        if (_controller == null) return;

        _isReceivingAll = true;
        _receiveAllButton.interactable = false;

        var (totalGold, totalDiamond) = await _controller.ReceiveAllAsync();

        if (totalGold > 0 || totalDiamond > 0)
        {
            _receiveAllInfoPanel.SetActive(true);

            if (totalGold > 0)
            {
                _gold.SetActive(true);
                _receiveAllInfoPanel.GetComponent<ReceiveAllPanel>().SetGoldInfo(totalGold);
            }

            if (totalDiamond > 0)
            {
                _diamond.SetActive(true);
                _receiveAllInfoPanel.GetComponent<ReceiveAllPanel>().SetDiamondInfo(totalDiamond);
            }
        }

        _isReceivingAll = false;
        _currentMails.Clear();
        CheckEmptyMailBox(_currentMails);
    }

    private void OnDestroy()
    {
        // 클린업
        if (_receiveAllButton != null)
        {
            _receiveAllButton.onClick.RemoveListener(OnClickReceiveAll);
        }
        
        if (_controller != null)
        {
            _controller.OnMailboxUpdated -= Apply;
        }
        
        if (_playerDataController != null)
        {
            _playerDataController.OnUpdateUI -= OnPlayerDataUpdated;
        }
    }
}
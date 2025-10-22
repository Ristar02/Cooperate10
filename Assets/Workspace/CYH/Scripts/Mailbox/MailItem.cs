using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailItem : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private Image _rewardImage;
    [SerializeField] private Sprite _goldSprite;
    [SerializeField] private Sprite _diamondSprite;
    [SerializeField] private TMP_Text _rewardText;

    [Header("Info")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;

    [Header("Button")]
    [SerializeField] private Button _receiveButton;
    [SerializeField] private TMP_Text _receiveText;
    [SerializeField] private TMP_Text _expireDateText;
    [SerializeField] private GameObject _timeBoxImage;
    [SerializeField] private GameObject _badgeImage;

    private bool _isClicked = true;
    private Coroutine _countdownRoutine;
    private PlayerMailBoxController _controller;
    private MailData _data;


    private void OnEnable()
    {
        StopCountdown();

        if (_data == null)
        {
            return;
        }

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (!_data.IsExpired(currentTime))
        {
            _countdownRoutine = StartCoroutine(CountdownRoutine(_data.ExpireDate));
        }
    }

    private void OnDisable()
    {
        StopCountdown();
    }

    private void OnDestroy()
    {
        StopCountdown();
    }

    /// <summary>
    /// 메일 정보를 UI와 바인딩하는 메서드
    /// </summary>
    /// <param name="data">메일 데이터</param>
    public void Bind(MailData data, PlayerMailBoxController controller)
    {
        _data = data;
        _controller = controller;
        _isClicked = false;

        _titleText.text = data.Title;
        _bodyText.text = data.Body;
        _rewardText.text = (data.Gold == 0) ? $"{data.Diamond}" : $"{data.Gold}";
        _rewardImage.sprite = (data.Gold == 0) ? _diamondSprite : _goldSprite;

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        bool expired = data.IsExpired(currentTime);

        _receiveButton.interactable = !data.IsReceived && !expired;
        _receiveButton.onClick.RemoveAllListeners();
        _receiveButton.onClick.AddListener(() =>
        {
            if (!_isClicked && !expired && !data.IsReceived)
            {
                _isClicked = true;
                StopCountdown();
                _controller.ReceiveRewardAsync(_data.MailId);
            }

            if (expired)
            {
                ExpiredMailAsync();
            }
        });

        if (!expired)
        {
            _timeBoxImage.SetActive(true);
            _badgeImage.SetActive(true);
        }
        else
        {
            ExpiredMailAsync();
        }
    }

    /// <summary>
    /// 기한 만료 메일 상태 변경 및 삭제하는 메서드
    /// </summary>
    private async void ExpiredMailAsync()
    {
        SetExpiredUI();
        await Task.Delay(3000);
        await _controller.DeleteMail(_data.MailId);
    }

    /// <summary>
    /// 메일 만료 시간 확인 및 남은 시간을 UI에 갱신하는 메서드
    /// 만료 시 기한 만료 상태로 업데이트
    /// </summary>
    /// <param name="expireDate">메일 만료 시간</param>
    private IEnumerator CountdownRoutine(long expireDate)
    {
        while (true)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long remain = expireDate - currentTime;

            if (remain <= 0)
            {
                SetExpiredUI();
                _countdownRoutine = null;
                _controller.SetIsExpired(_data.MailId);
                yield break;
            }

            UpdateRemainText(remain);
            yield return new WaitForSeconds(1f);
        }
    }

    private void StopCountdown()
    {
        if (_countdownRoutine != null)
        {
            StopCoroutine(_countdownRoutine);
            _countdownRoutine = null;
        }
    }

    /// <summary>
    /// 잔여 만료 시간 표시
    /// </summary>
    /// <param name="remains"></param>
    private void UpdateRemainText(long remains)
    {
        TimeSpan remain = TimeSpan.FromMilliseconds(remains);
        int days = remain.Days;
        int hours = remain.Hours;
        int minutes = remain.Minutes;
        int seconds = remain.Seconds;

        _expireDateText.text = $"{days}일 {hours}시간 {minutes}분 {seconds}초";
    }

    /// <summary>
    /// 기한 만료 UI 설정
    /// </summary>
    private void SetExpiredUI()
    {
        var receiveTextPos = _receiveText.GetComponent<RectTransform>();
        var anchoredPos = receiveTextPos.anchoredPosition;
        anchoredPos.y = -50f;
        receiveTextPos.anchoredPosition = anchoredPos;

        _receiveText.text = "기간 만료";
        _badgeImage.SetActive(false);
        _timeBoxImage.SetActive(false);

        // 버튼 비활성화
        _receiveButton.interactable = false;
    }
}

using System;
using System.Collections;
using System.Data;
using System.Net.Http.Headers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaUIController : MonoBehaviour
{
    [Header("CharAdButtonUI")]
    [SerializeField] private Image[] _charAdImages;
    [SerializeField] private GameObject _charAdCooltimeImage;
    [SerializeField] private TMP_Text _charAdCooltimeText;

    [Header("CharOneButtonUI")]
    [SerializeField] private TMP_Text _charOneText;
    [SerializeField] private GameObject _charFreeGacha;
    [SerializeField] private GameObject _charConsumeGacha;
    [SerializeField] private GameObject _charDailyCooltimeImage;
    [SerializeField] private TMP_Text _charDailyCooltimeText;

    [Header("StoneAdButtonUI")]
    [SerializeField] private Image[] _stoneAdImages;
    [SerializeField] private GameObject _stoneAdCooltimeImage;
    [SerializeField] private TMP_Text _stoneAdCooltimeText;

    [Header("StoneOneButtonUI")]
    [SerializeField] private TMP_Text _stoneOneText;
    [SerializeField] private GameObject _stoneFreeGacha;
    [SerializeField] private GameObject _stoneConsumeGacha;
    [SerializeField] private GameObject _stoneDailyCooltimeImage;
    [SerializeField] private TMP_Text _stoneDailyCooltimeText;

    private Coroutine _adCooltimeTimer;

    private Coroutine _dailyCooltimeTimer;

    private void Start()
    {
        UpdateUI();
    }

    #region Event

    private void OnEnable()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnDailyGachaInfoChanged += UpdateUI;
        StartAdTimer();
        StartDailyTimer();
    }

    private void OnDisable()
    {
        TimeManager.Instance.OnDailyGachaInfoChanged -= UpdateUI;
        StopAdTimer();
        StopDailyTimer();
    }

    #endregion

    #region AdButtonTimer

    private void StartAdTimer()
    {
        if (_adCooltimeTimer != null)
        {
            StopCoroutine(_adCooltimeTimer);
        }
        _adCooltimeTimer = StartCoroutine(AdCooltimeCoroutine());
    }

    private void StopAdTimer()
    {
        if (_adCooltimeTimer != null)
        {
            StopCoroutine(_adCooltimeTimer);
            _adCooltimeTimer = null;
        }
    }

    #endregion

    #region DailyButtonTimer

    private void StartDailyTimer()
    {
        if (_dailyCooltimeTimer != null)
        {
            StopCoroutine(_dailyCooltimeTimer);
        }
        _dailyCooltimeTimer = StartCoroutine(DailyCooltimeCoroutine());
    }

    private void StopDailyTimer()
    {
        if (_dailyCooltimeTimer != null)
        {
            StopCoroutine(_dailyCooltimeTimer);
            _dailyCooltimeTimer = null;
        }
    }

    #endregion

    #region UIUpdate

    private void UpdateUI()
    {
        UpdateAdButton();
        UpdateOneButton();
    }

    private void UpdateAdButton()
    {
        if (TimeManager.Instance != null)
        {
            switch (TimeManager.Instance.DailyCharAdGachaRewardInfo.state)
            {
                case 2:
                    _charAdImages[0].color = Color.white;
                    _charAdImages[1].color = Color.white;
                    _charAdCooltimeImage.gameObject.SetActive(false);
                    break;

                case 1:
                    _charAdImages[0].color = Color.grey;
                    _charAdImages[1].color = Color.white;
                    _charAdCooltimeImage.gameObject.SetActive(true);
                    break;
                case 0:
                    _charAdImages[0].color = Color.grey;
                    _charAdImages[1].color = Color.grey;
                    _charAdCooltimeImage.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }

            switch(TimeManager.Instance.DailyStoneAdGachaRewardInfo.state)
            {
                case 2:
                    _stoneAdImages[0].color = Color.white;
                    _stoneAdImages[1].color = Color.white;
                    _stoneAdCooltimeImage.gameObject.SetActive(false);
                    break;

                case 1:
                    _stoneAdImages[0].color = Color.grey;
                    _stoneAdImages[1].color = Color.white;
                    _stoneAdCooltimeImage.gameObject.SetActive(true);
                    break;
                case 0:
                    _stoneAdImages[0].color = Color.grey;
                    _stoneAdImages[1].color = Color.grey;
                    _stoneAdCooltimeImage.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }

    private void UpdateOneButton()
    {
        if (TimeManager.Instance != null)
        {
            if (TimeManager.Instance.DailyCharFreeGachaRewardInfo.state == 1)
            {
                _charFreeGacha.SetActive(true);
                _charConsumeGacha.SetActive(false);
                _charDailyCooltimeImage.gameObject.SetActive(false);
                _charOneText.text = "일일 무료";
            }
            else
            {
                _charFreeGacha.SetActive(false);
                _charConsumeGacha.SetActive(true);
                _charDailyCooltimeImage.gameObject.SetActive(true);
                _charOneText.text = "1회 뽑기";
            }

            if (TimeManager.Instance.DailyStoneFreeGachaRewardInfo.state == 1)
            {
                _stoneFreeGacha.SetActive(true);
                _stoneConsumeGacha.SetActive(false);
                _stoneDailyCooltimeImage.gameObject.SetActive(false);
                _stoneOneText.text = "일일 무료";
            }
            else
            {
                _stoneFreeGacha.SetActive(false);
                _stoneConsumeGacha.SetActive(true);
                _stoneDailyCooltimeImage.gameObject.SetActive(true);
                _stoneOneText.text = "1회 뽑기";
            }
        }
    }

    #endregion

    private IEnumerator AdCooltimeCoroutine()
    {
        while (true)
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.CanObtainAdGachaReward(GachaType.Char);
                TimeManager.Instance.CanObtainAdGachaReward(GachaType.Stone);
                
                DateTime now = DateTime.Now;

                DateTime charLastTime = TimeManager.Instance.DailyCharAdGachaRewardInfo.GetDateTime();
                TimeSpan charCooltime = charLastTime.AddHours(12) - now;
                _charAdCooltimeText.text = $"다음 초기화 : {charCooltime.Hours}시간 {charCooltime.Minutes}분";

                DateTime stoneLastTime = TimeManager.Instance.DailyStoneAdGachaRewardInfo.GetDateTime();
                TimeSpan stoneCooltime = stoneLastTime.AddHours(12) - now;
                _stoneAdCooltimeText.text = $"다음 초기화 : {stoneCooltime.Hours}시간 {stoneCooltime.Minutes}분";
            }

            UpdateAdButton();

            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator DailyCooltimeCoroutine()
    {
        while (true)
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.CanObtainedFreeGachaReward(GachaType.Char);
                TimeManager.Instance.CanObtainedFreeGachaReward(GachaType.Stone);

                DateTime now = DateTime.Now;
                DateTime charNextdate = TimeManager.Instance.DailyCharFreeGachaRewardInfo.GetDateTime();
                TimeSpan charCooltime = charNextdate - now;
                _charDailyCooltimeText.text = $"다음 초기화 : {charCooltime.Hours}시간 {charCooltime.Minutes}분";

                DateTime stoneNextdate = TimeManager.Instance.DailyStoneFreeGachaRewardInfo.GetDateTime();
                TimeSpan stoneCooltime = stoneNextdate - now;
                _stoneDailyCooltimeText.text = $"다음 초기화 : {stoneCooltime.Hours}시간 {stoneCooltime.Minutes}분";
            }

            UpdateOneButton();

            yield return new WaitForSeconds(1);
        }
    }
}
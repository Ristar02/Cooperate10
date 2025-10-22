using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Map
{
    public class EventPanel : MonoBehaviour
    {
        public static EventPanel Instance;

        [Header("UI References")] 
        public GameObject _eventPanelUI;
        public TMP_Text _titleText;
        public TMP_Text _descriptionText;
        public CanvasGroup _descriptionCanvasGroup;
        
        [Header("Choice Buttons")]
        public Button _yesButton;
        public Button _nvmButton;
        public Button _continueButton;

        [Header("Fade Settings")]
        public float _fadeDuration = 0.5f;

        private EventData _currentEvent;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (_eventPanelUI != null)
            {
                _eventPanelUI.SetActive(false);
            }

            if (_yesButton != null)
            {
                _yesButton.onClick.AddListener(() => OnChoiceSelected(true));
            }

            if (_nvmButton != null)
            {
                _nvmButton.onClick.AddListener(() => OnChoiceSelected(false));
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(CloseEvent);
                _continueButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 이벤트 표시
        /// </summary>
        public void ShowEvent(EventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            _currentEvent = eventData;
            _eventPanelUI.SetActive(true);

            _titleText.text = eventData._eventTitle;
            _descriptionText.text = eventData._eventDescription;
            
            _yesButton.gameObject.SetActive(true);
            _nvmButton.gameObject.SetActive(true);
            _continueButton.gameObject.SetActive(false);

            if (_descriptionCanvasGroup != null)
            {
                _descriptionCanvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Yes 또는 Nvm 선택
        /// </summary>
        private void OnChoiceSelected(bool acceptChallenge)
        {
            _yesButton.gameObject.SetActive(false);
            _nvmButton.gameObject.SetActive(false);

            StartCoroutine(ShowResultWithFade(acceptChallenge));
        }

        /// <summary>
        /// Fade 연출과 함께 결과 표시
        /// </summary>
        private IEnumerator ShowResultWithFade(bool acceptChallenge)
        {
            yield return StartCoroutine(FadeOut());

            EventOutcome outcome;
            
            if (acceptChallenge)
            {
                // Energy 소비
                ConsumeEnergy(_currentEvent._energyCost);
                
                // 50% 확률로 성공/실패 결정
                bool isSuccess = Random.Range(0f, 1f) < 0.5f;
                outcome = isSuccess ? EventOutcome.Success : EventOutcome.Failure;
            }
            else
            {
                outcome = EventOutcome.Declined;
            }

            // Switch로 결과 처리
            switch (outcome)
            {
                case EventOutcome.Success:
                    _descriptionText.text = _currentEvent._successText;
                    ApplyReward();
                    break;

                case EventOutcome.Failure:
                    _descriptionText.text = _currentEvent._failureText;
                    break;

                case EventOutcome.Declined:
                    _descriptionText.text = _currentEvent._declinedText;
                    break;
            }

            yield return StartCoroutine(FadeIn());

            _continueButton.gameObject.SetActive(true);
        }

        private IEnumerator FadeOut()
        {
            if (_descriptionCanvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _descriptionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
                yield return null;
            }
            _descriptionCanvasGroup.alpha = 0f;
        }

        private IEnumerator FadeIn()
        {
            if (_descriptionCanvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _descriptionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / _fadeDuration);
                yield return null;
            }
            _descriptionCanvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Energy 소비
        /// </summary>
        private void ConsumeEnergy(int amount)
        {
            // TODO: 실제 에너지 시스템 연결
        }

        /// <summary>
        /// 성공 시 보상 지급
        /// </summary>
        private void ApplyReward()
        {
            
                // TODO: 보상 시스템 연동해주시면 됩니당
                // 마법석 , 은화 , 증강
            
        }

        /// <summary>
        /// 실패 시 추가 패널티 적용
        /// </summary>
        private void ApplyPenalty()
        {
           
                // TODO: 패널티 시스템 연결
           
        }

        public void CloseEvent()
        {
            _eventPanelUI.SetActive(false);

            if (MapPlayerTracker.Instance != null)
            {
                MapPlayerTracker.Instance.Locked = false;
            }
        }

        private void OnDestroy()
        {
            if (_yesButton != null)
                _yesButton.onClick.RemoveAllListeners();
            
            if (_nvmButton != null)
                _nvmButton.onClick.RemoveAllListeners();
            
            if (_continueButton != null)
                _continueButton.onClick.RemoveAllListeners();
        }
    }
}
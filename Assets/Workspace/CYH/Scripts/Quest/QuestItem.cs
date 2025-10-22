using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestItem : MonoBehaviour
{
    [Header("Point")]
    [SerializeField] private TMP_Text _pointText;

    [Header("Info")]
    [SerializeField] private TMP_Text _questDesc;
    [SerializeField] private Image _progressBar;

    [Header("Condition")]
    [SerializeField] private Button _questButton;
    [SerializeField] private Image _questBoxImage;
    [SerializeField] private GameObject _badgeImage;
    [SerializeField] private TMP_Text _conditionText;

    public event Action OnRewardReceived;

    public void Init(IQuestView questData)
    {
        // TODO: [CYH] 정렬

        _pointText.text = questData.RewardPoint.ToString();
        _questDesc.text = questData.QuestDesc;

        _progressBar.fillAmount = (float)questData.CurProgress / questData.MaxProgress;
        _conditionText.text = $"{questData.CurProgress} / {questData.MaxProgress}";

        if (questData.IsReceive)
        {
            _questButton.interactable = false;
        }

        if (questData.IsComplete && !questData.IsReceive)
        {
            _questBoxImage.color = Color.red;
            _badgeImage.SetActive(true);
        }
                
        _questButton.onClick.RemoveAllListeners();
        _questButton.onClick.AddListener(() =>
        {
            if(questData.IsComplete)
            {
                QuestManager.Instance.ReceiveReward(questData);
                OnRewardReceived?.Invoke();
            } 
        });
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameSettingButtonPanel : MonoBehaviour
{
    [SerializeField] Button _accelerateButton;
    [SerializeField] TMP_Text _accelerateAmount;

    [SerializeField] Button _pauseButton;

    private void OnEnable()
    {
        _accelerateButton.onClick.AddListener(NextAccelerate);
        Manager.Game.CurrentAccelerate.AddEvent(AccelerateAmountUpdate);

        _pauseButton.onClick.AddListener(ChangePause);
    }

    private void OnDisable()
    {
        _accelerateButton.onClick.RemoveListener(NextAccelerate);
        Manager.Game.CurrentAccelerate.RemoveEvent(AccelerateAmountUpdate);

        _pauseButton.onClick.RemoveListener(ChangePause);
    }

    private void NextAccelerate()
    {
        Manager.Game.NextAccelerate();
    }

    private void ChangePause()
    {
        Manager.Game.ChangePause();
    }

    private void AccelerateAmountUpdate(float amount)
    {
        _accelerateAmount.text = amount.ToString();
    }
}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PresetListButtonController : MonoBehaviour
{
    private const int BUTTON_MOVE_ID = 1134256432;
    private float _currentY = 0;
    private Button[] _buttons;

    [SerializeField] Button _openButton;
    [SerializeField] TMP_Text _currentIdxText;
    [SerializeField] GameObject _presetButtons;
    [SerializeField] Transform _targetTransform;
    private bool _isOpen;

    private void Awake()
    {
        _currentY = _presetButtons.transform.position.y;
    }

    private void OnEnable()
    {
        _openButton.onClick.AddListener(Switch);
    }

    private void OnDisable()
    {
        _openButton.onClick.RemoveListener(Switch);
    }

    public void SetCurrentIdx(int currentIdx)
    {
        _currentIdxText.text = currentIdx.ToString();
    }

    private void Switch()
    {
        if(_isOpen)
        {
            DeActive();
        }
        else
        {
            Active();
        }
    }

    private void Active()
    {
        DOTween.Kill(BUTTON_MOVE_ID);

        _presetButtons.transform.DOMoveY(_targetTransform.position.y, .3f).SetId(BUTTON_MOVE_ID);
        _isOpen = true;
    }

    private void DeActive()
    {
        DOTween.Kill(BUTTON_MOVE_ID);

        _presetButtons.transform.DOMoveY(_currentY, .3f).SetId(BUTTON_MOVE_ID);
        _isOpen = false;
    }
}

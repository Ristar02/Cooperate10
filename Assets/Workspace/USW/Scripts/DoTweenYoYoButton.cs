using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class DoTweenYoYoButton : MonoBehaviour
{
    [Header("Target")] public Transform[] targets;
    public Button[] buttons;

    [Header("Scale Settings")] [Range(0.1f, 3f)]
    public float minScale = 0.8f;

    [Range(0.1f, 3f)] public float maxScale = 1.0f;

    [Header("Timing Settings")] [Min(0f)] public float moveDuration = 0.4f;
    [Min(0f)] public float yoyoDelay = 0.4f;

    [Header("Option")] public Ease upEase = Ease.OutSine;
    public bool ignoreTimeScale = false;
    
    
    private Sequence _current;
    private int _activeIndex = -1;


    private void Awake()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    private void Start()
    {
        DOTween.Init();
        Debug.Log("Dotween init");

        if (targets != null && targets.Length > 0)
        {
            SwitchActive(0);
            ApplyInteractableState(0);
        }
    }

    private void OnDisable()
    {
        KillCurrent();
    }

    void OnButtonClicked(int index)
    {
        int next = index + 1;

        if (next >= targets.Length)
        {
            KillCurrent();
            _activeIndex = -1;
            return;
        }

        SwitchActive(next);
    }

    void SwitchActive(int newIndex)
    {
        if (newIndex < 0 || newIndex >= targets.Length) return;
        
        KillCurrent();
        
        var target = targets[newIndex];
        if (!target) return;

        target.DOKill();
        
        target.localScale = Vector3.one*minScale;
        
        var yoyo = target
            .DOScale(maxScale,moveDuration)
            .SetEase(upEase)
            .SetLoops(2, LoopType.Yoyo);
        
        var seq = DOTween.Sequence();
        if (ignoreTimeScale) seq.SetUpdate(true);
        seq.Append(yoyo);
        seq.AppendInterval(yoyoDelay);
        seq.SetLoops(-1, LoopType.Restart);
        seq.Play();

        _current = seq;
        _activeIndex = newIndex;
        
        ApplyInteractableState(_activeIndex);
    }
    void KillCurrent()
    {
        if (_current != null && _current.IsActive())
        {
            _current.Kill();
            _current = null;
        }
    }

    void ApplyInteractableState(int activeIndex, bool falseAll = false)
    {
        if (buttons == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (!buttons[i]) continue;
            if (falseAll) {buttons[i].interactable = false; continue;}
            buttons[i].interactable = (i==activeIndex);
        }
    }
    
}
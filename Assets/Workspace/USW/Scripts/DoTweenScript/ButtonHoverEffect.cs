using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("스케일 설정")] [SerializeField] private float hoverScale = 0.8f;
    [SerializeField] float normalScale = 1.0f;
    
    [Header("Dotween animation Settings")]
    [SerializeField] float animationDuration = 0.3f;
    [SerializeField] Ease ease = Ease.InOutBack;
    
    Vector3 originalScale;
    Sequence hoverSequence;


    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSequence != null)
        {
            hoverSequence.Kill();
        }
        
        hoverSequence = DOTween.Sequence();
        
        hoverSequence.Append(transform.DOScale(originalScale * hoverScale, animationDuration).SetEase(ease));


        hoverSequence.Play();
        // 펀치 효과 ? 
        // ease 타입을 뭐로
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverSequence != null)
        {
            hoverSequence.Kill();
        }
        
        hoverSequence = DOTween.Sequence();
        
        hoverSequence.Append(transform.DOScale(originalScale * normalScale, animationDuration).SetEase(ease));
        
        hoverSequence.Play();
        
    }

    private void OnDestroy()
    {
        if (hoverSequence != null)
        {
            hoverSequence.Kill();
        }
    }
}

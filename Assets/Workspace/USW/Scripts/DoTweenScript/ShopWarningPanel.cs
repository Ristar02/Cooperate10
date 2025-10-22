using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShopWarningPanel : MonoBehaviour
{
    private Tween popupTween;
    
    private void Start()
    {
        popupTween = transform.DOScale(0.8f, 0.3f)
            .SetEase(Ease.OutBack)
            .SetAutoKill(false); 
    }
    
    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        if (popupTween != null)
        {
            popupTween.Restart();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerProfilePop : MonoBehaviour
{
    private Tween popupTween;
    
    private void Start()
    {
        popupTween = transform.DOScale(1f, 0.3f)
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


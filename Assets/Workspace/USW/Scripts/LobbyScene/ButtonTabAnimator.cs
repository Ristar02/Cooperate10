using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTabAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Button iconButton;
    [SerializeField] private Button frameButton;
    
    private bool isIconActive = true;
    
    private void Awake()
    {
        iconButton.onClick.AddListener(() => SwitchTo(true));
        frameButton.onClick.AddListener(() => SwitchTo(false));
    }
    
    private void SwitchTo(bool toIcon)
    {
        // 이미 같은 상태면 무시
        if (isIconActive == toIcon) return;
        
        // 상태 변경
        isIconActive = toIcon;
        
        // 애니메이션 재생
        if (toIcon)
            animator.SetTrigger("ToIcon");
        else
            animator.SetTrigger("ToFrame");
    }
}

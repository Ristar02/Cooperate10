using UnityEngine;
using UnityEngine.UI;

public class ButtonAnimator : MonoBehaviour
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
        if (isIconActive == toIcon) return;
        isIconActive = toIcon;
        
        if (toIcon)
            animator.SetTrigger("ToIcon");
        else
            animator.SetTrigger("ToFrame");
    }
}
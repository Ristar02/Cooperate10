using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StagePanelToggle : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform panel;      // 내려올 패널
    [SerializeField] private Button toggleButton;      // 탭(클릭) 트리거

    [Header("Motion")]
    [SerializeField] private float dropDistance = 750f;
    [SerializeField] private float dropDuration = 1.5f;
    [SerializeField] private float settleUp = 20f;
    [SerializeField] private float settleDuration = 0.15f;
    [SerializeField] private float hideDuration = 1.2f;

    [SerializeField] private Ease dropEase = Ease.OutBounce;
    [SerializeField] private Ease settleEase = Ease.OutQuad;
    [SerializeField] private Ease hideEase = Ease.InQuad;

    private bool isShown = false;
    private Sequence seq;
    private Vector2 anchoredShown;
    private Vector2 anchoredHidden;

    private void Awake()
    {
        anchoredShown = panel.anchoredPosition;
        anchoredHidden = anchoredShown + new Vector2(0, dropDistance);
        panel.anchoredPosition = anchoredHidden;

        if (toggleButton != null) toggleButton.onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        if (seq != null && seq.IsActive()) seq.Kill();

        if (!isShown) PlayShow();
        else PlayHide();

        isShown = !isShown;
    }

    private void PlayShow()
    {
        seq = DOTween.Sequence()
            .Append(panel.DOAnchorPos(anchoredShown, dropDuration).SetEase(dropEase))
            .Append(panel.DOAnchorPos(anchoredShown + new Vector2(0, settleUp), settleDuration * 0.6f).SetEase(settleEase))
            .Append(panel.DOAnchorPos(anchoredShown, settleDuration).SetEase(settleEase));
    }

    private void PlayHide()
    {
        seq = DOTween.Sequence()
            .Append(panel.DOAnchorPos(anchoredHidden, hideDuration).SetEase(hideEase));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 편의 디버그: 참조 비었는지 에디터에서 바로 경고
        if (panel == null) Debug.LogWarning("[StagePanelToggle] panel 참조가 비었습니다.");
        if (toggleButton == null) Debug.LogWarning("[StagePanelToggle] toggleButton 참조가 비었습니다.");
    }
#endif
}

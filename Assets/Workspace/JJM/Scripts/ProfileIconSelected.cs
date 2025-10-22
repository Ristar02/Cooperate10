using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ProfileIconSelected : MonoBehaviour
{
    [Header("Target Panel")]
    [SerializeField] private GameObject targetPanel;

    [Header("Button")]
    [SerializeField] private Button showButton;
    [SerializeField] private Button closeButton;

    [Header("Offset")]
    [SerializeField] private float initScale = 1.0f;
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.1f;

    private void Start()
    {
        DOTween.Init();
        showButton.onClick.AddListener(ShowPanel);
        closeButton.onClick.AddListener(HidePanel);
        targetPanel.transform.localScale = Vector3.one;
        targetPanel.SetActive(false);
    }

    private void ShowPanel()
    {
        targetPanel.SetActive(true);

        var seq = DOTween.Sequence();

        seq.Append(targetPanel.transform.DOScale(popScale, popDuration).SetEase(Ease.OutQuad));
        seq.Append(targetPanel.transform.DOScale(initScale, popDuration));

        seq.Play();
    }


    private void HidePanel()
    {
        var seq = DOTween.Sequence();
        targetPanel.transform.localScale = Vector3.one;

        seq.Append(targetPanel.transform.DOScale(popScale, popDuration).SetEase(Ease.OutQuad));
        seq.Append(targetPanel.transform.DOScale(initScale, popDuration));

        seq.Play().OnComplete(() => {  targetPanel.SetActive(false); });
    }
}

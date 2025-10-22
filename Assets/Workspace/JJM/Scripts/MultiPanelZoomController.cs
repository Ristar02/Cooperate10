using UnityEngine;
using DG.Tweening;

public class MultiPanelZoomController : MonoBehaviour
{
    [Header("Panels")]
    public RectTransform playerPanel;
    public RectTransform enemyPanel;

    [Header("Settings")]
    public float zoomScale = 0.7f;
    public Vector2 playerStartPos;
    public Vector2 enemyStartPos;
    public Vector2 centerPos = Vector2.zero;
    public float duration = 0.8f;

    public void PlayBattleUIAnim()
    {
        // 시작 위치 저장
        playerStartPos = playerPanel.anchoredPosition;
        enemyStartPos = enemyPanel.anchoredPosition;

        Sequence seq = DOTween.Sequence();

        // 1. 줌 (Scale 줄이기)
        seq.Append(playerPanel.DOScale(zoomScale, duration).SetEase(Ease.InOutQuad));
        seq.Join(enemyPanel.DOScale(zoomScale, duration).SetEase(Ease.InOutQuad));

        // 2. 중앙으로 이동
        seq.Join(playerPanel.DOAnchorPos(centerPos, duration).SetEase(Ease.OutBack));
        seq.Join(enemyPanel.DOAnchorPos(centerPos, duration).SetEase(Ease.OutBack));

        // 3. 충돌 효과
        seq.Append(playerPanel.DOShakePosition(0.3f, 20, 10));
        seq.Join(enemyPanel.DOShakePosition(0.3f, 20, 10));

        // 4. 원래 상태 복귀
        seq.Append(playerPanel.DOAnchorPos(playerStartPos, duration).SetEase(Ease.InOutQuad));
        seq.Join(enemyPanel.DOAnchorPos(enemyStartPos, duration).SetEase(Ease.InOutQuad));
        seq.Join(playerPanel.DOScale(1f, duration).SetEase(Ease.InOutQuad));
        seq.Join(enemyPanel.DOScale(1f, duration).SetEase(Ease.InOutQuad));

        seq.Play();
    }
}
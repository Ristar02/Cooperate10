using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SwipeUI : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public RectTransform content;   // 페이지들을 담고 있는 부모 (ex: Horizontal Layout Group 적용된 Content)
    public float swipeThreshold = 100f; // 스와이프 판정 거리
    public float tweenDuration = 0.3f;  // DOTween 이동 시간
    public Ease easeType = Ease.OutQuart; // 전환 이징

    public int currentPage = 0;
    public int totalPages = 3; // 페이지 개수 (자동으로 계산해도 됨)

    private Vector2 dragStartPos;

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 중일 때는 content 따라 움직이게 할 수도 있지만,
        // 간단히 구현하려면 생략 가능 (바로 EndDrag에서 처리)
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float diff = eventData.position.x - eventData.pressPosition.x;

        if (Mathf.Abs(diff) > swipeThreshold)
        {
            if (diff > 0 && currentPage > 0)
                currentPage--;
            else if (diff < 0 && currentPage < totalPages - 1)
                currentPage++;
        }

        MoveToPage(currentPage);
    }

    private void MoveToPage(int pageIndex)
    {
        float width = ((RectTransform)transform).rect.width;
        Vector2 targetPos = new Vector2(-pageIndex * width, 0);

        content.DOAnchorPos(targetPos, tweenDuration).SetEase(easeType);
    }
}

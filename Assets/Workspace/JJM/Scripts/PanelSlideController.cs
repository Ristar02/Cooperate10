using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;


public enum SlideDirection
{
    Up,
    Down,
    Left,
    Right
}
[System.Serializable]
public class SlideButtonInfo
{
    public Button moveButton;
    public RectTransform targetPanel;
    public SlideDirection direction;
}

public class PanelSlideController : MonoBehaviour
{
    public List<RectTransform> panels; // 패널은 한 번씩만 등록
    public List<SlideButtonInfo> slideButtons; // 버튼/타겟패널/방향 등록
    public float slideDuration = 0.4f;

    private RectTransform currentPanel;

    void Start()
    {
        if (panels == null || panels.Count == 0) return;

        // 첫 번째 패널을 기본 패널로 설정
        currentPanel = panels[0];
        currentPanel.anchoredPosition = Vector2.zero;

        // 나머지 패널은 화면 밖에 배치
        for (int i = 1; i < panels.Count; i++)
        {
            panels[i].anchoredPosition = new Vector2(2000, 0); // 임의의 화면 밖 위치
        }

        // 버튼에 이벤트 연결
        foreach (var info in slideButtons)
        {
            if (info.moveButton != null && info.targetPanel != null)
            {
                var targetPanel = info.targetPanel;
                var direction = info.direction;
                info.moveButton.onClick.AddListener(() => SlideToPanel(targetPanel, direction));
            }
        }
    }

    Vector2 GetStartPosition(SlideDirection direction, Rect rect)
    {
        switch (direction)
        {
            case SlideDirection.Up: return new Vector2(0, rect.height);
            case SlideDirection.Down: return new Vector2(0, -rect.height);
            case SlideDirection.Left: return new Vector2(-rect.width, 0);
            case SlideDirection.Right: return new Vector2(rect.width, 0);
            default: return Vector2.zero;
        }
    }

    Vector2 GetEndPosition(SlideDirection direction, Rect rect)
    {
        switch (direction)
        {
            case SlideDirection.Up: return new Vector2(0, -rect.height);
            case SlideDirection.Down: return new Vector2(0, rect.height);
            case SlideDirection.Left: return new Vector2(rect.width, 0);
            case SlideDirection.Right: return new Vector2(-rect.width, 0);
            default: return Vector2.zero;
        }
    }

    void SlideToPanel(RectTransform targetPanel, SlideDirection direction)
    {
        if (currentPanel == targetPanel)
            return;

        currentPanel.DOAnchorPos(GetEndPosition(direction, currentPanel.rect), slideDuration).SetEase(Ease.InOutQuad);
        targetPanel.anchoredPosition = GetStartPosition(direction, targetPanel.rect);
        targetPanel.DOAnchorPos(Vector2.zero, slideDuration).SetEase(Ease.InOutQuad);

        currentPanel = targetPanel;
    }
}
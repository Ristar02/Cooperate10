using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private float xOffset = 150;
    [SerializeField] private float yOffset = 150;
    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    protected virtual void AdjustPosition(Vector2 screenPos)
    {
        Vector2 offset = Vector2.zero;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        offset.x = (screenPos.x > screenSize.x / 2f) ? -xOffset : xOffset;
        offset.y = (screenPos.y > screenSize.y / 2f) ? -yOffset : yOffset;

        Vector2 finalScreenPos = screenPos + offset;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            finalScreenPos,
            null,
            out Vector2 localPoint
        );

        rect.localPosition = localPoint;
    }
}

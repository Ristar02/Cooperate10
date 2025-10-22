using System;
using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private Vector2 _minAnchor;
    private Vector2 _maxAnchor;
    private RectTransform _rectTransform;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void OnEnable()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        _minAnchor = Screen.safeArea.min;
        _maxAnchor = Screen.safeArea.max;

        _minAnchor.x /= Screen.width;
        _minAnchor.y /= Screen.height;
        _maxAnchor.x /= Screen.width;
        _maxAnchor.y /= Screen.height;

        _rectTransform.anchorMin = _minAnchor;
        _rectTransform.anchorMax = _maxAnchor;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 부모 RectTransform의 크기 변화에 따라 TextMeshProUGUI의 폰트 크기를 자동으로 조정하는 스크립트
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
[ExecuteAlways] // 에디터 모드에서도 동작
public class TextUISize : MonoBehaviour
{
    public RectTransform targetRect; // 크기를 참조할 패널(부모 등)
    public float baseFontSize = 36f; // 기준 폰트 크기(디자인 시점)
    public float baseWidth = 500f;   // 기준 패널 너비(디자인 시점)
    public float minFontSize = 18f; // 최소 폰트 크기
    public float maxFontSize = 100f; // 최대 폰트 크기

    private TextMeshProUGUI tmp;

    void Awake()
    {
        // TextMeshProUGUI 컴포넌트 캐싱
        tmp = GetComponent<TextMeshProUGUI>();
        // targetRect가 지정되지 않았다면 부모 RectTransform을 자동 할당
        if (targetRect == null)
            targetRect = GetComponentInParent<RectTransform>();
    }

    void Start()
    {
        UpdateFontSize();
    }

    void OnValidate()
    {
        // 인스펙터에서 값이 바뀔 때마다 폰트 크기 갱신
        if (tmp == null) tmp = GetComponent<TextMeshProUGUI>();
        UpdateFontSize();
    }

    /// <summary>
    /// targetRect의 현재 너비를 기준으로 폰트 크기를 비율에 맞게 조정
    /// </summary>
    void UpdateFontSize()
    {
        if (tmp == null) tmp = GetComponent<TextMeshProUGUI>();
        if (targetRect == null) return;
        float ratio = targetRect.rect.width / baseWidth;
        float newFontSize = baseFontSize * ratio;
        newFontSize = Mathf.Clamp(newFontSize, minFontSize, maxFontSize);
        tmp.fontSize = Mathf.RoundToInt(newFontSize);
    }
    /// <summary>
    /// RectTransform의 크기가 변경될 때마다 폰트 크기 갱신
    /// </summary>
    void OnRectTransformDimensionsChange()
    {
        if (tmp == null) tmp = GetComponent<TextMeshProUGUI>();
        UpdateFontSize();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GridLayoutGroup의 셀 크기와 간격을 부모 RectTransform의 크기에 맞춰 자동으로 조정하는 스크립트
/// </summary>
[RequireComponent(typeof(GridLayoutGroup))]
[ExecuteAlways]// 에디터 모드에서도 실행
public class GridLayoutUI : MonoBehaviour
{
    [Header("Grid 설정")]
    [Min(1)]
    public int columns = 10; // 한 줄에 몇 개

   
    public float spacingX = 10f; // 가로 간격
   
    public float spacingY = 10f; // 세로 간격

    [Header("Cell 비율 설정")]
    public float baseCellWidth = 250f;  // 기준 셀 가로
    public float baseCellHeight = 100f; // 기준 셀 세로

    private GridLayoutGroup grid;
    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
    }

    void Start()
    {
        // 인스펙터 값 적용 및 셀 크기 초기화
        ApplyInspectorSettings();
        UpdateCellSize();
    }

    void OnValidate()
    {
        // 인스펙터에서 값이 바뀔 때 바로 반영
        if (grid == null) grid = GetComponent<GridLayoutGroup>();
        ApplyInspectorSettings();
        UpdateCellSize();
    }

    /// <summary>
    /// 인스펙터에서 설정한 spacing 값을 GridLayoutGroup에 적용
    /// </summary>
    void ApplyInspectorSettings()
    {
        if (grid != null)
        {
            grid.spacing = new Vector2(spacingX, spacingY);
        }
    }

    /// <summary>
    /// 부모 RectTransform의 크기와 columns, spacing을 기준으로 셀 크기를 자동 계산하여 적용
    /// </summary>
    void UpdateCellSize()
    {
        if (grid == null) grid = GetComponent<GridLayoutGroup>();
        var rect = GetComponent<RectTransform>().rect;
        float totalSpacingX = (columns - 1) * grid.spacing.x;
        float cellWidth = (rect.width - totalSpacingX - grid.padding.left - grid.padding.right) / columns;

        // 비율 유지: baseCellWidth:baseCellHeight 비율로 세로 크기 계산
        float ratio = baseCellHeight / baseCellWidth;
        float cellHeight = cellWidth * ratio;

        grid.cellSize = new Vector2(cellWidth, cellHeight);
    }
    /// <summary>
    /// RectTransform의 크기가 변경될 때마다 셀 크기 재계산
    /// </summary>
    void OnRectTransformDimensionsChange()
    {
        UpdateCellSize();
    }
}

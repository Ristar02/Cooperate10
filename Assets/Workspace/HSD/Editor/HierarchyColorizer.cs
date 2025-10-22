#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class HierarchyColorizer
{
    static HierarchyColorizer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }

    private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (gameObject == null)
            return;

        HierarchyColorTag colorTag = gameObject.GetComponent<HierarchyColorTag>();
        if (colorTag != null && colorTag.BackgroundColor != Color.clear)
        {
            if (!Selection.Contains(instanceID))
            {
                Color bgColor = colorTag.BackgroundColor;
                EditorGUI.DrawRect(selectionRect, bgColor);

                // 텍스트 색상 계산 (밝기 기반)
                float luminance = (0.299f * bgColor.r + 0.587f * bgColor.g + 0.114f * bgColor.b);
                Color textColor = (luminance > 0.5f) ? Color.black : Color.white;

                // 기존 ObjectContent 가져오기
                var content = EditorGUIUtility.ObjectContent(gameObject, typeof(GameObject));

                // GUI 색상 변경 후 LabelField 출력
                var oldColor = GUI.color;
                GUI.color = textColor;
                EditorGUI.LabelField(selectionRect, content);
                GUI.color = oldColor;
            }
        }
    }
}
#endif

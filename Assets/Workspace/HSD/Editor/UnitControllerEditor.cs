using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitController))]
public class UnitControllerEditor : Editor
{
    private UnitController _controller;

    private void OnEnable()
    {
        _controller = (UnitController)target;
    }

    public override void OnInspectorGUI()
    {
        // 기본 Inspector 먼저 그려줌
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== Unit Grid Debug View ===", EditorStyles.boldLabel);

        // _unitGrid 접근 (리플렉션 사용 - private 변수라서)
        var gridField = typeof(UnitController).GetField("_unitGrid",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (gridField == null)
        {
            EditorGUILayout.HelpBox("_unitGrid not found!", MessageType.Error);
            return;
        }

        var grid = gridField.GetValue(_controller) as UnitBase[,];

        if (grid == null)
        {
            EditorGUILayout.HelpBox("Unit Grid is NULL (게임 실행 중에만 값이 생성됨)", MessageType.Info);
            return;
        }

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // 2D 그리드처럼 그리기
        for (int y = 0; y < rows; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < cols; x++)
            {
                UnitBase unit = grid[y, x];
                GUIContent content = unit != null ? new GUIContent(unit.name) : new GUIContent("Empty");

                if (GUILayout.Button(content, GUILayout.Width(80), GUILayout.Height(40)))
                {
                    if (unit != null)
                        Selection.activeObject = unit.gameObject; // 클릭하면 하이라키에서 선택
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

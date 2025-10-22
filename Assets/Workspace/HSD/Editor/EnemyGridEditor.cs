using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class EnemyGridEditor : EditorWindow
{
    private const int GRID_WIDTH = 3;
    private const int GRID_HEIGHT = 4;
    private const int SLOT_SIZE = 60;
    private const string BASE_SAVE_PATH = "Assets/Workspace/HSD/Datas/EnemyGridData";
    private const string UNIT_DATA_SEARCH_PATH = "Assets/";

    private UnitData selectedUnitData;
    private int selectedLevel = 0;
    private UnitGridDataSO currentGridData;
    private UnitGridDataSO[] availableGridDatas;
    private UnitData[] availableUnitDatas;
    private Vector2 scrollPosition;
    private Vector2 gridListScrollPosition;
    private Vector2 unitDataListScrollPosition;
    private string newGridName = "그리드 이름 입력";
    private string additionalFolderPath = "";
    private bool showUnitDataList = true;

    private UnitGridDataSO selectedGridDataForEdit;
    private bool isEditMode = false;

    private enum UnitRank
    {
        Normal = 0,
        Elite = 1,
        Boss = 2
    }

    private Color normalColor = new Color(0.7f, 0.9f, 0.7f);
    private Color eliteColor = new Color(0.9f, 0.8f, 0.5f);
    private Color bossColor = new Color(0.95f, 0.5f, 0.5f);

    [MenuItem("Collecting_RPG/EnemyGrid_Editor")]
    public static void ShowWindow()
    {
        GetWindow<EnemyGridEditor>("유닛 데이터 그리드 편집기");
    }

    private void OnEnable()
    {
        CreateNewGridData();
        RefreshAvailableGridDatas();
        RefreshAvailableUnitDatas();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawUnitDataSelector();
        EditorGUILayout.Space(10);

        DrawGrid();
        EditorGUILayout.Space(10);

        DrawCreateSection();
        EditorGUILayout.Space(10);

        DrawGridDataSelector();

        EditorGUILayout.EndScrollView();
    }

    private void DrawUnitDataSelector()
    {
        EditorGUILayout.LabelField("유닛 데이터 설정", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("UnitData 새로고침", GUILayout.Width(150)))
        {
            RefreshAvailableUnitDatas();
        }
        showUnitDataList = EditorGUILayout.Toggle("유닛 데이터 리스트 보기", showUnitDataList);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("수동 선택:", GUILayout.Width(100));
        UnitData newUnitData = (UnitData)EditorGUILayout.ObjectField(selectedUnitData, typeof(UnitData), false);
        if (newUnitData != selectedUnitData)
        {
            selectedUnitData = newUnitData;
        }
        EditorGUILayout.EndHorizontal();

        if (selectedUnitData != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField($"'{selectedUnitData.Name}' 등급 선택", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            Color originalBg = GUI.backgroundColor;

            GUIStyle centeredButtonStyle = new GUIStyle(GUI.skin.button);
            centeredButtonStyle.alignment = TextAnchor.MiddleCenter;

            GUI.backgroundColor = normalColor;
            if (GUILayout.Button("일반", selectedLevel == (int)UnitRank.Normal ? centeredButtonStyle : GUI.skin.button, GUILayout.Height(35)))
            {
                selectedLevel = (int)UnitRank.Normal;
                Debug.Log($"등급 선택: {selectedUnitData.Name} - 일반");
            }

            GUI.backgroundColor = eliteColor;
            if (GUILayout.Button("엘리트", selectedLevel == (int)UnitRank.Elite ? centeredButtonStyle : GUI.skin.button, GUILayout.Height(35)))
            {
                selectedLevel = (int)UnitRank.Elite;
                Debug.Log($"등급 선택: {selectedUnitData.Name} - 엘리트");
            }

            GUI.backgroundColor = bossColor;
            if (GUILayout.Button("보스", selectedLevel == (int)UnitRank.Boss ? centeredButtonStyle : GUI.skin.button, GUILayout.Height(35)))
            {
                selectedLevel = (int)UnitRank.Boss;
                Debug.Log($"등급 선택: {selectedUnitData.Name} - 보스");
            }

            GUI.backgroundColor = originalBg;

            EditorGUILayout.EndHorizontal();

            string rankText = selectedLevel == 0 ? "일반" : selectedLevel == 1 ? "엘리트" : "보스";
            EditorGUILayout.LabelField($"현재 선택: {rankText}", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.EndVertical();
        }

        if (showUnitDataList)
        {
            DrawUnitDataList();
        }

        if (selectedUnitData != null)
        {
            DrawSelectedUnitDataPreview();
        }
    }

    private void DrawGrid()
    {
        EditorGUILayout.LabelField("그리드 크기 (3x4)", EditorStyles.boldLabel);

        if (selectedUnitData != null)
        {
            string rankText = selectedLevel == 0 ? "일반" : selectedLevel == 1 ? "엘리트" : "보스";
            Color rankColor = selectedLevel == 0 ? normalColor : selectedLevel == 1 ? eliteColor : bossColor;

            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = rankColor;
            EditorGUILayout.LabelField($"배치할 유닛: {selectedUnitData.Name} [{rankText}]", EditorStyles.helpBox);
            GUI.backgroundColor = originalBg;
        }
        else
        {
            EditorGUILayout.LabelField("배치할 유닛을 먼저 선택하세요", EditorStyles.helpBox);
        }

        EditorGUILayout.LabelField("좌클릭: UnitStatus 배치 | 우클릭: UnitStatus 삭제");

        EditorGUILayout.BeginVertical(GUI.skin.box);

        for (int y = GRID_HEIGHT; y > 0; y--)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = GRID_WIDTH; x > 0; x--)
            {
                Vector2Int position = new Vector2Int(x - 1, y - 1);
                UnitStatus unitStatus = currentGridData.GetUnitStatus(position);

                Rect slotRect = GUILayoutUtility.GetRect(SLOT_SIZE, SLOT_SIZE);

                Color originalColor = GUI.backgroundColor;
                if (unitStatus != null)
                {
                    if (unitStatus.Level == 0)
                        GUI.backgroundColor = normalColor;
                    else if (unitStatus.Level == 1)
                        GUI.backgroundColor = eliteColor;
                    else if (unitStatus.Level == 2)
                        GUI.backgroundColor = bossColor;
                }

                GUI.Box(slotRect, "", GUI.skin.button);
                GUI.backgroundColor = originalColor;

                if (unitStatus != null && unitStatus.Data != null && unitStatus.Data.Icon != null)
                {
                    Rect iconRect = new Rect(slotRect.x + 2, slotRect.y + 2, slotRect.width - 4, slotRect.height - 20);
                    GUI.DrawTexture(iconRect, unitStatus.Data.Icon.texture, ScaleMode.ScaleToFit);

                    Rect textRect = new Rect(slotRect.x, slotRect.y + slotRect.height - 18, slotRect.width, 18);
                    GUIStyle centeredStyle = new GUIStyle(EditorStyles.miniLabel);
                    centeredStyle.alignment = TextAnchor.MiddleCenter;
                    centeredStyle.fontSize = 8;

                    string rankText = unitStatus.Level == 0 ? "일반" : unitStatus.Level == 1 ? "엘리트" : "보스";
                    GUI.Label(textRect, $"{unitStatus.Data.Name}\n{rankText}", centeredStyle);
                }
                else
                {
                    Rect posRect = new Rect(slotRect.x, slotRect.y + slotRect.height - 15, slotRect.width, 15);
                    GUIStyle centeredStyle = new GUIStyle(EditorStyles.miniLabel);
                    centeredStyle.alignment = TextAnchor.MiddleCenter;
                    centeredStyle.fontSize = 8;
                    GUI.Label(posRect, $"({x},{y})", centeredStyle);
                }

                Event currentEvent = Event.current;
                if (slotRect.Contains(currentEvent.mousePosition))
                {
                    if (currentEvent.type == EventType.MouseDown)
                    {
                        if (currentEvent.button == 0)
                        {
                            if (selectedUnitData != null)
                            {
                                UnitStatus newUnitStatus = new UnitStatus(selectedUnitData, selectedLevel);
                                currentGridData.SetUnitStatus(position, newUnitStatus);
                                EditorUtility.SetDirty(currentGridData);
                                string rankText = selectedLevel == 0 ? "일반" : selectedLevel == 1 ? "엘리트" : "보스";
                                Debug.Log($"배치됨: {selectedUnitData.Name} [{rankText}] at ({x},{y})");
                                Repaint();
                            }
                            else
                            {
                                Debug.LogWarning("배치할 유닛을 먼저 선택하세요!");
                            }
                        }
                        else if (currentEvent.button == 1)
                        {
                            if (unitStatus != null)
                            {
                                string rankText = unitStatus.Level == 0 ? "일반" : unitStatus.Level == 1 ? "엘리트" : "보스";
                                Debug.Log($"삭제됨: {unitStatus.Data.Name} [{rankText}] from ({x},{y})");
                            }
                            currentGridData.RemoveUnitData(position);
                            EditorUtility.SetDirty(currentGridData);
                            Repaint();
                        }
                        currentEvent.Use();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        int totalUnits = currentGridData.unitDatas.Count;
        EditorGUILayout.LabelField($"배치된 유닛 수: {totalUnits} / {GRID_WIDTH * GRID_HEIGHT}", EditorStyles.helpBox);
    }

    private void DrawCreateSection()
    {
        EditorGUILayout.LabelField("저장하기", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("추가 폴더:", GUILayout.Width(70));
        additionalFolderPath = EditorGUILayout.TextField(additionalFolderPath);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("이름:", GUILayout.Width(70));
        newGridName = EditorGUILayout.TextField(newGridName);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("새로 생성하기", GUILayout.Height(30)))
        {
            CreateGridDataAsset();
        }

        GUI.enabled = isEditMode && selectedGridDataForEdit != null;
        if (GUILayout.Button("현재 그리드 수정하기", GUILayout.Height(30)))
        {
            UpdateGridDataAsset();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (isEditMode && selectedGridDataForEdit != null)
        {
            EditorGUILayout.HelpBox($"수정 모드: {selectedGridDataForEdit.gridName}", MessageType.Info);
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("그리드 전체 삭제", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("그리드 전체 삭제", "정말로 모든 그리드를 삭제하시겠습니까?", "예", "아니오"))
            {
                currentGridData.ClearAllUnitDatas();
                EditorUtility.SetDirty(currentGridData);
                Repaint();
            }
        }
    }

    private void DrawGridDataSelector()
    {
        EditorGUILayout.LabelField("불러오기", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("새로고침", GUILayout.Width(100)))
        {
            RefreshAvailableGridDatas();
        }

        if (GUILayout.Button("새 그리드 만들기", GUILayout.Width(150)))
        {
            CreateNewGridData();
            isEditMode = false;
            selectedGridDataForEdit = null;
        }
        EditorGUILayout.EndHorizontal();

        if (availableGridDatas != null && availableGridDatas.Length > 0)
        {
            gridListScrollPosition = EditorGUILayout.BeginScrollView(gridListScrollPosition, GUILayout.Height(150));

            foreach (UnitGridDataSO gridData in availableGridDatas)
            {
                if (gridData != null)
                {
                    bool isCurrentlyEditing = isEditMode && selectedGridDataForEdit == gridData;
                    Color originalColor = GUI.backgroundColor;
                    if (isCurrentlyEditing)
                    {
                        GUI.backgroundColor = Color.yellow;
                    }

                    EditorGUILayout.BeginHorizontal(GUI.skin.box);

                    if (GUILayout.Button(gridData.gridName, EditorStyles.label))
                    {
                        LoadGridData(gridData);
                    }

                    EditorGUILayout.LabelField($"유닛: {gridData.unitDatas.Count}", GUILayout.Width(80));

                    if (GUILayout.Button("수정", GUILayout.Width(40)))
                    {
                        LoadGridDataForEdit(gridData);
                    }

                    EditorGUILayout.EndHorizontal();
                    GUI.backgroundColor = originalColor;
                }
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("그리드 구성이 없습니다. 먼저 하나를 만들어주세요!", MessageType.Info);
        }
    }

    private void CreateNewGridData()
    {
        currentGridData = CreateInstance<UnitGridDataSO>();
        currentGridData.gridName = "임시 구성";
    }

    private void CreateGridDataAsset()
    {
        if (string.IsNullOrEmpty(newGridName))
        {
            EditorUtility.DisplayDialog("오류", "유효한 이름을 입력해주세요!", "확인");
            return;
        }

        string savePath = BASE_SAVE_PATH;
        if (!string.IsNullOrEmpty(additionalFolderPath))
        {
            savePath = Path.Combine(BASE_SAVE_PATH, additionalFolderPath);
        }

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string fileName = $"{newGridName}.asset";
        string fullPath = Path.Combine(savePath, fileName);

        if (File.Exists(fullPath))
        {
            if (!EditorUtility.DisplayDialog("파일 존재", $"파일 {fileName}이(가) 이미 존재합니다. 덮어쓰시겠습니까?", "예", "아니오"))
            {
                return;
            }
        }

        UnitGridDataSO newGridData = CreateInstance<UnitGridDataSO>();
        newGridData.gridName = newGridName;

        foreach (var unitDataInfo in currentGridData.unitDatas)
        {
            newGridData.unitDatas.Add(new GridUnitData(unitDataInfo.position,
                new UnitStatus(unitDataInfo.unitStatus.Data, unitDataInfo.unitStatus.Level)));
        }

        AssetDatabase.CreateAsset(newGridData, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("성공", $"그리드 구성이 {fileName}으로 저장되었습니다!", "확인");

        RefreshAvailableGridDatas();
    }

    private void RefreshAvailableGridDatas()
    {
        if (!Directory.Exists(BASE_SAVE_PATH))
        {
            availableGridDatas = new UnitGridDataSO[0];
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:UnitGridDataSO", new[] { BASE_SAVE_PATH });
        availableGridDatas = new UnitGridDataSO[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            availableGridDatas[i] = AssetDatabase.LoadAssetAtPath<UnitGridDataSO>(path);
        }
    }

    private void LoadGridData(UnitGridDataSO gridData)
    {
        currentGridData.ClearAllUnitDatas();

        foreach (var gridUnitData in gridData.unitDatas)
        {
            UnitStatus loadedStatus = new UnitStatus(gridUnitData.unitStatus.Data, gridUnitData.unitStatus.Level);
            currentGridData.SetUnitStatus(gridUnitData.position, loadedStatus);
        }

        EditorUtility.SetDirty(currentGridData);

        isEditMode = false;
        selectedGridDataForEdit = null;

        Repaint();

        Debug.Log($"그리드 구성을 불러왔습니다: {gridData.gridName} (유닛 {gridData.unitDatas.Count}개)");
    }

    private void LoadGridDataForEdit(UnitGridDataSO gridData)
    {
        currentGridData.ClearAllUnitDatas();

        foreach (var gridUnitData in gridData.unitDatas)
        {
            UnitStatus loadedStatus = new UnitStatus(gridUnitData.unitStatus.Data, gridUnitData.unitStatus.Level);
            currentGridData.SetUnitStatus(gridUnitData.position, loadedStatus);
        }

        if (currentGridData != null)
            EditorUtility.SetDirty(currentGridData);

        isEditMode = true;
        selectedGridDataForEdit = gridData;
        newGridName = gridData.gridName;

        Repaint();

        Debug.Log($"수정 모드로 그리드 구성을 불러왔습니다: {gridData.gridName} (유닛 {gridData.unitDatas.Count}개)");
    }

    private void UpdateGridDataAsset()
    {
        if (selectedGridDataForEdit == null)
        {
            EditorUtility.DisplayDialog("오류", "수정할 그리드 데이터가 선택되지 않았습니다!", "확인");
            return;
        }

        if (string.IsNullOrEmpty(newGridName))
        {
            EditorUtility.DisplayDialog("오류", "유효한 이름을 입력해주세요!", "확인");
            return;
        }

        selectedGridDataForEdit.gridName = newGridName;
        selectedGridDataForEdit.unitDatas.Clear();

        foreach (var unitDataInfo in currentGridData.unitDatas)
        {
            selectedGridDataForEdit.unitDatas.Add(new GridUnitData(unitDataInfo.position,
                new UnitStatus(unitDataInfo.unitStatus.Data, unitDataInfo.unitStatus.Level)));
        }

        EditorUtility.SetDirty(selectedGridDataForEdit);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("성공", $"그리드 구성 '{newGridName}'이(가) 성공적으로 수정되었습니다!", "확인");

        isEditMode = false;
        selectedGridDataForEdit = null;

        RefreshAvailableGridDatas();
        Repaint();

        Debug.Log($"그리드 구성이 수정되었습니다: {newGridName}");
    }

    private void RefreshAvailableUnitDatas()
    {
        string[] unitDataGuids = AssetDatabase.FindAssets("t:UnitData", new[] { UNIT_DATA_SEARCH_PATH });
        System.Collections.Generic.List<UnitData> unitDataList = new System.Collections.Generic.List<UnitData>();

        foreach (string guid in unitDataGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnitData unitData = AssetDatabase.LoadAssetAtPath<UnitData>(path);

            if (unitData != null)
            {
                unitDataList.Add(unitData);
            }
        }

        availableUnitDatas = unitDataList.ToArray();

        System.Array.Sort(availableUnitDatas, (a, b) => a.Name.CompareTo(b.Name));
    }

    private void DrawUnitDataList()
    {
        EditorGUILayout.LabelField("UnitData 목록:", EditorStyles.miniLabel);

        if (availableUnitDatas == null || availableUnitDatas.Length == 0)
        {
            EditorGUILayout.HelpBox("UnitData ScriptableObject를 찾을 수 없습니다. UnitData SO가 프로젝트에 있는지 확인해주세요.", MessageType.Info);
            return;
        }

        unitDataListScrollPosition = EditorGUILayout.BeginScrollView(unitDataListScrollPosition, GUILayout.Height(120));

        for (int i = 0; i < availableUnitDatas.Length; i++)
        {
            if (availableUnitDatas[i] == null) continue;

            UnitData unitData = availableUnitDatas[i];

            bool isSelected = selectedUnitData == unitData;
            Color originalColor = GUI.backgroundColor;
            if (isSelected)
            {
                GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
            }

            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            if (unitData.Icon != null)
            {
                Texture2D iconTexture = AssetPreview.GetAssetPreview(unitData.Icon);
                if (iconTexture != null)
                {
                    GUILayout.Label(iconTexture, GUILayout.Width(24), GUILayout.Height(24));
                }
                else
                {
                    GUILayout.Space(28);
                }
            }
            else
            {
                GUILayout.Space(28);
            }

            if (GUILayout.Button($"{unitData.Name}", EditorStyles.label))
            {
                selectedUnitData = unitData;
                selectedLevel = 0;
            }

            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = originalColor;
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSelectedUnitDataPreview()
    {
        if (selectedUnitData != null)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("선택된 유닛 데이터 미리보기:", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();

            if (selectedUnitData.Icon != null)
            {
                Texture2D iconTexture = AssetPreview.GetAssetPreview(selectedUnitData.Icon);
                if (iconTexture != null)
                {
                    GUILayout.Label(iconTexture, GUILayout.Width(48), GUILayout.Height(48));
                }
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("이름:", selectedUnitData.Name);

            string rankText = selectedLevel == 0 ? "일반" : selectedLevel == 1 ? "엘리트" : "보스";
            EditorGUILayout.LabelField("등급:", rankText);

            if (selectedUnitData.UnitStats != null && selectedLevel < selectedUnitData.UnitStats.Length && selectedUnitData.UnitStats[selectedLevel] != null)
            {
                var stats = selectedUnitData.UnitStats[selectedLevel];
                EditorGUILayout.LabelField("체력:", stats.MaxHealth.ToString());
                EditorGUILayout.LabelField("물리데미지:", stats.PhysicalDamage.ToString());
                EditorGUILayout.LabelField("마법데미지:", stats.MagicDamage.ToString());
                EditorGUILayout.LabelField("공격속도:", stats.AttackSpeed.ToString("F2"));
                EditorGUILayout.LabelField("이동속도:", stats.MoveSpeed.ToString("F2"));
            }
            else
            {
                EditorGUILayout.LabelField("스탯:", "없음");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}
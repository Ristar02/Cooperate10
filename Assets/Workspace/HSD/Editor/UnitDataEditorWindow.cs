using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class UnitDataEditorWindow : EditorWindow
{
    private const string SAVE_PATH = "Assets/Workspace/HSD/Data/Unit";
    private const string UNITDATA_SEARCH_PATH = "Assets/"; // UnitData SO 검색 경로

    private UnitData currentUnitData;
    private UnitData[] availableUnitDatas;
    private Vector2 scrollPosition;
    private Vector2 unitDataListScrollPosition;
    private Vector2 mainScrollPosition;
    private string newUnitName = "새유닛";
    private bool showUnitDataList = true;

    // 폴더블 섹션들
    private bool showMetaDataSection = true;
    private bool showUnitStatsSection = true;
    private bool showAttackDataSection = true;
    private bool showEnhancementSection = true;

    // 레벨별 스탯 편집
    private int selectedLevel = 0;

    [MenuItem("Collecting_RPG/UnitData_Editor")]
    public static void ShowWindow()
    {
        GetWindow<UnitDataEditorWindow>("유닛 데이터 편집기");
    }

    private void OnEnable()
    {
        CreateNewUnitData();
        RefreshAvailableUnitDatas();
    }

    private void OnGUI()
    {
        mainScrollPosition = EditorGUILayout.BeginScrollView(mainScrollPosition);

        DrawUnitDataSelector();
        EditorGUILayout.Space(10);

        DrawUnitDataEditor();
        EditorGUILayout.Space(10);

        DrawCreateSection();
        EditorGUILayout.Space(10);

        DrawUnitDataList();

        EditorGUILayout.EndScrollView();
    }

    private void DrawUnitDataSelector()
    {
        EditorGUILayout.LabelField("유닛 데이터 선택", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("UnitData 새로고침", GUILayout.Width(150)))
        {
            RefreshAvailableUnitDatas();
        }
        showUnitDataList = EditorGUILayout.Toggle("유닛 데이터 리스트 보기", showUnitDataList);
        EditorGUILayout.EndHorizontal();

        // 수동 선택
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("수동 선택:", GUILayout.Width(100));
        UnitData newUnitData = (UnitData)EditorGUILayout.ObjectField(currentUnitData, typeof(UnitData), false);
        if (newUnitData != currentUnitData && newUnitData != null)
        {
            LoadUnitData(newUnitData);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawUnitDataEditor()
    {
        if (currentUnitData == null) return;

        EditorGUILayout.LabelField("유닛 데이터 편집", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(GUI.skin.box);

        // MetaData 섹션
        showMetaDataSection = EditorGUILayout.Foldout(showMetaDataSection, "MetaData", true, EditorStyles.foldoutHeader);
        if (showMetaDataSection)
        {
            EditorGUI.indentLevel++;
            currentUnitData.Grade = (Grade)EditorGUILayout.EnumPopup("Grade", currentUnitData.Grade);            
            currentUnitData.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", currentUnitData.Icon, typeof(Sprite), false);
            currentUnitData.ID = EditorGUILayout.IntField("ID", currentUnitData.ID);
            currentUnitData.Name = EditorGUILayout.TextField("Name", currentUnitData.Name);
            currentUnitData.Description = EditorGUILayout.TextArea(currentUnitData.Description, GUILayout.Height(60));
            currentUnitData.Cost = EditorGUILayout.IntField("Cost", currentUnitData.Cost);
            currentUnitData.UpgradeData.CurrentUpgradeData.UpgradeLevel = EditorGUILayout.IntField("Upgrade Count", currentUnitData.UpgradeData.CurrentUpgradeData.UpgradeLevel);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }

        // Unit Stats 섹션
        showUnitStatsSection = EditorGUILayout.Foldout(showUnitStatsSection, "Unit Stats", true, EditorStyles.foldoutHeader);
        if (showUnitStatsSection)
        {
            EditorGUI.indentLevel++;

            // 배열 크기 조정
            int newSize = EditorGUILayout.IntField("레벨 수", currentUnitData.UnitStats?.Length ?? 1);
            if (newSize < 1) newSize = 1;

            if (currentUnitData.UnitStats == null || currentUnitData.UnitStats.Length != newSize)
            {
                System.Array.Resize(ref currentUnitData.UnitStats, newSize);

                // 새로운 요소들 초기화
                for (int i = 0; i < newSize; i++)
                {
                    if (currentUnitData.UnitStats[i] == null)
                    {
                        currentUnitData.UnitStats[i] = CreateDefaultUnitStats();
                    }
                }
            }

            // 레벨 선택
            if (currentUnitData.UnitStats.Length > 0)
            {
                selectedLevel = EditorGUILayout.IntSlider("편집할 레벨", selectedLevel, 0, currentUnitData.UnitStats.Length - 1);
                EditorGUILayout.LabelField($"레벨 {selectedLevel + 1} 스탯 편집", EditorStyles.boldLabel);

                if (selectedLevel < currentUnitData.UnitStats.Length)
                {
                    var stats = currentUnitData.UnitStats[selectedLevel];
                    if (stats != null)
                    {
                        DrawUnitStatsEditor(stats);
                    }
                }

                // 레벨별 스탯 복사 기능
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("이전 레벨에서 복사") && selectedLevel > 0)
                {
                    CopyUnitStats(currentUnitData.UnitStats[selectedLevel - 1], currentUnitData.UnitStats[selectedLevel]);
                }
                if (GUILayout.Button("모든 레벨에 적용"))
                {
                    for (int i = 0; i < currentUnitData.UnitStats.Length; i++)
                    {
                        if (i != selectedLevel)
                        {
                            CopyUnitStats(currentUnitData.UnitStats[selectedLevel], currentUnitData.UnitStats[i]);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }

        // Attack Data 섹션
        showAttackDataSection = EditorGUILayout.Foldout(showAttackDataSection, "Attack Data", true, EditorStyles.foldoutHeader);
        if (showAttackDataSection)
        {
            EditorGUI.indentLevel++;
            currentUnitData.Skill = (UnitSkill)EditorGUILayout.ObjectField("Skill", currentUnitData.Skill, typeof(UnitSkill), false);
            currentUnitData.AttackData = (UnitAttackData)EditorGUILayout.ObjectField("Attack Data", currentUnitData.AttackData, typeof(UnitAttackData), false);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.EndVertical();

        // 변경사항이 있으면 dirty 표시
        if (GUI.changed)
        {
            EditorUtility.SetDirty(currentUnitData);
        }
    }

    private void DrawUnitStatsEditor(UnitStats stats)
    {
        EditorGUI.indentLevel++;

        // Status 섹션
        EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
        stats.MaxHealth = EditorGUILayout.IntField("Max Health", stats.MaxHealth);
        stats.MaxMana = EditorGUILayout.IntField("Max Mana", stats.MaxMana);
        stats.ManaGain = EditorGUILayout.IntField("Mana Gain", stats.ManaGain);
        stats.AttackSpeed = EditorGUILayout.FloatField("Attack Speed", stats.AttackSpeed);
        stats.MoveSpeed = EditorGUILayout.FloatField("Move Speed", stats.MoveSpeed);

        EditorGUILayout.Space(3);

        // Damage 섹션
        EditorGUILayout.LabelField("Damage", EditorStyles.boldLabel);
        stats.PhysicalDamage = EditorGUILayout.IntField("Physical Damage", stats.PhysicalDamage);
        stats.MagicDamage = EditorGUILayout.IntField("Magic Damage", stats.MagicDamage);

        EditorGUILayout.Space(3);

        // Critical 섹션
        EditorGUILayout.LabelField("Critical", EditorStyles.boldLabel);
        stats.CritChance = EditorGUILayout.IntField("Crit Chance", stats.CritChance);

        EditorGUILayout.Space(3);

        // Defense 섹션
        EditorGUILayout.LabelField("Defense", EditorStyles.boldLabel);
        stats.PhysicalDefense = EditorGUILayout.IntField("Physical Defense", stats.PhysicalDefense);
        stats.MagicDefense = EditorGUILayout.IntField("Magic Defense", stats.MagicDefense);

        EditorGUILayout.Space(3);

        // Range 섹션
        EditorGUILayout.LabelField("Range", EditorStyles.boldLabel);
        stats.AttackRange = EditorGUILayout.FloatField("Attack Range", stats.AttackRange);
        stats.AttackCount = EditorGUILayout.IntField("Attack Count", stats.AttackCount);

        EditorGUI.indentLevel--;
    }

    private UnitStats CreateDefaultUnitStats()
    {
        return new UnitStats
        {
            MaxHealth = 100,
            MaxMana = 50,
            ManaGain = 5,
            AttackSpeed = 1.0f,
            MoveSpeed = 1.0f,
            PhysicalDamage = 10,
            MagicDamage = 0,
            CritChance = 5,
            PhysicalDefense = 5,
            MagicDefense = 5,
            AttackRange = 1,
            AttackCount = 1,    
        };
    }

    private void CopyUnitStats(UnitStats source, UnitStats target)
    {
        target.MaxHealth = source.MaxHealth;
        target.MaxMana = source.MaxMana;
        target.ManaGain = source.ManaGain;
        target.AttackSpeed = source.AttackSpeed;
        target.MoveSpeed = source.MoveSpeed;
        target.PhysicalDamage = source.PhysicalDamage;
        target.MagicDamage = source.MagicDamage;
        target.CritChance = source.CritChance;
        target.PhysicalDefense = source.PhysicalDefense;
        target.MagicDefense = source.MagicDefense;
        target.AttackRange = source.AttackRange;
        target.AttackCount = source.AttackCount;
    }

    private void DrawCreateSection()
    {
        EditorGUILayout.LabelField("저장하기", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("이름:", GUILayout.Width(50));
        newUnitName = EditorGUILayout.TextField(newUnitName);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("새 UnitData 생성", GUILayout.Height(30)))
        {
            CreateUnitDataAsset();
        }

        if (currentUnitData != null && GUILayout.Button("현재 데이터 저장", GUILayout.Height(30)))
        {
            SaveCurrentUnitData();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        if (GUILayout.Button("데이터 초기화", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("데이터 초기화", "정말로 현재 데이터를 초기화하시겠습니까?", "예", "아니오"))
            {
                CreateNewUnitData();
                Repaint();
            }
        }
    }

    private void DrawUnitDataList()
    {
        EditorGUILayout.LabelField("불러오기", EditorStyles.boldLabel);

        if (GUILayout.Button("새로고침"))
        {
            RefreshAvailableUnitDatas();
        }

        if (showUnitDataList)
        {
            DrawAvailableUnitDataList();
        }
    }

    private void CreateNewUnitData()
    {
        currentUnitData = CreateInstance<UnitData>();
        currentUnitData.Name = "새유닛";
        currentUnitData.UnitStats = new UnitStats[1];
        currentUnitData.UnitStats[0] = CreateDefaultUnitStats();
        selectedLevel = 0;
    }

    private void CreateUnitDataAsset()
    {
        if (string.IsNullOrEmpty(newUnitName))
        {
            EditorUtility.DisplayDialog("오류", "유효한 이름을 입력해주세요!", "확인");
            return;
        }

        if (!Directory.Exists(SAVE_PATH))
        {
            Directory.CreateDirectory(SAVE_PATH);
        }

        string fileName = $"{newUnitName}_1.asset";
        string fullPath = Path.Combine(SAVE_PATH, fileName);

        if (File.Exists(fullPath))
        {
            if (!EditorUtility.DisplayDialog("파일 존재", $"파일 {fileName}이(가) 이미 존재합니다. 덮어쓰시겠습니까?", "예", "아니오"))
            {
                return;
            }
        }

        UnitData newUnitData = CreateInstance<UnitData>();
        CopyUnitData(currentUnitData, newUnitData);
        newUnitData.Name = newUnitName;

        AssetDatabase.CreateAsset(newUnitData, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("성공", $"유닛 데이터가 {fileName}으로 저장되었습니다!", "확인");

        RefreshAvailableUnitDatas();
        LoadUnitData(newUnitData);
    }

    private void SaveCurrentUnitData()
    {
        if (currentUnitData != null)
        {
            EditorUtility.SetDirty(currentUnitData);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("저장 완료", "현재 유닛 데이터가 저장되었습니다!", "확인");
        }
    }

    private void RefreshAvailableUnitDatas()
    {
        string[] unitDataGuids = AssetDatabase.FindAssets("t:UnitData", new[] { UNITDATA_SEARCH_PATH });
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

    private void LoadUnitData(UnitData unitData)
    {
        if (unitData != null)
        {
            currentUnitData = unitData;
            selectedLevel = 0;
            Repaint();
            Debug.Log($"유닛 데이터를 불러왔습니다: {unitData.Name}");
        }
    }

    private void DrawAvailableUnitDataList()
    {
        EditorGUILayout.LabelField("UnitData 목록:", EditorStyles.miniLabel);

        if (availableUnitDatas == null || availableUnitDatas.Length == 0)
        {
            EditorGUILayout.HelpBox("UnitData ScriptableObject를 찾을 수 없습니다. UnitData SO가 프로젝트에 있는지 확인해주세요.", MessageType.Info);
            return;
        }

        unitDataListScrollPosition = EditorGUILayout.BeginScrollView(unitDataListScrollPosition, GUILayout.Height(200));

        for (int i = 0; i < availableUnitDatas.Length; i++)
        {
            if (availableUnitDatas[i] == null) continue;

            UnitData unitData = availableUnitDatas[i];
            bool isSelected = currentUnitData == unitData;

            // 선택된 항목의 배경색 변경
            Color originalColor = GUI.backgroundColor;
            if (isSelected)
            {
                GUI.backgroundColor = Color.cyan;
            }

            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            // 아이콘과 정보를 포함한 버튼 생성
            EditorGUILayout.BeginHorizontal();

            // 아이콘 표시
            if (unitData.Icon != null)
            {
                Texture2D iconTexture = AssetPreview.GetAssetPreview(unitData.Icon);
                if (iconTexture != null)
                {
                    GUILayout.Label(iconTexture, GUILayout.Width(32), GUILayout.Height(32));
                }
                else
                {
                    GUILayout.Space(36);
                }
            }
            else
            {
                GUILayout.Space(36);
            }

            // 유닛 정보 표시 및 클릭 처리
            EditorGUILayout.BeginVertical();

            // 클릭 가능한 라벨로 변경
            if (GUILayout.Button($"{unitData.Name} (레벨 {unitData.UnitStats?.Length ?? 0}개)", EditorStyles.label))
            {
                LoadUnitData(unitData);
            }

            // 첫 번째 레벨의 스탯 정보 표시 (존재할 경우)
            if (unitData.UnitStats != null && unitData.UnitStats.Length > 0 && unitData.UnitStats[0] != null)
            {
                var firstStats = unitData.UnitStats[0];
                EditorGUILayout.LabelField($"ID: {unitData.ID} | HP: {firstStats.MaxHealth} | ATK: {firstStats.PhysicalDamage + firstStats.MagicDamage}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField($"ID: {unitData.ID} | 스탯 없음", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();

            // 배경색 복원
            GUI.backgroundColor = originalColor;
        }

        EditorGUILayout.EndScrollView();
    }

    private void CopyUnitData(UnitData source, UnitData target)
    {
        target.Grade = source.Grade;
        target.Icon = source.Icon;
        target.ID = source.ID;
        target.Name = source.Name;
        target.Description = source.Description;
        target.Cost = source.Cost;
        target.UpgradeData.CurrentUpgradeData.UpgradeLevel = source.UpgradeData.CurrentUpgradeData.UpgradeLevel;
        target.Skill = source.Skill;
        target.AttackData = source.AttackData;

        // UnitStats 배열 복사
        if (source.UnitStats != null)
        {
            target.UnitStats = new UnitStats[source.UnitStats.Length];
            for (int i = 0; i < source.UnitStats.Length; i++)
            {
                if (source.UnitStats[i] != null)
                {
                    target.UnitStats[i] = new UnitStats();
                    CopyUnitStats(source.UnitStats[i], target.UnitStats[i]);
                }
            }
        }
    }
}
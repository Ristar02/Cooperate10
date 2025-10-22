#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SynergyEffect))]
public class SynergyEffectEditor : Editor
{
    private SerializedProperty descriptionProp;
    private SerializedProperty isActivationsClearProp;
    private SerializedProperty isAttackProp;
    private SerializedProperty isBuffProp;
    private SerializedProperty isSpawnProp;
    private SerializedProperty isDelayProp;
    private SerializedProperty isFirstOnlyProp;

    private SerializedProperty delayTimeProp;

    private SerializedProperty synergyEffectAddressProps;
    private SerializedProperty effectDuration;

    private SerializedProperty isUnitPositionProp;
    private SerializedProperty IsMultiplierProp;
    private SerializedProperty spawnTypeProp;
    private SerializedProperty unitDataAddressProp;
    private SerializedProperty unitLevelProp;
    private SerializedProperty spawnUnitStatsProp;
    private SerializedProperty spawnSynergyProp;
    private SerializedProperty spawnAddressProp;
    private SerializedProperty spawnEffectAddressProp;

    private SerializedProperty effectAttackTypeProp;
    private SerializedProperty spawnPositionTypeProp;
    private SerializedProperty powerProp;
    private SerializedProperty attackDealyProp;
    private SerializedProperty addressProp;

    private SerializedProperty effectTypeProp;
    private SerializedProperty synergyBuffDatasProp;
    private SerializedProperty statModifiersProp;

    private SerializedProperty triggerTypeProp;
    private SerializedProperty intervalProp;
    private SerializedProperty maxActivationsProp;

    private SerializedProperty targetTypeProp;

    private SerializedProperty nextEffectProp;

    private void OnEnable()
    {
        descriptionProp = serializedObject.FindProperty("Description");
        isActivationsClearProp = serializedObject.FindProperty("IsActivationsClear");
        isAttackProp = serializedObject.FindProperty("IsAttack");
        isBuffProp = serializedObject.FindProperty("IsBuff");
        isSpawnProp = serializedObject.FindProperty("IsSpawn");
        isDelayProp = serializedObject.FindProperty("IsDelay");
        isFirstOnlyProp = serializedObject.FindProperty("IsFirstOnly");

        delayTimeProp = serializedObject.FindProperty("DelayTime");

        synergyEffectAddressProps = serializedObject.FindProperty("SynergyEffectAddress");
        effectDuration = serializedObject.FindProperty("EffectDuration");

        isUnitPositionProp = serializedObject.FindProperty("IsUnitPosition");
        IsMultiplierProp = serializedObject.FindProperty("IsMultiplier");
        spawnTypeProp = serializedObject.FindProperty("SpawnType");
        unitDataAddressProp = serializedObject.FindProperty("UnitDataAddress");
        unitLevelProp = serializedObject.FindProperty("UnitLevel");
        spawnUnitStatsProp = serializedObject.FindProperty("SpawnUnitStats");
        spawnSynergyProp = serializedObject.FindProperty("SpawnSynergy");
        spawnAddressProp = serializedObject.FindProperty("SpawnAddress");
        spawnEffectAddressProp = serializedObject.FindProperty("SpawnEffectAddress");

        effectAttackTypeProp = serializedObject.FindProperty("EffectApplyType");
        spawnPositionTypeProp = serializedObject.FindProperty("SpawnPositionType");
        powerProp = serializedObject.FindProperty("Power");
        attackDealyProp = serializedObject.FindProperty("AttackDealy");
        addressProp = serializedObject.FindProperty("AttackAddress");

        effectTypeProp = serializedObject.FindProperty("EffectType");
        synergyBuffDatasProp = serializedObject.FindProperty("SynergyBuffDatas");
        statModifiersProp = serializedObject.FindProperty("StatModifiers");

        triggerTypeProp = serializedObject.FindProperty("TriggerType");
        intervalProp = serializedObject.FindProperty("Interval");
        maxActivationsProp = serializedObject.FindProperty("MaxActivations");

        targetTypeProp = serializedObject.FindProperty("TargetType");

        nextEffectProp = serializedObject.FindProperty("NextEffect");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawHeader("기본");
        EditorGUILayout.PropertyField(descriptionProp);

        EditorGUILayout.Space(5);

        EditorGUILayout.PropertyField(isActivationsClearProp, new GUIContent("스텍이 다 차면 리셋?"));
        EditorGUILayout.PropertyField(isSpawnProp, new GUIContent("소환할 것 인지?"));
        EditorGUILayout.PropertyField(isAttackProp, new GUIContent("공격할 것 인지?"));
        EditorGUILayout.PropertyField(isBuffProp, new GUIContent("버프를 줄 것 인지?"));
        EditorGUILayout.PropertyField(isDelayProp, new GUIContent("딜레이 설정을 할껀지?"));
        EditorGUILayout.PropertyField(isFirstOnlyProp, new GUIContent("최초 1회만 발동?"));

        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(effectAttackTypeProp, new GUIContent("적용타입", "각자패시브 or 전체공용 패시브"));
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(synergyEffectAddressProps, new GUIContent("시너지 이펙트"));
        EditorGUILayout.PropertyField(effectDuration, new GUIContent("이펙트 지속시간"));
        EditorGUILayout.Space(10);

        if (isDelayProp.boolValue)
        {
            DrawHeader("딜레이 설정");
            DrawColoredSection(() =>
            {
                EditorGUILayout.PropertyField(delayTimeProp, new GUIContent("딜레이 시간 (초)", "다음 효과까지의 딜레이(초)"));
            }, new Color(0.9f, 0.9f, 1f, 0.3f));
            EditorGUILayout.Space(10);
        }

        if (isSpawnProp.boolValue)
        {
            DrawHeader("소환 세팅");
            DrawColoredSection(() =>
            {
                EditorGUILayout.PropertyField(isUnitPositionProp, new GUIContent("스폰위치 설정", "유닛위치 or 전장중앙"));
                EditorGUILayout.PropertyField(IsMultiplierProp, new GUIContent("가중치 적용여부")); 
                EditorGUILayout.PropertyField(spawnTypeProp, new GUIContent("스폰할 유닛 스텟 타입", "시너지 유닛의 Level에 따른 or 한단계 낮은 레벨로 소환"));
                EditorGUILayout.PropertyField(unitDataAddressProp, new GUIContent("유닛 데이터 주소"));

                if ((SpawnStatType)spawnTypeProp.intValue == SpawnStatType.Level)
                {
                    EditorGUILayout.PropertyField(unitLevelProp, new GUIContent("레벨 설정"));
                }
                if (IsMultiplierProp.boolValue)
                {
                    EditorGUILayout.PropertyField(spawnUnitStatsProp, new GUIContent("가중치"));
                }
                EditorGUILayout.PropertyField(spawnSynergyProp, new GUIContent("스폰 시너지"));
                EditorGUILayout.PropertyField(spawnAddressProp, new GUIContent("주소"));
                EditorGUILayout.PropertyField(spawnEffectAddressProp, new GUIContent("스폰 이펙트"));

            }, new Color(1f, 1f, 0.9f, 0.3f));

            EditorGUILayout.Space(10);
        }

        if (isAttackProp.boolValue)
        {
            DrawHeader("공격 설정");

            DrawColoredSection(() =>
            {           
                EditorGUILayout.PropertyField(spawnPositionTypeProp, new GUIContent("공격 위치", "자신기준 or 타겟기준"));
                DrawColoredField("계수", powerProp, Color.red);
                DrawColoredField("공격 딜레이", attackDealyProp, Color.yellow);
                EditorGUILayout.PropertyField(addressProp, new GUIContent("주소"));
            }, new Color(1f, 0.9f, 0.9f, 0.3f));

            EditorGUILayout.Space(10);
        }

        if (isBuffProp.boolValue)
        {
            DrawHeader("버프 설정");

            DrawColoredSection(() =>
            {
                EditorGUILayout.PropertyField(effectTypeProp, new GUIContent("버프 타입", "능력치 상승 or 버프&디버프"));

                EffectType currentEffectType = (EffectType)effectTypeProp.enumValueIndex;

                EditorGUILayout.Space(5);

                if (currentEffectType == EffectType.Buff_Debuff)
                {
                    DrawSubHeader("버프 디버프 설정:", Color.green);
                    EditorGUILayout.PropertyField(synergyBuffDatasProp, true);
                }
                else if (currentEffectType == EffectType.Increase)
                {
                    DrawSubHeader("능력치 상승 설정:", Color.cyan);
                    EditorGUILayout.PropertyField(statModifiersProp, true);
                }
            }, new Color(0.9f, 1f, 0.9f, 0.3f));

            EditorGUILayout.Space(10);
        }

        DrawHeader("시너지 발동 설정");
        EditorGUILayout.PropertyField(triggerTypeProp, new GUIContent("발동 설정", "바로 발동, 일정시간마다, 죽었을 때, 공격했을 때, 스킬을 사용했을 때 등"));

        TriggerType currentTriggerType = (TriggerType)triggerTypeProp.enumValueIndex;
        if (currentTriggerType == TriggerType.OnInterval)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(intervalProp, new GUIContent("발동 간격 (초)"));
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(maxActivationsProp, new GUIContent("최대 스텍"));

        EditorGUILayout.Space(10);

        DrawHeader("적용 유닛 설정");
        EditorGUILayout.PropertyField(targetTypeProp);

        DrawTargetTypeInfo((EffectTargetType)targetTypeProp.enumValueIndex);

        EditorGUILayout.Space(10);

        DrawHeader("이어지는 효과 설정 (선택)");
        EditorGUILayout.PropertyField(nextEffectProp, new GUIContent("다음 효과 (선택)"));

        if (nextEffectProp.objectReferenceValue == target)
        {
            DrawWarningBox("경고: 순환 참조가 감지되었습니다! 이 효과가 자기 자신을 가리키고 있습니다.");
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTargetTypeInfo(EffectTargetType targetType)
    {
        var (info, color) = targetType switch
        {
            EffectTargetType.Enemy => ("적으로 설정", new Color(1f, 0.9f, 0.9f, 1f)),
            EffectTargetType.Ally => ("아군으로 설정", new Color(0.9f, 1f, 0.9f, 1f)),
            EffectTargetType.SameSynergy => ("같은 시너지 유닛들로 설정", new Color(0.9f, 0.9f, 1f, 1f)),
            EffectTargetType.Column => ("시너지 유닛이 존재하는 세로줄 전체를 대상으로 설정", new Color(1f, 1f, 0.9f, 1f)),
            EffectTargetType.Row => ("시너지 유닛이 존재하는 가로줄 전체를 대상으로 설정", new Color(1f, 0.9f, 1f, 1f)),
            EffectTargetType.ColumnAndRow => ("시너지 유닛이 존재하는 가로줄과 세로줄 전체를 대상으로 설정", new Color(0.9f, 1f, 1f, 1f)),
            EffectTargetType.Cross => ("시너지 유닛 주변 십자 형태의 인접 유닛들을 대상으로 설정", new Color(1f, 1f, 1f, 1f)),
            _ => ("", Color.white)
        };

        if (!string.IsNullOrEmpty(info))
        {
            DrawInfoBox(info, color);
        }
    }

    private void DrawHeader(string title, Color? backgroundColor = null)
    {
        EditorGUILayout.Space(5);
        var rect = EditorGUILayout.GetControlRect(false, 25);

        Color bgColor = backgroundColor ?? GetHeaderColor(title);
        EditorGUI.DrawRect(rect, bgColor);

        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.black);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), Color.black);

        var labelRect = new Rect(rect.x + 10, rect.y + 2, rect.width - 20, rect.height - 4);

        var headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.normal.textColor = Color.white;
        headerStyle.fontSize = 12;

        EditorGUI.LabelField(labelRect, title, headerStyle);
    }

    private Color GetHeaderColor(string title)
    {
        return title switch
        {
            "기본" => new Color(0.2f, 0.4f, 0.8f, 0.8f),        // 파란색
            "소환 설정" => new Color(0.6f, 0.6f, 0.2f, 0.8f),  // 노란색
            "공격 설정" => new Color(0.8f, 0.2f, 0.2f, 0.8f), // 빨간색
            "버프 설정" => new Color(0.2f, 0.8f, 0.2f, 0.8f),  // 초록색
            "시너지 발동 설정" => new Color(0.8f, 0.6f, 0.2f, 0.8f), // 주황색
            "적용 유닛 설정" => new Color(0.6f, 0.2f, 0.8f, 0.8f),  // 보라색
            "이어지는 효과 설정 (선택)" => new Color(0.4f, 0.7f, 0.7f, 0.8f),     // 청록색
            "딜레이 설정" => new Color(0.9f, 0.4f, 0.7f, 0.8f),  // 분홍색
            _ => new Color(0.3f, 0.3f, 0.3f, 0.8f)                   // 기본 회색
        };
    }

    private void DrawColoredSection(System.Action drawContent, Color backgroundColor)
    {
        var rect = EditorGUILayout.BeginVertical();
        GUI.backgroundColor = backgroundColor;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;

        drawContent?.Invoke();

        GUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }

    private void DrawColoredField(string label, SerializedProperty property, Color labelColor)
    {
        EditorGUILayout.BeginHorizontal();

        var originalColor = GUI.color;
        GUI.color = labelColor;
        GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
        GUI.color = originalColor;

        EditorGUILayout.PropertyField(property, GUIContent.none);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSubHeader(string title, Color textColor)
    {
        var style = new GUIStyle(EditorStyles.miniBoldLabel);
        style.normal.textColor = textColor;
        EditorGUILayout.LabelField(title, style);
    }

    private void DrawWarningBox(string message)
    {
        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.8f, 0.8f, 1f);
        EditorGUILayout.HelpBox(message, MessageType.Warning);
        GUI.backgroundColor = originalColor;
    }

    private void DrawInfoBox(string message, Color backgroundColor)
    {
        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = backgroundColor;
        EditorGUILayout.HelpBox(message, MessageType.Info);
        GUI.backgroundColor = originalColor;
    }
}
#endif
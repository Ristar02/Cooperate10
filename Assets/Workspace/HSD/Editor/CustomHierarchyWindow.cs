#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class CompleteHierarchyOverride
{
    private static Dictionary<int, Texture2D> gradientTextures = new Dictionary<int, Texture2D>();
    private static HierarchyColorRules colorRules;

    static CompleteHierarchyOverride()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        EditorApplication.hierarchyChanged += ClearTextureCache;
        LoadColorRules();
    }

    private static void LoadColorRules()
    {
        string[] guids = AssetDatabase.FindAssets("t:HierarchyColorRules");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            colorRules = AssetDatabase.LoadAssetAtPath<HierarchyColorRules>(path);
        }

        if (colorRules == null)
        {
            return;
        }
    }

    private static void ClearTextureCache()
    {
        foreach (var texture in gradientTextures.Values)
        {
            if (texture != null)
                Object.DestroyImmediate(texture);
        }
        gradientTextures.Clear();
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (gameObject == null) return;

        if (Selection.Contains(instanceID))
        {
            DrawComponentIcons(gameObject, selectionRect);
            return;
        }

        Color detectedColor = GetColorByRules(gameObject.name);

        if (detectedColor != Color.clear)
        {
            DrawCompleteBackground(selectionRect, detectedColor);

            DrawCompleteContent(gameObject, selectionRect, detectedColor);
        }

        DrawComponentIcons(gameObject, selectionRect);
    }

    private static Color GetColorByRules(string gameObjectName)
    {
        if (colorRules == null || colorRules.rules == null) return Color.clear;

        var matchedRules = new List<HierarchyColorRules.ColorRule>();

        foreach (var rule in colorRules.rules)
        {
            if (DoesNameMatchRule(gameObjectName, rule))
            {
                matchedRules.Add(rule);
            }
        }

        if (matchedRules.Count > 0)
        {
            var bestRule = matchedRules.OrderByDescending(r => r.priority).First();
            return bestRule.backgroundColor;
        }

        return Color.clear;
    }

    private static bool DoesNameMatchRule(string name, HierarchyColorRules.ColorRule rule)
    {
        if (string.IsNullOrEmpty(name)) return false;

        string nameToCheck = rule.caseSensitive ? name : name.ToUpper();

        if (!string.IsNullOrEmpty(rule.firstCharPattern))
        {
            string pattern = rule.caseSensitive ? rule.firstCharPattern : rule.firstCharPattern.ToUpper();
            if (nameToCheck.StartsWith(pattern))
                return true;
        }

        return false;
    }

    private static void DrawCompleteBackground(Rect rect, Color backgroundColor)
    {
        Color defaultBg = EditorGUIUtility.isProSkin ?
            new Color(0.22f, 0.22f, 0.22f, 1f) :
            new Color(0.76f, 0.76f, 0.76f, 1f);
        EditorGUI.DrawRect(rect, defaultBg);

        DrawGradientRect(rect, backgroundColor);
    }

    private static void DrawGradientRect(Rect rect, Color baseColor)
    {
        int textureKey = baseColor.GetHashCode();

        if (!gradientTextures.ContainsKey(textureKey))
        {
            CreateGradientTexture(textureKey, baseColor);
        }

        if (gradientTextures.ContainsKey(textureKey) && gradientTextures[textureKey] != null)
        {
            var oldColor = GUI.color;
            GUI.color = Color.white;

            GUI.DrawTexture(rect, gradientTextures[textureKey], ScaleMode.StretchToFill, true);

            GUI.color = oldColor;
        }
        else
        {
            EditorGUI.DrawRect(rect, baseColor);
        }
    }

    private static void CreateGradientTexture(int key, Color baseColor)
    {
        Texture2D gradientTexture = new Texture2D(256, 1);

        for (int i = 0; i < 256; i++)
        {
            float t = i / 255f;

            Color leftColor = Color.Lerp(baseColor * 0.8f, baseColor * 0.3f, 0.5f);
            Color rightColor = Color.Lerp(baseColor, Color.white, 0.1f);

            float curve = Mathf.Pow(t, 1.2f);
            Color pixelColor = Color.Lerp(leftColor, rightColor, curve);

            pixelColor.a = baseColor.a * Mathf.Lerp(0.9f, 0f, t);

            gradientTexture.SetPixel(i, 0, pixelColor);
        }

        gradientTexture.Apply();
        gradientTextures[key] = gradientTexture;
    }

    private static void DrawCompleteContent(GameObject gameObject, Rect rect, Color backgroundColor)
    {
        var content = EditorGUIUtility.ObjectContent(gameObject, typeof(GameObject));
        Color textColor = GetOptimalTextColor(backgroundColor);

        string displayName = GetDisplayName(gameObject.name);

        GUIStyle customStyle = new GUIStyle(EditorStyles.label);
        customStyle.normal.textColor = textColor;
        customStyle.fontStyle = FontStyle.Bold;
        customStyle.alignment = TextAnchor.MiddleCenter;
        customStyle.richText = true;

        Rect contentRect = new Rect(rect.x, rect.y, rect.width - 80, rect.height);

        if (!gameObject.activeInHierarchy)
        {
            customStyle.normal.textColor = Color.Lerp(textColor, Color.gray, 0.6f);
        }

        PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(gameObject);
        if (prefabType != PrefabAssetType.NotAPrefab)
        {
            customStyle.normal.textColor = Color.Lerp(textColor, Color.cyan, 0.3f);
        }

        var customContent = new GUIContent(displayName, content.image);
        EditorGUI.LabelField(contentRect, customContent, customStyle);

        if (gameObject.transform.childCount > 0)
        {
            string childInfo = $"({gameObject.transform.childCount})";
            Rect childRect = new Rect(contentRect.x + contentRect.width - 40, contentRect.y, 40, contentRect.height);

            GUIStyle childStyle = new GUIStyle(EditorStyles.miniLabel);
            childStyle.normal.textColor = Color.Lerp(textColor, Color.gray, 0.4f);
            childStyle.alignment = TextAnchor.MiddleRight;

            EditorGUI.LabelField(childRect, childInfo, childStyle);
        }
    }

    private static string GetDisplayName(string originalName)
    {
        string[] colorCodes = { "#R", "#G", "#B", "#Y", "#O", "#P", "#C" };

        foreach (string code in colorCodes)
        {
            if (originalName.StartsWith(code))
            {
                return originalName.Substring(code.Length);
            }
        }

        return originalName;
    }

    private static Color GetOptimalTextColor(Color backgroundColor)
    {
        //float luminance = (0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b);
        //return (luminance > 0.4f) ? Color.black : Color.white;

        return Color.white;
    }

    private static void DrawComponentIcons(GameObject gameObject, Rect rect)
    {
        var components = gameObject.GetComponents<Component>()
            .Where(c => c != null && !(c is Transform))
            .ToArray();

        if (components.Length == 0) return;

        float iconSize = 16f;
        float iconSpacing = 18f;
        int maxIcons = 4;

        float totalWidth = Mathf.Min(components.Length, maxIcons) * iconSpacing;
        float startX = rect.x + rect.width - totalWidth - 5;

        for (int i = 0; i < Mathf.Min(components.Length, maxIcons); i++)
        {
            var component = components[i];
            if (component == null) continue;

            Rect iconRect = new Rect(
                startX + (i * iconSpacing),
                rect.y + (rect.height - iconSize) * 0.5f,
                iconSize,
                iconSize
            );

            var content = EditorGUIUtility.ObjectContent(component, component.GetType());

            Color iconBgColor = new Color(0f, 0f, 0f, 0.2f);
            EditorGUI.DrawRect(new Rect(iconRect.x - 1, iconRect.y - 1, iconRect.width + 2, iconRect.height + 2), iconBgColor);

            if (GUI.Button(iconRect, content.image, GUIStyle.none))
            {
                Selection.activeGameObject = gameObject;
                EditorGUIUtility.PingObject(component);
                EditorApplication.ExecuteMenuItem("Window/General/Inspector");
            }

            if (iconRect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(iconRect, new Color(1f, 1f, 1f, 0.1f));
                GUI.tooltip = component.GetType().Name;
            }
        }

        if (components.Length > maxIcons)
        {
            Rect moreRect = new Rect(startX + (maxIcons * iconSpacing), rect.y + 2, 12, rect.height - 4);
            GUIStyle moreStyle = new GUIStyle(EditorStyles.miniLabel);
            moreStyle.alignment = TextAnchor.MiddleCenter;
            moreStyle.normal.textColor = Color.gray;
            EditorGUI.LabelField(moreRect, "+", moreStyle);
        }
    }
}
#endif
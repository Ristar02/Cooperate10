using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

[CreateAssetMenu(fileName = "HierarchyColorRules", menuName = "Editor/Hierarchy Color Rules")]
public class HierarchyColorRules : ScriptableObject
{
    [System.Serializable]
    public class ColorRule
    {
        [Header("Rule Settings")]
        public string ruleName = "New Rule";

        [Header("Pattern Matching")]
        [Tooltip("Match by first character (#R, #G, etc.)")]
        public string firstCharPattern = "";

        [Header("Color Settings")]
        public Color backgroundColor = Color.blue;

        [Header("Priority (higher = more important)")]
        public int priority = 0;

        [Header("Case Sensitive")]
        public bool caseSensitive = false;
    }

    [Header("Color Rules (higher priority wins)")]
    public List<ColorRule> rules = new List<ColorRule>()
    {
        // 기본 규칙들
        new ColorRule
        {
            ruleName = "Red Objects (#R)",
            firstCharPattern = "#R",
            backgroundColor = Color.red,
            priority = 10
        },
        new ColorRule
        {
            ruleName = "Green Objects (#G)",
            firstCharPattern = "#G",
            backgroundColor = Color.green,
            priority = 10
        },
        new ColorRule
        {
            ruleName = "Blue Objects (#B)",
            firstCharPattern = "#B",
            backgroundColor = Color.blue,
            priority = 10
        },
        new ColorRule
        {
            ruleName = "Yellow Objects (#Y)",
            firstCharPattern = "#Y",
            backgroundColor = Color.yellow,
            priority = 10
        },
        new ColorRule
        {
            ruleName = "Cyan Objects (#C)",
            firstCharPattern = "#Y",
            backgroundColor = Color.cyan,
            priority = 10
        },
    };
}
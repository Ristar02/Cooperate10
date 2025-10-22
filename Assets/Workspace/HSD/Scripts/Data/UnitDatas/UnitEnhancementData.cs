using UnityEngine;

[CreateAssetMenu(fileName = "UnitEnhancementData", menuName = "Data/Unit/UnitEnhancementData")]
public class UnitEnhancementData : ScriptableObject
{
    [Header("Synergy")]
    public ClassType ClassSynergy;
    public Synergy Synergy;

#if UNITY_EDITOR
    private void OnValidate()
    {
        string newName = $"{ClassSynergy}_{Synergy}";

        if (name != newName)
        {
            UnityEditor.AssetDatabase.RenameAsset(
                UnityEditor.AssetDatabase.GetAssetPath(this),
                newName
            );
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }
#endif
}

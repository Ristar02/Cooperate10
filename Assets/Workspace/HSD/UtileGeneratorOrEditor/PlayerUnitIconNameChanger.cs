#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerUnitIconNameChanger : MonoBehaviour
{
    [SerializeField] private string suffix = "_Icon";
    [SerializeField] private Sprite[] sprites;

    [ContextMenu("Rename Sprites")]
    private void RenameSprites()
    {
        foreach (var sprite in sprites)
        {
            if (sprite == null) continue;

            string path = AssetDatabase.GetAssetPath(sprite);
            string originalName = sprite.name;

            string[] parts = originalName.Split('_');
            if (parts.Length >= 1)
            {
                string newName = parts[0] + suffix;

                AssetDatabase.RenameAsset(path, newName);

                Debug.Log($"Renamed: {originalName} -> {newName}");
            }
            else
            {
                Debug.LogWarning($"이름 형식이 맞지 않아 무시됨: {originalName}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
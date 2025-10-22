#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SpriteRenameTool : MonoBehaviour
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

            // '_' 기준으로 앞의 두 부분만 유지 (Monster_10212)
            string[] parts = originalName.Split('_');
            if (parts.Length >= 2)
            {
                string newName = parts[0] + "_" + parts[1] + suffix;

                // 파일 이름을 바꾸고 싶으면:
                AssetDatabase.RenameAsset(path, newName);

                // 만약 PNG는 그대로 두고 Sprite 내부 이름만 바꾸고 싶으면:
                // sprite.name = newName;
                // EditorUtility.SetDirty(sprite);

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

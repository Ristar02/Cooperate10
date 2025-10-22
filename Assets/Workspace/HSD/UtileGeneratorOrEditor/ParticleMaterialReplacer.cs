#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
 // URP 관련 네임스페이스

public class ParticleMaterialReplacer : MonoBehaviour
{
    [SerializeField] Shader targetShader;

    [ContextMenu("Replace")]
    public void ReplaceParticleMaterials()
    {

        if (targetShader == null)
        {
            Debug.LogError("❌ URP 2D Lit Shader를 찾을 수 없습니다. 프로젝트에 URP 2D Renderer가 적용되어 있는지 확인하세요.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        int changedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            var renderers = prefab.GetComponentsInChildren<ParticleSystemRenderer>(true);
            bool modified = false;

            foreach (var renderer in renderers)
            {
                if (renderer.sharedMaterial != null && renderer.sharedMaterial.shader != targetShader)
                {
                    renderer.sharedMaterial.shader = targetShader;
                    EditorUtility.SetDirty(renderer.sharedMaterial);
                    modified = true;
                }
            }

            if (modified)
            {
                changedCount++;
                Debug.Log($"Shader replaced in prefab: {path}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"교체 완료! 총 {changedCount}개의 프리팹에서 Shader가 수정되었습니다.");
    }
}
#endif

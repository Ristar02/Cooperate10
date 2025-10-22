
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class ParticleSystemLayerSetting
{
    private const string SORTING_LAYER = "FX";
    private const string TARGET_FOLDER = "Assets/Imports/Monster_AttackEffects";

    [MenuItem("Tools/Set Prefab ParticleSystem Renderers To FX Sorting Layer")]
    private static void SetPrefabParticleSystemSortingLayers()
    {
        // 대상 폴더에 있는 프리팹 GUID 전부 가져오기
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { TARGET_FOLDER });

        int changedCount = 0;

        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null) continue;

            // Prefab 수정 시작
            bool modified = false;

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

            ParticleSystemRenderer[] renderers = prefabRoot.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var psRenderer in renderers)
            {
                if (psRenderer.sortingLayerName != SORTING_LAYER)
                {
                    psRenderer.sortingLayerName = SORTING_LAYER;
                    modified = true;
                    changedCount++;
                }
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
                Debug.Log($"[Updated] {assetPath}");
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Monster_AttackEffects 폴더 내 Prefab의 ParticleSystemRenderer SortingLayer를 \"{SORTING_LAYER}\"로 변경 완료! 총 {changedCount} 개 변경됨.");
    }
}
#endif

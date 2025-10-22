using Unity.VisualScripting;
using UnityEngine;

public class PrefabLayerSetter : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private string targetLayerName;

    [ContextMenu("Apply Layer To Prefabs")]
    public void ApplyLayerToPrefabs()
    {
        foreach (var prefab in prefabs)
        {
            if (prefab == null) continue;

            // 🔹 SpriteRenderer 레이어 변경
            SpriteRenderer[] renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.sortingLayerName = targetLayerName;
            }

            Debug.Log($"{prefab.name} 안의 SpriteRenderer {renderers.Length}개 레이어를 '{targetLayerName}'(으)로 변경 완료.");

            // 🔹 레이어 / 태그
            prefab.layer = LayerMask.NameToLayer("Player");
            prefab.tag = "Unit";

            // 🔹 중복 제거 + 보장
            var ub = EnsureSingleComponent<UnitBase>(prefab);
            var usc = EnsureSingleComponent<UnitStatusController>(prefab);
            var col = EnsureSingleComponent<CapsuleCollider2D>(prefab);

            col.size = new Vector2(0.5f, .8f);
            col.offset = new Vector2(0, prefab.transform.localScale.y / 3);
            col.isTrigger = true;

            var rb = EnsureSingleComponent<Rigidbody2D>(prefab);
            rb.gravityScale = 0;

            // 🔹 첫 번째 자식 처리
            Transform child = prefab.transform.GetChild(0);
            child.tag = "UnitTrigger";

            var fsm = EnsureSingleComponent<BaseFSM>(child.gameObject);
            fsm.Owner = ub;

            var boxCol = EnsureSingleComponent<BoxCollider2D>(child.gameObject);
            boxCol.isTrigger = true;
            boxCol.offset = new Vector2(0, prefab.transform.localScale.y / 3);

            prefab.GetComponent<UnitBase>().Inject();
        }
    }


    public T EnsureSingleComponent<T>(GameObject go) where T : Component
    {
        T[] comps = go.GetComponents<T>();
        if (comps.Length > 1)
        {
            for (int i = 1; i < comps.Length; i++)
            {
                Object.DestroyImmediate(comps[i]);
            }
        }

        if (comps.Length == 0)
        {
            return go.AddComponent<T>();
        }

        return comps[0];
    }
}

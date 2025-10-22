using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPrefabLayerAllChange : MonoBehaviour
{
    [Header("Layer를 바꿀 대상 오브젝트들")]
    public GameObject[] targets;

    [Header("변경할 Layer 이름")]
    public string targetLayerName = "Default";

    [ContextMenu("Change Layer For All")]
    public void ChangeLayerForAll()
    {
        int layer = LayerMask.NameToLayer(targetLayerName);

        if (layer == -1)
        {
            Debug.LogError($"지정한 Layer \"{targetLayerName}\" 가 존재하지 않습니다.");
            return;
        }

        foreach (var obj in targets)
        {            
            if (obj != null)
            {
                SetLayerRecursively(obj, layer);
            }
        }

        Debug.Log($"모든 대상 오브젝트와 자식들의 Layer가 \"{targetLayerName}\"(ID:{layer}) 로 변경되었습니다.");
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UnitSortingLayerSetting : MonoBehaviour
{
    [SerializeField] GameObject[] _objs;
    [SerializeField] string _layerName = "Unit";

    [ContextMenu("Setting")]
    private void Setting()
    {
        foreach (var obj in _objs)
        {
            var renderers = obj.GetComponentsInChildren<SortingGroup>();
            foreach (var renderer in renderers)
            {
                renderer.sortingLayerName = _layerName;
            }
        }
    }
}

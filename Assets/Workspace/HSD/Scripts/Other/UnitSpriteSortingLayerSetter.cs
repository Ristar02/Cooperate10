using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpriteSortingLayerSetter : MonoBehaviour
{
    [SerializeField] string sortingLayerName = "Unit";

    [ContextMenu("Set")]
    public void SetUp()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer.sortingLayerName != sortingLayerName)
            {
                spriteRenderer.sortingLayerName = sortingLayerName;
            }
        }
    }
}

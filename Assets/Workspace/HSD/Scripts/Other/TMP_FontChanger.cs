using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMP_FontChanger : MonoBehaviour
{
    [SerializeField] TMP_FontAsset fontAsset;

    [ContextMenu("Change")]
    public void ChangeFont()
    {
        var texts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);

        foreach (var text in texts)
        {
            text.font = fontAsset;            
        }
    }
}

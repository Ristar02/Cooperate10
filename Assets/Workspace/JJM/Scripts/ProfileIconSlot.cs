using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileIconSlot : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;

    public void SetData(UnitData data)
    {
        iconImage.sprite = data.Icon;
        nameText.text = data.Name;
    }
}

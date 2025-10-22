using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitCountPanel : MonoBehaviour
{
    [SerializeField] TMP_Text _unitCountText;

    private int _maxCount = 0;

    private void Awake()
    {
        _maxCount = UnitController.UnitMaxCount;
        UpdateUnitCount(0);
    }

    public void UpdateUnitCount(int currentCount)
    {
        _unitCountText.text = $"{currentCount}/{_maxCount}";
    }
}

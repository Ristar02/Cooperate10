using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitTotalPowerPanel : MonoBehaviour
{
    [SerializeField] protected TMP_Text _powerText;
    protected int _totalPower = 0;

    private void Awake()
    {
        UpdateTotalPower(0);
    }

    public virtual void UpdateTotalPower(int power)
    {
        _totalPower += power;

        _powerText.text = _totalPower.ToString("N0");
    }
}

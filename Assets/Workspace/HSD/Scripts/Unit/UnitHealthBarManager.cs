using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHealthBarManager : MonoBehaviour
{
    [SerializeField] Transform _healthBarContent;    
    [SerializeField] string _healthBarAddress;
    private List<UnitHealthBar> _hpbarList = new List<UnitHealthBar>(20);

    public void Init(UnitBase[] playerUnits, UnitBase[] enemyUnits)
    {
        foreach (var unit in playerUnits)
        {          
            SetHealthBar(unit);
        }        

        foreach (var unit in enemyUnits)
        {
            SetHealthBar(unit);
        }
    }

    public void SetHealthBar(UnitBase unit)
    {
        if (unit == null) return;

        UnitHealthBar bar = Manager.Resources.Instantiate<GameObject>(
            _healthBarAddress,
            unit.GetBarPosition(),
            Quaternion.identity,
            _healthBarContent,
            true
            ).GetComponent<UnitHealthBar>();

        bar.Setup(unit);

        _hpbarList.Add(bar);
    }

    public void Clear()
    {
        foreach (UnitHealthBar healthBar in _hpbarList)
        {
            if (healthBar != null && healthBar.gameObject.activeSelf)
                healthBar.BarDestroy();
        }

        _hpbarList.Clear();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpMeterController : MonoBehaviour
{
    [SerializeField] HpMeterBar _playerMeterBar;
    [SerializeField] HpMeterBar _enemyMeterBar;

    public void Init(UnitBase[] playerUnits, UnitBase[] enemyUnits)
    {
        _playerMeterBar.Init(playerUnits);
        _enemyMeterBar.Init(enemyUnits);
    }
}

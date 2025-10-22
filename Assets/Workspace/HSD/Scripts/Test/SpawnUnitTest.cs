using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnUnitTest : MonoBehaviour
{
    [SerializeField] UnitBase spawnUnit;
    [SerializeField] UnitBase[] enemys;

    [SerializeField] UnitData unitData;
    [SerializeField] int level = 0;

    [ContextMenu("Init")]
    private void Init()
    {
        spawnUnit.Status = new UnitStatus(unitData, level);
        spawnUnit.Init();
    }

    [ContextMenu("Fight")]
    private void Fight()
    {
        foreach (var enemy in enemys)
        {
            enemy.Status = new UnitStatus(unitData, level);
            enemy.Init();
        }

        spawnUnit.Fight();
    }
}

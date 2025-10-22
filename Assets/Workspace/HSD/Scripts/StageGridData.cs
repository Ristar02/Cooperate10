using Cysharp.Threading.Tasks;
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageGridData
{
    private UnitGridDataSO[] _minorEnemyGridDatas;
    private UnitGridDataSO[] _eliteEnemyGridDatas;
    private UnitGridDataSO _bossEnemyGridData;

    private List<UnitGridDataSO> _shuffledMinorList;
    private List<UnitGridDataSO> _shuffledEliteList;
    private int _minorCurrentIndex = 0;
    private int _eliteCurrentIndex = 0;    

    public async UniTask SetGridData(int region, int stage)
    {
        await Manager.Resources.LoadLabel($"Stage{region}");

        _minorEnemyGridDatas = await Manager.Resources.LoadAll<UnitGridDataSO>($"Stage{region}_Minor");
        _eliteEnemyGridDatas = await Manager.Resources.LoadAll<UnitGridDataSO>($"Stage{region}_Elite");
        _bossEnemyGridData = Manager.Resources.Load<UnitGridDataSO>($"Stage{region}-{stage}_Boss");

        InitializeShuffledList(ref _shuffledMinorList, _minorEnemyGridDatas);
        InitializeShuffledList(ref _shuffledEliteList, _eliteEnemyGridDatas);
        _minorCurrentIndex = 0;
        _eliteCurrentIndex = 0;
    }

    public UnitGridDataSO GetGridData(NodeType enemyType)
    {
        return enemyType switch
        {
            NodeType.MinorEnemy => GetNextShuffledData(ref _shuffledMinorList, ref _minorCurrentIndex),
            NodeType.EliteEnemy => GetNextShuffledData(ref _shuffledEliteList, ref _eliteCurrentIndex),
            NodeType.Boss => _bossEnemyGridData,
            _ => null
        };
    }

    private void InitializeShuffledList(ref List<UnitGridDataSO> shuffledList, UnitGridDataSO[] sourceArray)
    {
        if (sourceArray == null || sourceArray.Length == 0)
        {
            shuffledList = null;
            return;
        }
        shuffledList = new List<UnitGridDataSO>(sourceArray);
        Shuffle(shuffledList);
    }

    private UnitGridDataSO GetNextShuffledData(ref List<UnitGridDataSO> shuffledList, ref int currentIndex)
    {
        if (shuffledList == null || shuffledList.Count == 0)
        {
            return null;
        }

        if (currentIndex >= shuffledList.Count)
        {
            Shuffle(shuffledList);
            currentIndex = 0;
        }

        return shuffledList[currentIndex++];
    }

    private void Shuffle(List<UnitGridDataSO> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            UnitGridDataSO value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
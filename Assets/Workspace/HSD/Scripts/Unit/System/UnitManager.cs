using Cysharp.Threading.Tasks;
using DG.Tweening;
using Map;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [Header("Test")]
    public bool IsTest;

    [Header("Center")]
    [SerializeField] Transform _center;

    [Header("Manager")]
    [SerializeField] MapManager _mapManager;
    [SerializeField] BattleManager _battleManager;

    [Header("UI")]
    public MapUIController MapUIController;
    [SerializeField] UnitUIManager _unitUIManager;
    [SerializeField] UI_UnitSlotController _unitSlotController;

    [Header("Unit_Controller")]
    public UnitController UnitController;
    public EnemyController EnemyController;

    [Header("Data")]
    private UnitSpawnChanceData _unitSpawnChanceData;    
    [SerializeField] int _upgradeNeedCount = 3;

    private List<UnitBase> _spawnUnitList = new List<UnitBase>(10);

    private void Awake()
    {
#if UNITY_EDITOR
        if (IsTest)
        {
            CsvDownloader.OnDataSetupCompleted += InitAsync;
            Manager.Data.InitAsync().Forget();
        }
        else
            InitAsync();
#else
        InitAsync();
#endif
    }

    private void OnDestroy()
    {
        UnSubscrube();
    }

    private async void InitAsync()
    {
        if (IsTest)
        {
            await Manager.Resources.LoadLabel("Stage");
            await Manager.Data.StageGridData.SetGridData(1,1);
        }

        MapPlayerTracker.Instance.unitManager = this;
        _mapManager.GenerateNewMap();
        _unitSpawnChanceData = Manager.Data.UnitSpawnChanceData;
        Manager.Data.SynergyDB.ResetSynergys();

        UnitController.Init();
        EnemyController.Init();

        Subscribe();

        _unitUIManager.SynergyPanel.Init(Manager.Data.SynergyDB);
        _unitUIManager.SynergySlotPanel.Init(Manager.Data.SynergyDB);

        PresetSetting();
        _unitSpawnChanceData.CalculateChances(0);
    }

    private void PresetSetting()
    {
        if (Manager.Data == null)
            return;

        TeamPresetData preset = Manager.Data.PresetDB.ReadCurrentSelectedPreset();

        if (preset == null)
            return;

        if (preset.Statuses[0] == null || preset.Statuses[0].Data == null)
            return;

        for (int i = 0; i < preset.Statuses.Length; i++)
        {
            if (preset.Statuses[i].Data != null)
            {
                _unitSlotController.AddEmptySlotUnit(preset.Statuses[i]);
            }
        }

        _unitSpawnChanceData.CurrentLeaderSynergy = preset.Statuses[0].Data.Synergy;
    }

    #region EventHandler
    private void Subscribe()
    {
        BattleManager.OnSpawnUnit += _unitUIManager.UnitHealthBarManager.SetHealthBar;
        BattleManager.OnSpawnUnit += SpawnUnitAdded;
        BattleManager.OnBattleEnded += GameEndedUnitStandby;
        BattleManager.OnBattleEnded += MapPlayerTracker.OnEventEnded;
        BattleManager.OnPlayerVictory += MapUIController.MapEnter;
        BattleManager.OnBattleStarted += ApplyHealAugment;

        UnitController.SynergyController.OnSynergyChanged += _unitUIManager.SynergySlotPanel.UpdateSynergySlot;
        UnitController.SynergyController.OnSynergyChanged += _unitUIManager.SynergyPanel.UpdateSynergySlot;

        UnitController.OnUnitCountChanged += _unitUIManager.UnitCountPanel.UpdateUnitCount;

        for (int i = 0; i < _unitUIManager.UnitTotalPowerPanel.Length; i++)
        {
            UnitController.OnUnitPowerChanged += _unitUIManager.UnitTotalPowerPanel[i].UpdateTotalPower;
        }
    }

    private void UnSubscrube()
    {
        BattleManager.OnSpawnUnit -= _unitUIManager.UnitHealthBarManager.SetHealthBar;
        BattleManager.OnSpawnUnit -= SpawnUnitAdded;
        BattleManager.OnBattleEnded -= GameEndedUnitStandby;
        BattleManager.OnBattleEnded -= MapPlayerTracker.OnEventEnded;
        BattleManager.OnPlayerVictory -= MapUIController.MapEnter;
        BattleManager.OnBattleStarted -= ApplyHealAugment;

        UnitController.SynergyController.OnSynergyChanged -= _unitUIManager.SynergySlotPanel.UpdateSynergySlot;
        UnitController.SynergyController.OnSynergyChanged -= _unitUIManager.SynergyPanel.UpdateSynergySlot;

        UnitController.OnUnitCountChanged -= _unitUIManager.UnitCountPanel.UpdateUnitCount;

        for (int i = 0; i < _unitUIManager.UnitTotalPowerPanel.Length; i++)
        {
            UnitController.OnUnitPowerChanged -= _unitUIManager.UnitTotalPowerPanel[i].UpdateTotalPower;
        }
    }
    #endregion

    public void ApplyHealAugment()
    {
        UnitBase[] units = UnitController.GetUnits();
        for (int i = 0; i < AugmentManager.Instance.currentAugment.Count; i++)
        {
            if (AugmentManager.Instance.currentAugment[i].Trigger == TriggerType.OnBattleStart)
            {
                for (int j = 0; j < units.Length; j++)
                {
                    AugmentManager.Instance.ApplyHealAugment(units[j]);
                }
            }
            else if (AugmentManager.Instance.currentAugment[i].Trigger == TriggerType.OnEnemyDied)
            {
                // TODO : 적이 죽었을 때 조건 추가 필요

                for (int j = 0; j < units.Length; j++)
                {
                    AugmentManager.Instance.ApplyHealAugment(units[j]);
                }
            }
        }
    }

    #region Fight
    public void Fight()
    {
        if (UnitController.GetUnitsCount() == 0)
            return;

        _unitUIManager.StandbyUIDeActive().Forget();

        FightRoutine().Forget();
    }

    private async UniTask FightRoutine()
    {
        await Camera.main.transform.DOLocalMoveX(0, 50f)
        .SetSpeedBased()
        .SetEase(Ease.OutQuad)
        .AsyncWaitForCompletion();

        await Camera.main.DOOrthoSize(17, 0.5f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
        SlotsDeActive();

        await UniTask.WaitForSeconds(.1f);
        UnitsMove();

        UnitController.BattleParent.DOMoveX(_center.position.x - 5, 2).SetEase(Ease.Linear);
        Camera.main.transform.DOMoveX(_center.position.x, 2.2f);
        await UniTask.WaitForSeconds(1);

        EnemyController.BattleParent.DOMoveX(-(_center.position.x - 5), 1).SetEase(Ease.Linear);
        await UniTask.WaitForSeconds(1);

        UnitsIdle();

        await UniTask.WaitForSeconds(0.3f);

        UnitController.UnitFight();
        EnemyController.EnemyFight();

        FightUISetup();
        _battleManager.BattleStart();
        _battleManager.Init(UnitController.GetUnits(), EnemyController.GetUnits());
    }

    private void SlotsDeActive()
    {
        UnitController.SlotsDeActive();
        EnemyController.SlotsDeActive();
    }

    private void FightUISetup()
    {
        _unitUIManager.BattleUISetting().Forget();

        _unitUIManager.DamageMeterController.Init(UnitController.GetUnits());
        _unitUIManager.SkillPopUpController.Init(UnitController.GetUnits(), EnemyController.GetUnits());
        _unitUIManager.HpMeterController.Init(UnitController.GetUnits(), EnemyController.GetUnits());
        _unitUIManager.UnitHealthBarManager.Init(UnitController.GetUnits(), EnemyController.GetUnits());
    }
    #endregion

    public void GameEndedUnitStandby()
    {
        SpawnUnitStnaby();
        UnitController.UnitsGameEndedStandby();
        EnemyController.EnemyStandby();
    }

    public void GameStandby()
    {
        ClearSpawnUnit();
        EnemyController.ResetEnemy();
        EnemyController.BattleParent.transform.position = Vector2.zero;
        UnitController.UnitStandbyAndSetSlotPosition();
        UnitController.BattleParent.transform.position = Vector2.zero;
        _unitUIManager.StandbyUISetting().Forget();

        _battleManager.GameStanby();
        Camera.main.transform.position = new Vector3(0, 0, -10);
        Camera.main.orthographicSize = 13;

        MapUIController.MapExit();
    }    

    private void UnitsIdle()
    {
        UnitController.UnitIdle();
        EnemyController.EnemyIdle();
    }

    private void UnitsMove()
    {
        UnitController.UnitMove();
        EnemyController.EnemyMove();
    }

    #region Unit
    public void RandomSpawn()
    {
        int slotIdx = _unitSlotController.GetEmptySlot();

        if (slotIdx == -1)
        {
            Debug.Log("슬롯이 부족합니다.");
            return;
        }

        if (!InGameManager.Instance.SpendGold(InGameManager.Instance.SpawnGold))
        {
            Debug.Log("골드가 부족합니다.");
            return;
        }

        UnitData unit = _unitSpawnChanceData.RollUnit();
        UnitStatus unitStatus = new UnitStatus(unit);

        AddSlotUnit(unitStatus, slotIdx);
    }

    public void AddSlotUnit(UnitStatus unit, int slotIdx)
    {
        SetSlot(unit, slotIdx);
    }

    public void AddBattleUnit(UnitStatus unit, UnitSlot slot)
    {
        GameObject obj = Instantiate(unit.Data.UnitPrefab);
        UnitBase unitBase = ComponentProvider.Get<UnitBase>(obj);
        unitBase.Status = unit;
        unitBase.Init();

        UnitController.AddUnit(slot, unitBase);
    }

    private void SetSlot(UnitStatus unit, int idx)
    {
        _unitSlotController.SetSlot(unit, idx);

        CheckUpgrade(unit);
    }

    private void CheckUpgrade(UnitStatus unit)
    {
        if (unit.Level == 2)
        {
            Debug.Log($"최종 유닛 {unit.Data.Name} 업그레이드 불가");
            return;
        }

        if (GetUnitCount(unit) >= 3)
        {
            UpgradeUnit(unit);
        }
    }

    private void UpgradeUnit(UnitStatus unit)
    {
        UnitStatus newUnit = new UnitStatus(unit.Data, unit.Level + 1);

        int upgradeNeedCount = _upgradeNeedCount;

        int slotCount = _unitSlotController.GetUnitCount(unit);
        int unitCount = UnitController.GetUnitCount(unit);

        Vector2Int pos = Vector2Int.zero;

        for (int i = 0; i < slotCount; i++)
        {
            upgradeNeedCount--;
            _unitSlotController.RemoveLastUnit(unit);
        }

        for (int i = 0; i < upgradeNeedCount; i++)
        {
            pos = UnitController.RemoveUnitGetPosition(unit);
        }

        if (pos != Vector2Int.zero && !UnitController.IsUnitMaxCount())
        {
            GameObject obj = Instantiate(newUnit.Data.UnitPrefab);
            UnitBase unitBase = ComponentProvider.Get<UnitBase>(obj);
            unitBase.Status = newUnit;
            unitBase.Init();

            UnitController.AddUnit(unitBase, pos);
        }
        else
        {
            _unitSlotController.SetSlot(newUnit, _unitSlotController.GetEmptySlot());
        }

        CheckUpgrade(newUnit);
    }

    private int GetUnitCount(UnitStatus unit)
    {
        return UnitController.GetUnitCount(unit) + _unitSlotController.GetUnitCount(unit);
    }
    #endregion

    private void SpawnUnitAdded(UnitBase unit)
    {
        _spawnUnitList.Add(unit);
    }

    private void SpawnUnitStnaby()
    {
        foreach (var unit in _spawnUnitList)
        {
            unit.GameEndedStanby();
        }
    }

    private void ClearSpawnUnit()
    {
        foreach (var unit in _spawnUnitList)
        {
            Destroy(unit.gameObject);
        }

        _spawnUnitList.Clear();
    }
}

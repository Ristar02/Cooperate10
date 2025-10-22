using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static event Action OnPlayerVictory;
    public static event Action OnPlayerDefeat;

    public static event Action OnBattleStarted;
    public static event Action OnBattleEnded;
    public static event Action OnGameStanby;
    public static Action<UnitBase> OnSpawnUnit;

    [SerializeField] LayerMask _playerLayer;

    private bool _isPlayerWin;

    [SerializeField] float _cameraZoomDuration = .3f;
    private Vector2 _lastTargetPos;

    [Header("UnitCount")]
    private int _playerUnitCount;
    private int _enemyUnitCount;    

    private void OnDestroy()
    {
        OnBattleStarted = null;
        OnBattleEnded = null;
        OnSpawnUnit = null;
        OnPlayerVictory = null;
        OnPlayerDefeat = null;
        OnGameStanby = null;
    }

    public void Init(UnitBase[] playerUnits, UnitBase[] enemyUnits)
    {
        UnitBase[] notNullPlayerUnits = GetNotNullUnits(playerUnits);
        UnitBase[] notNullEnemyUnits = GetNotNullUnits(enemyUnits);

        RegisterEvent(notNullPlayerUnits);
        RegisterEvent(notNullEnemyUnits);

        _playerUnitCount = notNullPlayerUnits.Length;
        _enemyUnitCount = notNullEnemyUnits.Length;

        OnSpawnUnit += SpawnUnit;
    }

    private void SpawnUnit(UnitBase unit)
    {
        _playerUnitCount++;
        unit.StatusController.OnUnitDied += CheckBattleEnded;
    }

    public void BattleStart()
    {
        OnBattleStarted?.Invoke();
    }

    public void GameStanby()
    {
        OnGameStanby?.Invoke();
    }

    private void CheckBattleEnded(UnitStatusController statusCon)
    {
        if (!statusCon.IsDead) return;

        if (_playerLayer.Contain(statusCon.gameObject.layer))
        {
            _playerUnitCount--;
        }
        else
        {
            _enemyUnitCount--;
        }
        Debug.Log($"플레이어 유닛 수: {_playerUnitCount}, 적 유닛 수: {_enemyUnitCount}");
        statusCon.OnUnitDied -= CheckBattleEnded;
        _lastTargetPos = statusCon.transform.position;

        if (_playerUnitCount > 0 && _enemyUnitCount > 0)
            return;

        if (_playerUnitCount <= 0)
        {  
            _isPlayerWin = false;
            Debug.Log("플레이어 패배");
        }
        else if (_enemyUnitCount <= 0)
        {
            _isPlayerWin = true;
            Debug.Log("플레이어 승리");
        }

        GameEnd(_isPlayerWin);
    }

    private UnitBase[] GetNotNullUnits(UnitBase[] units)
    {
        List<UnitBase> notNullUnits = new List<UnitBase>();
        foreach (var unit in units)
        {
            if (unit != null)
                notNullUnits.Add(unit);
        }
        return notNullUnits.ToArray();
    }

    private void RegisterEvent(UnitBase[] units)
    {
        for (int i = 0; i < units.Length; i++)
        {
            units[i].StatusController.OnUnitDied += CheckBattleEnded;
        }
    }    

    private void GameEnd(bool isPlayerWin)
    {
        GameEndRoutineAsync(isPlayerWin).Forget();
    }

    private async UniTask GameEndRoutineAsync(bool isPlayerWin)
    {
        Manager.Game.CameraDoMove(_lastTargetPos, _cameraZoomDuration, 5).Forget();
        await Manager.Game.SlowMotionAsync(.1f, 2);

        if(isPlayerWin)
        {
            OnPlayerVictory?.Invoke();
        }
        else
        {
            OnPlayerDefeat?.Invoke();
        }

        OnBattleEnded?.Invoke();
    }
}

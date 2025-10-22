using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class UnitPassive
{
    public SynergyEffect Effect;
    private UnitBase _owner;
    private int _currentActivations;
    private int _mulriplier;
    private bool _isActive;

    private CancellationTokenSource _cts; // Interval 루프 중단용

    public UnitPassive(SynergyEffect effect, UnitBase owner, int statMulriplier = 1)
    {
        _currentActivations = 0;
        Effect = effect;
        _owner = owner;
        _mulriplier = statMulriplier;

        _cts = new CancellationTokenSource();
    }
    #region Active & Deactive
    /// <summary>
    /// 패시브 발동 시작
    /// </summary>
    public void Active()
    {
        switch (Effect.TriggerType)
        {
            case TriggerType.Base:
                EffectActives();
                break;
            case TriggerType.OnDied:
                _owner.StatusController.OnDied += EffectActives;
                break;
            case TriggerType.OnAttack:
                _owner.StatusController.OnAttack += EffectActives;
                break;
            case TriggerType.OnUseSkill:
                _owner.StatusController.OnSkill += EffectActives;
                break;
            case TriggerType.OnInterval:
                BattleManager.OnBattleStarted += OnInterval;
                BattleManager.OnBattleEnded += TokenClear;
                break;

            case TriggerType.OnBattleStart:
                BattleManager.OnBattleStarted += EffectActives;
                break;

            case TriggerType.OnBattleEnded:
                BattleManager.OnBattleEnded += EffectActives;
                break;
        }
    }

    /// <summary>
    /// 패시브 비활성화 및 정리
    /// </summary>
    public void Deactive()
    {
        TokenClear();
        _isActive = false;

        switch (Effect.TriggerType)
        {
            case TriggerType.Base:
                RemoveStat();
                break;
            case TriggerType.OnAttack:
                _owner.StatusController.OnAttack -= EffectActives;
                break;
            case TriggerType.OnDied:
                _owner.StatusController.OnDied -= EffectActives;
                break;
            case TriggerType.OnUseSkill:
                _owner.StatusController.OnSkill -= EffectActives;
                break;
            case TriggerType.OnInterval:
                BattleManager.OnBattleStarted -= OnInterval;
                BattleManager.OnBattleEnded -= TokenClear;
                break;

            case TriggerType.OnBattleStart:
                BattleManager.OnBattleStarted -= EffectActives;
                break;

            case TriggerType.OnBattleEnded:
                BattleManager.OnBattleEnded -= EffectActives;
                break;
        }
    }
    #endregion

    private void TokenClear()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
    }

    #region Interval
    private void OnInterval()
    {
        if (Effect.IsDelay)
            OnDelayEffectAsync(_cts.Token).Forget();
        else
            OnIntervalEffectAsync(_cts.Token).Forget();
    }

    /// <summary>
    /// 일반적인 Interval 효과 (MaxActivations 만큼 실행)
    /// </summary>
    private async UniTask OnIntervalEffectAsync(CancellationToken token)
    {
        while (_currentActivations < Effect.MaxActivations && !token.IsCancellationRequested)
        {
            EffectActives();

            try
            {
                await UniTask.WaitForSeconds(Effect.Interval, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        if (Effect.NextEffect != null)
        {
            await RunNextEffectAsync(token);
        }
    }

    /// <summary>
    /// Delay 효과 (첫 효과 후 Delay → NextEffect)
    /// </summary>
    private async UniTask OnDelayEffectAsync(CancellationToken token)
    {
        while (_currentActivations < Effect.MaxActivations && !token.IsCancellationRequested)
        {
            try
            {
                await UniTask.WaitForSeconds(Effect.Interval, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            EffectActives();
        }

        // Delay 대기
        try
        {
            await UniTask.WaitForSeconds(Effect.DelayTime, cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        // NextEffect 실행
        if (Effect.NextEffect != null)
        {
            await RunNextEffectAsync(token);
        }
    }

    /// <summary>
    /// NextEffect 실행 로직 (Interval 여부에 따라 단발/주기)
    /// </summary>
    private async UniTask RunNextEffectAsync(CancellationToken token)
    {
        if (Effect.NextEffect.Interval > 0)
        {
            while (!token.IsCancellationRequested)
            {
                NextEffectActives();

                try
                {
                    await UniTask.WaitForSeconds(Effect.NextEffect.Interval, cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
        else
        {
            NextEffectActives();
        }
    }
    #endregion

    #region EffectActives

    private void EffectActives()
    {
        if (Effect.IsFirstOnly && _isActive)
            return;

        _isActive = true;

        if (_currentActivations < Effect.MaxActivations)
        {
            _currentActivations++;

            SpawnActive();
            BuffEffectActive();
            AttackActive();
            if (!string.IsNullOrEmpty(Effect.SynergyEffectAddress))
            {
                Manager.Resources.Destroy(
                Manager.Resources.Instantiate<GameObject>(Effect.SynergyEffectAddress, _owner.GetCenter()), Effect.EffectDuration
                );
            }
        }
        else
        {
            NextEffectActives();

            if (Effect.IsActivationsClear)
                _currentActivations = 0;
        }
    }

    private void RemoveStat()
    {
        if (!Effect.IsBuff)
            return;

        foreach (var stat in Effect.StatModifiers)
        {
            _owner.StatusController.RemoveStat(stat.StatType, Effect.Key);
        }
    }

    private void NextEffectActives()
    {
        if (Effect.NextEffect == null)
            return;

        if (Effect.NextEffect.EffectApplyType == EffectApplyType.All)
            return;

        NextBuffEffectActive();
        NextAttackActive();
    }

    #region BuffEffect
    private void BuffEffectActive()
    {
        if (!Effect.IsBuff)
            return;

        BuffEffectActive(Effect);
    }
    private void NextBuffEffectActive()
    {
        if (!Effect.NextEffect.IsBuff)
            return;

        BuffEffectActive(Effect.NextEffect);
    }
    private void BuffEffectActive(SynergyEffect _effect)
    {
        switch (_effect.EffectType)
        {
            case EffectType.Buff_Debuff:
                for (int i = 0; i < _effect.SynergyBuffDatas.Length; i++)
                {
                    SynergyBuffData data = _effect.SynergyBuffDatas[i];
                    _owner.StatusController.ApplyEffect(
                        new BuffEffectData { StatType = data.StatType, Duration = data.Duration },
                        data.Value * _mulriplier, _effect.Key);
                }
                break;

            case EffectType.Increase:
                foreach (var stat in _effect.StatModifiers)
                {
                    if (stat.StatType == StatType.CurHp)
                        _owner.StatusController.IncreaseHealth(Mathf.RoundToInt(stat.Value * _mulriplier));
                    else if (stat.StatType == StatType.CurMana)
                        _owner.StatusController.IncreaseMana(Mathf.RoundToInt(stat.Value * _mulriplier));
                    else if (stat.StatType == StatType.Shield)
                        _owner.StatusController.IncreaseShield(Mathf.RoundToInt(stat.Value * _mulriplier));
                    else
                        _owner.StatusController.AddStat(stat.StatType, stat.Value * _mulriplier, $"{_effect.Key}__{_currentActivations}");
                }
                break;
        }
    }
    #endregion

    #region AttackEffect
    private void AttackActive()
    {
        if (!Effect.IsAttack)
            return;

        AttackSpawn(Effect);
    }

    private void NextAttackActive()
    {
        if (!Effect.NextEffect.IsAttack)
            return;

        AttackSpawn(Effect.NextEffect);
    }

    private void AttackSpawn(SynergyEffect effect)
    {
        if (effect.AttackPrefab == null)
        {
            Debug.LogWarning($"[시너지 공격 시스템] 해당 주소에 Prefab이 없습니다. 주소 : {effect.AttackAddress}");
            return;
        }

        GameObject attackObj = effect.SpawnPositionType == SpawnPositionType.Self ?
            Manager.Resources.Instantiate(effect.AttackPrefab, _owner.transform.position, Quaternion.identity, true) :
            Manager.Resources.Instantiate(effect.AttackPrefab, _owner.Target.position, Quaternion.identity, true);

        AttackObject attack = ComponentProvider.Get<AttackObject>(attackObj);

        attack.Setup(effect.Power, effect.AttackDealy);
    }
    #endregion

    #region SpawnEffect
    private void SpawnActive()
    {
        if (!Effect.IsSpawn)
            return;

        Vector3 pos = _owner.transform.position;
        Spawn(pos);
    }

    private void Spawn(Vector3 pos)
    {
        if (Effect.SpawnPrefab == null)
        {
            Debug.LogWarning($"[시너지 스폰 시스템] 해당 주소에 Prefab이 없습니다. 주소 : {Effect.SpawnAddress}");
            return;
        }

        Debug.Log($"[시너지 스폰 시스템] {Effect.SpawnPrefab.name} 소환");

        GameObject spawnEffect = Manager.Resources.Instantiate<GameObject>(Effect.SpawnEffectPrefab, pos, true);        
        Manager.Resources.Destroy(spawnEffect, 2);

        GameObject unitObj = GameObject.Instantiate(Effect.SpawnPrefab, Vector3.zero, Quaternion.identity);
        UnitBase spawnUnit = ComponentProvider.Get<UnitBase>(unitObj);
        UnitData data = Manager.Resources.Load<UnitData>(Effect.UnitDataAddress);

        spawnUnit.StatusController.Invincible();

        if (spawnUnit == null)
            return;

        if (Effect.SpawnType == SpawnStatType.Level)
        {
            spawnEffect.transform.localScale = new Vector3(Effect.UnitLevel, Effect.UnitLevel, Effect.UnitLevel);

            if (Effect.IsMultiplier)
            {
                spawnUnit.StatusController.StatMultiplier = Effect.UnitStatMultiplier;
                spawnUnit.Status = new UnitStatus(data, Effect.UnitLevel);
                spawnUnit.Init(Effect.SpawnUnitStats);
                spawnUnit.Fight();
            }
            else
            {
                spawnUnit.Init();
            }
        }
        else if (Effect.SpawnType == SpawnStatType.LowUpgrade)
        {
            int level = _owner.Status.Level - 1 >= 0 ? _owner.Status.Level - 1 : 0;
            spawnUnit.Status = new UnitStatus(data, level);
            spawnUnit.Init();
            spawnUnit.Fight();
        }

        unitObj.transform.position = pos;
        BattleManager.OnSpawnUnit?.Invoke(spawnUnit);
    }
    #endregion
    #endregion
}

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EffectController
{
    private readonly EffectAddressData _addressData;
    private readonly Transform _transform;

    // 활성 이펙트 추적
    private Dictionary<BuffEffect, ActiveEffectData> _activeEffects = new Dictionary<BuffEffect, ActiveEffectData>();
    private GameObject _shieldEffect;
    private CancellationTokenSource _shieldCTS;

    private class ActiveEffectData
    {
        public GameObject EffectObject;
        public CancellationTokenSource CTS;
        public float EndTime;
    }

    public EffectController(Transform transform)
    {
        _transform = transform;
        _addressData = Manager.Resources.Load<EffectAddressData>("Data/EffectAddressData");
    }

    public void AddBuffEffect(BuffEffect buffEffect, float duration = 2)
    {
        if (!InGameManager.Instance.IsBattle)
            return;

        string address = _addressData.GetBuffAddress(buffEffect);
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError($"BuffEffect {buffEffect}에 대한 주소가 없음!");
            return;
        }

        // 쉴드는 별도 처리
        if (buffEffect == BuffEffect.Shield)
        {
            return; // Shield는 IncreaseShield에서 처리
        }

        // 같은 버프가 이미 활성화되어 있는 경우
        if (_activeEffects.ContainsKey(buffEffect))
        {
            RefreshEffect(buffEffect, duration);
        }
        else
        {
            SpawnNewEffect(buffEffect, address, duration);
        }
    }

    /// <summary>
    /// 쉴드 이펙트 시작
    /// </summary>
    public void StartShieldEffect()
    {
        if (_shieldEffect != null)
            return;

        string address = _addressData.GetBuffAddress(BuffEffect.Shield);
        if (string.IsNullOrEmpty(address))
            return;

        _shieldCTS = new CancellationTokenSource();
        Vector3 spawnPos = GetSpawnPos(BuffEffect.Shield);
        _shieldEffect = Manager.Resources.Instantiate<GameObject>(
            address, spawnPos, Quaternion.identity, _transform, true
        );

        MonitorShield(_shieldCTS.Token).Forget();
    }

    /// <summary>
    /// ld 값 모니터링
    /// </summary>
    private async UniTaskVoid MonitorShield(CancellationToken token)
    {
        var statusController = _transform.GetComponent<UnitStatusController>();
        if (statusController == null)
        {
            StopShieldEffect();
            return;
        }

        try
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(token);

                if (statusController.Shield.Value <= 0)
                {
                    StopShieldEffect();
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 정상 취소
        }
    }

    /// <summary>
    /// 쉴드 이펙트 중지
    /// </summary>
    public void StopShieldEffect()
    {
        if (_shieldEffect != null)
        {
            Manager.Resources.Destroy(_shieldEffect);
            _shieldEffect = null;
        }

        if (_shieldCTS != null)
        {
            _shieldCTS.Cancel();
            _shieldCTS.Dispose();
            _shieldCTS = null;
        }
    }

    /// <summary>
    /// 기존 이펙트의 duration 갱신
    /// </summary>
    private void RefreshEffect(BuffEffect buffEffect, float duration)
    {
        var effectData = _activeEffects[buffEffect];

        // 기존 CTS 취소
        effectData.CTS?.Cancel();
        effectData.CTS?.Dispose();

        // 새로운 CTS 생성 및 종료 시간 갱신
        effectData.CTS = new CancellationTokenSource();
        effectData.EndTime = Time.time + duration;

        // 새로운 타이머 시작
        DestroyEffectAfterDelay(buffEffect, duration, effectData.CTS.Token).Forget();
    }

    /// <summary>
    /// 새 이펙트 생성
    /// </summary>
    private void SpawnNewEffect(BuffEffect buffEffect, string address, float duration)
    {
        Vector3 spawnPos = GetSpawnPos(buffEffect);
        GameObject effectObject = Manager.Resources.Instantiate<GameObject>(
            address, spawnPos, Quaternion.identity, _transform, true
        );

        var cts = new CancellationTokenSource();
        var effectData = new ActiveEffectData
        {
            EffectObject = effectObject,
            CTS = cts,
            EndTime = Time.time + duration
        };

        _activeEffects[buffEffect] = effectData;

        DestroyEffectAfterDelay(buffEffect, duration, cts.Token).Forget();
    }

    /// <summary>
    /// 지연 후 이펙트 제거
    /// </summary>
    private async UniTaskVoid DestroyEffectAfterDelay(BuffEffect buffEffect, float duration, CancellationToken token)
    {
        try
        {
            await UniTask.WaitForSeconds(duration, cancellationToken: token);

            if (_activeEffects.TryGetValue(buffEffect, out var effectData))
            {
                if (effectData.EffectObject != null)
                {
                    Manager.Resources.Destroy(effectData.EffectObject);
                }

                effectData.CTS?.Dispose();
                _activeEffects.Remove(buffEffect);
            }
        }
        catch (OperationCanceledException)
        {
            // duration이 갱신되어 취소된 경우, 아무것도 하지 않음
        }
    }

    /// <summary>
    /// 모든 이펙트 정리
    /// </summary>
    public void ClearAllEffects()
    {
        foreach (var kvp in _activeEffects)
        {
            if (kvp.Value.EffectObject != null)
            {
                Manager.Resources.Destroy(kvp.Value.EffectObject);
            }
            kvp.Value.CTS?.Cancel();
            kvp.Value.CTS?.Dispose();
        }
        _activeEffects.Clear();

        StopShieldEffect();
    }

    private Vector3 GetSpawnPos(BuffEffect buffEffect)
    {
        return (buffEffect) switch
        {
            BuffEffect.Buff => _transform.position,
            BuffEffect.Stun => _transform.GetTopPosition(),
            BuffEffect.Debuff => _transform.GetTopPosition(),
            BuffEffect.Damage => _transform.GetCenterPosition(),
            BuffEffect.Heal => _transform.GetCenterPosition(),
            BuffEffect.Shield => _transform.GetCenterPosition(),
            BuffEffect.AttackSpeed => _transform.GetCenterPosition(),
            BuffEffect.Defense => _transform.GetCenterPosition(),
            _ => _transform.position
        };
    }
}
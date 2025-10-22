using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class UnitStatusController : MonoBehaviour, IDamageable, IEffectable
{
    private struct StatStruct
    {
        public int MaxHealth;
        public float AttackSpeed;

        public int PhysicalDamage;
        public int MagicDamage;

        public int PhysicalDefense;
        public int MagicDefense;

        public int CritChance;
    }

    [field : SerializeField] public UnitStatus Status { get; set; }

    #region Stat
    [Header("Status")]
    public Stat<int> MaxHealth;
    public Stat<int> MaxMana;
    public Stat<int> ManaGain;
    public Stat<float> AttackSpeed;
    public Stat<float> MoveSpeed;

    [Header("Damage")]
    public Stat<int> PhysicalDamage;
    public Stat<int> MagicDamage;

    [Header("CritRate")]
    public Stat<int> CritChance;

    [Header("Defense")]
    public Stat<int> PhysicalDefense;
    public Stat<int> MagicDefense;

    [Header("Range")]
    public Stat<float> AttackRange;
    public Stat<int> AttackCount;
    public float DetectionRange = 5f;
    #endregion

    public Property<int> CurHp = new Property<int>();
    public Property<int> CurMana = new Property<int>();
    public Property<int> Shield = new Property<int>();
    public Property<int> TotalDamage = new Property<int>();
    public Property<bool> IsStunned = new Property<bool>();

    #region Controller
    public EffectController EffectController { get; set; }
    public UnitPassiveController PassiveController { get; set; }
    public UnitFXController UnitFXController { get; set; }
    #endregion

    #region Events
    public Action<UnitStatusController> OnUnitDied;
    public Action<UnitStatus> OnUseSkill;

    public event Action OnDied;
    public Action OnSkill;
    public Action OnAttack;
    #endregion

    private CancellationTokenSource _cts = new();

    public UnitAttackData CurrentAttackData;

    [HideInInspector] public float StatMultiplier = 0;

    public bool IsDead { get; set; }
    public bool IsInvincible { get; set; }

    private void OnDestroy()
    {
        PassiveController?.DeActiveAllPassive();
        EffectController?.ClearAllEffects();

        _cts.Cancel();
        _cts.Dispose();
    }

    #region Init&Clear
    public void Init(UnitStatus status, UnitStats plusUnitStat = null)
    {
        Transform root = transform.GetChild(0);
        DetectionRange = 10;

        if (root.childCount > 0)
            root = root.GetChild(0);

        PassiveController = new UnitPassiveController(gameObject);
        EffectController = new EffectController(transform);
        UnitFXController = new UnitFXController(
            root.GetComponentsInChildren<SpriteRenderer>(),
            GetComponentInChildren<SortingGroup>()
            );

        Status = status;
        CurrentAttackData = Status.Data.AttackData;
        IsDead = false;
        IsStunned.Value = false;

        if (plusUnitStat == null)
        {
            SetBaseStat(status.GetCurrentStat());
        }
        else
        {
            SetBaseStat(status.GetCurrentStat(), plusUnitStat);
        }
    }

    #region SetStat
    private void SetBaseStat(UnitStats stat)
    {
        StatStruct addStat = GetStatStruct();

        MaxHealth.SetBaseStat(stat.MaxHealth + addStat.MaxHealth);
        MaxMana.SetBaseStat(stat.MaxMana);
        ManaGain.SetBaseStat(stat.ManaGain);

        AttackSpeed.SetBaseStat(stat.AttackSpeed + addStat.AttackSpeed);
        MoveSpeed.SetBaseStat(stat.MoveSpeed);

        PhysicalDamage.SetBaseStat(stat.PhysicalDamage + addStat.PhysicalDamage);
        MagicDamage.SetBaseStat(stat.MagicDamage + addStat.MagicDamage);

        CritChance.SetBaseStat(stat.CritChance + addStat.CritChance);

        PhysicalDefense.SetBaseStat(stat.PhysicalDefense + addStat.PhysicalDefense);
        MagicDefense.SetBaseStat(stat.MagicDefense + addStat.MagicDefense);

        AttackRange.SetBaseStat(stat.AttackRange);
        AttackCount.SetBaseStat(stat.AttackCount);

        CurHp.Value = MaxHealth.Value;
        CurMana.Value = MaxMana.Value;
        TotalDamage.Value = 0;
    }

    private void SetBaseStat(UnitStats baseStat, UnitStats plusStat)
    {
        var stat = new UnitStats
        {
            MaxHealth = Mathf.RoundToInt(baseStat.MaxHealth + plusStat.MaxHealth * StatMultiplier),
            MaxMana = Mathf.RoundToInt(baseStat.MaxMana + plusStat.MaxMana * StatMultiplier),
            ManaGain = Mathf.RoundToInt(baseStat.ManaGain + plusStat.ManaGain * StatMultiplier),

            AttackSpeed = baseStat.AttackSpeed + plusStat.AttackSpeed * StatMultiplier,
            MoveSpeed = baseStat.MoveSpeed + plusStat.MoveSpeed * StatMultiplier,

            PhysicalDamage = Mathf.RoundToInt(baseStat.PhysicalDamage + plusStat.PhysicalDamage * StatMultiplier),
            MagicDamage = Mathf.RoundToInt(baseStat.MagicDamage + plusStat.MagicDamage * StatMultiplier),

            CritChance = Mathf.RoundToInt(baseStat.CritChance + plusStat.CritChance * StatMultiplier),

            PhysicalDefense = Mathf.RoundToInt(baseStat.PhysicalDefense + plusStat.PhysicalDefense * StatMultiplier),
            MagicDefense = Mathf.RoundToInt(baseStat.MagicDefense + plusStat.MagicDefense * StatMultiplier),

            AttackRange = Mathf.RoundToInt(baseStat.AttackRange + plusStat.AttackRange * StatMultiplier),
            AttackCount = Mathf.RoundToInt(baseStat.AttackCount + plusStat.AttackCount * StatMultiplier),
        };

        StatStruct addStat = GetStatStruct();

        MaxHealth.SetBaseStat(stat.MaxHealth + addStat.MaxHealth);
        MaxMana.SetBaseStat(stat.MaxMana);
        ManaGain.SetBaseStat(stat.ManaGain);

        AttackSpeed.SetBaseStat(stat.AttackSpeed + addStat.AttackSpeed);
        MoveSpeed.SetBaseStat(stat.MoveSpeed);

        PhysicalDamage.SetBaseStat(stat.PhysicalDamage + addStat.PhysicalDamage);
        MagicDamage.SetBaseStat(stat.MagicDamage + addStat.MagicDamage);

        CritChance.SetBaseStat(stat.CritChance + addStat.CritChance);

        PhysicalDefense.SetBaseStat(stat.PhysicalDefense + addStat.PhysicalDefense);
        MagicDefense.SetBaseStat(stat.MagicDefense + addStat.MagicDefense);

        AttackRange.SetBaseStat(stat.AttackRange);
        AttackCount.SetBaseStat(stat.AttackCount);

        CurHp.Value = MaxHealth.Value;
        CurMana.Value = MaxMana.Value;
        TotalDamage.Value = 0;
    }

    private StatStruct GetStatStruct()
    {
        AugmentManager augment = AugmentManager.Instance;
        
        bool isPlayer = gameObject.layer == LayerMask.NameToLayer("Player");

        return new StatStruct
        {
            MaxHealth = isPlayer ? augment.MaxHealth.Value : 0,

            AttackSpeed = isPlayer ? augment.AttackSpeed.Value : 0f,

            PhysicalDamage = isPlayer ? augment.PhysicalDamage.Value : 0,
            MagicDamage = isPlayer ? augment.MagicDamage.Value : 0,

            PhysicalDefense = isPlayer ? augment.PhysicalDefense.Value : 0,
            MagicDefense = isPlayer ? augment.MagicDefense.Value : 0,

            CritChance = isPlayer ? augment.CritChance.Value : 0,
        };
    }
    #endregion

    public void Refresh()
    {
        ClearAllStat();
        PassiveController.RefreshBaseStats();
        IsDead = false;
    }

    public void ClearAllStat()
    {
        // 모든 스탯의 모디파이어 제거
        MaxHealth.ClearModifiers();
        MaxMana.ClearModifiers();
        ManaGain.ClearModifiers();

        AttackSpeed.ClearModifiers();
        MoveSpeed.ClearModifiers();

        PhysicalDamage.ClearModifiers();
        MagicDamage.ClearModifiers();

        CritChance.ClearModifiers();

        PhysicalDefense.ClearModifiers();
        MagicDefense.ClearModifiers();

        AttackRange.ClearModifiers();
        AttackCount.ClearModifiers();

        EffectController?.ClearAllEffects();

        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();

        SetBaseStat(Status.GetCurrentStat());
    }
    #endregion

    #region TakeDamage
    public void TakeDamage(int amount, bool isCrit = false)
    {
        if (IsDead || IsInvincible)
            return;

        if (Shield.Value > 0)
        {
            if (amount > Shield.Value)
            {
                amount -= Shield.Value;
                Shield.Value = 0;
            }
            else
            {
                Shield.Value -= amount;
                return;
            }
        }

        CurHp.Value = Mathf.Clamp(CurHp.Value - amount, 0, int.MaxValue);
        UnitFXController?.Flash();

        if (CurHp.Value <= 0)
        {
            Die();
        }
    }

    public void TakeTickDamage(int amount, float tickCount, float tickInterval)
    {
        TickDamage(amount, tickCount, tickInterval).Forget();
    }

    private async UniTask TickDamage(int amount, float tickCount, float tickInterval)
    {
        var destroyToken = this.GetCancellationTokenOnDestroy();
        int count = 0;

        while (tickCount > count)
        {
            count++;
            if (IsDead)
                return;

            TakeDamage(amount);

            try
            {
                await UniTask.WaitForSeconds(tickInterval, cancellationToken: destroyToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
    #endregion

    #region Increase
    public void IncreaseHealth(int amount, bool isEffect = true)
    {
        if (amount < 0)
        {
            TakeDamage(amount);
            return;
        }

        CurHp.Value += amount;

        if(isEffect)
            EffectController.AddBuffEffect(BuffEffect.Heal);

        Debug.Log($"[체력회복] {name} {amount} 만큼 체력 회복");
        if (CurHp.Value > MaxHealth.Value)
        {
            CurHp.Value = MaxHealth.Value;
        }
    }

    public void IncreaseMana(int amount)
    {
        CurMana.Value += amount;        

        if (CurMana.Value > MaxMana.Value)
        {
            CurMana.Value = MaxMana.Value;
        }
    }

    public void GetMana()
    {        
        IncreaseMana(ManaGain.Value);
    }

    public void IncreaseShield(int amount)
    {
        Debug.Log($"[쉴드추가] {name} : {amount} 만큼 쉴드추가");

        int prevShield = Shield.Value;
        Shield.Value += amount;

        if (Shield.Value < 0)
            Shield.Value = 0;

        if (prevShield <= 0 && Shield.Value > 0)
        {
            EffectController.StartShieldEffect();
        }
        else if (prevShield > 0 && Shield.Value <= 0)
        {
            EffectController.StopShieldEffect();
        }
    }
    #endregion

    #region Stun
    public void Stun(float stunDuration)
    {
        StunDelay(stunDuration).Forget();
    }

    private async UniTask StunDelay(float stunTime)
    {
        IsStunned.Value = true;

        await UniTask.WaitForSeconds(stunTime, cancellationToken: _cts.Token);

        if (IsDead)
            return;

        IsStunned.Value = false;
    }
    #endregion

    #region Invincible
    public void Invincible(float duration = 0.2f)
    {
        InvincibleActive(duration).Forget();
    }
    #endregion

    private async UniTask InvincibleActive(float duration)
    {
        IsInvincible = true;

        await UniTask.WaitForSeconds(duration, cancellationToken: _cts.Token);

        IsInvincible = false;
    }

    private void Die()
    {
        EffectController.ClearAllEffects();
        _cts.Cancel();
        IsDead = true;
        OnDied?.Invoke();
    }

    public void AttackDataChange(UnitAttackData attackData, float _duration)
    {
        EffectController.AddBuffEffect(BuffEffect.Buff);

        ChangeAttackData(attackData, _duration).Forget();
    }

    private async UniTask ChangeAttackData(UnitAttackData attackData, float _duration)
    {
        CurrentAttackData = attackData;

        await UniTask.WaitForSeconds(_duration, cancellationToken: this.GetCancellationTokenOnDestroy());

        CurrentAttackData = Status.Data.AttackData;
    }

    #region Effect
    public void ApplyEffect(BuffEffectData buffEffectData, float value, string source)
    {
        var key = new SourceKey(buffEffectData.StatType, source);

        if (buffEffectData.IsTicking)
        {
            TickEffect(buffEffectData, value, source).Forget();
            Debug.Log($"[스텟 이펙트] {buffEffectData.StatType.ToString()}이 {buffEffectData.Duration} 동안 {buffEffectData.TickInterval} 마다 발동");
            return;
        }

        Debug.Log($"[스텟 이펙트] {buffEffectData.StatType.ToString()}이 {buffEffectData.Duration} 동안 발동");

        AddStat(buffEffectData.StatType, value, source);
        ClearEffectAsync(buffEffectData, value, source, _cts.Token).Forget();
    }

    private async UniTaskVoid ClearEffectAsync(BuffEffectData buffEffectData, float value, string source, CancellationToken token)
    {
        try
        {
            await UniTask.WaitForSeconds(buffEffectData.Duration, cancellationToken: token);

            RemoveStat(buffEffectData.StatType, source, value);
        }
        catch (OperationCanceledException)
        {
            // 갱신으로 취소된 경우 RemoveStat 안 함
        }
    }

    private async UniTask TickEffect(BuffEffectData buffEffectData, float value, string source)
    {
        int count = (int)(buffEffectData.Duration / buffEffectData.TickInterval);

        switch(buffEffectData.StatType)
        {
            case StatType.PhysicalDamage:
            case StatType.MagicDamage:
                EffectController.AddBuffEffect(BuffEffect.Damage, buffEffectData.Duration);
                break;
            case StatType.PhysicalDefense:
            case StatType.MagicDefense:
                EffectController.AddBuffEffect(BuffEffect.Defense, buffEffectData.Duration);
                break;
            case StatType.AttackSpeed:
                EffectController.AddBuffEffect(BuffEffect.AttackSpeed, buffEffectData.Duration);
                break;
        }

        for (int i = 0; i < count; i++)
        {
            AddStat(buffEffectData.StatType, value, source);
            await UniTask.WaitForSeconds(buffEffectData.TickInterval, cancellationToken: _cts.Token);
        }
    }
    #endregion

    #region Stat Management

    public void AddStat(StatType statType, float value, string source)
    {
        switch (statType)
        {
            case StatType.MaxHealth:
            case StatType.MaxMana:
            case StatType.ManaGain:
            case StatType.PhysicalDamage:
            case StatType.MagicDamage:
            case StatType.CritChance:
            case StatType.PhysicalDefense:
            case StatType.MagicDefense:
            case StatType.AttackSpeed:
                {
                    if (value > 0)
                        EffectController.AddBuffEffect(BuffEffect.Buff);
                    else
                        EffectController.AddBuffEffect(BuffEffect.Debuff);

                    switch (statType)
                    {
                        case StatType.MaxHealth:
                            MaxHealth.AddModifier((int)value, source);
                            IncreaseHealth((int)value, false);
                            break;
                        case StatType.MaxMana:
                            MaxMana.AddModifier((int)value, source);
                            break;
                        case StatType.ManaGain:
                            ManaGain.AddModifier((int)value, source);
                            break;
                        case StatType.PhysicalDamage:
                            PhysicalDamage.AddModifier((int)value, source);
                            break;
                        case StatType.MagicDamage:
                            MagicDamage.AddModifier((int)value, source);
                            break;
                        case StatType.CritChance:
                            CritChance.AddModifier((int)value, source);
                            break;
                        case StatType.PhysicalDefense:
                            PhysicalDefense.AddModifier((int)value, source);
                            break;
                        case StatType.MagicDefense:
                            MagicDefense.AddModifier((int)value, source);
                            break;
                        case StatType.AttackSpeed:
                            AttackSpeed.AddModifier(value, source);
                            break;
                    }
                }
                break;

            case StatType.CurHp:
                IncreaseHealth((int)value);
                break;
            case StatType.CurMana:
                IncreaseMana((int)value);
                break;
            case StatType.Shield:
                IncreaseShield((int)value);
                break;
        }
    }


    public void RemoveStat(StatType statType, string source, float value = 0)
    {
        switch (statType)
        {
            case StatType.MaxHealth:
                MaxHealth.RemoveModifier(source);
                break;
            case StatType.MaxMana:
                MaxMana.RemoveModifier(source);
                break;
            case StatType.ManaGain:
                ManaGain.RemoveModifier(source);
                break;
            case StatType.PhysicalDamage:
                PhysicalDamage.RemoveModifier(source);
                break;
            case StatType.MagicDamage:
                MagicDamage.RemoveModifier(source);
                break;
            case StatType.CritChance:
                CritChance.RemoveModifier(source);
                break;
            case StatType.PhysicalDefense:
                PhysicalDefense.RemoveModifier(source);
                break;
            case StatType.MagicDefense:
                MagicDefense.RemoveModifier(source);
                break;
            case StatType.AttackSpeed:
                AttackSpeed.RemoveModifier(source);
                break;
            case StatType.Shield:
                IncreaseShield(-(int)value);
                break;                
        }
    }
    #endregion
}

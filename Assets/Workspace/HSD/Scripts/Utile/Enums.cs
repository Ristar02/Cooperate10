using System;

#region Synergy
public enum ClassType
{
    TANK = 0, MELEE, RANGED, SUPPORT
}

public enum Synergy
{
    KINGDOM = 4,    // 왕국 경비대
    FOREST,         // 숲의 순찰자
    WIZARD,         // 고위 마법사단
    HOLY,           // 신성 교단
    SHADOW,         // 밤거리 암살단
    BANDIT,         // 지하 조직
    Length
}
#endregion

public enum SortingType
{
    Grade,
    Power
}

public enum Grade
{
    NORMAL, RARE, UNIQUE, LEGEND
}

public enum SubGrade
{
    SILVER, GOLD, PRISM
}

#region Type
public enum BuffEffect
{
    Shield,
    Stun,
    Buff,
    Debuff,
    Heal,
    Damage,
    AttackSpeed,
    Defense
}

public enum AnimatorType
{

}

public enum ActivationCondition
{    
    AnyCollision,  // 도중에 맞은 모든 충돌 대상 발동
    TargetOnly,    // 목표(Target)에 도달했을 때만 발동
    Target         // 처음 타겟 위치에서 발동
}
public enum ThrowType
{
    Parabola,
    Straight
}

public enum SpawnPositionType
{
    Self,
    Target
}

public enum UnitMultiplierType
{
    None,
    UnitLevel,
}

public enum PopupType
{
    PhysicalDamage,
    MagicDamage,
    Heal,
    Mana,
    Buff,
    Debuff,    
    Crit
}

public enum MagicStoneSearchType
{
    All,
    LowHp,
    Random
}

/// <summary>
/// 유닛 레벨에 따른 가중치 곱하기 or 1단계 아래 레벨
/// </summary>
public enum SpawnStatType
{
    Level,
    LowUpgrade
}

/// <summary>
/// 이벤트 트리거 타입
/// </summary>
public enum TriggerType
{
    Base,
    OnBattleStart,
    OnDied,
    OnEnemyDied,
    OnAttack,
    OnUseSkill,
    OnInterval,
    OnBattleEnded,
}

/// <summary>
/// 효과 대상 타입
/// </summary>
public enum EffectTargetType
{
    Enemy,
    Ally,
    SameSynergy,
    SameClassType,
    Column,
    Row,
    ColumnAndRow,
    Cross,
    Leader
}

/// <summary>
/// 버프디버프, 즉시증가
/// </summary>
public enum EffectType
{
    Buff_Debuff,
    Increase,
    Recover,
    Currency
}

/// <summary>
/// 효과 공격 타입, 각자공격 or 전체공격
/// </summary>
public enum EffectApplyType
{
    Self,
    All,    
}

public enum TargetType
{
    Enemy,
    Ally,
    Self,
    Boss,
    RandomEnemy
}

public enum EffectTime
{
    Permanent,
    Temporary
}

public enum AttackAreaType
{
    Single, Radius, Pierce, Row
}

public enum DamageType
{
    Physical, Magic
}

public enum SearchType
{
    Circle, Box, Capsule, Sector
}

public enum EffectSpawnType
{
    OwnerFront,
    Target
}

#endregion

public enum AutoUnitType
{
    Unit,
    Slot
}

public enum Priority
{
    None,
    Target,
    TargetRadius,
    Close,
    Far,
    LowHp,
    HightHp,
    Tank,
    Melee,
    Ranged,
    Support
}

public enum StatType
{
    // Status
    MaxHealth,
    MaxMana,
    ManaGain,
    AttackSpeed,
    MoveSpeed,

    // Damage
    PhysicalDamage,
    MagicDamage,

    // Crit
    CritChance,
    CritDamage,

    // Defense
    PhysicalDefense,
    MagicDefense,
    Shield,

    // Range
    AttackRange,
    AttackCount,

    // Default
    CurHp,
    CurMana,

    // Currency
    GoldBonus,

    TakeDamage,
}
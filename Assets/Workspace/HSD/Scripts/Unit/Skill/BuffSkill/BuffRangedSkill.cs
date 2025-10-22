using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffRangedSkill", menuName = "Data/Unit/Skill/BuffThrow")]
public class BuffRangedSkill : RangedSkill
{
    [Header("Type")]
    [SerializeField] ActivationCondition _activationCondition;
    [SerializeField] ThrowType _throwType;    
    [SerializeField] float _parabolaYOffset;
    [SerializeField] bool _isAttack;

    [Header("Buff")]
    [SerializeField] bool _isModifier;
    [SerializeField] bool _isBuff;
    [SerializeField] bool _isAlly;
    [SerializeField] float _radius;
    [SerializeField] BuffEffectData _buffEffectData;
    [SerializeField] StatEffectModifier _statModifier;
    
    public override void Active(IAttacker attacker)
    {
        GameObject spawnObject = Manager.Resources.Load<GameObject>(EffectAddress);

        SplashBuffProjectile projectile = ComponentProvider.Get<SplashBuffProjectile>(
            Manager.Resources.Instantiate<GameObject>(
                spawnObject,
                GetSpawnPoint(attacker),
                true
                )
            );

        Transform target = GetTarget(attacker);

        if (target == null || projectile == null)
            return;

        projectile.transform.right = (target.position - projectile.transform.position).normalized;
        projectile.transform.Rotate(0, 0, spawnObject.transform.rotation.eulerAngles.z);

        if (target == null)
        {
            target = Utils.GetClosestTargetNonAlloc(GetSpawnPoint(attacker), 10f, GetLayerMask(attacker));
        }

        if (_isBuff)
        {
            projectile.Init(_buffEffectData, _radius, _throwType, _activationCondition, _isAttack, _isModifier,
            target, attacker.GetStatusController(), PhysicalPower, AbilityPower, DamageType, GetLayerMask(attacker), _projectileSpeed, GetExplosionEffect(),
            _parabolaYOffset);
        }
        else if (!_isBuff)
        {
            projectile.Init(_statModifier, _radius, _throwType, _activationCondition, _isAttack, _isModifier,
            target, attacker.GetStatusController(), PhysicalPower, AbilityPower, DamageType, GetLayerMask(attacker), _projectileSpeed, GetExplosionEffect(),
            _parabolaYOffset);
        }
    }

    protected override GameObject GetTargetSingle(IAttacker attacker)
    {
        var target = Utils.GetTargetsNonAllocSingle(attacker, SearchType.Circle, 100, Vector2.zero, 1, GetLayerMask(attacker), GetPriorityFilter());
        return target;
    }

    private LayerMask GetLayerMask(IAttacker attacker)
    {
        if (_isAlly)
        {
            return attacker.GetAllyLayerMask();
        }
        else
            return attacker.TargetLayer;
    }

    protected Transform GetTarget(IAttacker attacker)
    {
        if (Priority == Priority.None)
            return null;

        if (Priority == Priority.Target)
        {
            if (attacker.GetTarget() == null)
            {
                Debug.Log("[스킬] 타켓이 없습니다.");
                
                return Utils.GetClosestTargetNonAlloc(attacker.GetCenter(), 10, attacker.TargetLayer)?.transform;
            }

            return attacker.GetTarget();
        }
        else
        {
            return GetTargetSingle(attacker)?.transform;
        }
    }

    public override string GetCalculateValueString(UnitStatus status)
    {
        UnitStats stat = status.GetCurrentStat();
        float value = stat.MagicDamage * (AbilityPower / 100);

        if(_isBuff)
        {
            if (_buffEffectData.StatType == StatType.CurHp || _buffEffectData.StatType == StatType.Shield)
                return Mathf.RoundToInt(value).ToString();
            else
                return value.ToString("F1");
        }        
        else
        {
            if (_statModifier.StatType == StatType.CurHp || _statModifier.StatType == StatType.Shield)
                return Mathf.RoundToInt(value).ToString();
            else
                return value.ToString("F1");
        }
    }

#if UNITY_EDITOR
    public override void DrawGizmos(IAttacker attacker)
    {
        base.DrawGizmos(attacker);

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(attacker.GetCenter(), _radius);
    }
#endif
}

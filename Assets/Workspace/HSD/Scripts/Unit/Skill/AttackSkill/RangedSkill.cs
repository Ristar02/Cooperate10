using UnityEngine;


[CreateAssetMenu(fileName = "RangedSkill", menuName = "Data/Unit/Skill/Ranged")]
public class RangedSkill : AttackSkill
{   
    [SerializeField] protected float _projectileSpeed = 10f;
    [SerializeField] protected string _explosionEffect;
    [SerializeField] float _distance = 5f;

    public override void Active(IAttacker attacker)
    {
        Projectile projectile = ComponentProvider.Get<Projectile>(
            Manager.Resources.Instantiate<GameObject>(
                EffectAddress,
                GetSpawnPoint(attacker),
                true
                )
            );

        projectile.transform.localScale = attacker.GetTransform().localScale;

        Transform target = GetTarget(attacker);

        if (target == null)
        {
            target = Utils.GetClosestTargetNonAlloc(GetSpawnPoint(attacker), 100f, attacker.TargetLayer);
        }

        projectile.Init(target, attacker.GetStatusController(), 
            PhysicalPower, AbilityPower, DamageType, attacker.TargetLayer, _projectileSpeed,
            GetExplosionEffect(), _distance);
    }

    protected GameObject GetExplosionEffect()
    {        
        return Manager.Resources.Load<GameObject>(_explosionEffect);
    }

    protected override GameObject GetTargetSingle(IAttacker attacker)
    {
        var target = Utils.GetTargetsNonAllocSingle(attacker, SearchType.Circle, 100, Vector2.zero, 1, attacker.TargetLayer, GetPriorityFilter());
        return target;
    }

    private Transform GetTarget(IAttacker attacker)
    {
        if (Priority == Priority.None)
            return null;

        if (Priority == Priority.Target)
        {
            if (attacker.GetTarget() == null)
            {
                Debug.Log("[스킬] 타켓이 없습니다.");
                return null;
            }

            return attacker.GetTarget();
        }
        else
        {
            return GetTargetSingle(attacker)?.transform;
        }       
    }    
}

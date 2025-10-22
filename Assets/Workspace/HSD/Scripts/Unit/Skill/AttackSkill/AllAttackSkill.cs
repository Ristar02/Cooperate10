using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllAttackSkill", menuName = "Data/Unit/Skill/AllAttack")]
public class AllAttackSkill : AttackSkill
{
    private const string ALL_ATTACKER = "AllAttacker";
    [Header("All Attack")]
    [SerializeField] string _explosionEffect;
    [SerializeField] float _speed;

    public override void Active(IAttacker attacker)
    {
        Manager.Resources.Instantiate<GameObject>(ALL_ATTACKER, GetSpawnPoint(attacker), true).GetComponent<AllAttacker>().
            Setup(attacker, Manager.Resources.Load<GameObject>(EffectAddress), Manager.Resources.Load<GameObject>(_explosionEffect), PhysicalPower, AbilityPower, DamageType, _speed);
    }
}

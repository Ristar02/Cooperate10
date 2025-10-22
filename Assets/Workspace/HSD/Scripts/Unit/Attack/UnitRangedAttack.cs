using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RangedAttack", menuName = "Data/Unit/Attack/Ranged")]
public class UnitRangedAttack : UnitAttackData
{
    [SerializeField] protected string _projectileAddress;
    [SerializeField] protected float _projectileSpeed = 10f;

    [Header("Offset")]
    public Vector2 AttackPointOffset;

    public override void Attack(IAttacker attacker)
    {
        base.Attack(attacker);

        Vector2 offset = AttackPointOffset;
        offset.x *= attacker.GetTransform().GetFacingDir();

        Vector2 spawnPoint = (Vector2)attacker.GetTransform().position + offset;

        GameObject obj = Manager.Resources.Instantiate<GameObject>(_projectileAddress, spawnPoint, true);
        Projectile projectile = ComponentProvider.Get<Projectile>(obj);

        attacker.GetStatusController().GetMana();

        //projectile.Init(
        //    attacker.GetTarget(),
        //    attacker.GetStatusController(),
        //    AttackPower,            
        //    DamageType,
        //    attacker.TargetLayer,
        //    _projectileSpeed
        //    );
    }
}

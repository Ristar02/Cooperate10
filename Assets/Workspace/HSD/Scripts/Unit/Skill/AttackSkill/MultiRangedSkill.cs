using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MultiRangedSkill", menuName = "Data/Unit/Skill/MultiRanged")]
public class MultiRangedSkill : RangedSkill
{
    [SerializeField] float _interval;
    [SerializeField] float _count;
    [SerializeField] string _address;

    public override void Active(IAttacker attacker)
    {
        SpawnEffect(attacker);

        MultiRangedAttack(attacker).Forget();
    }

    private async UniTask MultiRangedAttack(IAttacker attacker)
    {
        for (int i = 0; i < _count; i++)
        {
            await UniTask.WaitForSeconds(_interval);
            GameObject obj = Manager.Resources.Instantiate<GameObject>(_address, GetSpawnPoint(attacker), true);
            Projectile projectile = ComponentProvider.Get<Projectile>(obj);
            projectile.Init(attacker.GetTarget(), attacker.GetStatusController(), PhysicalPower, AbilityPower, DamageType, attacker.TargetLayer, _projectileSpeed);
        }
    }
}

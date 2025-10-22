using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class BasicProjectile : Projectile
{
    public override void Init(Transform target, UnitStatusController status, float attackPower, float abilityPower, DamageType damageType,
        LayerMask targetLayer, float speed, GameObject effect, float distance)
    {
        base.Init(target, status, attackPower, abilityPower, damageType, targetLayer, speed);
        transform.right = _targetDir;
    }

    protected override async UniTask MoveAndDestroy(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {            
            transform.Translate(_targetDir * _speed * Time.deltaTime, Space.World);
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, _source.Token);
        }

        ProjectileDestroy();
    }
}

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class HoverProjectile : Projectile
{
    private bool isHovering = true;

    public override void Init(Transform target, UnitStatusController status, float attackPower, float abilityPower, DamageType damageType,
        LayerMask targetLayer, float speed, GameObject effect, float distancez)
    {
        base.Init(target, status, attackPower, abilityPower, damageType, targetLayer, speed);

    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if(_targetLayer.Contain(collision.gameObject.layer))
        {
            isHovering = false; // 충돌 시 Hover 상태 해제
        }

        base.OnTriggerEnter2D(collision);
    }

    protected override async UniTask MoveAndDestroy(float duration)
    {
        float elapsed = 0f;

        // 발사체가 파괴되면 자동 취소되도록 CancellationToken 연결
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        while (elapsed < duration && !token.IsCancellationRequested && isHovering)
        {
            transform.Translate(_targetDir * _speed * Time.deltaTime, Space.World);
            transform.right = _targetDir;
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token); // 프레임마다 대기
        }

        if (!token.IsCancellationRequested)
            Destroy(gameObject);
    }
}

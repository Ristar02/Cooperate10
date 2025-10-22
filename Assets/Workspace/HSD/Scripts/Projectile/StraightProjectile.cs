using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightProjectile : Projectile
{
    enum StraightType { Facing, Up, Down, Left, Right, Target}

    [Header("Straight")]
    [SerializeField] StraightType _straightType;

    protected override async UniTask MoveAndDestroy(float duration)
    {
        SetDirection();

        await transform.DOMove((Vector2)transform.position + (_dir * _distance), _speed)
            .SetEase(Ease.Linear)
            .SetSpeedBased()
            .AsyncWaitForCompletion();

        ProjectileDestroy();
    }

    private void SetDirection()
    {
        switch (_straightType)
        {
            case StraightType.Target:
                _dir = GetTargetDir();
                break;
            case StraightType.Facing:
                _dir = new Vector2(_status.transform.GetFacingDir(), 0);
                break;
            case StraightType.Up:
                _dir = new Vector2(0,1);
                break;
            case StraightType.Down:
                _dir = new Vector2(0, -1);
                break;
            case StraightType.Left:
                _dir = new Vector2(-1,0);
                break;
            case StraightType.Right:
                _dir = new Vector2(1, 0);
                break;
        }
    }
}

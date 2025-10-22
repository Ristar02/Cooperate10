using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AllAttacker : MonoBehaviour
{
    private IAttacker _attacker;    
    private UnitStatusController _status;
    private GameObject _attackEffect;
    private GameObject _explosionEffect;
    private DamageType _damageType;
    private LayerMask _targetLayer;
    private float _physicalPower;
    private float _abilityPower;
    private float _speed;
    private Collider2D[] _objs = new Collider2D[20];
    
    public void Setup(IAttacker attacker, GameObject attackEffect, GameObject explosionEffect, float physicalPower, float abilityPower, DamageType damageType, float speed)
    {
        _attacker = attacker;
        _targetLayer = _attacker.TargetLayer;
        _speed = speed;
        _status = _attacker.GetStatusController();
        _attackEffect = attackEffect;
        _explosionEffect = explosionEffect;
        _physicalPower = physicalPower;
        _abilityPower = abilityPower;

        AttackAll();
    }

    private void AttackAll()
    {
        for (int i = 0; i < Physics2D.OverlapCircleNonAlloc(transform.position, 10, _objs, _targetLayer); i++)
        {
            MoveToEnemy(_objs[i].gameObject).Forget();
        }

        Manager.Resources.Destroy(gameObject, 2);
    }

    private async UniTask MoveToEnemy(GameObject target)
    {
        await UniTask.Yield();
        Vector3 targetPos = target.GetCenterPosition();

        GameObject attack = Manager.Resources.Instantiate(_attackEffect, transform.position, true);
        Vector2 dir = (targetPos - attack.transform.position).normalized;
        attack.transform.right = dir;
        attack.transform.Rotate(0f, 0f, _attackEffect.transform.rotation.eulerAngles.z);

        await UniTask.Yield();
        await attack.transform.DOMove(target.GetCenterPosition(), _speed)
            .SetSpeedBased()
            .SetEase(Ease.Linear)
            .AsyncWaitForCompletion();

        _status.CalculateDamage(_physicalPower, _abilityPower, _damageType, ComponentProvider.Get<UnitBase>(target).StatusController);
        Manager.Resources.Destroy(attack);

        GameObject effect = Manager.Resources.Instantiate(_explosionEffect, targetPos, true);
        effect.transform.right = dir;
        effect.transform.Rotate(0f, 0f, _explosionEffect.transform.rotation.eulerAngles.z);

        Manager.Resources.Destroy(effect, 2);
    }
}

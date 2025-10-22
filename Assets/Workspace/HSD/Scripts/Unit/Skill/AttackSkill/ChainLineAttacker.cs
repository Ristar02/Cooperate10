using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChainLineAttacker : MonoBehaviour
{
    private CancellationTokenSource _source = new();
    private Transform _target;
    private GameObject _effect;
    private float _interval;
    private float _physicalPower;
    private float _abilityPower;
    private int _count;
    private int _ratio;
    private DamageType _damageType;
    private LayerMask _targetLayer;
    private Vector2 _currentPos;
    private IAttacker _attacker;

    private List<Transform> _targetList = new List<Transform>(5);
    private Collider2D[] _overlapResults = new Collider2D[32];
    private float _attackThickness = 1f;

    private void OnDisable()
    {
        Dispose();
    }

    private void Dispose()
    {
        if (_source != null)
        {
            _source.Cancel();
            _source.Dispose();
            _source = null;
        }
    }


    public void Setup(IAttacker attacker, GameObject effect, Transform target, int count, float interval, 
        LayerMask targetLayer, float physicalPower, float abilityPower, DamageType damageType, float attackThickness, int ratio)
    {
        _count = count;
        _target = target;
        _effect = effect;
        _interval = interval;
        _damageType = damageType;
        _physicalPower = physicalPower;
        _abilityPower = abilityPower;
        _targetLayer = targetLayer;
        _attacker = attacker;
        _ratio = ratio;
        _attackThickness = attackThickness;

        _targetList.Clear();
        _currentPos = transform.position;

        if (_source == null)
            _source = new();

        ChainLineAttack().Forget();
    }

    private async UniTask ChainLineAttack()
    {
        for (int i = 0; i < _count; i++)
        {
            if (_target == null)
            {
                Manager.Resources.Destroy(gameObject);
                Dispose();
                return;
            }

            SpawnEffect();
            ChangeTarget();

            await UniTask.WaitForSeconds(_interval, cancellationToken: _source.Token);
        }

        Manager.Resources.Destroy(gameObject);
    }
    
    private void SpawnEffect()
    {
        Vector2 spawnPos = GetSpawnPosition(_target.position);

        GameObject effect = Manager.Resources.Instantiate(_effect, spawnPos, true);
        effect.transform.right = (_target.position - effect.transform.position).normalized;

        Vector3 scale = effect.transform.localScale;
        float scaleX = Vector2.Distance(_target.position, _currentPos);
        scale.x = scaleX / _ratio;
        effect.transform.localScale = scale;

        float boxWidth = scaleX;
        float boxHeight = _attackThickness;
        float angle = effect.transform.eulerAngles.z;

        AttackBox(spawnPos, new Vector2(boxWidth, boxHeight), angle);

        Manager.Resources.Destroy(effect, 2);
    }

    private void ChangeTarget()
    {
        _targetList.Add(_target);
        _currentPos = ComponentProvider.Get<UnitBase>(_target.gameObject).GetCenter();
        _target = Utils.GetClosestTargetNonAlloc(_currentPos, 100, _targetLayer, Filter);        
    }

    private void AttackBox(Vector2 center, Vector2 size, float angleDegrees)
    {
        int layerMask = _targetLayer;

        int hitCount = Physics2D.OverlapBoxNonAlloc(center, size, angleDegrees, _overlapResults, layerMask);

        for (int i = 0; i < hitCount; i++)
        {
            var col = _overlapResults[i];
            if (col == null) continue;

            _attacker.GetStatusController().CalculateDamage(_physicalPower, _abilityPower, _damageType, ComponentProvider.Get<UnitBase>(col.gameObject).StatusController);
        }

        for (int i = hitCount; i < _overlapResults.Length; i++)
            _overlapResults[i] = null;
    }

    private Vector2 GetSpawnPosition(Vector2 targetPosition)
    {
        return (_currentPos + targetPosition) / 2;
    }

    private bool Filter(Transform target)
    {
        return !ComponentProvider.Get<UnitBase>(target.gameObject).StatusController.IsDead && _targetList.Contains(target);
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_target == null) return;

        Vector2 spawn = GetSpawnPosition(_target.position);
        float width = Vector2.Distance(_target.position, _currentPos);
        Vector2 size = new Vector2(width, _attackThickness);

        Vector2 dir = (_target.position - (Vector3)_currentPos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(spawn, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
#endif
}

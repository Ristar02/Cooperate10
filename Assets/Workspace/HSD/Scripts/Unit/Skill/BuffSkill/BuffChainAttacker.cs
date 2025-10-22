using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BuffChainAttacker : MonoBehaviour
{
    private Transform _target;
    private GameObject _effect;
    private int _count;
    private float _abilityPower;
    private float _interval;
    private Vector2 _currentPos;
    private LayerMask _targetLayer;
    private BuffEffectData _buffEffectData;
    private List<Transform> _targetList = new List<Transform>(5);
    private Collider2D[] _overlapResults = new Collider2D[32];
    private CancellationTokenSource _source = new();
    private IAttacker _attacker;

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

    public void Setup(BuffEffectData buffEffectData, IAttacker attacker, GameObject effect, Transform target, int count, float interval, LayerMask targetLayer, float abilityPower)
    {
        _effect = effect;
        _count = count;
        _interval = interval;
        _abilityPower = abilityPower;
        _targetLayer = targetLayer;
        _attacker = attacker;
        _buffEffectData = buffEffectData;
        _target = target;

        _targetList.Clear();
        _currentPos = transform.position;

        if (_source == null)
            _source = new();

        ChainLineBuffProvide().Forget();
    }

    private async UniTask ChainLineBuffProvide()
    {
        for (int i = 0; i < _count; i++)
        {
            if (_target == null)
            {
                Debug.LogWarning($"[Buff Chain Provider] 타겟을 찾을 수 없습니다.");
                Manager.Resources.Destroy(gameObject);
                Dispose();
                return;
            }

            SpawnEffect();

            _attacker.GetStatusController().ProvideEffect(_buffEffectData, _abilityPower,
                _attacker.GetTransform().name, ComponentProvider.Get<UnitBase>(_target.gameObject).StatusController);

            ChangeTarget();

            await UniTask.WaitForSeconds(_interval, cancellationToken: _source.Token);
        }

        Manager.Resources.Destroy(gameObject);
    }

    private void SpawnEffect()
    {
        Manager.Resources.Destroy(Manager.Resources.Instantiate(_effect, _currentPos, true));
    }

    private void ChangeTarget()
    {
        _targetList.Add(_target);
        _currentPos = ComponentProvider.Get<UnitBase>(_target.gameObject).GetCenter();
        _target = Utils.GetClosestTargetNonAlloc(_currentPos, 100, _targetLayer, Filter);
    }

    private bool Filter(Transform target)
    {
        return !ComponentProvider.Get<UnitBase>(target.gameObject).StatusController.IsDead && _targetList.Contains(target);
    }
}

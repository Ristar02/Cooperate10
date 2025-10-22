using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] protected float _lifeTime = 5f; // 발사체의 생명 시간
    [SerializeField] protected bool _isPirece = true;
    protected int _pireceCount;
    protected float _physicalPower;
    protected float _abilityPower;
    protected DamageType _damageType;
    protected LayerMask _targetLayer;
    protected Transform _target;
    protected UnitStatusController _status;
    protected float _speed;
    protected float _distance;
    protected Vector2 _targetDir => GetTargetDir();
    protected Vector2 _dir;
    protected GameObject _effect;

    protected CancellationTokenSource _source = new();

    protected virtual void Awake()
    {
        ComponentProvider.Add(gameObject, this);
    }

    protected virtual void OnDestroy()
    {
        ComponentProvider.Remove<Projectile>(gameObject);
    }

    private void OnEnable()
    {
        if (_source != null)
            _source.Dispose();

        _source = new();
    }

    private void OnDisable()
    {        
        _source.Cancel();
    }    

    public virtual void Init(Transform target, UnitStatusController status, float physicalPower, float abilityPower, DamageType damageType,
        LayerMask targetLayer, float speed, GameObject effect = null, float distance = 0)
    {        
        _status = status;
        _target = target;
        _targetLayer = targetLayer;
        _damageType = damageType;
        _pireceCount = status.AttackCount.Value;
        _physicalPower = physicalPower;
        _abilityPower = abilityPower;
        _speed = speed;
        _distance = distance;
        _effect = effect;

        if (target == null)
            Manager.Resources.Destroy(gameObject);

        MoveAsync().Forget();
    }
    private async UniTask MoveAsync()
    {
        await UniTask.Yield();
        await MoveAndDestroy(_lifeTime);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (_targetLayer.Contain(collision.gameObject.layer))
        {
            _status.CalculateDamage(_physicalPower, _abilityPower, _damageType, ComponentProvider.Get<UnitBase>(collision.gameObject).StatusController);

            if (!_isPirece)
            {
                _pireceCount--;

                if (_pireceCount <= 0)
                {
                    ProjectileDestroy();
                }
            }            
        }
    }

    protected virtual async UniTask MoveAndDestroy(float duration)
    {
        await UniTask.Yield();
    }

    protected GameObject SpawnEffect()
    {
        if (_effect == null) return null;

        GameObject obj = Manager.Resources.Instantiate(_effect, transform.position, true);
        Manager.Resources.Destroy(obj, 2);

        return obj;
    }

    protected Vector2 GetTargetDir()
    {
        if (_target == null) 
            return new Vector2(_status.transform.GetFacingDir(), 0);

        return (_target.position - transform.position).normalized;
    }

    protected void ProjectileDestroy()
    {
        Manager.Resources.Destroy(gameObject);
    }
}

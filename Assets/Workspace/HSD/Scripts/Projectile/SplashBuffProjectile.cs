using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SplashBuffProjectile : Projectile
{
    [Header("Stat")]
    [SerializeField] ThrowType _throwType;
    [SerializeField] StatEffectModifier _statEffectModifier;
    [SerializeField] BuffEffectData _buffEffectData;
    [SerializeField] bool _isRotate;
    [SerializeField] float _rotSpeed;
    private float _radius;
    private bool _isModifier;
    private bool _isBuff;
    private bool _isAttck;
    private ActivationCondition _activationCondition;

    private UnitStatusController _targetStatus;

    protected override void Awake()
    {
        ComponentProvider.Add(gameObject, this);
    }

    protected override void OnDestroy()
    {
        ComponentProvider.Remove<SplashBuffProjectile>(gameObject);        
    }
    private void Update()
    {
        if (_isRotate)
        {
            transform.Rotate(Vector3.forward, _rotSpeed * Time.deltaTime);
        }
    }

    public void Init(StatEffectModifier statEffectModifier, float radius, ThrowType _throwType, ActivationCondition activationCondition, bool isAttck,
        bool isModifier, Transform target, UnitStatusController status, float attackPower, float abilityPower, DamageType damageType, LayerMask targetLayer, float speed, 
        GameObject effect, float distance = 0)
    {
        _statEffectModifier = statEffectModifier;
        _radius = radius;
        _activationCondition = activationCondition;
        _isAttck = isAttck;
        _isModifier = isModifier;
        _isBuff = false;

        base.Init(target, status, attackPower, abilityPower, damageType, targetLayer, speed, effect, distance);
    }

    public void Init(BuffEffectData buffEffectData, float radius, ThrowType _throwType, ActivationCondition activationCondition, bool isAttck,
        bool isModifier, Transform target, UnitStatusController status, float attackPower, float abilityPower, DamageType damageType, LayerMask targetLayer, float speed,
        GameObject effect, float distance = 0)
    {
        _buffEffectData = buffEffectData;
        _radius = radius;
        _activationCondition = activationCondition;
        _isAttck = isAttck;
        _isModifier = isModifier;
        _isBuff = true;

        base.Init(target, status, attackPower, abilityPower, damageType, targetLayer, speed, effect, distance);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (_activationCondition == ActivationCondition.Target)
            return;

        if(_activationCondition == ActivationCondition.AnyCollision)
        {
            if (_targetLayer.Contain(collision.gameObject.layer))
            {
                ApplyEffect(collision.gameObject);
            }
        }
        else if (_activationCondition == ActivationCondition.TargetOnly)
        {
            if (_target == null)
                return;

            if (collision.transform == _target)
            {
                ApplyEffect(collision.gameObject);

                SpawnEffect();
            }
        }        
    }

    protected override async UniTask MoveAndDestroy(float duration)
    {
        await UniTask.Yield();

        Vector2 dir = (_target.position - transform.position).normalized;

        if (_throwType == ThrowType.Straight)
        {
            await transform.DOMove(_target.position, _speed)
                .SetEase(Ease.Linear).SetSpeedBased()
                .AsyncWaitForCompletion();

            if (_activationCondition == ActivationCondition.Target)
            {
                foreach (var target in Physics2D.OverlapCircleAll(transform.position, _radius, _targetLayer))
                {
                    ApplyEffect(target.gameObject);
                }

                GameObject effect = SpawnEffect();
                effect.transform.right = dir;
            }
        }
        else if (_throwType == ThrowType.Parabola)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = _target.position;
            duration /= _speed;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

                pos.y += _distance * 4f * t * (1f - t);

                transform.position = pos;
                await UniTask.Yield();
            }

            SpawnEffect();

            if (_activationCondition == ActivationCondition.Target)
            {
                foreach (var target in Physics2D.OverlapCircleAll(transform.position, _radius, _targetLayer))
                {
                    ApplyEffect(target.gameObject);
                }
            }
        }

        ProjectileDestroy();
    }

    private void ApplyEffect(GameObject obj)
    {
        _targetStatus = ComponentProvider.Get<UnitBase>(obj.gameObject).StatusController;

        if (_isAttck)
        {
            _status.CalculateDamage(_physicalPower, _abilityPower, _damageType, _targetStatus);
        }

        if(_isModifier)
        {
            if (_isBuff)
            {
                _status.ProvideEffect(_buffEffectData, _abilityPower, name, _targetStatus);
            }
            else
            {
                _status.ProvideStat(_statEffectModifier, _abilityPower, name, _targetStatus);
            }
        }        
    }
}

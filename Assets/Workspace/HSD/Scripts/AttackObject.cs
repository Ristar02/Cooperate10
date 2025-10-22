using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackObject : MonoBehaviour
{
    [SerializeField] SearchType _searchType;
    [SerializeField] float _sizeOrRadius;
    [SerializeField] Vector2 _boxSize;
    [SerializeField] Vector2 _offset;
    [SerializeField] LayerMask _targetLayer;
    private float _power;
    private float _delay;

    private void OnEnable()
    {
        ComponentProvider.Add(gameObject, this);
    }
    private void OnDisable()
    {
        ComponentProvider.Remove<AttackObject>(gameObject);
    }

    public void Setup(float power, float delay)
    {
        _power = power;
        _delay = delay;

        Attack().Forget();
    }    

    private async UniTask Attack()
    {
        await UniTask.WaitForSeconds(_delay);

        GameObject[] objs = Utils.GetTargetsNonAlloc(GetAttackPoint(transform), _searchType, _sizeOrRadius, _boxSize, _targetLayer);

        if (objs == null)
            return;

        foreach (var target in objs)
        {
            ComponentProvider.Get<UnitBase>(target.gameObject).StatusController.TakeDamage((int)_power);
        }
    }

    private Vector2 GetAttackPoint(Transform owner)
    {
        return (Vector2)owner.position
            + new Vector2(
            owner.GetFacingDir() * _offset.x * ((1 + Mathf.Abs(owner.localScale.x)) / 2),
            _offset.y * Mathf.Abs(owner.localScale.y
            )
        );
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector2 attackPoint = GetAttackPoint(transform);
        Gizmos.color = Color.cyan;

        switch (_searchType)
        {
            case SearchType.Circle:
                Gizmos.DrawWireSphere(attackPoint, _sizeOrRadius);
                break;

            case SearchType.Box:
                Gizmos.matrix = Matrix4x4.TRS(attackPoint, Quaternion.identity, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, _boxSize);
                Gizmos.matrix = Matrix4x4.identity;
                break;
        }
    }
#endif
}

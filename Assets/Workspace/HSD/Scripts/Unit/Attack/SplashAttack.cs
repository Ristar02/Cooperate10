    using UnityEngine;

[CreateAssetMenu(fileName = "SplashAttack", menuName = "Data/Unit/Attack/Splash")]
public class SplashAttack : UnitAttackData
{
    [SerializeField] SearchType _searchType;

    [Header("Splash")]
    [SerializeField] float _radius;
    [SerializeField] float _angle;
    [SerializeField] Vector2 _offset;
    public override void Attack(IAttacker attacker)
    {
        base.Attack(attacker);

        GameObject effectPrefab = Manager.Resources.Load<GameObject>(EffectAddress);
        if (effectPrefab == null) return;

        GameObject effect = Manager.Resources.Instantiate(effectPrefab, attacker.GetTarget().position, true);
        Manager.Resources.Destroy(effect, 2);

        effect.transform.right = attacker.GetTargetDir();

        GameObject[] targets = Utils.GetTargetsNonAlloc(attacker, attacker.GetTarget().position,
            _searchType, _radius, Vector2.zero, _angle, 10, attacker.TargetLayer);

        foreach (var target in targets)
        {
            attacker.GetStatusController().CalculateDamage(ComponentProvider.Get<UnitBase>(target).StatusController);
        }
    }

    private Vector2 GetAttackPoint(IAttacker attacker)
    {
        return attacker.GetCenter()
            + new Vector2(
            attacker.GetTransform().GetFacingDir() * _offset.x * ((1 + Mathf.Abs(attacker.GetTransform().localScale.x)) / 2),
            _offset.y * Mathf.Abs(attacker.GetTransform().localScale.y
            )
        );
    }

#if UNITY_EDITOR
    public void DrawGizmos(IAttacker attacker)
    {
        if (attacker == null || attacker.GetTarget() == null) return;

        Vector2 targetDir = attacker.GetTargetDir();

        switch (_searchType)
        {
            case SearchType.Circle:
                Gizmos.DrawWireSphere(attacker.GetTarget().position, _radius);
                break;            
            case SearchType.Sector:
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attacker.GetTarget().position, _radius);

                Vector3 rightDir = Quaternion.Euler(0, 0, -_angle / 2) * targetDir;
                Vector3 leftDir = Quaternion.Euler(0, 0, _angle / 2) * targetDir;

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(attacker.GetTarget().position, attacker.GetTarget().position + rightDir * _radius);
                Gizmos.DrawLine(attacker.GetTarget().position, attacker.GetTarget().position + leftDir * _radius);
                break;
        } 
    }
#endif
}

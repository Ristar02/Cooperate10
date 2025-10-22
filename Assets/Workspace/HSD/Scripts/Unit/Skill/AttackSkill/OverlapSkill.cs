using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackSkill", menuName = "Data/Unit/Skill/Attack")]
public class OverlapSkill : AttackSkill
{
    [Header("Stun")]
    public bool IsStun;
    public float StunDuration;

    [Header("TargetType")]
    public TargetType TargetType;

    [Header("Overlap")]
    public SearchType SearchType;
    public Vector2 AttackPointOffset;
    public float SizeOrRadius;
    public Vector2 BoxSize;
    public float Angle;

    public int SearchCount;

    public override void Active(IAttacker attacker)
    {
        base.Active(attacker);

        Attack(attacker);
        SpawnEffect(attacker);
    }

    private void Attack(IAttacker attacker)
    {
        if (Priority == Priority.Target)
        {
            if (attacker.GetTarget() == null)
            {
                Debug.Log("[스킬] 타켓이 없습니다.");
                return;
            }

            UnitStatusController targetStatus = ComponentProvider.Get<UnitBase>(attacker.GetTarget().gameObject).StatusController;

            attacker.GetStatusController().CalculateDamage(
                PhysicalPower,
                AbilityPower,
                DamageType,
                targetStatus
                );

            if (IsStun)
                targetStatus.Stun(StunDuration * (attacker.GetStatusController().Status.GetCurrentStat().MagicDamage * (AbilityPower / 100)));
        }
        else if (Priority == Priority.TargetRadius)
        {
            foreach (var target in Utils.GetTargetsNonAlloc(attacker,
            attacker.GetTarget().position, SearchType, SizeOrRadius, BoxSize, Angle, MaxCount, attacker.TargetLayer))
            {
                UnitStatusController targetStatus = ComponentProvider.Get<UnitBase>(target).StatusController;

                attacker.GetStatusController().CalculateDamage(
                    PhysicalPower,
                    AbilityPower,
                    DamageType,
                    targetStatus
                    );

                if (IsStun)
                    targetStatus.Stun(StunDuration * (attacker.GetStatusController().Status.GetCurrentStat().MagicDamage * (AbilityPower / 100)));
            }
        }
        else if (Priority == Priority.None)
        {
            foreach (var target in GetTargets(attacker))
            {
                UnitBase ub = ComponentProvider.Get<UnitBase>(target);
                if (ub == null)
                {
                    Debug.LogWarning($"[OverlapSkill] target {target.name} 에 UnitBase가 없습니다.");
                    continue;
                }

                if (ub.StatusController == null)
                {
                    Debug.LogWarning($"[OverlapSkill] target {target.name} 의 StatusController가 null입니다.");
                    continue;
                }

                UnitStatusController targetStatus = ub.StatusController;
                attacker.GetStatusController().CalculateDamage(PhysicalPower, AbilityPower, DamageType, targetStatus);

                if (IsStun)
                    targetStatus.Stun(StunDuration * (attacker.GetStatusController().Status.GetCurrentStat().MagicDamage * (AbilityPower / 100)));
            }
        }
        else
        {
            GameObject target = GetTargetSingle(attacker);

            if (target == null)
                return;

            UnitBase ub = ComponentProvider.Get<UnitBase>(GetTargetSingle(attacker));
            if (ub == null)
            {
                Debug.LogWarning($"[OverlapSkill] target {target.name} 에 UnitBase가 없습니다.");
                return;
            }

            if (ub.StatusController == null)
            {
                Debug.LogWarning($"[OverlapSkill] target {target.name} 의 StatusController가 null입니다.");
                return;
            }

            UnitStatusController targetStatus = ub.StatusController;
            attacker.GetStatusController().CalculateDamage(PhysicalPower, AbilityPower, DamageType, targetStatus);

            if (IsStun)
                targetStatus.Stun(StunDuration * (attacker.GetStatusController().Status.GetCurrentStat().MagicDamage * (AbilityPower / 100)));
        }
    }

    protected GameObject[] GetTargets(IAttacker attacker)
    {
        Vector2 attackPoint = GetAttackPoint(attacker);

        return Utils.GetTargetsNonAlloc(attacker,
            attackPoint,
            SearchType,
            SizeOrRadius,
            BoxSize,
            Angle,
            MaxCount,
            attacker.TargetLayer);
    }

    protected override GameObject GetTargetSingle(IAttacker attacker)
    {
        var target = Utils.GetTargetsNonAllocSingle(attacker, SearchType.Circle, SizeOrRadius, BoxSize, Angle, attacker.TargetLayer, GetPriorityFilter());
        return target;
    }

    private Vector2 GetAttackPoint(IAttacker attacker)
    {
        return attacker.GetCenter()
            + new Vector2(
            attacker.GetTransform().GetFacingDir() * AttackPointOffset.x * ((1 + Mathf.Abs(attacker.GetTransform().localScale.x)) / 2),
            AttackPointOffset.y * Mathf.Abs(attacker.GetTransform().localScale.y
            )
        );
    }

    public override string GetCalculateValueString(UnitStatus status)
    {
        if(IsStun)
        {
            UnitStats stat = status.GetCurrentStat();
            return Mathf.RoundToInt(StunDuration * (stat.MagicDamage * (AbilityPower / 100))).ToString("F1");
        }
        else
            return base.GetCalculateValueString(status);
    }

#if UNITY_EDITOR
    public override void DrawGizmos(IAttacker attacker)
    {
        /*
        //Transform transform = attacker.GetTransform();
        //Handles.color = Color.yellow;

        //// 시야의 시작 방향 벡터 계산
        //Vector2 startDirection = Quaternion.Euler(0, 0, Fov / 2) * attacker.GetTargetDir();

        //// DrawSolidArc 함수를 이용하여 시야 범위를 나타내는 부채꼴 그리기
        //Handles.DrawSolidArc(transform.position, Vector3.back, startDirection, Fov, SizeOrRadius);
        */
        base.DrawGizmos(attacker);

        if (attacker == null || attacker.GetTarget() == null) return;

        Vector2 attackPoint = Priority == Priority.TargetRadius ? attacker.GetTarget().position : GetAttackPoint(attacker);
        Vector2 targetDir = attacker.GetTargetDir();
        Transform transform = attacker.GetTransform();

        Handles.color = Color.yellow;

        switch (SearchType)
        {
            case SearchType.Circle:                
                Handles.DrawWireDisc(attackPoint, Vector3.forward, SizeOrRadius);
                break;

            case SearchType.Box:
                Vector3 boxCenter = attackPoint;
                Quaternion rot = Quaternion.Euler(0, 0, Angle);
                Handles.DrawWireCube(boxCenter, BoxSize);
                Handles.matrix = Matrix4x4.TRS(boxCenter, rot, Vector3.one);
                Handles.DrawWireCube(Vector3.zero, BoxSize);
                Handles.matrix = Matrix4x4.identity;
                break;

            case SearchType.Sector:
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attacker.GetTransform().position, SizeOrRadius);

                Vector3 rightDir = Quaternion.Euler(0, 0, -Angle / 2) * targetDir;
                Vector3 leftDir = Quaternion.Euler(0, 0, Angle / 2) * targetDir;

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, transform.position + rightDir * SizeOrRadius);
                Gizmos.DrawLine(transform.position, transform.position + leftDir * SizeOrRadius);
                break;
        }
        // 공격 포인트 위치 표시
        Handles.color = Color.red;
        Handles.DrawSolidDisc(attackPoint, Vector3.forward, 0.05f);
    }
#endif
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class UnitSkill : ScriptableObject
{
    [Header("ID")]
    public int ID;

    [Header("Default")]
    public Sprite Icon;
    public string SkillName;
    [TextArea] public string Description;

    [Header("Stat")]
    public int MaxCount = 1;
    public float PhysicalPower = 1;
    public float AbilityPower = 1;
    public int ManaCost;

    [Header("Type")]
    public Priority Priority;

    [Header("Effect")]
    public string EffectAddress;
    public EffectSpawnType EffectSpawnType;
    public Vector2 SpawnPointOffset;    

    public virtual void Active(IAttacker attacker)
    {
        
    }

    protected void SpawnEffect(IAttacker attacker)
    {
        if (string.IsNullOrEmpty(EffectAddress))
            return;
        
        if (EffectSpawnType == EffectSpawnType.Target)
        {
            Manager.Resources.Destroy(Manager.Resources.Instantiate<GameObject>(
                    EffectAddress,
                    attacker.GetTarget().gameObject.GetCenterPosition(),
                    true
                ), 
            2f);
        }
        else
        {
            GameObject prefab = Manager.Resources.Load<GameObject>(EffectAddress);
            GameObject obj = Manager.Resources.Instantiate(
                prefab,
                GetSpawnPoint(attacker),
                prefab.transform.rotation,
                true
            );

            int attackerDir = attacker.GetTransform().GetFacingDir();
            int objDir = obj.transform.GetRotFacingDir();

            obj.transform.rotation = Quaternion.Euler(
                prefab.transform.eulerAngles.x,
                attackerDir != objDir ? 180 : 0,
                prefab.transform.eulerAngles.z
                );

            Manager.Resources.Destroy(obj, 2f);
        }
    }

    protected Vector2 GetSpawnPoint(IAttacker attacker)
    {
        return attacker.GetCenter()
            + new Vector2(
            attacker.GetTransform().GetFacingDir() * SpawnPointOffset.x * ((1 + Mathf.Abs(attacker.GetTransform().localScale.x)) / 2),
            SpawnPointOffset.y * Mathf.Abs(attacker.GetTransform().localScale.y
            )
        );
    }
    protected virtual GameObject GetTargetSingle(IAttacker attacker)
    {
        var target = Utils.GetTargetsNonAllocSingle(attacker, SearchType.Circle, 100, Vector2.zero, 1, attacker.TargetLayer, GetPriorityFilter());
        return target;
    }

    protected System.Func<IAttacker, List<GameObject>, GameObject> GetPriorityFilter()
    {
        switch (Priority)
        {
            case Priority.Close:
                return Close;
            case Priority.Far:
                return Far;
            case Priority.LowHp:
                return LowHp;
            case Priority.HightHp:
                return HighHp;
            case Priority.Tank:
                return ClassFilter(ClassType.TANK);
            case Priority.Melee:
                return ClassFilter(ClassType.MELEE);
            case Priority.Ranged:
                return ClassFilter(ClassType.RANGED);
            case Priority.Support:
                return ClassFilter(ClassType.SUPPORT);
            default:
                return null;
        }
    }

    #region Priority
    protected GameObject Close(IAttacker attacker, List<GameObject> targets)
    {
        if (targets == null || targets.Count == 0)
            return null; // 대상 없음

        return targets[0];
    }

    protected GameObject Far(IAttacker attacker, List<GameObject> targets)
    {
        if (targets == null || targets.Count == 0)
            return null;

        Vector2 origin = attacker.GetTransform().position;

        targets.Sort((a, b) =>
        {
            Vector2 posA = a.transform.position;
            Vector2 posB = b.transform.position;
            float distA = (origin - posA).sqrMagnitude;
            float distB = (origin - posB).sqrMagnitude;
            return distB.CompareTo(distA); // 역순
        });

        return targets[0];
    }

    protected GameObject LowHp(IAttacker attacker, List<GameObject> targets)
    {
        if (targets == null || targets.Count == 0)
            return null;

        List<GameObject> validTargets = new List<GameObject>();

        foreach (var target in targets)
        {            
            var statusController = ComponentProvider.Get<UnitBase>(target)?.StatusController;
            if (statusController != null && !statusController.IsDead)
            {
                validTargets.Add(target);
            }
        }

        if (validTargets.Count == 0)
            return null;

        validTargets.Sort((a, b) =>
        {
            var statusA = ComponentProvider.Get<UnitBase>(a).StatusController;
            var statusB = ComponentProvider.Get<UnitBase>(b).StatusController;

            float hpPercentA = (float)statusA.CurHp.Value / statusA.MaxHealth.Value;
            float hpPercentB = (float)statusB.CurHp.Value / statusB.MaxHealth.Value;

            return hpPercentA.CompareTo(hpPercentB);
        });

        return validTargets[0];
    }


    protected GameObject HighHp(IAttacker attacker, List<GameObject> targets)
    {
        if (targets == null || targets.Count == 0)
            return null;

        List<GameObject> validTargets = new List<GameObject>();

        foreach (var target in targets)
        {
            var statusController = ComponentProvider.Get<UnitBase>(target).StatusController;
            if (statusController != null && !statusController.IsDead)
            {
                validTargets.Add(target);
            }
        }

        if (validTargets.Count == 0)
            return null;

        // HP 높은 순으로 정렬
        validTargets.Sort((a, b) =>
        {
            var statusA = ComponentProvider.Get<UnitBase>(a).StatusController;
            var statusB = ComponentProvider.Get<UnitBase>(b).StatusController;

            float hpPercentA = (float)statusA.CurHp.Value / statusA.MaxHealth.Value;
            float hpPercentB = (float)statusB.CurHp.Value / statusB.MaxHealth.Value;

            return hpPercentB.CompareTo(hpPercentA); // 역순
        });

        return validTargets[0];
    }

    protected System.Func<IAttacker, List<GameObject>, GameObject> ClassFilter(ClassType classType)
    {
        return (attacker, targets) =>
        {
            foreach (var target in targets)
            {
                var statusController = ComponentProvider.Get<UnitBase>(target).StatusController;
                if (statusController != null && !statusController.IsDead)
                {
                    if (statusController.Status.Data.ClassSynergy == classType)
                    {
                        return target;
                    }
                }
            }

            return targets[UnityEngine.Random.Range(0, targets.Count)];
        };
    }
    #endregion

    public virtual string GetCalculateValueString(UnitStatus status)
    {
        UnitStats stat = status.GetCurrentStat();

        return Mathf.RoundToInt(stat.PhysicalDamage * PhysicalPower + stat.MagicDamage * (AbilityPower / 100)).ToString("F0");
    }    

    #if  UNITY_EDITOR
    public virtual void DrawGizmos(IAttacker attacker) // 씬 창에서 부채꼴 범위 그리기
    {
        if (attacker == null || attacker.GetTarget() == null) return;

        Handles.color = Color.blue;
        Handles.DrawSolidDisc(GetSpawnPoint(attacker), Vector3.forward, 0.05f);
    }
    #endif
}
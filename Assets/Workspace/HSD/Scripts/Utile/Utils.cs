using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class Utils
{
    public static bool Contain(this LayerMask layerMask, int layer)
    {
        return ((1 << layer) & layerMask) != 0;
    }
    public static int GetFacingDir(this Transform transform)
    {
        return transform.localScale.x > 0 ? -1 : 1;
    }
    public static int GetRotFacingDir(this Transform transform)
    {        
        return transform.rotation.y == 0 ? 1 : -1;
    }
    public static Vector2 GetTopPosition(this GameObject obj)
    {
        return GetCenterPosition(obj) + new Vector2(0, obj.transform.localScale.y / 2);
    }
    public static Vector2 GetTopPosition(this Transform transform)
    {
        return GetTopPosition(transform.gameObject);
    }
    public static Vector2 GetCenterPosition(this GameObject obj)
    {
        return ComponentProvider.Get<UnitBase>(obj).GetCenter();
    }
    public static Vector2 GetCenterPosition(this Transform transform)
    {
        return GetCenterPosition(transform.gameObject);
    }

    #region Damage Calculation
    public static void CalculateDamage(this UnitStatusController status, float physicalPower, float abilityPower, DamageType damageType, UnitStatusController enemy)
    {
        // 총 데미지 = 물리 계수 * 물리 공격력 + 마법 계수 * (마법 공격력 / 100)

        int defense = damageType == DamageType.Physical ? enemy.PhysicalDefense.Value : enemy.MagicDefense.Value;
        float total = status.PhysicalDamage.Value * physicalPower + status.MagicDamage.Value * (status.MagicDamage.Value / 100);

        bool isCrit = false;

        if (status.CritChance.Value > Random.Range(0f, 100f))
        {
            isCrit = true;
            total *= 1.5f;
        }

        float totalDefense = defense / (defense + 100f);

        int totalDamage = Mathf.RoundToInt(total * (1f - totalDefense));

        status.TotalDamage.Value += totalDamage;
        enemy.TakeDamage(totalDamage, isCrit);
    }
    public static void CalculateDamage(this UnitStatusController status, UnitStatusController enemy)
    {
        bool isCrit = false;
        float total = status.PhysicalDamage.Value;

        if (status.CritChance.Value > Random.Range(0f, 100f))
        {
            isCrit = true;
            total *= 1.5f;
        }
        float totalDefense = enemy.PhysicalDefense.Value / (enemy.PhysicalDefense.Value + 100f);

        int totalDamage = Mathf.RoundToInt(total * (1f - totalDefense));
        status.TotalDamage.Value += totalDamage;

        enemy.TakeDamage(totalDamage, isCrit);
    }
    #endregion

    #region ApplyEffect
    public static void ProvideEffect(this UnitStatusController status, BuffEffectData buffEffectData, float abilityPower,
        string source, UnitStatusController enemy)
    {
        float value = abilityPower * (status.MagicDamage.Value / 100);

        enemy.ApplyEffect(buffEffectData, value, source);
    }
    public static void ProvideStat(this UnitStatusController status, StatEffectModifier statEffectModifier, float abilityPower, 
        string source, UnitStatusController enemy)
    {
        float value = abilityPower * (status.MagicDamage.Value / 100);

        enemy.AddStat(statEffectModifier.StatType, value, source);
    }
    #endregion

    #region GetClosestTargetNonAlloc
    private static Collider2D[] _hitBuffer = new Collider2D[50];
    private static readonly List<GameObject> _cachedTargets = new List<GameObject>(50);

    public static Transform GetClosestTargetNonAlloc(Vector3 origin, float radius, LayerMask enemyMask, 
        System.Func<Transform, bool> filter = null)
    {
        int count = Physics2D.OverlapCircleNonAlloc(origin, radius, _hitBuffer, enemyMask);
        
        Transform closest = null;
        float bestDistSq = float.PositiveInfinity;

        var result = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = _hitBuffer[i].gameObject;
        }

        for (int i = 0; i < result.Length; i++)
        {
            var hit = result[i];
            if (hit == null) continue;
            if (ComponentProvider.Get<UnitBase>(hit).StatusController.IsDead) continue;            
            if (filter != null && filter(hit.transform)) continue;

            float distSq = (hit.transform.position - origin).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                closest = hit.transform;
            }
        }

        return closest;
    }    
    #endregion

    #region GetTargetsNonAlloc
    public static GameObject[] GetTargetsNonAlloc(IAttacker attacker, Vector2 origin, SearchType shape, float sizeOrRadius, Vector2 boxSize,
        float angle, int maxCount, LayerMask layerMask, System.Func<IAttacker, List<GameObject>, int, GameObject[]> filter = null, int maxTargets = 50, 
        bool sortByDistance = true)
    {
        _cachedTargets.Clear(); // 재사용

        int hitCount = 0;

        switch (shape)
        {
            case SearchType.Circle:
                hitCount = Physics2D.OverlapCircleNonAlloc(origin, sizeOrRadius, _hitBuffer, layerMask);
                break;
            case SearchType.Box:
                hitCount = Physics2D.OverlapBoxNonAlloc(origin, boxSize, angle, _hitBuffer, layerMask);
                break;
            case SearchType.Capsule:
                hitCount = Physics2D.OverlapCapsuleNonAlloc(origin, boxSize, CapsuleDirection2D.Vertical, angle, _hitBuffer, layerMask);
                break;
            case SearchType.Sector:
                List<Collider2D> results = new List<Collider2D>();

                Physics2D.OverlapCircleNonAlloc(attacker.GetTransform().position, sizeOrRadius, _hitBuffer, layerMask);

                foreach (var hit in _hitBuffer)
                {
                    if (hit == null) continue;

                    Vector2 dirToTarget = (hit.transform.position - attacker.GetTransform().position).normalized;
                    float dot = Vector2.Dot(attacker.GetTargetDir(), dirToTarget);

                    float theta = Mathf.Acos(dot) * Mathf.Rad2Deg;

                    if (theta <= angle / 2f)
                    {
                        results.Add(hit);
                    }
                }

                hitCount = results.Count;
                _hitBuffer = results.ToArray();
                break;
        }

        for (int i = 0; i < hitCount && _cachedTargets.Count < maxCount; i++)
        {
            if (_hitBuffer[i] != null && _hitBuffer[i].gameObject != null)
                _cachedTargets.Add(_hitBuffer[i].gameObject);
        }

        _cachedTargets.RemoveAll(target =>
            ComponentProvider.Get<UnitBase>(target).StatusController.IsDead);

        if (sortByDistance && _cachedTargets.Count > 1)
        {
            _cachedTargets.Sort((a, b) =>
            {
                Vector2 posA = a.transform.position;
                Vector2 posB = b.transform.position;
                float distA = (origin.x - posA.x) * (origin.x - posA.x) + (origin.y - posA.y) * (origin.y - posA.y);
                float distB = (origin.x - posB.x) * (origin.x - posB.x) + (origin.y - posB.y) * (origin.y - posB.y);
                return distA.CompareTo(distB);
            });
        }

        GameObject[] result;

        if (filter != null)
            result = filter.Invoke(attacker, _cachedTargets, maxTargets);
        else
            result = _cachedTargets.ToArray();

        return result;
    }

    public static GameObject[] GetTargetsNonAlloc(Vector2 origin, SearchType searchType, float sizeOrRadius, Vector2 boxSize, LayerMask layerMask)
    {
        int hitCount = 0;
        Debug.Log(layerMask.ToString());
        switch (searchType)
        {
            case SearchType.Circle:
                hitCount = Physics2D.OverlapCircleNonAlloc(origin, sizeOrRadius, _hitBuffer, layerMask);
                break;
            case SearchType.Box:
                hitCount = Physics2D.OverlapBoxNonAlloc(origin, boxSize, 0, _hitBuffer, layerMask);
                break;
            case SearchType.Capsule:
                hitCount = Physics2D.OverlapCapsuleNonAlloc(origin, boxSize, CapsuleDirection2D.Vertical, 0, _hitBuffer, layerMask);
                break;
        }

        if (hitCount <= 0)
            return null;
        
        var result = new GameObject[hitCount];

        for (int i = 0; i < hitCount; i++)
        {
            UnitBase unit = ComponentProvider.Get<UnitBase>(_hitBuffer[i].gameObject);

            if(unit == null)
                continue;

            if (unit.StatusController.IsDead)
                continue;

            result[i] = _hitBuffer[i].gameObject;
        }

        return result;
    }

    public static GameObject GetTargetsNonAllocSingle(IAttacker attacker, SearchType shape, float sizeOrRadius, Vector2 boxSize, float angle,
        LayerMask layerMask, System.Func<IAttacker, List<GameObject>, GameObject> filter = null)
    {
        _cachedTargets.Clear();
        int hitCount = 0;
        Vector2 origin = attacker.GetTransform().position;

        switch (shape)
        {
            case SearchType.Circle:
                hitCount = Physics2D.OverlapCircleNonAlloc(origin, sizeOrRadius, _hitBuffer, layerMask);
                break;
            case SearchType.Box:
                hitCount = Physics2D.OverlapBoxNonAlloc(origin, boxSize, angle, _hitBuffer, layerMask);
                break;
            case SearchType.Capsule:
                hitCount = Physics2D.OverlapCapsuleNonAlloc(origin, boxSize, CapsuleDirection2D.Vertical, angle, _hitBuffer, layerMask);
                break;
            case SearchType.Sector:
                int total = Physics2D.OverlapCircleNonAlloc(origin, sizeOrRadius, _hitBuffer, layerMask);
                for (int i = 0; i < total; i++)
                {
                    var hit = _hitBuffer[i];
                    if (hit == null) continue;

                    Vector2 dirToTarget = (hit.transform.position - attacker.GetTransform().position).normalized;
                    float dot = Vector2.Dot(attacker.GetTargetDir(), dirToTarget);
                    float theta = Mathf.Acos(dot) * Mathf.Rad2Deg;

                    if (theta <= angle / 2f)
                    {
                        _cachedTargets.Add(hit.gameObject);
                    }
                }
                hitCount = _cachedTargets.Count;
                break;
        }

        if (shape != SearchType.Sector)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i] != null)
                    _cachedTargets.Add(_hitBuffer[i].gameObject);
            }
        }

        if (_cachedTargets.Count == 0)
            return null;

        _cachedTargets.RemoveAll(target =>
            ComponentProvider.Get<UnitBase>(target).StatusController.IsDead);

        if (_cachedTargets.Count == 0)
            return null;

        return filter != null
            ? filter.Invoke(attacker, _cachedTargets)
            : _cachedTargets[0];
    }
    #endregion

    #region String
    public static string ToAbbreviation(long value)
    {
        if (value >= 1_000_000_000)
            return $"{(value / 1_000_000_000f).ToString("0.#")}B";
        if (value >= 1_000_000)
            return $"{(value / 1_000_000f).ToString("0.#")}M";
        if (value >= 1_000)
            return $"{(value / 1_000f).ToString("0.#")}k";

        return value.ToString();
    }

    private static StringBuilder sb = new StringBuilder();

    public static void AppendString(string str)
    {
        sb.Append(str);
    }

    public static void AppendLine(string str)
    {
        sb.AppendLine(str);
    }

    public static string GetString()
    {
        string result = sb.ToString();
        sb.Clear();
        return result;
    }

    public static void ClearStringBuilder()
    {
        sb.Clear();
    }

    #endregion

    #region Color
    public static Color GetSynergyColor(this SynergyData data)
    {
        return (data.CurrentUpgradeIdx) switch
        {
            -1 => new Color(85f / 255f, 85f / 255f, 85f / 255f),        // 짙은 회색
            0 => new Color(217f / 255f, 217f / 255f, 217f / 255f),      // 밝은 회색
            1 => new Color(241f / 255f, 229f / 255f, 109f / 255f),      // 노란색
            2 => new Color(63f / 255f, 239f / 255f, 239f / 255f),       // Cyan
            _ => new Color(85f / 255f, 85f / 255f, 85f / 255f)
        };
    }

    public static Color GetGradeColor(this UnitStatus status)
    {
        return status.Data.Grade switch
        {
            Grade.NORMAL => new Color(173f / 255f, 255f / 255f, 47f / 255f),
            Grade.RARE => new Color32(0x7B, 0x7B, 0xD9, 0xFF),
            Grade.UNIQUE => new Color32(0xC8, 0x5D, 0xD8, 0xFF),
            Grade.LEGEND => new Color32(0xF2, 0x93, 0x38, 0xFF),
            _ => Color.grey,
        };
    }

    public static Color GetEnemyLevelColor(this UnitStatus status)
    {
        return (status.Level) switch
        {
            2 => new Color32(0x55, 0x55, 0x55, 0xFF), // Boss : 단색 회색
            1 => new Color32(0xC7, 0x40, 0x40, 0xFF), // Elite : 단색 빨강
            0 => new Color32(0xB0, 0xB0, 0xB0, 0xFF), // Normal : 단색 회색
            _ => Color.grey
        };
    }

    #endregion

    #region UI
    public static void SetupGridLayoutGroup(
    this GridLayoutGroup gridLayoutGroup,
    Transform content,
    int columns,
    int rows,
    int offset = 10,
    bool keepSquare = false)
    {
        RectTransform rectTransform = content as RectTransform;
        if (rectTransform == null) return;

        gridLayoutGroup.padding = new RectOffset(offset, offset, offset, offset);

        float availableWidth = rectTransform.rect.width
                             - gridLayoutGroup.padding.left - gridLayoutGroup.padding.right;

        float availableHeight = rectTransform.rect.height
                              - gridLayoutGroup.padding.top - gridLayoutGroup.padding.bottom;

        if (!keepSquare)
        {
            // 기존 방식
            float totalWidth = availableWidth - (gridLayoutGroup.spacing.x * (columns - 1));
            float totalHeight = availableHeight - (gridLayoutGroup.spacing.y * (rows - 1));

            float cellWidth = totalWidth / columns;
            float cellHeight = totalHeight / rows;

            gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
        }
        else
        {
            // 정사각형 셀 크기
            float cellWidth = availableWidth / columns;
            float cellHeight = (availableHeight - (gridLayoutGroup.spacing.y * (rows - 1))) / rows;
            float cellSize = Mathf.Min(cellWidth, cellHeight);

            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

            // spacing.x 재계산
            float totalCellWidth = cellSize * columns;
            float remainingWidth = Mathf.Max(0, availableWidth - totalCellWidth);
            float spacingX = columns > 1 ? remainingWidth / (columns - 1) : 0;

            gridLayoutGroup.spacing = new Vector2(spacingX, gridLayoutGroup.spacing.y);
        }
    }
    #endregion

    #region Status
    public static UnitStats StatMultiply(this UnitStats unitStats, float multiply)
    {
        return new UnitStats
        {
            MaxHealth = Mathf.RoundToInt(unitStats.MaxHealth * multiply),

            PhysicalDamage = Mathf.RoundToInt(unitStats.PhysicalDamage * multiply),
            MagicDamage = Mathf.RoundToInt(unitStats.MagicDamage * multiply),

            PhysicalDefense = Mathf.RoundToInt(unitStats.PhysicalDefense * multiply),
            MagicDefense = Mathf.RoundToInt(unitStats.MagicDefense * multiply)
        };
    }
    #endregion

    public static int GetSynergyUnitsTotalLevel(this UnitBase[] units, Synergy synergy)
    {
        int count = 0;

        foreach (var unit in units)
        {
            if(unit != null && unit.Status.Data.Synergy == synergy)
            {
                count += unit.Status.Level + 1;
            }
        }

        return count;
    }
}
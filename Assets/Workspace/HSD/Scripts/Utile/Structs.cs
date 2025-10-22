using System;
using UnityEngine;

[Serializable]
public struct StatEffectModifier
{
    public StatType StatType;
    public float Value;
}

[Serializable]
public struct SynergyBuffData
{
    public StatType StatType;
    public float Duration;
    public float Value;
}

[Serializable]
public struct BuffEffectData
{  
    public StatType StatType;
    public float Duration;
    public bool IsTicking;
    public float TickInterval;
}

[Serializable]
public struct TickDamageData
{
    public int TickCount;
    public float TickInterval;
}

[Serializable]
public struct CsvData
{
    public CsvType CsvType;
    public int StartLine;

    [TextArea]
    [SerializeField] string _url;

    [Header("예시 : A2:C12")]
    [SerializeField] string range;

    [SerializeField] int gid;

    public string GetURL()
    {
        string baseUrl = _url;
        
        int editIndex = baseUrl.IndexOf("/edit");
        if (editIndex > -1)
        {
            baseUrl = baseUrl.Substring(0, editIndex);
        }

        string result = "";

        if(string.IsNullOrEmpty(range))
            result = $"{baseUrl}/export?format=tsv&gid={gid}";
        else
            result = $"{baseUrl}/export?format=tsv&gid={gid}&range={range}";

        return result;
    }
}

public readonly struct SourceKey : IEquatable<SourceKey>
{
    public StatType StatType { get; }
    public string Source { get; }

    public SourceKey(StatType statType, string source)
    {
        StatType = statType;
        Source = source;
    }

    public bool Equals(SourceKey other) =>
        StatType == other.StatType && Source == other.Source;

    public override bool Equals(object obj) =>
        obj is SourceKey other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(StatType, Source);

    public override string ToString() => $"{StatType} ({Source})";
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitAttackData : ScriptableObject
{
    [Header("ID")]
    public int ID;

    public DamageType DamageType;
    public float AttackPower;

    [Header("Effect")]
    public string EffectAddress;

    public virtual void Attack(IAttacker attacker)
    {
        
    }
}
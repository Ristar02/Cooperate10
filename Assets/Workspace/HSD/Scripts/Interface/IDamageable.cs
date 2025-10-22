using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(int amount, bool isCrit);
    public void TakeTickDamage(int amount, float tickCount, float tickInterval);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageMeterView
{
    void SetDamage(int damage);

    void SetIcon(Sprite icon);
}

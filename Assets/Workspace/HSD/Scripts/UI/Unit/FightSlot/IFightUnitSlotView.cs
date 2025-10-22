using UnityEngine;

public interface IFightUnitSlotView
{
    void SetIcon(Sprite icon);
    void SetHp(float value, float max);
    void SetMana(float value, float max);
}
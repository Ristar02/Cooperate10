using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanSlotCreater : SlotCreater
{
    [SerializeField] GameObject _iconPrefab;
    [SerializeField] Sprite[] _icons;

    public override Dictionary<Vector2Int, UnitSlot> Init()
    {
        for (int i = 0; i < _icons.Length; i++)
        {
            GameObject iconObj = Instantiate(_iconPrefab, _slotParent);            
            iconObj.transform.position = GetCenterPosition(i);            

            iconObj.GetComponent<SpriteRenderer>().sprite = _icons[i];
        }

        return base.Init();
    }

    private Vector2 GetCenterPosition(int x)
    {
        float yPos = _slotParent.transform.position.y;

        float totalWidth = (Size.x - 1) * _offset.x;
        float xOffset = -totalWidth / 2f;

        return new Vector2(_slotParent.position.x + x * _offset.x + xOffset, yPos);
    }

    public override void ActiveSlots()
    {
        base.ActiveSlots();
    }

    public override void DeActiveSlots()
    {
        base.DeActiveSlots();
    }
}

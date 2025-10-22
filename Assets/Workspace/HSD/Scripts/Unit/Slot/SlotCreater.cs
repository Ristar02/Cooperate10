using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlotCreater : MonoBehaviour
{
    [SerializeField] protected GameObject _slotPrefab;
    [SerializeField] protected Transform _slotParent;
    protected static Vector2 _offset = new Vector2(1.7f, 1.6f);
    public Vector2Int Size;    

    public virtual Dictionary<Vector2Int, UnitSlot> Init()
    {
        Dictionary<Vector2Int, UnitSlot> unitSlotDic = new Dictionary<Vector2Int, UnitSlot>();

        for (int i = 0; i < Size.y; i++)
        {
            for (int j = 0; j < Size.x; j++)
            {
                UnitSlot slot = Instantiate(_slotPrefab, GetPos(j, i), Quaternion.identity, _slotParent).GetComponent<UnitSlot>();

                int reversedX = Size.x - j;

                Vector2Int pos = new Vector2Int(reversedX, i + 1);
                slot.Init(reversedX, pos);

                unitSlotDic.Add(pos, slot);
            }
        }

        return unitSlotDic;
    }

    protected Vector2 GetPos(int x, int y)
    {
        Vector2 parentPos = _slotParent.position;

        float totalWidth = (Size.x - 1) * _offset.x;
        float totalHeight = (Size.y - 1) * _offset.y;

        float xOffset = -totalWidth / 2f;
        float yOffset = -totalHeight / 2f;

        return parentPos + new Vector2(x * _offset.x + xOffset, y * _offset.y + yOffset);
    }

    public virtual void DeActiveSlots()
    {
        _slotParent.gameObject.SetActive(false);
    }

    public virtual void ActiveSlots()
    {
        _slotParent.gameObject.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class FightUnitSlotController : MonoBehaviour
{
    [SerializeField] Transform _content;
    [SerializeField] GridLayoutGroup _grid;
    [SerializeField] UI_FightUnitSlot[] _slots;

    private void Start()
    {
        CreateSlots();

        _grid.SetupGridLayoutGroup(_content, 10, 1, 0, true);
    }

    private void CreateSlots()
    {
        int _slotCount = UnitController.UnitMaxCount;

        for (int i = 0; i < _slotCount; i++)
        {
            _slots[i].Init(null);
        }
    }

    public void Init(UnitBase[] units)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (units[i] != null)
            {
                _slots[i].Init(units[i].StatusController);
            }
            else
            {
                _slots[i].Init(null);
            }
        }
    }
}

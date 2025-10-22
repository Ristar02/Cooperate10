using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GachaResultUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Prefab")]
    [SerializeField] private GameObject _gachaResultSlotUI;
    [Header("UI Reference")]
    [SerializeField] private Transform _content;
    [Header("Icon Reference")]
    [SerializeField] private Sprite _coinImage;
    [SerializeField] private Sprite _diaImage;
    [Header("Slots")]
    [SerializeField] private Image[] _gachaResultImages;

    private GameObject[] _slots = new GameObject[10];
    
    public void HeroGachaUpdate(UnitData data, int index, string amount)
    {
        if (_slots[index] == null)
        {
            _slots[index] = Instantiate(_gachaResultSlotUI, _content);
        }
        GachaResultUISlot slot = _slots[index].GetComponent<GachaResultUISlot>();
        slot.UpdateUI(data.Icon, amount);
    }

    public void StoneGachaUpdate(MagicStoneData data, int index, string amount)
    {
        if (_slots[index] == null)
        {
            _slots[index] = Instantiate(_gachaResultSlotUI, _content);
        }
        GachaResultUISlot slot = _slots[index].GetComponent<GachaResultUISlot>();
        slot.UpdateUI(data.Icon, amount);
    }

    public void StoneGachaUpdate(MagicStoneRewardType type, int index, string amount)
    {
        if (_slots[index] == null)
        {
            _slots[index] = Instantiate(_gachaResultSlotUI, _content);
        }
        GachaResultUISlot slot = _slots[index].GetComponent<GachaResultUISlot>();
        switch(type)
        {
            case MagicStoneRewardType.Gold500:
            case MagicStoneRewardType.Gold1000:
            case MagicStoneRewardType.Gold10000:
                slot.UpdateUI(_coinImage, amount);            
                break;
            case MagicStoneRewardType.Dia50:
            case MagicStoneRewardType.Dia100:
            case MagicStoneRewardType.Dia1000:
                slot.UpdateUI(_diaImage, amount);
                break;
        }        
    }

    // 비활성화와 동시에 슬롯을 한개만 남겨두고 전부 파괴(Grid UI를 위해서 임시 처리)
    // 오브젝트 풀 반영할 수 있을 것 같습니다 -> 추후 풀 반영
    private void OnDisable()
    {
        if (_slots.Length <= 1) return;
        else
        {
            for (int i = 1; i < _slots.Length; i++)
            {
                Destroy(_slots[i]);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
    }
}
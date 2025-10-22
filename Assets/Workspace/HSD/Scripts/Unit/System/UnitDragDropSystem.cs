using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitDragDropSystem : MonoBehaviour
{
    public static bool IsDragging;    
    public ToolTipController ToolTipController;

    private GameObject _currentUnit;
    private UnitBase _currentUnitBase;
    private Vector2 _offset;
    private Vector2 _pos;
    private int _currentSlotIdx = 0;
    [SerializeField] LayerMask _targetLayer;

    public event Action<UnitSlot, UnitBase> OnUnitDropped;

    private Action<Collider2D, UnitBase> OnSlotChanged;

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // --- 에디터/PC 전용 입력 ---
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            HandleClick();
        }

        if (IsDragging && _currentUnit != null && Input.GetMouseButton(0))
        {
            DragUnit(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && IsDragging)
        {
            ReleaseUnit();
        }

#elif UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                HandleClick();
            }

            if (IsDragging && _currentUnit != null && touch.phase == TouchPhase.Moved)
            {
                DragUnit(touch.position);
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (IsDragging)
                {
                    ReleaseUnit();
                }
            }
        }
#endif
    }

    private void HandleClick()
    {
        ToolTipController.UnitToolTip.Close();   

        ToolTipController.SynergyToolTip.Close();
        Vector2 worldMouse = GetWorldMouse();
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldMouse, Vector2.zero);

        if (hits.Length == 0)
            return;

        bool isInteractable = false;
        bool isSetUnit = false;
        bool isEnemy = false;

        for (int i = 0; i < hits.Length; i++)
        {
            if(hits[i].collider != null && hits[i].collider.CompareTag("Unit"))
            {
                isEnemy = ComponentProvider.Get<UnitBase>(hits[i].collider.gameObject).GetAllyLayerMask().Contain(LayerMask.NameToLayer("Enemy"));
            }

            if (hits[i].collider != null && hits[i].collider.CompareTag("UnitTrigger"))
            {
                if (isSetUnit) return;

                isInteractable = true;
                isSetUnit = true;
                SetUnit(hits[i].collider.gameObject);  
            }
            else if (hits[i].collider != null && hits[i].collider.CompareTag("BattleUnit"))
            {
                isInteractable = true;
                ToolTipController.UnitToolTip.Show(
                    ComponentProvider.Get<UnitBase>(hits[i].collider.gameObject).Status
                    , false, false, isEnemy);

                ToolTipController.SynergyToolTip.Close();
            }
        }

        if (!isInteractable)
        {
            ToolTipController.UnitToolTip.Close();
            ToolTipController.SynergyToolTip.Close();
        }
    }
    private void DragUnit(Vector3 inputPosition)
    {
        inputPosition.z = -Camera.main.transform.position.z;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(inputPosition);

        _currentUnitBase.transform.position = mouseWorldPos + _offset;
    }
    private void ReleaseUnit()
    {
        bool isSlot;
        CheckUISlot(out isSlot);

        IsDragging = false;

        if (isSlot)
        {
            return;
        }

        if (_currentUnit != null)
        {
            CheckSlot();
        }

        Clear();
    }
    private void CheckUISlot(out bool isSlot)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);  

        isSlot = false;
        foreach (var result in results)
        {
            if (!result.gameObject.CompareTag("Slot"))
                return;

            var dropHandler = result.gameObject.GetComponent<UI_UnitSlot>();
            if (dropHandler != null)
            {
                dropHandler.OnDrop(pointerData);

                isSlot = true;
                break;
            }
        }
    }

    private void CheckSlot()
    {
        // 슬롯 체크
        Collider2D slotCollider = Physics2D.OverlapPoint(_currentUnitBase.transform.position, LayerMask.GetMask("Slot"));

        if (_currentSlotIdx != -1 && _currentUnitBase != null)
        {
            OnSlotChanged?.Invoke(slotCollider, _currentUnitBase);
        }

        if (slotCollider != null)
        {
            UnitSlot slot = slotCollider.GetComponent<UnitSlot>();

            OnUnitDropped?.Invoke(slot, _currentUnitBase);
        }
        else
        {
            if (_currentUnitBase != null)
            {
                if (_currentUnitBase.CurrentSlot == Vector2Int.zero)
                    Destroy(_currentUnitBase.gameObject);
                else
                    _currentUnitBase.transform.position = _pos; // 원래 위치로 되돌리기

                _currentUnitBase.Idle();
            }
        }
    }

    private static Vector2 GetWorldMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector2 worldMouse = Camera.main.ScreenToWorldPoint(mousePos);
        return worldMouse;
    }

    public void SetUnit(GameObject unit)
    {
        IsDragging = true;
        _currentUnit = unit;
        _currentUnitBase = _currentUnit.GetComponentInParent<UnitBase>();
        _currentUnitBase.Drag();
        _offset = Vector2.zero;
        _pos = _currentUnitBase.transform.position;
    }

    public void SetUnit(GameObject unit, Action<Collider2D, UnitBase> action, int slotIdx)
    {
        OnSlotChanged = action;

        _currentSlotIdx = slotIdx;
        IsDragging = true;
        _currentUnit = unit;
        _currentUnitBase = _currentUnit.GetComponentInParent<UnitBase>();
        _currentUnitBase.Drag();

        _offset = Vector2.zero;
        _pos = _currentUnitBase.transform.position;
    }

    private void Clear()
    {
        _currentUnit = null;
        _currentUnitBase = null;
        OnSlotChanged = null;
        _currentSlotIdx = -1;
    }

    public GameObject GetCurrentUnit()
    {
        return _currentUnit;
    }

    public UnitBase GetCurrentUnitBase()
    {
        return _currentUnitBase;
    }
    public int GetCurrentSlotIdx()
    {
        return _currentSlotIdx;
    }
}

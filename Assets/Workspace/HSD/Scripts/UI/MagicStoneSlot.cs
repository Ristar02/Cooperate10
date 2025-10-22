using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class MagicStoneSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private MagicStoneData _magicStoneData;
    public MagicStoneData MagicStoneData => _magicStoneData;

    public event System.Action<MagicStoneSlot> OnCleared;

    [Header("UI")]
    [SerializeField] private RectTransform _magicStone;
    [SerializeField] private Image _magicStoneIcon;
    [SerializeField] private Image _highlight;
    private Image[] _images;

    [Header("Drag Settings")]
    private Transform _dropAreaPanel;
    private Vector3 _originalPos;

    public void Init(Transform dropArea)
    {
        _dropAreaPanel = dropArea;
        _images = GetComponentsInChildren<Image>(true);
        DragEnd();
    }

    public void ClearMagicStone()
    {
        _magicStoneData = null;
        MagicStoneUIUpdate();
        OnCleared?.Invoke(this);
    }

    public void SetMagicStone(MagicStoneData magicStoneData)
    {
        _magicStoneData = magicStoneData;
        MagicStoneUIUpdate();
    }

    private void MagicStoneUIUpdate()
    {
        if (MagicStoneData == null)
        {
            gameObject.SetActive(false);
            _magicStoneIcon.color = Color.clear;
            _magicStoneIcon.sprite = null;
            return;
        }

        gameObject.SetActive(true);
        _magicStoneIcon.color = Color.white;
        _magicStoneIcon.sprite = MagicStoneData.Icon;
    }

    private void UseMagicStone(Vector2 pos)
    {
        if (MagicStoneData == null || !InGameManager.Instance.IsBattle) return;

        MagicStoneData.UseMagicStone(pos);
        ClearMagicStone();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPos = _highlight.rectTransform.position;
        DragStart();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (MagicStoneData == null || !InGameManager.Instance.IsBattle) return;

        Vector3 worldPos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _magicStone,
            eventData.position,
            eventData.pressEventCamera,
            out worldPos))
        {
            _magicStone.position = worldPos;
        }

        bool insideDropArea = RectTransformUtility.RectangleContainsScreenPoint(
            _dropAreaPanel as RectTransform,
            eventData.position,
            eventData.pressEventCamera);

        _highlight.enabled = insideDropArea;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (MagicStoneData == null || !InGameManager.Instance.IsBattle) return;

        bool insideDropArea = RectTransformUtility.RectangleContainsScreenPoint(
            _dropAreaPanel as RectTransform,
            eventData.position,
            eventData.pressEventCamera);

        if (insideDropArea)
        {
            UseMagicStone(Camera.main.ScreenToWorldPoint(eventData.position));
        }

        _magicStone.position = _originalPos;

        _highlight.enabled = false;

        DragEnd();
    }

    private void DragStart()
    {
        foreach (var image in _images)
        {
            image.maskable = false;
        }
    }

    private void DragEnd()
    {
        foreach (var image in _images)
        {
            image.maskable = true;
        }
    }
}

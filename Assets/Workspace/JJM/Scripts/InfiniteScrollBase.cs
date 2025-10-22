using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public abstract class InfiniteScrollBase<TSlot, TData> : MonoBehaviour where TSlot : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] protected ScrollRect scrollRect;
    [SerializeField] protected RectTransform content;
    [SerializeField] protected TSlot slotPrefab;

    [SerializeField] protected int visibleCount = 10;
    [SerializeField] protected int buffer = 2;
    [SerializeField] protected float itemHeight = 100f;
    [SerializeField] protected float itemWidth = 100f;
    [SerializeField] protected ScrollDirection scrollDirection;

    protected List<TData> allData = new List<TData>();
    protected List<TSlot> itemPool = new List<TSlot>();
    protected int totalItemCount;
    private int currentTopIndex = -1;

    protected virtual void Start()
    {
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        Initialize();
    }

    protected virtual void Reset()
    {
        Canvas.ForceUpdateCanvases();
        if (scrollDirection == ScrollDirection.Horizontal)
            scrollRect.horizontalNormalizedPosition = 0f;
        else if (scrollDirection == ScrollDirection.Vertical)
            scrollRect.verticalNormalizedPosition = 1f;

        totalItemCount = allData.Count;

        float totalWidth = content.sizeDelta.x;
        float totalHeight = content.sizeDelta.y;

        switch (scrollDirection)
        {
            case ScrollDirection.Horizontal:
                totalWidth = itemWidth * totalItemCount;
                break;
            case ScrollDirection.Vertical:
                totalHeight = itemHeight * totalItemCount;
                break;
        }

        content.sizeDelta = new Vector2(totalWidth, totalHeight);
        UpdateVisibleItems(true);
    }

    protected virtual void Initialize()
    {
        totalItemCount = allData.Count;

        float totalWidth = content.sizeDelta.x;
        float totalHeight = content.sizeDelta.y;

        switch (scrollDirection)
        {
            case ScrollDirection.Horizontal:
                totalWidth = itemWidth * totalItemCount;
                break;
            case ScrollDirection.Vertical:
                totalHeight = itemHeight * totalItemCount;
                break;
        }

        content.sizeDelta = new Vector2(totalWidth, totalHeight);

        int poolSize = visibleCount + buffer;
        for (int i = 0; i < poolSize; i++)
        {
            TSlot slot = Instantiate(slotPrefab, content);
            slot.gameObject.SetActive(true);
            itemPool.Add(slot);
        }

        UpdateVisibleItems();
    }

    private void OnScrollValueChanged(Vector2 scrollPos)
    {
        UpdateVisibleItems();
    }

    protected virtual void UpdateVisibleItems(bool force = false)
    {
        int firstVisibleIndex = 0;

        switch (scrollDirection)
        {
            case ScrollDirection.Horizontal:
                float scrollX = -content.anchoredPosition.x;
                firstVisibleIndex = Mathf.FloorToInt(scrollX / itemWidth);
                break;
            case ScrollDirection.Vertical:
                float scrollY = content.anchoredPosition.y;
                firstVisibleIndex = Mathf.FloorToInt(scrollY / itemHeight);
                break;
        }

        firstVisibleIndex = Mathf.Clamp(firstVisibleIndex, 0, Mathf.Max(0, totalItemCount - 1));

        if (firstVisibleIndex == currentTopIndex && force == false)
            return;
        currentTopIndex = firstVisibleIndex;

        for (int i = 0; i < itemPool.Count; i++)
        {
            int dataIndex = firstVisibleIndex + i;

            if (dataIndex < 0 || dataIndex >= totalItemCount)
            {
                itemPool[i].gameObject.SetActive(false);
                continue;
            }

            itemPool[i].gameObject.SetActive(true);

            RectTransform rt = itemPool[i].GetComponent<RectTransform>();
            Vector2 anchoredPosition = Vector2.zero;

            switch (scrollDirection)
            {
                case ScrollDirection.Horizontal:
                    anchoredPosition = new Vector2(dataIndex * itemWidth, 0f);
                    break;
                case ScrollDirection.Vertical:
                    anchoredPosition = new Vector2(0f, -dataIndex * itemHeight);
                    break;
            }

            rt.anchoredPosition = anchoredPosition;
            SetSlotData(itemPool[i], allData[dataIndex], dataIndex);
        }
    }

    /// <summary>
    /// 슬롯 데이터 바인딩 (상속 클래스에서 구현)
    /// </summary>
    protected abstract void SetSlotData(TSlot slot, TData data, int index);

    public enum ScrollDirection
    {
        Horizontal,
        Vertical
    }
}
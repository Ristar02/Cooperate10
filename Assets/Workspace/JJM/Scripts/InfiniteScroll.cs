using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteScroll : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] ScrollRect scroll;
    [SerializeField] RectTransform content;
    [SerializeField] GameObject slotPrefab; //Content에 들어갈 슬롯 프리팹 (버튼, 텍스트 등)
    [SerializeField] int viewCnt = 8; // 모바일에서 화면에 보이는 슬롯 수 (세로 기준)

    int dataCnt; // 총 데이터 개수 (임시)
    int topIdx = 0;    // 현재 최상단 인덱스
    float slotH;
    List<RectTransform> slots = new List<RectTransform>();
    List<string> datas = new List<string>(); // 데이터 저장 (예: 아이템 이름)

    void Start()
    {
        slotH = 1920f / 8f;

        // 슬롯 프리팹 크기 조정
        var rtPrefab = slotPrefab.GetComponent<RectTransform>();
        rtPrefab.sizeDelta = new Vector2(rtPrefab.sizeDelta.x, slotH);

        // 슬롯 생성
        for (int i = 0; i < viewCnt + 2; i++)
        {
            var obj = Instantiate(slotPrefab, content);
            var rt = obj.GetComponent<RectTransform>();
            slots.Add(rt);
        }
    }

    void Update()
    {
        if (dataCnt == 0) return;

        float scrollY = content.anchoredPosition.y;
        int newTop = Mathf.FloorToInt(scrollY / slotH);

        if (newTop != topIdx)
        {
            topIdx = newTop;
            UpdateSlots();
        }
    }
    public void SetData(List<string> newData)
    {
        datas = newData;
        dataCnt = datas.Count;

        // 컨텐츠 높이 다시 계산
        content.sizeDelta = new Vector2(content.sizeDelta.x, dataCnt * slotH);

        // 초기 슬롯 배치
        topIdx = -1; // 강제로 업데이트 되게
        UpdateSlots();
    }
    void UpdateSlots() //스크롤 위치에 따라 슬롯 재배치 + 데이터 갱신
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int idx = topIdx + i;
            if (idx < 0 || idx >= dataCnt)
            {
                slots[i].gameObject.SetActive(false);
                continue;
            }

            slots[i].gameObject.SetActive(true);

            // Pivot 위쪽 기준
            slots[i].anchoredPosition = new Vector2(0, -idx * slotH);

            var txt = slots[i].GetComponentInChildren<Text>();
            if (txt != null) txt.text = datas[idx];
        }
    }
}

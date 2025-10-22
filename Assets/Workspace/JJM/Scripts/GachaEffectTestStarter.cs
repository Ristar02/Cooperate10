using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaEffectTestStarter : MonoBehaviour
{
    public GachaEffectController gachaEffectController;
    public UnitData testUnitData;

    public InfiniteScroll infiniteScroll;
    public PlayerInventory playerInventory;


    void Start()
    {
        // 씬 시작 시 자동 테스트
        gachaEffectController.RequestGachaEffect(testUnitData);

        // 2) PlayerInventory에 있는 유닛 이름 가져오기
        List<string> names = new List<string>();
        foreach (UnitData unit in playerInventory.ownedUnits)
        {
            names.Add(unit.Name); // UnitData 안에 unitName 필드 있다고 가정
        }

        // 3) 무한 스크롤에 전달
        infiniteScroll.SetData(names);
    }
}

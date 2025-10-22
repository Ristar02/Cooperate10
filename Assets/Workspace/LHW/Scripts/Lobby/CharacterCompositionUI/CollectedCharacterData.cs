using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 획득한 캐릭터의 개수 및 목록을 반환하기 위한 데이터 저장
/// </summary>
public class CollectedCharacterData : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CharacterUnit[] _charUnits;

    [SerializeField] private List<UnitStatus> _collectedUnit = new List<UnitStatus>();
    public List<UnitStatus> CollectedUnit => _collectedUnit;

    private int _characterCount;
    public int CharacterCount => _characterCount;

    private int _collectedCharacterCount;
    public int CollectedCharacterCount => _collectedCharacterCount;
    private void Awake()
    {
        _characterCount = _charUnits.Length;
    }

    // 임시 처리 - DB 연동이 끝나면 구조 변경 필요
    private void OnEnable()
    {
        //_collectedUnit.Clear();
        //for (int i = 0; i < _charUnits.Length; i++)
        //{
        //    // 획득여부 체크는 나중에 추가
        //    //if(_charUnits[i].IsCollected)
        //    //{
        //    //_collectedCharData.Add(_charUnits[i].CharData);

        //    _collectedUnit.Add(_charUnits[i].Status);
        //    //}
        //}
        
        //_collectedCharacterCount = _collectedUnit.Count;
    }
}
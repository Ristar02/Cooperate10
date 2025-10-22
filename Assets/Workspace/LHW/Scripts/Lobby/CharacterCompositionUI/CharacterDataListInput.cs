using System.Collections.Generic;
using UnityEngine;

// 현재 정상작동하지 않아 테스트중
public class CharacterDataListInput : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject[] _characterGroups;
    [SerializeField] private List<CharacterSO> _characterSOs = new List<CharacterSO>();

    private void Awake()
    {
        /*
        for(int i = 0; i < _characterGroups.Length; i++)
        {
            CharacterUnit[] data = _characterGroups[i].GetComponentsInChildren<CharacterUnit>();
            for(int j = 0; j < data.Length; j++)
            {
                data[j].InputData(_characterSOs[(i * data.Length) + (j)]);
            }
        }
        */
    }
}
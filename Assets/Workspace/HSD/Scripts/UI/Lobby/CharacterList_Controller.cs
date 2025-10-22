using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterList_Controller : MonoBehaviour
{
    [SerializeField] GameObject _characterListPrefab;
    [SerializeField] Transform _content;
    private CharacterList_UI[] _characterList_UIs;

    private void Start()
    {
        Setup();
    }

    public void Setup()
    {
        SynergyDatabase db = Manager.Data.SynergyDB;
        UnitData[] playerUnits = Manager.Data.PlayerUnitDatas;
        _characterList_UIs = new CharacterList_UI[(int)Synergy.Length - ((int)ClassType.SUPPORT + 1)];

        int count = 0;
        for (int i = (int)Synergy.KINGDOM; i < (int)Synergy.Length; i++)
        {
            CharacterList_UI characterList_UI = Instantiate(_characterListPrefab, _content).GetComponent<CharacterList_UI>();
            SynergyData synergy = db.GetSynergy(i);
            characterList_UI.Setup(synergy.SynergyName, System.Array.FindAll(playerUnits, unit => unit.Synergy == (Synergy)i));
            _characterList_UIs[count] = characterList_UI;
            count++;
        }
    }

    public void Sorting(SortingType sortingType)
    {
        switch (sortingType)
        {
            case SortingType.Power:
                PowerSortings();
                break;
            case SortingType.Grade:
                GradeSortings();
                break;
        }
    }

    private void PowerSortings()
    {
        for (global::System.Int32 i = 0; i < _characterList_UIs.Length; i++)
        {
            _characterList_UIs[i].PowerSorting();
        }
    }

    private void GradeSortings()
    {
        for (global::System.Int32 i = 0; i < _characterList_UIs.Length; i++)
        {
            _characterList_UIs[i].GradeSorting();
        }
    }
}

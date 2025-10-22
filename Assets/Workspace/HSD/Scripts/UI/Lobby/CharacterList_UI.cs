using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterList_UI : MonoBehaviour
{
    [SerializeField] TMP_Text _synergyName;
    [SerializeField] GameObject _characterUnit;
    [SerializeField] Transform _content;
    private CharacterUnit[] _synergyUnits;

    public void Setup(string synergyName, UnitData[] synergyUnits)
    {
        _synergyName.text = synergyName;

        _synergyUnits = new CharacterUnit[synergyUnits.Length];

        for (int i = 0; i < synergyUnits.Length; i++)
        {
            CharacterUnit characterUnit = Instantiate(_characterUnit, _content).GetComponent<CharacterUnit>();
            characterUnit.Setup(synergyUnits[i]);
            _synergyUnits[i] = characterUnit;
        }

        GradeSorting();
    }

    public void PowerSorting()
    {
        System.Array.Sort(_synergyUnits, (a, b) => b.Status.CombatPower.CompareTo(a.Status.CombatPower));

        for (int i = 0; i < _synergyUnits.Length; i++)
        {
            _synergyUnits[i].transform.SetSiblingIndex(i);
        }
    }

    public void GradeSorting()
    {
        System.Array.Sort(_synergyUnits, (a, b) =>
            ((int)b.Status.Data.Grade).CompareTo((int)a.Status.Data.Grade));

        for (int i = 0; i < _synergyUnits.Length; i++)
        {
            _synergyUnits[i].transform.SetSiblingIndex(i);
        }
    }
}

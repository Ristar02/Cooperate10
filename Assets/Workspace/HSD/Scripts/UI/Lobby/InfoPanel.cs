using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] TeamOrganizeManager _manager;
    [SerializeField] TMP_Text _costText;
    [SerializeField] TMP_Text _totalPowerText;
    [SerializeField] Button _sortingButton;
    [SerializeField] TMP_Text _sortingText;

    private void OnEnable()
    {
        _sortingButton.onClick.AddListener(_manager.Sorting);
        _manager.CurrentSortingType.AddEvent(SortingTextUpdate);
        _manager.CurrentCost.AddEvent(CostUpdate);
        _manager.CurrentOverallPower.AddEvent(TotalPowerUpdate);
    }

    private void OnDisable()
    {
        _sortingButton.onClick.RemoveListener(_manager.Sorting);
        _manager.CurrentSortingType.RemoveEvent(SortingTextUpdate);
        _manager.CurrentCost.RemoveEvent(CostUpdate);
        _manager.CurrentOverallPower.RemoveEvent(TotalPowerUpdate);
    }

    private void Start()
    {
        CostUpdate(_manager.CurrentCost.Value);
        TotalPowerUpdate(_manager.CurrentOverallPower.Value);
        SortingTextUpdate(_manager.CurrentSortingType.Value);
    }

    private void CostUpdate(int currentCost)
    {
        _costText.text = $"{currentCost} / {_manager.TotalCost}";
    }
    
    private void TotalPowerUpdate(int totalPower)
    {
        _totalPowerText.text = totalPower.ToString();
    }

    private void SortingTextUpdate(SortingType sortingType)
    {
        switch (sortingType)
        {
            case SortingType.Grade:
                _sortingText.text = "등급 순";
                break;
            case SortingType.Power:
                _sortingText.text = "전투력 순";
                break;
        }
    }
}

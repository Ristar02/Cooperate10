using UnityEngine;
using UnityEngine.UI;

public class StageSelectUnit : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] GameObject _partySelectPanel;

    [Header("Button")]
    [SerializeField] Button _selectButton;

    private void Start()
    {
        _selectButton.onClick.AddListener(SelectStage);
    }

    private void SelectStage()
    {
        _partySelectPanel.SetActive(true);
    }
}
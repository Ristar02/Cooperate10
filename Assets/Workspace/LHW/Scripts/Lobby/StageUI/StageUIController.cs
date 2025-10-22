using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageUIController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject _mapSelectPanel;
    [SerializeField] private GameObject _stageSelectPanel;
    
    [Header("Button")]
    [SerializeField] Button _mapSelectButton;
    [SerializeField] Button _gameStartButton;

    [Header("MapSelectUI")]
    [SerializeField] Image _mapImage;
    [SerializeField] TMP_Text _mapNameText;


    private int _mapIndex = 0;

    private void Start()
    {
        _mapSelectButton.onClick.AddListener(MapSelect);
        _gameStartButton.onClick.AddListener(StageSelect);
        MapUIUpdate(0);
    }

    #region UI Update

    public void MapUIUpdate(int index)
    {
        _mapIndex = index + 1;
        MapData data = Manager.Data.MapDB.ReturnMapData(index);
        _mapImage.sprite = data.MapImage;
        _mapNameText.text = data.MapName;
    }

    #endregion

    #region Button Click

    private void MapSelect()
    {
        _mapSelectPanel.SetActive(true);
    }

    private void StageSelect()
    {
        _stageSelectPanel.SetActive(true);
    }

    #endregion
}
using UnityEngine;
using UnityEngine.UI;

public class LobbyButtonController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _shopButton;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private Button _stageButton;
    [SerializeField] private Button _characterCompositionButton;
    [SerializeField] private Button _gachaButton;
    [SerializeField] private Button _gameStartButton;
    [SerializeField] private Button[] _stageSelectButtons;
    [SerializeField] private Button _rearrangeButton;
    [SerializeField] private Button[] _disabledPartyButtons;
    [SerializeField] private Button _mapSelectButton;
    [SerializeField] private Button _mapBackButton;
    [SerializeField] private Button _mapSelectConfirmButton;

    [Header("Panels")]
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private GameObject _stagePanel;
    [SerializeField] private GameObject _characterCompositionPanel;
    [SerializeField] private GameObject _gachaPanel;
    [SerializeField] private GameObject _stageSelectPanel;
    [SerializeField] private GameObject _partySelectPanel;
    [SerializeField] private GameObject _mapSelectPanel;

    private void Start()
    {
        _shopButton.onClick.AddListener(ShopOpen);
        _upgradeButton.onClick.AddListener(UpgradeOpen);
        _stageButton.onClick.AddListener(StageOpen);
        _characterCompositionButton.onClick.AddListener(CharacterCompositionOpen);
        _gachaButton.onClick.AddListener(GachaOpen);
        _gameStartButton.onClick.AddListener(StageSelectOpen);
        for(int i = 0; i < _stageSelectButtons.Length; i++)
        {
            _stageSelectButtons[i].onClick.AddListener(PartySelectOpen);
        }
        _rearrangeButton.onClick.AddListener(CharacterCompositionOpen);
        for(int i = 0; i < _disabledPartyButtons.Length; i++)
        {
            _disabledPartyButtons[i].onClick.AddListener(CharacterCompositionOpen);
        }
        _mapSelectButton.onClick.AddListener(MapSelectOpen);
        _mapBackButton.onClick.AddListener(StageOpen);
        _mapSelectConfirmButton.onClick.AddListener(StageOpen);

        StageOpen();
    }

    private void ShopOpen()
    {
        SetActivePanel("ShopPanel");
    }

    private void UpgradeOpen()
    {
        SetActivePanel("UpgradeUIPanel");
    }

    private void StageOpen()
    {
        SetActivePanel("StageUIPanel");
    }

    private void StageSelectOpen()
    {
        SetActivePanel("StageSelectPanel");
    }

    private void PartySelectOpen()
    {
        SetActivePanel("PartySelectPanel");
    }

    private void CharacterCompositionOpen()
    {
        SetActivePanel("CharacterCompositionUIPanel");
    }

    private void GachaOpen()
    {
        SetActivePanel("GachaUIPanel");
    }

    private void MapSelectOpen()
    {
        SetActivePanel("MapSelectPanel");
    }

    private void SetActivePanel(string activePanel)
    {
        _shopPanel.SetActive(activePanel.Equals(_shopPanel.name));
        _upgradePanel.SetActive(activePanel.Equals(_upgradePanel.name));
        _stagePanel.SetActive(activePanel.Equals(_stagePanel.name));
        _characterCompositionPanel.SetActive(activePanel.Equals(_characterCompositionPanel.name));
        _gachaPanel.SetActive(activePanel.Equals(_gachaPanel.name));
        _stageSelectPanel.SetActive(activePanel.Equals(_stageSelectPanel.name));
        _partySelectPanel.SetActive(activePanel.Equals(_partySelectPanel.name));
        _mapSelectPanel.SetActive(activePanel.Equals(_mapSelectPanel.name));
    }
}
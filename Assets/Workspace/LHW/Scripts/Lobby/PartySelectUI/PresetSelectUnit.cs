using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class PresetSelectUnit : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Sprite _xImage;

    [Header("Index")]
    [SerializeField] private int _index;

    [Header("Buttons")]
    [SerializeField] private GameObject _activePartyButton;
    [SerializeField] private GameObject _disabledPartyButton;
    [SerializeField] private GameObject _lockedPartyButton;
    [SerializeField] private GameObject _highlightPanel;

    [Header("UI")]
    [SerializeField] private Image[] _presetImages;
    [SerializeField] private TMP_Text _leaderName;
    [SerializeField] private TMP_Text _partyDamageText;
    [SerializeField] private TMP_Text _synergy1Text;
    [SerializeField] private TMP_Text _synergy2Text;

    private PartySelectPanelController _controller;

    private void Awake()
    {
        _controller = GetComponentInParent<PartySelectPanelController>();
        _activePartyButton.GetComponent<Button>().onClick.AddListener(SelectPreset);
        _disabledPartyButton.GetComponent<Button>().onClick.AddListener(RearrangePreset);
        _lockedPartyButton.GetComponent<Button>().onClick.AddListener(UnlockPreset);
    }

    private void OnEnable()
    {
        Init();
        _controller.OnSelectedIndexChanged += ActiveHighlight;
    }

    private void OnDisable()
    {
        _controller.OnSelectedIndexChanged -= ActiveHighlight;
    }

    private void Init()
    {
        if (Manager.Data == null) return;

        _controller.SetSelectedPresetIndex(-1);

        // 프리셋이 활성화가 안 되어 있으면 잠겨 있다고 표시하는 UI 출력
        if(Manager.Data.PresetDB.PresetData.Count < _index + 1)
        {
            SetactiveGameobject("LockedPartyButton");
            if(Manager.Data.PresetDB.PresetData.Count < _index)
            {
                _lockedPartyButton.GetComponent<Button>().interactable = false;
            }
            return;
        }

        // 프리셋이 세팅되어 있지 않으면(리더가 없는 상태이면) 프리셋 추가 UI 출력
        if (Manager.Data.PresetDB.PresetData[_index].Statuses[0].Data == null)
        {
            SetactiveGameobject("DisabledPartyButton");
            return;
        }

        // 프리셋이 사용 가능한 상태이면 하단의 과정 진행
        SetactiveGameobject("ActivePartyButton");
        UpdateUI();
        ActiveHighlight();
    }

    #region UI Update

    private void UpdateUI()
    {
        UnitStatus[] preset = Manager.Data.PresetDB.PresetData[_index].Statuses;
        int damage = 0;

        bool activePresetExists = false;

        for (int i = 0; i < preset.Length; i++)
        {
            if (preset[i].Data != null)
            {
                if(!activePresetExists)
                {
                    _controller.SetSelectedPresetIndex(i);
                    activePresetExists = true;
                }

                _presetImages[i].color = Color.white;
                _presetImages[i].sprite = preset[i].Data.Icon;
                damage += preset[i].CombatPower;
            }
            else
            {
                _presetImages[i].color = Color.black;
                _presetImages[i].sprite = _xImage;
            }
        }
        _leaderName.text = $"리더 : {preset[0].Data.Name}";
        _partyDamageText.text = $"파티 전투력 : {damage}";
        // 시너지 입력 방식은 시너지 활성화 기능 구현 이후 진행
        _synergy1Text.text = preset[0].Data.Synergy.ToString();
        _synergy2Text.text = preset[0].Data.ClassSynergy.ToString();
    }

    private void SetactiveGameobject(string activeObject)
    {
        _activePartyButton.SetActive(activeObject.Equals(_activePartyButton.name));
        _disabledPartyButton.SetActive(activeObject.Equals(_disabledPartyButton.name));
        _lockedPartyButton.SetActive(activeObject.Equals(_lockedPartyButton.name));
    }

    public void ActiveHighlight()
    {
        if (_index == Manager.Data.PresetDB.SelectedPresetIndex)
        {
            _highlightPanel.SetActive(true);
        }
        else
        {
            _highlightPanel.SetActive(false);
        }
    }

    #endregion

    #region Button Event

    private void SelectPreset()
    {
        _controller.SetSelectedPresetIndex(_index);        
    }

    private void RearrangePreset()
    {
        _controller.ArrangePreset();
    }

    private void UnlockPreset()
    {
        if (Manager.Data.PresetDB.PresetData.Count < _index + 1)
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.instance.ShowConfirmationPopup("프리셋을 추가하시겠습니까?\n500 골드 소모", () => CreatePreset(), null);
            }
        }
    }

    private void CreatePreset()
    {
        // TODO : 금액이 부족할 시에 조건 추가

        Debug.Log("Used 500 Gold");
        Manager.Data.PresetDB.CreatePreset(5);
        SetactiveGameobject("DisabledPartyButton");
    }

    #endregion
}
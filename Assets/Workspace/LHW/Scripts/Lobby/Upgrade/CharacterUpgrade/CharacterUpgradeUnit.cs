using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUpgradeUnit : MonoBehaviour
{
    [Header("Data Input")]
    private UnitStatus _status;
    public UnitStatus Status => _status;
    [SerializeField] UnitData _unitData;

    [Header("UI")]
    [SerializeField] private TMP_Text _charText;
    [SerializeField] private Image _characterImg;
    [SerializeField] private Image _roleSynergyImg;
    [SerializeField] private TMP_Text _levelText;

    private UpgradeManager _manager;

    private bool _isCollected = true;
    public bool IsCollected => _isCollected;

    [SerializeField] private float _requiredPointerDownTime = 2f;
    private Coroutine _holdCoroutine;

    private void Awake()
    {
        //DataInit();
        GetComponent<Button>().onClick.AddListener(ShowPopUp);
        _manager = GetComponentInParent<UpgradeManager>();
    }

    private void Start()
    {
        UIUpdate();
    }

    private void DataInit()
    {
        _status = new UnitStatus(_unitData, 1);
    }

    private void OnEnable()
    {
        if (_manager != null)
        {
            _manager.PopUpUI.OnCharacterStatusChanged += UIUpdate;
            UIUpdate();
        }
    }

    private void OnDisable()
    {
        if (_manager != null) _manager.PopUpUI.OnCharacterStatusChanged -= UIUpdate;
    }

    #region Onclick

    private void ShowPopUp()
    {
        _manager.PopUpUI.GetCurrentCharacterUnitData(this);
        _manager.ShowCharacterPopUp();
    }

    #endregion

    #region UI Update

    private void UIUpdate()
    {
        if(_status == null) return;

        _charText.text = $"{_status.Data.Name}";
        _characterImg.sprite = _status.Data.Icon;
        if (Manager.Data.SynergyDB != null)
        {
            _roleSynergyImg.sprite = Manager.Data.SynergyDB.GetSynergy((int)_status.Data.ClassSynergy).Icon;
        }
        _levelText.text = $"Lv.{_status.Data.UpgradeData.CurrentUpgradeData.UpgradeLevel}";
    }


    #endregion

    #region Data Input

    // 데이터 입력 관련 메소드
    public void InitUnitStatus(UnitData data)
    {
        _status = new UnitStatus(data, data.UpgradeData.CurrentUpgradeData.UpgradeLevel);
    }

    #endregion
}

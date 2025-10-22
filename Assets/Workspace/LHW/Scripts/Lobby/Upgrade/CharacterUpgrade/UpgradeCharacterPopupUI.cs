using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCharacterPopupUI : MonoBehaviour
{
    [Header("Character Profile")]
    [SerializeField] private TMP_Text _gradeText;
    [SerializeField] private TMP_Text _characterNameText;
    [SerializeField] private TMP_Text _characterLevelText;
    [SerializeField] private Image _characterImage;
    [SerializeField] private Image _costImage;
    [SerializeField] private Image _synergyImage;
    [SerializeField] private Image _classImage;

    [SerializeField] private ImageSO _costImages;

    [Header("Character Status")]
    [SerializeField] private TMP_Text[] _statuses;

    [Header("Character Skill Info")]
    [SerializeField] private Image _skillIcon;
    [SerializeField] private TMP_Text _skillNameText;
    [SerializeField] private TMP_Text _skillDescriptionText;

    [Header("Character Upgrade Info")]
    [SerializeField] private TMP_Text[] _characterUpgradeLevelText;
    [SerializeField] private TMP_Text[] _characterUpgradeDescriptionText;
    [SerializeField] private GameObject[] _upgradeStatusDisablePanel;

    [Header("Level Up Button UI")]
    [SerializeField] private TMP_Text _pieceText;
    [SerializeField] private Image _pieceGauge;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _openPieceText;
    [SerializeField] private Image _openPieceGauge;

    [Header("Button")]
    [SerializeField] private Button _levelUpButton;
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _closeButton;

    private CharacterUpgradeUnit _currentCharUnit;

    public Action OnCharacterStatusChanged;

    private void Awake()
    {
        _closeButton.onClick.AddListener(CloseUI);
        _levelUpButton.onClick.AddListener(LevelUp);
        _openButton.onClick.AddListener(LevelUp);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        OnCharacterStatusChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDisable()
    {
        OnCharacterStatusChanged -= UpdateUI;
    }

    #region Read Data

    public void GetCurrentCharacterUnitData(CharacterUpgradeUnit charUnit)
    {
        _currentCharUnit = charUnit;
    }

    #endregion

    #region Level Up

    private async void LevelUp()
    {
        if (_currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.UpgradeLevel >= 10)
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.instance.ShowPopup("이미 최대 레벨입니다.");
                return;
            }
        }

        bool success = await _currentCharUnit.Status.Data.UpgradeData.LevelUpWithPiecesOnly();

        if (!success && _currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.UpgradeLevel >= 1)
        {
            bool canUseMyth = await _currentCharUnit.Status.Data.UpgradeData.CanLevelUpWithMythStone();

            if (!canUseMyth)
            {
                if (PopupManager.Instance != null)
                {
                    PopupManager.instance.ShowPopup("조각이 부족합니다.");
                    return;
                }
            }

            if (PopupManager.Instance != null)
            {
                PopupManager.instance.ShowConfirmationPopup(
                    "조각이 부족합니다. 신화석을 사용해서 레벨업 하시겠습니까?",
                    async () =>
                    {
                        await _currentCharUnit.Status.Data.UpgradeData.LevelUpWithMythStone();
                        await DBManager.Instance.charDB.SaveCharacterUpgradeData(_currentCharUnit.Status.Data);
                        OnCharacterStatusChanged?.Invoke();

                    },
                    () =>
                    {
                        PopupManager.instance.ShowPopup("레벨업이 취소되었습니다.");
                    }
                );
            }
        }
        else
        {
            await DBManager.Instance.charDB.SaveCharacterUpgradeData(_currentCharUnit.Status.Data);
            OnCharacterStatusChanged?.Invoke();
        }
    }

    #endregion

    #region Update UI

    private void UpdateUI()
    {
        if (_currentCharUnit != null && _currentCharUnit.Status.Data.UpgradeData != null && _currentCharUnit.Status.Data.LevelUpData != null)
        {
            // UI 표기
            CharacterProfileUpdate();
            CharacterStatusUpdate();
            LevelUpButtonUpdate();
            SkillUIUpdate();
            StatusUpgradeUIUpdate();
        }
    }

    private void CharacterProfileUpdate()
    {
        _gradeText.text = _currentCharUnit.Status.Data.Grade.ToString();
        _characterNameText.text = _currentCharUnit.Status.Data.Name;
        _characterLevelText.text = $"LV.{_currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.UpgradeLevel.ToString()}";
        _characterImage.sprite = _currentCharUnit.Status.Data.Icon;
        _costImage.sprite = _costImages.CostSprites[_currentCharUnit.Status.Data.Cost - 1];
        if (Manager.Data.SynergyDB != null)
        {
            _synergyImage.sprite = Manager.Data.SynergyDB.GetSynergy((int)_currentCharUnit.Status.Data.Synergy).Icon;
            _classImage.sprite = Manager.Data.SynergyDB.GetSynergy((int)_currentCharUnit.Status.Data.ClassSynergy).Icon;
        }
    }

    private void CharacterStatusUpdate()
    {
        int level = _currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.UpgradeLevel;
        _statuses[0].text = _currentCharUnit.Status.Data.UpgradeStats[0].MaxHealth.ToString();
        _statuses[1].text = _currentCharUnit.Status.Data.UpgradeStats[0].MaxMana.ToString();
        _statuses[2].text = _currentCharUnit.Status.Data.UpgradeStats[0].PhysicalDamage.ToString();
        _statuses[3].text = _currentCharUnit.Status.Data.UpgradeStats[0].MagicDamage.ToString();
        _statuses[4].text = _currentCharUnit.Status.Data.UpgradeStats[0].PhysicalDefense.ToString();
        _statuses[5].text = _currentCharUnit.Status.Data.UpgradeStats[0].MagicDefense.ToString();
        _statuses[6].text = _currentCharUnit.Status.Data.UpgradeStats[0].AttackRange.ToString();
        _statuses[7].text = _currentCharUnit.Status.Data.UpgradeStats[0].AttackSpeed.ToString();
        _statuses[8].text = _currentCharUnit.Status.Data.UpgradeStats[0].CritChance.ToString();
        _statuses[9].text = _currentCharUnit.Status.CombatPower.ToString();
    }

    private void LevelUpButtonUpdate()
    {
        if (_currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.UpgradeLevel == 0)
        {
            _openButton.gameObject.SetActive(true);
            _levelUpButton.gameObject.SetActive(false);
        }
        else
        {
            _openButton.gameObject.SetActive(false);
            _levelUpButton.gameObject.SetActive(true);
        }

        int requirePiece = _currentCharUnit.Status.Data.UpgradeData.GetRequiredPiece();
        int requireGold = _currentCharUnit.Status.Data.UpgradeData.GetRequiredGold();
        if (_currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.CurrentPieces == 0)
        {
            _pieceGauge.fillAmount = 0;
            _openPieceGauge.fillAmount = 0;
        }
        else
        {
            _pieceGauge.fillAmount = (float)_currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.CurrentPieces / requirePiece;
            _openPieceGauge.fillAmount = (float)_currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.CurrentPieces / 10;
        }
        _pieceText.text = $"{_currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.CurrentPieces}/{requirePiece}";
        _goldText.text = requireGold.ToString();
        _openPieceText.text = $"{_currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.CurrentPieces}/10";
    }

    private void SkillUIUpdate()
    {
        _skillIcon.sprite = _currentCharUnit.Status.Data.Skill.Icon;
        _skillNameText.text = _currentCharUnit.Status.Data.Skill.SkillName;
        _skillDescriptionText.text = _currentCharUnit.Status.Data.Skill.Description;
    }

    private void StatusUpgradeUIUpdate()
    {
        for (int i = 0; i < _characterUpgradeLevelText.Length; i++)
        {
            _characterUpgradeLevelText[i].text = $"LV.{2 * (i + 1)}";

            List<StatusGrowth> statusGrowths = _currentCharUnit.Status.Data.UpgradeStatData.GetCurrentStatusData(_currentCharUnit.Status.Data.Grade, 2 * (i + 1));

            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < statusGrowths.Count; j++)
            {
                sb.Append($"{StatTypeTranslate(statusGrowths[j].Type)} {statusGrowths[j].Value} ");
                if (j != statusGrowths.Count - 1 && j % 2 == 1) sb.Append("\n");
            }

            sb.Append("증가");

            _characterUpgradeDescriptionText[i].text = sb.ToString();
        }

        ActiveUpgradeStatusInfo();
    }

    private void ActiveUpgradeStatusInfo()
    {
        int currentUpgradeLevel = _currentCharUnit.Status.Data.UpgradeData.CurrentUpgradeData.UpgradeLevel / 2;

        for (int i = 0; i < currentUpgradeLevel; i++)
        {
            _upgradeStatusDisablePanel[i].SetActive(false);
        }
        for (int i = currentUpgradeLevel; i < _upgradeStatusDisablePanel.Length; i++)
        {
            _upgradeStatusDisablePanel[i].SetActive(true);
        }
    }

    private string StatTypeTranslate(StatType type)
    {
        string text = "";

        switch (type)
        {
            case StatType.MaxHealth: text = "최대체력"; break;
            case StatType.MaxMana: text = "최대마나"; break;
            case StatType.ManaGain: text = "마나 회복속도"; break;
            case StatType.AttackSpeed: text = "공격속도"; break;
            case StatType.MoveSpeed: text = "공격속도"; break;
            case StatType.PhysicalDamage: text = "물리공격력"; break;
            case StatType.MagicDamage: text = "마법공격력"; break;
            case StatType.CritChance: text = "크리티컬확률"; break;
            case StatType.CritDamage: text = "크리티컬 데미지"; break;
            case StatType.PhysicalDefense: text = "물리방어력"; break;
            case StatType.MagicDefense: text = "마법방어력"; break;
            case StatType.Shield: text = "쉴드"; break;
            case StatType.AttackRange: text = "공격범위"; break;
            case StatType.AttackCount: text = "공격횟수"; break;
            case StatType.CurHp: text = "현재체력"; break;
            case StatType.CurMana: text = "현재마나"; break;
        }
        return text;
    }

    #endregion

    #region CloseUI

    private void CloseUI()
    {
        gameObject.SetActive(false);
    }

    #endregion
}
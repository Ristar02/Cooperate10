using TMPro;
using UnityEngine;

public class UpgradeStonePopupUI : MonoBehaviour
{
    [Header("Stone Info")]
    [SerializeField] private TMP_Text _stoneLevelText;
    [SerializeField] private TMP_Text _stoneNameText;
    [SerializeField] private TMP_Text _stoneDescriptionText;
    [SerializeField] private TMP_Text _stoneEffectText;
    [SerializeField] private TMP_Text _stoneProbableAddText;
    [SerializeField] private TMP_Text[] _stoneProbleText;

    private void OnEnable()
    {
        UpdateStoneInfo();
        UpdateStoneProbable();
    }

    private void UpdateStoneInfo()
    {
        // 마법석 정보 표시
    }

    private void UpdateStoneProbable()
    {
        // 마법석 현재 적용 확률 표시
    }
}

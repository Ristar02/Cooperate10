using UnityEngine;
using TMPro;

public class ReceiveAllPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _diamondText;

    public void SetGoldInfo(int gold)
    {
        _goldText.text = $"{gold}";
    }

    public void SetDiamondInfo(int diamond)
    {
        _diamondText.text = $"{diamond}";
    }
}
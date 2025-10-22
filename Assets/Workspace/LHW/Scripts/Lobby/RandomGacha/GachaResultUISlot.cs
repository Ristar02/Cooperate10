using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultUISlot : MonoBehaviour
{
    [SerializeField] Image _resultImage;
    [SerializeField] TMP_Text _resultText;

    public void UpdateUI(Sprite sprite, string amount)
    {
        _resultImage.sprite = sprite;
        _resultText.text = amount;
    }
}

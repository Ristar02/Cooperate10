using TMPro;
using UnityEngine;

public class GoldPanel : MonoBehaviour
{
    [SerializeField] TMP_Text _goldText;

    private void OnEnable()
    {
        InGameManager.Instance.Gold.AddEvent(GoldTextUpdate);
        GoldTextUpdate(InGameManager.Instance.Gold.Value);
    }

    private void OnDisable()
    {
        InGameManager.Instance?.Gold.RemoveEvent(GoldTextUpdate);
    }

    private void GoldTextUpdate(int amount)
    {
        _goldText.text = amount.ToString();
    }
}

using TMPro;
using UnityEngine;

public class UpgradeSynergyTextUI : MonoBehaviour
{
    [SerializeField] CharacterUpgradeUnit _character;

    private TMP_Text _text;
    private void Start()
    {
        _text = GetComponent<TMP_Text>();
        if (_character != null && _character.Status != null) _text.text = _character.Status.Data.Synergy.ToString();
    }
}
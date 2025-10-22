using TMPro;
using UnityEngine;

public class SynergyTextUI : MonoBehaviour
{
    [SerializeField] CharacterUnit _character;

    private TMP_Text _text;
    private void Start()
    {
        _text = GetComponent<TMP_Text>();
        if(_character != null && _character.Status != null) _text.text = _character.Status.Data.ClassSynergy.ToString();
    }
}

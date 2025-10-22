using UnityEngine;

public class UpgradeCharacterDataInput : MonoBehaviour
{
    private CharacterUpgradeUnit[] units;
    [SerializeField] UnitData[] datas;

    private void Awake() => Init();

    private void Init()
    {
        units = GetComponentsInChildren<CharacterUpgradeUnit>();
        for(int i = 0; i < units.Length; i++)
        {
            units[i].InitUnitStatus(datas[i]);
        }
    }
}
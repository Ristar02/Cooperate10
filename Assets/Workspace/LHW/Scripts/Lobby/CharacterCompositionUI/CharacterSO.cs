using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName ="CharacterData", menuName = "Data/CharacterData")]
public class CharacterSO : ScriptableObject
{
    [field:SerializeField] public TestSynergy CharacterSynergy {  get; private set; }
    [field:SerializeField] public int Cost { get; private set; }
    [field:SerializeField] public Sprite CostImg { get; private set; }
    [field:SerializeField] public int OverallPower { get; private set; }
    [field:SerializeField] public string LeaderEffectDescription { get; private set; }
    [field:SerializeField] public Sprite CharacterImage { get; private set; }
    [field:SerializeField] public int Level { get; private set; }
}

[Serializable]
public struct TestSynergy
{
    public Sprite JobSynergy;
    public Sprite RoleSynergy;
}
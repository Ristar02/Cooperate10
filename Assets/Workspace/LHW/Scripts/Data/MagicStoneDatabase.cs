using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MagicStoneDatabase", menuName = "Data/MagicStoneDatabase")]
public class MagicStoneDatabase : ScriptableObject
{
    public List<MagicStoneData> MagicStoneDatas;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MagicStoneGachaSO", menuName ="Data/MagicStoneGachaSO")]
public class MagicStoneGachaSO : ScriptableObject
{
    public List<RewardEntry> rewards;
}

[Serializable]
public class RewardEntry
{
    public MagicStoneRewardType rewardType;
    public float RewardProbability;
    public List<PieceEntry> PieceEntries;
}

[Serializable]
public class PieceEntry
{
    public int PieceNum;
    public float PieceProbability;
}

public enum MagicStoneRewardType
{
    MagicStone,
    Gold500,
    Gold1000,
    Gold10000,
    Dia50,
    Dia100,
    Dia1000
}
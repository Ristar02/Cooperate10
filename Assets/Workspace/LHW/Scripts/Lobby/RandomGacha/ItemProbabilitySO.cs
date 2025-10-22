using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Probability", menuName ="Data/Probability")]
public class ItemProbabilitySO : ScriptableObject
{
    [field:SerializeField] public List<ProbableItems> ItemsProbability { get; private set; }
}

[Serializable]
public class ProbableItems
{
    public Grade ItemGrade;
    public float Probability;
    public List<ProbablePiece> Pieces;
}

[Serializable]
public class ProbablePiece
{
    public int PieceNum;
    public float PieceProbability;
}
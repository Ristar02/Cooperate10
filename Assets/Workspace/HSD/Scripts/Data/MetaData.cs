using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MetaData : ScriptableObject
{
    [Header("MetaData")]
    public Sprite Icon;
    public string Name;
    [TextArea]
    public string Description;
}

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectable
{
    void ApplyEffect(BuffEffectData buffEffectData, float value, string source);
}

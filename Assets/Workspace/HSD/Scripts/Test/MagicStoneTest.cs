using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicStoneTest : MonoBehaviour
{
    [SerializeField] MagicStonePanel _magicStonePanel;
    [SerializeField] MagicStoneData _testData;

#if UNITY_EDITOR
    [Button("Set Magic Stone")]
#endif
    public void SetMagicStone()
    {
        for (int i = 0; i < 3; i++)
            _magicStonePanel.SetMagicStone(i, _testData);
    }
}


using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class AddressableLoadData
{
    public AssetLabelReference[] labels;

    public async UniTask AssetsLoad()
    {
        await Manager.Resources.LoadAllLabel(labels);         
    }

    public async UniTask AssetsUnload()
    {
        await Manager.Resources.UnloadAllLabel(labels);
    }
}

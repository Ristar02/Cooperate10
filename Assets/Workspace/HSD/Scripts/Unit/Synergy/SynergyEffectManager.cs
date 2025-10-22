using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SynergyEffectManager : InGameSingleton<SynergyEffectManager>
{
    public Vector3 Center;
    
    public GlobalPassiveController GlobalPassiveController { get; private set; }

    private void Init()
    {
        GlobalPassiveController = new GlobalPassiveController(Center);
    }

    public void SetCenter(Vector3 center)
    {
        Center = center;
        Center -= new Vector3(0, 25, 0);
        Init();
    }
}

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TestAddressableLoad : MonoBehaviour
{
    [SerializeField] Unit_InitTest _initTest;

    private void Start()
    {
        TestInit().Forget();
    }

    private async UniTask TestInit()
    {
        await Manager.Resources.LoadLabel("Stage");
        await Manager.Resources.LoadLabel("Test");
        _initTest.Init();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyDataInit : MonoBehaviour
{
    private SynergyController _controller;
    private void Awake()
    {
        _controller = GetComponentInChildren<SynergyController>();
        _controller.Init();
    }
}

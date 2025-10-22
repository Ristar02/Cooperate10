using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOutline : MonoBehaviour
{
    public Button summonButton;
    public Behaviour outlineable;
    public float outlineOnThick = 2f;
    public Color outlineColor = Color.red;

    private Tween pulse;

    private void Awake()
    {
        // 나중에 비활성화 버튼 붙이기
    }

    private void Start()
    {
        
    }
}

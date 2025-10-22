using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    private Vector3 _battlePosition;
    private Vector3 _enemyPosition;

    private void Awake()
    {
        _battlePosition = transform.position;
    }

    public void MoveToEnemy()
    {
        transform.DOMove(_enemyPosition, .3f);
    }

    public void MoveToBattle()
    {
        transform.DOMove(_battlePosition, .3f);
    }
}
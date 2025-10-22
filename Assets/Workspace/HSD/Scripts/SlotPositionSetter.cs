using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotPositionSetter : MonoBehaviour
{
    [SerializeField] VerticalSwipePager _swipePager;

    [Header("Center")]
    [SerializeField] Transform _center;
    [Space]

    [Header("Slots")]
    [SerializeField] GameObject _battleSlot;
    [SerializeField] GameObject _planSlot;
    [SerializeField] GameObject _enemySlot;
    [Space]

    [SerializeField] float _zOffset = 10f;
    [SerializeField] float _distance = 3;
    [SerializeField] float _battleSlotRatio = 0.65f;
    [SerializeField] float _unitSlotRatio = 0.75f;

    [ContextMenu("Setting_Position")]
    public void SetPositions()
    {
        Rect safe = Screen.safeArea;

        float centerX = safe.x + safe.width * 0.5f;
        float centerYInSafe = safe.y + safe.height * _unitSlotRatio;
        float centerYNextSafe = safe.y + safe.height * _battleSlotRatio;

        float screenZ = Mathf.Abs(Camera.main.transform.position.z + _zOffset);

        Vector3 planScreen = new Vector3(centerX, centerYInSafe + Screen.height * 0, screenZ);
        Vector3 battleScreen = new Vector3(centerX, centerYNextSafe + Screen.height * 1, screenZ);
        Vector3 enemyScreen = new Vector3(centerX + safe.width * _distance, centerYNextSafe + Screen.height * 1, screenZ);

        Vector3 planWorld = Camera.main.ScreenToWorldPoint(planScreen);
        Vector3 battleWorld = Camera.main.ScreenToWorldPoint(battleScreen);
        Vector3 enemyWorld = battleWorld + Vector3.right * _distance;
        Vector3 centerWorld = battleWorld + Vector3.right * (_distance / 2);

        _planSlot.transform.position = planWorld;
        _battleSlot.transform.position = battleWorld;
        _enemySlot.transform.position = enemyWorld;
        _center.position = centerWorld;

        _swipePager.XLimit = _distance;

        SynergyEffectManager.Instance.SetCenter(_center.position);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ZoomController : MonoBehaviour
{
    [Header("Zoom Target (필요시 코드에서 할당 가능)")]
    public RectTransform zoomTarget;

    [Header("Zoom Settings")]
    public float zoomInScale = 1f;    // 줌인(원래 크기)
    public float zoomOutScale = 0.7f; // 줌아웃(작아진 크기)
    public float zoomDuration = 0.4f;


    private bool isZoomedOut = false;
    // 전투개시 버튼에서 호출 (토글 방식)
    public void ToggleZoom()
    {
        if (zoomTarget == null) return;

        if (isZoomedOut)
        {
            ZoomIn();
        }
        else
        {
            ZoomOut();
        }
        isZoomedOut = !isZoomedOut;

        // 버튼 클릭 시 포커스 해제
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    // 전투개시 버튼에서 호출
    public void ZoomOut()
    {
        if (zoomTarget != null)
            zoomTarget.DOScale(zoomOutScale, zoomDuration).SetEase(Ease.InOutQuad);

        // 버튼 클릭 시 포커스 해제
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
    // 전투 종료 시 호출
    public void ZoomIn()
    {
        Debug.Log("ZoomIn called");
        if (zoomTarget != null)
            zoomTarget.DOScale(zoomInScale, zoomDuration).SetEase(Ease.InOutQuad);
        else
            Debug.LogWarning("zoomTarget is not assigned!");
    }
    

    // 코드에서 타겟을 동적으로 할당할 수 있도록 메서드 추가
    public void SetZoomTarget(RectTransform target)
    {
        zoomTarget = target;
    }
}


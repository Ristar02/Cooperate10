using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public class GachaEffectController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text operIntroText;
    public TMP_Text operNameText;
    public TMP_Text operRoleText;
    public Image operBg;
    public Image operProfile;
    public GameObject sterEffect;
    public Button nextButton;

    private Queue<Action> gachaEffectQueue = new Queue<Action>();
    private bool isPlaying = false;
    private UnitData currentUnitData;

    // 외부에서 연출 요청
    public void RequestGachaEffect(UnitData unitData)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        gachaEffectQueue.Enqueue(() =>
        {
            currentUnitData = unitData;
            GachaEffectUnitInfoSetting(currentUnitData);
            PlayEffectSequence();
        });

        TryPlayNextEffect();
    }

    // 연출 실행 조건
    private void TryPlayNextEffect()
    {
        if (isPlaying || gachaEffectQueue.Count == 0) return;

        isPlaying = true;
        var next = gachaEffectQueue.Dequeue();
        next.Invoke();
    }

    // 다음 연출 버튼
    public void OnClickNextButton()
    {
        TryPlayNextEffect();

        if (gachaEffectQueue.Count == 0)
            gameObject.SetActive(false);
    }

    // 정보 설정
    private void GachaEffectUnitInfoSetting(UnitData unitData)
    {
        operIntroText.text = unitData.Description;
        operNameText.text = unitData.Name;
        SetRole(unitData != null ? unitData.ClassSynergy : null);
        OperIconSetting(unitData);
    }

    // 직업 텍스트 설정
    private void SetRole(ClassType? classType)
    {
        if (classType == null)
        {
            operRoleText.text = "";
            return;
        }

        switch (classType)
        {
            case ClassType.TANK: operRoleText.text = "탱커"; break;
            case ClassType.MELEE: operRoleText.text = "근접형"; break;
            case ClassType.RANGED: operRoleText.text = "원거리형"; break;
            case ClassType.SUPPORT: operRoleText.text = "지원형"; break;
            default: operRoleText.text = ""; break;
        }
    }

    // 유닛 아이콘 설정
    private void OperIconSetting(UnitData unitData)
    {
        // 인벤토리에서 해당 유닛을 소유하고 있는지 확인 (예시: ownedUnits 리스트)
        var playerInventory = FindObjectOfType<PlayerInventory>();
        var inventory = playerInventory != null ? playerInventory.ownedUnits : null;


        if (inventory != null && inventory.Contains(unitData))
        {
            Sprite unitSprite = unitData.Icon;
            operBg.sprite = unitSprite;
            operProfile.sprite = unitSprite;
        }
        else
        {
            // 소유하지 않은 경우 기본 이미지 처리
            operBg.sprite = null;
            operProfile.sprite = null;
        }
    }
    private void PlayEffectSequence()
    {
        ResetEffectUI();

        Sequence seq = DOTween.Sequence();

        // 1단계: 배경+소개문구만 1.2초간 보여줌
        seq.AppendCallback(() =>
        {
            operIntroText.gameObject.SetActive(true);
            operProfile.gameObject.SetActive(false);
            operNameText.gameObject.SetActive(false);
            operRoleText.gameObject.SetActive(false);
        })
        .AppendInterval(1.2f)

        // 2단계: 소개문구 숨기고 캐릭터/이름/클래스 표시
        .AppendCallback(() =>
        {
            operIntroText.gameObject.SetActive(false);
            operProfile.gameObject.SetActive(true);
            operNameText.gameObject.SetActive(true);
            operRoleText.gameObject.SetActive(true);
        })
        // 이펙트 연출
        .Append(sterEffect.transform.DOScale(Vector3.one * 8f, 1f).SetEase(Ease.OutBack))
        .AppendCallback(() =>
        {
            sterEffect.transform.DOScale(Vector3.one * 7f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        })
        .AppendInterval(0.5f)
        .OnComplete(() =>
        {
            isPlaying = false;
            nextButton.gameObject.SetActive(true);
        });
    }
    // 연출 애니메이션 실행
    //private void PlayEffectSequence()
    //{
    //    ResetEffectUI();

    //    Sequence seq = DOTween.Sequence();

    //    seq.AppendInterval(2f)
    //        .AppendCallback(() =>
    //        {
    //            operIntroPanel.SetActive(false);
    //        })
    //        .Append(sterEffect.transform.DOScale(Vector3.one * 8f, 1f).SetEase(Ease.OutBack))
    //        .OnUpdate(() =>
    //        {
    //            if (!operProfile.gameObject.activeSelf && sterEffect.transform.localScale.x >= 7f)
    //            {
    //                operProfile.gameObject.SetActive(true);
    //            }
    //        })
    //        .AppendCallback(() =>
    //        {
    //            sterEffect.transform.DOScale(Vector3.one * 7f, 0.5f)
    //                .SetLoops(-1, LoopType.Yoyo)
    //                .SetEase(Ease.InOutSine);
    //        })
    //        .AppendInterval(0.5f)
    //        .OnComplete(() =>
    //        {
    //            isPlaying = false;
    //            nextButton.gameObject.SetActive(true);
    //        });
    //}

    // 초기화
    private void ResetEffectUI()
    {
        //operIntroPanel.SetActive(true);
        //operProfile.gameObject.SetActive(false);

        //sterEffect.transform.localScale = Vector3.zero;

        //DOTween.Kill(sterEffect.transform);

        //nextButton.gameObject.SetActive(false);
        // 배경은 항상 켜둠
        operBg.gameObject.SetActive(true);

        // 1단계: 소개문구만 먼저 보여줌
        operIntroText.gameObject.SetActive(true);

        // 2단계: 캐릭터/이름/클래스는 나중에 보여줌
        operProfile.gameObject.SetActive(false);
        operNameText.gameObject.SetActive(false);
        operRoleText.gameObject.SetActive(false);

        sterEffect.transform.localScale = Vector3.zero;
        DOTween.Kill(sterEffect.transform);

        nextButton.gameObject.SetActive(false);
    }
}
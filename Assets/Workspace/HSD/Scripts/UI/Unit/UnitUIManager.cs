using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUIManager : MonoBehaviour
{           
    [SerializeField] Button _fightButton;
    [SerializeField] CanvasGroup _battleUI;
    [SerializeField] CanvasGroup _notBattleUI;

    [SerializeField] private float _fadeDuration = 0.3f;

    [Header("Battle")]
    public DamageMeterController DamageMeterController;
    public HpMeterController HpMeterController;
    public SkillPopUpController SkillPopUpController;
    public UnitHealthBarManager UnitHealthBarManager;

    [Header("Not Battle")]
    public SynergyPanel SynergyPanel;
    public SynergySlotPanel SynergySlotPanel;
    public UnitCountPanel UnitCountPanel;
    public UnitTotalPowerPanel[] UnitTotalPowerPanel;

    public async UniTask BattleUISetting()
    {
        await UIFadeOut(_notBattleUI);

        await UIFadeIn(_battleUI);
    }

    public async UniTask StandbyUISetting()
    {
        UnitHealthBarManager.Clear();

        await UIFadeOut(_battleUI);

        _fightButton.gameObject.SetActive(true);

        await UIFadeIn(_notBattleUI);
    }

    public async UniTask StandbyUIDeActive()
    {
        await UIFadeOut(_notBattleUI);

        _fightButton.gameObject.SetActive(false);
    }

    private async UniTask UIFadeIn(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        await canvasGroup.DOFade(1f, _fadeDuration).AsyncWaitForCompletion();

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private async UniTask UIFadeOut(CanvasGroup canvasGroup)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        await canvasGroup.DOFade(0f, _fadeDuration).AsyncWaitForCompletion();
    }
}

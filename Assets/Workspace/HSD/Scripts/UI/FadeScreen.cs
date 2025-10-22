using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] Image _screen;
    [SerializeField] TMP_Text _loadingText;
    [SerializeField] float _fadeDuration = 0.5f;

    /// <summary>
    /// 화면을 어둡게 (0 -> 1)
    /// </summary>
    /// 
    [ContextMenu("FadeIn")]
    public async UniTask FadeIn()
    {
        _screen.gameObject.SetActive(true);
        _screen.color = new Color(0, 0, 0, 0);

        _loadingText.gameObject.SetActive(true);
        _loadingText.alpha = 0; // TMP_Text는 color.a 대신 alpha 속성 사용 가능

        // 동시에 페이드 인 실행
        _screen.DOFade(1f, _fadeDuration);
        _loadingText.DOFade(1f, _fadeDuration);

        await UniTask.Delay(TimeSpan.FromSeconds(_fadeDuration));
    }

    /// <summary>
    /// 화면을 밝게 (1 -> 0)
    /// </summary>
    public async UniTask FadeOut()
    {        
        _screen.DOFade(0f, _fadeDuration);
        _loadingText.DOFade(0f, _fadeDuration);

        await UniTask.Delay(TimeSpan.FromSeconds(_fadeDuration));

        _loadingText.gameObject.SetActive(false);
        _screen.gameObject.SetActive(false);
    }
}

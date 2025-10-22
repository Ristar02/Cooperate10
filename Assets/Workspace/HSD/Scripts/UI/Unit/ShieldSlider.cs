using UnityEngine;
using UnityEngine.UI;

public class ShieldSlider : MonoBehaviour
{
    [Header("References (make sure pivots are left: 0,0.5)")]
    [SerializeField] RectTransform _barContainer;   // 전체 바(배경) — full width 기준
    [SerializeField] RectTransform _hpFill;         // HP Fill Rect (optional)
    [SerializeField] Image _shieldBar;              // Shield image (RectTransform as child of _barContainer)

    float FullWidth => _barContainer.rect.width;

    [Header("Test")]
    [SerializeField] Slider _slider;
    [SerializeField] int curHp;
    [SerializeField] int maxHp;
    [SerializeField] int shield;

    [ContextMenu("Test")]
    public void Test()
    {
        DebugRects();        
        UpdateShield(curHp, maxHp, shield);
        _slider.maxValue = maxHp;
        _slider.value = curHp;
    }

    public void UpdateShield(int currentHp, int maxHp, int shieldAmount)
    {
        if (_shieldBar == null || _barContainer == null) return;

        if (shieldAmount <= 0)
        {
            _shieldBar.gameObject.SetActive(false);
            return;
        }
        _shieldBar.gameObject.SetActive(true);

        float hpRatio = Mathf.Clamp01((float)currentHp / maxHp);
        float shieldRatio = Mathf.Clamp01((float)shieldAmount / maxHp);

        float hpWidth = FullWidth * hpRatio;
        float shieldWidth = FullWidth * shieldRatio;

        var rt = _shieldBar.rectTransform;
        rt.pivot = new Vector2(1f, 0.5f);

        if (hpRatio + shieldRatio <= 1f)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, shieldWidth);
            rt.anchoredPosition = new Vector2(hpWidth * 1.5f, rt.anchoredPosition.y);
        }
        else
        {
            float overflow = (hpRatio + shieldRatio - 1f) * FullWidth;
            float size = (FullWidth - hpWidth) + overflow;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            rt.anchoredPosition = new Vector2(FullWidth, rt.anchoredPosition.y);
        }
    }


    void DebugRects()
    {
        Debug.Log($"BarContainer pivot:{_barContainer.pivot} anchors:{_barContainer.anchorMin}-{_barContainer.anchorMax} rect:{_barContainer.rect.width}");
        Debug.Log($"HPFill pivot:{_hpFill.pivot} anchors:{_hpFill.anchorMin}-{_hpFill.anchorMax} rect:{_hpFill.rect.width}");
        Debug.Log($"Shield pivot:{_shieldBar.rectTransform.pivot} anchors:{_shieldBar.rectTransform.anchorMin}-{_shieldBar.rectTransform.anchorMax} rect:{_shieldBar.rectTransform.rect.width} anchoredPos:{_shieldBar.rectTransform.anchoredPosition}");
    }

}

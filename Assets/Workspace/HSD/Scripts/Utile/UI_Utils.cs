using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public static class UI_Utils
{
    public static void Lerp(this Slider slider, float endValue, float duration = 0.5f, Ease ease = Ease.OutQuad)
    {
        DOTween.Kill(slider, id: "Value");

        slider
            .DOValue(endValue, duration)
            .SetEase(ease)
            .SetId("Value")
            .SetTarget(slider);
    }

    public static void LerpText(this TMP_Text text, int endValue, float duration = 0.5f, Ease ease = Ease.OutCubic)
    {
        int startValue = int.TryParse(text.text, out var parsed) ? parsed : 0;

        DOTween.Kill(text, id: "Value");

        DOTween.To(
            () => startValue,
            x => {
                startValue = x;
                text.text = Utils.ToAbbreviation(x);
            },
            endValue,
            duration
        )
        .SetEase(ease)
        .SetId("Value")
        .SetTarget(text);
    }   
}

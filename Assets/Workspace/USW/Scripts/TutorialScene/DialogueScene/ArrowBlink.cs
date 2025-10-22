using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArrowBlink : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField]
    private    float  fadeTime = 0.5f;       // 페이드 인/아웃 시간
    [SerializeField]
    private    float  minAlpha = 0f;         // 최소 투명도
    [SerializeField]
    private    float  maxAlpha = 1f;         // 최대 투명도
    [SerializeField]
    private    bool   enableBlink = true;    // 깜빡임 활성화 여부
    
    private    Image  fadeImage;             // 페이드 효과가 적용되는 Image UI
    private    Color  originalColor;         // 원본 색상 저장
    private    bool   isBlinking = false;    // 깜빡임 상태 확인
    private    Coroutine blinkCoroutine;     // 코루틴 참조 저장

    private void Awake()
    {
        // Image 컴포넌트 가져오기
        fadeImage = GetComponent<Image>();
        
        if (fadeImage != null)
        {
            originalColor = fadeImage.color;
        }
        else
        {
            Debug.LogError("ArrowBlink: Image 컴포넌트를 찾을 수 없습니다!");
        }
        
        // 초기값 검증
        ValidateSettings();
    }

    private void OnEnable()
    {
        if (enableBlink && fadeImage != null)
        {
            StartBlinking();
        }
    }

    private void OnDisable()
    {
        StopBlinking();
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지
        StopBlinking();
    }

    private void ValidateSettings()
    {
        // 설정값 유효성 검사 및 보정
        fadeTime = Mathf.Max(0.1f, fadeTime);
        minAlpha = Mathf.Clamp01(minAlpha);
        maxAlpha = Mathf.Clamp01(maxAlpha);
        
        // min과 max 값이 잘못 설정된 경우 보정
        if (minAlpha > maxAlpha)
        {
            float temp = minAlpha;
            minAlpha = maxAlpha;
            maxAlpha = temp;
        }
    }

    public void StartBlinking()
    {
        if (isBlinking || fadeImage == null) return;
        
        isBlinking = true;
        // Fade 효과를 In -> Out 순서로 반복한다.
        blinkCoroutine = StartCoroutine(FadeInOut());
    }

    public void StopBlinking()
    {
        if (!isBlinking) return;
        
        isBlinking = false;
        
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        
        // 원본 색상으로 복원
        if (fadeImage != null)
        {
            fadeImage.color = originalColor;
        }
    }

    private IEnumerator FadeInOut()
    {
        while (isBlinking && fadeImage != null)
        {
            // Fade In (투명 -> 불투명)
            yield return StartCoroutine(Fade(minAlpha, maxAlpha));
            
            if (!isBlinking) break;
            
            // Fade Out (불투명 -> 투명)
            yield return StartCoroutine(Fade(maxAlpha, minAlpha));
        }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadeImage == null) yield break;
        
        float currentTime = 0f;
        float percent = 0f;
        Color currentColor = fadeImage.color;

        while (percent < 1f && isBlinking)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / fadeTime;

            // 알파값 보간
            currentColor.a = Mathf.Lerp(startAlpha, endAlpha, percent);
            fadeImage.color = currentColor;

            yield return null;
        }

        // 마지막 값으로 정확히 설정 (부동소수점 오차 방지)
        if (isBlinking && fadeImage != null)
        {
            currentColor.a = endAlpha;
            fadeImage.color = currentColor;
        }
    }

    // 공개 메서드 - 외부에서 페이드 시간 설정
    public void SetFadeTime(float time)
    {
        fadeTime = Mathf.Max(0.1f, time);
    }

    // 공개 메서드 - 외부에서 알파 범위 설정
    public void SetAlphaRange(float min, float max)
    {
        minAlpha = Mathf.Clamp01(min);
        maxAlpha = Mathf.Clamp01(max);
        
        if (minAlpha > maxAlpha)
        {
            float temp = minAlpha;
            minAlpha = maxAlpha;
            maxAlpha = temp;
        }
    }

    // 공개 메서드 - 외부에서 깜빡임 활성화/비활성화
    public void SetBlinkEnabled(bool enabled)
    {
        enableBlink = enabled;
        
        if (enabled && gameObject.activeInHierarchy)
        {
            StartBlinking();
        }
        else
        {
            StopBlinking();
        }
    }

    // 공개 메서드 - 깜빡임 상태 확인
    public bool IsBlinking()
    {
        return isBlinking;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

/// <summary>
/// 여러 UI 패널의 슬라이드 애니메이션 및 배경 이동을 관리하는 컨트롤러
/// 플레이어/적 각각의 패널 전환을 지원
/// </summary>
public class PanelMultiSlideController : MonoBehaviour
{
    public RectTransform[] slideUpPanels;      // 플레이어용: 위로 사라질 패널들
    public RectTransform[] slideInPanels;      // 플레이어용: 아래에서 올라올 패널들

    private Vector2[] slideUpOriginalPositions; // 플레이어용 패널의 원래 위치 저장 배열
    private Vector2[] slideInOriginalPositions; // 플레이어용 새 패널의 원래 위치 저장 배열

    public RectTransform[] enemySlideUpPanels;    // 적용: 위로 사라질 패널들
    public RectTransform[] enemySlideInPanels;    // 적용: 아래에서 올라올 패널들

    private Vector2[] enemySlideUpOriginalPositions; // 적용 패널의 원래 위치 저장 배열
    private Vector2[] enemySlideInOriginalPositions; // 적용 새 패널의 원래 위치 저장 배열

    public float slideDuration = 0.5f;          // 슬라이드 애니메이션 지속 시간(초)
    public float slideOffset = 2280f;           // 패널이 이동할 오프셋(화면 밖 위치)

    public Button moveButton;
    public Button summonSceneButton;

    public Transform backgroundTransform;// 배경 오브젝트
    private Vector3 backgroundOriginalPosition;// 배경의 원래 위치 저장

    public static bool IsBattleActive = false;// 전투 중 여부(패널 전환 예외 처리용)

    void Awake()
    {
        slideUpOriginalPositions = new Vector2[slideUpPanels.Length];// 플레이어 패널 원래 위치 배열 생성
        for (int i = 0; i < slideUpPanels.Length; i++)
            slideUpOriginalPositions[i] = slideUpPanels[i].anchoredPosition;// 각 패널의 원래 위치 저장

        slideInOriginalPositions = new Vector2[slideInPanels.Length];// 새 패널 원래 위치 배열 생성
        for (int i = 0; i < slideInPanels.Length; i++)
        {
            slideInOriginalPositions[i] = slideInPanels[i].anchoredPosition;// 새 패널 원래 위치 저장
            // 시작 위치 아래로 오프셋
            slideInPanels[i].anchoredPosition = slideInOriginalPositions[i] - new Vector2(0, slideOffset);
            slideInPanels[i].gameObject.SetActive(false);
        }

        if (enemySlideUpPanels != null)
        {
            // 적용 패널 원래 위치 배열 생성
            enemySlideUpOriginalPositions = new Vector2[enemySlideUpPanels.Length];
            for (int i = 0; i < enemySlideUpPanels.Length; i++)
            {
                // 적용 패널 원래 위치 저장
                enemySlideUpOriginalPositions[i] = enemySlideUpPanels[i].anchoredPosition;
                // 시작 위치 아래로 오프셋
                enemySlideUpPanels[i].anchoredPosition = enemySlideUpOriginalPositions[i] - new Vector2(0, slideOffset);
                enemySlideUpPanels[i].gameObject.SetActive(false);
            }
        }
        if (enemySlideInPanels != null)
        {
            // 적용 새 패널 원래 위치 배열 생성
            enemySlideInOriginalPositions = new Vector2[enemySlideInPanels.Length];
            for (int i = 0; i < enemySlideInPanels.Length; i++)
            {
                // 적용 새 패널 원래 위치 저장
                enemySlideInOriginalPositions[i] = enemySlideInPanels[i].anchoredPosition;
                // 원래 위치
                enemySlideInPanels[i].anchoredPosition = enemySlideInOriginalPositions[i];
                enemySlideInPanels[i].gameObject.SetActive(true);
            }
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (backgroundTransform != null)// 배경 오브젝트가 할당되어 있으면
            backgroundOriginalPosition = backgroundTransform.position;// 배경의 원래 위치 저장

        if (moveButton != null)// 이동 버튼이 할당되어 있으면
            moveButton.onClick.AddListener(SlideUpPanels);// 이동 버튼 클릭 시 SlideUpPanels 함수 연결

        if (summonSceneButton != null)// 소환씬 버튼이 할당되어 있으면
            // 소환씬 버튼 클릭 시 SlideDownPanels 함수 연결
            summonSceneButton.onClick.AddListener(SlideDownPanels); 
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (backgroundTransform == null)// 배경 오브젝트가 없으면
        {
            var bgMap = FindObjectOfType<AreaBackgroundMap>();// AreaBackgroundMap 오브젝트 찾기
            if (bgMap != null)
            {
                backgroundTransform = bgMap.transform;// 배경 Transform 할당
                backgroundOriginalPosition = backgroundTransform.position;// 배경 원래 위치 저장
            }
        }
    }

    /// <summary>
    /// 플레이어용: 기존 패널 위로 슬라이드(사라짐), 새 패널 아래에서 슬라이드(등장)
    /// 배경도 y=1.1로 이동
    /// </summary>
    public void SlideUpPanels()
    {
        float delayStep = 0.08f;// 패널별 애니메이션 딜레이 간격


        // 기존 패널 위로 슬라이드 & 비활성화
        for (int i = 0; i < slideUpPanels.Length; i++)
        {
            int idx = i;
            if (slideUpPanels[idx].gameObject.name == "WaitingBattlePlayer" ||
                slideUpPanels[idx].gameObject.name == "WaitingBattleEnemy")// 특정 패널은 건너뜀
            {
                continue;
            }
            slideUpPanels[idx].DOKill();// 기존 트윈 중지
            slideUpPanels[idx].DOAnchorPos(slideUpOriginalPositions[idx] + new Vector2(0, slideOffset), slideDuration)
                .SetEase(Ease.InOutQuad)
                .SetDelay(idx * delayStep)
                .OnComplete(() => slideUpPanels[idx].gameObject.SetActive(false));
        }

        // 새 패널 활성화 & 아래에서 위로 슬라이드
        for (int i = 0; i < slideInPanels.Length; i++)
        {
            int idx = i;
            bool isExceptionPanel =
                slideInPanels[idx].gameObject.name == "WaitingBattlePlayer" ||
                slideInPanels[idx].gameObject.name == "WaitingBattleEnemy";

            if (IsBattleActive && isExceptionPanel)// 전투 중이면 위치 복구
            {
                slideUpPanels[idx].anchoredPosition = slideUpOriginalPositions[idx];
                continue;
            }

            slideInPanels[idx].gameObject.SetActive(true);
            slideInPanels[idx].DOKill(); // 기존 트윈 중지
            slideInPanels[idx].anchoredPosition = slideInOriginalPositions[idx] - new Vector2(0, slideOffset);
            slideInPanels[idx].DOAnchorPos(slideInOriginalPositions[idx], slideDuration)
                .SetEase(Ease.OutCubic)
                .SetDelay(idx * delayStep);
        }
        if (backgroundTransform != null)// 배경 이동
        {
            Vector3 targetPos = new Vector3(backgroundOriginalPosition.x, 1.1f, backgroundOriginalPosition.z);
            backgroundTransform.DOMove(targetPos, slideDuration)
                .SetEase(Ease.OutCubic)
                .SetDelay((slideUpPanels.Length + 1) * delayStep);
        }
    }

    /// <summary>
    /// 플레이어용: 새 패널 아래로 슬라이드(사라짐), 기존 패널 위에서 아래로 슬라이드(복귀)
    /// 배경도 원래 위치로 복귀
    /// </summary>
    public void SlideDownPanels()
    {
        float delayStep = 0.08f;

        // 새 패널 아래로 슬라이드 & 비활성화
        for (int i = 0; i < slideInPanels.Length; i++)
        {
            int idx = i;
            slideInPanels[idx].DOKill();// 기존 트윈 중지
            slideInPanels[idx].DOAnchorPos(slideInOriginalPositions[idx] - new Vector2(0, slideOffset), slideDuration)
                .SetEase(Ease.InOutQuad)
                .SetDelay(idx * delayStep)
                .OnComplete(() => slideInPanels[idx].gameObject.SetActive(false));
        }

        // 기존 패널 활성화 & 위에서 아래로 슬라이드
        for (int i = 0; i < slideUpPanels.Length; i++)
        {
            int idx = i;
            if (slideUpPanels[idx].gameObject.name == "WaitingBattlePlayer" ||
                slideUpPanels[idx].gameObject.name == "WaitingBattleEnemy")// 특정 패널은 건너뜀
            {
                continue;
            }
            slideUpPanels[idx].gameObject.SetActive(true);
            slideUpPanels[idx].DOKill();// 기존 트윈 중지
            slideUpPanels[idx].DOAnchorPos(slideUpOriginalPositions[idx], slideDuration)
                .SetEase(Ease.OutCubic)
                .SetDelay(idx * delayStep);
        }
        if (backgroundTransform != null)// 배경 원래 위치로 복귀
        {
            backgroundTransform.DOMove(backgroundOriginalPosition, slideDuration)
                .SetEase(Ease.OutCubic)
                .SetDelay((slideUpPanels.Length + 1) * delayStep);
        }
    }

    /// <summary>
    /// 적용: 기존 패널 아래로 슬라이드(사라짐), 새 패널 위에서 아래로 슬라이드(등장)
    /// 배경도 y=2.7로 이동
    /// </summary>
    public void SlideEnemyToSummonScene()
    {
        float delayStep = 0.08f;
        for (int i = 0; i < enemySlideUpPanels.Length; i++)
        {
            int idx = i;
            enemySlideUpPanels[idx].DOKill();// 기존 트윈 중지
            enemySlideUpPanels[idx].DOAnchorPos(enemySlideUpOriginalPositions[idx] - new Vector2(0, slideOffset), slideDuration)
                .SetEase(Ease.InOutQuad)
                .SetDelay(idx * delayStep)
                .OnComplete(() => enemySlideUpPanels[idx].gameObject.SetActive(false));
        }
        for (int i = 0; i < enemySlideInPanels.Length; i++)
        {
            int idx = i;
            enemySlideInPanels[idx].gameObject.SetActive(true);
            enemySlideInPanels[idx].DOKill();// 기존 트윈 중지
            enemySlideInPanels[idx].anchoredPosition = enemySlideInOriginalPositions[idx] + new Vector2(0, slideOffset);
            enemySlideInPanels[idx].DOAnchorPos(enemySlideInOriginalPositions[idx], slideDuration)
                .SetEase(Ease.OutCubic)
                .SetDelay(idx * delayStep);
        }
        if (backgroundTransform != null)// 배경 이동
        {
            Vector3 targetPos = new Vector3(backgroundOriginalPosition.x, 2.7f, backgroundOriginalPosition.z);
            backgroundTransform.DOKill();
            backgroundTransform.DOMove(targetPos, slideDuration)
                .SetEase(Ease.OutCubic)
                .SetDelay((enemySlideUpPanels.Length + 1) * delayStep);
        }
    }
}
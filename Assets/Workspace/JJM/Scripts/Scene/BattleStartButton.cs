
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleStartButton : MonoBehaviour
{
    public RectTransform playerBatchUI;
    public RectTransform enemyBatchUI;
    public RectTransform waitingBattlePlayer;
    public RectTransform waitingBattleEnemy;

    public GameObject waitingBattleUI;
    public GameObject fightSceneUI;
    public GameObject skillUI;
    public GameObject enemyInfoMove;
    public GameObject summonSceneMovePlayer;
    public GameObject restMove;
    public GameObject summonSceneMoveMonster;

    private Vector3 playerBatchOriginScale;// 플레이어 배치 원래 스케일
    private Vector3 enemyBatchOriginScale;// 적 배치 원래 스케일
    private Vector2 waitingBattlePlayerOriginPos; // 플레이어 대기 패널 원래 위치
    private Vector2 waitingBattleEnemyOriginPos;// 적 대기 패널 원래 위치

    public Button battleStartButton;

    public GameObject fightButtonS;   // 항상 활성화
    public GameObject fightButton;    // 활성/비활성 전환용

    private Vector2 waitingBattlePlayerBattlePos;// 플레이어 배치 전투 위치
    private Vector2 waitingBattleEnemyBattlePos;// 적 배치 전투 위치
    private Vector3 playerBatchZoomScale;// 플레이어 배치 줌 스케일
    private Vector3 enemyBatchZoomScale; // 적 배치 줌 스케일
    private Transform fightButtonSOriginalParent;// 버튼 원래 부모

    private const float DefaultGridOffsetX = 300f; // 배치 X 오프셋
    private const float DefaultBattleY = 0f;// 배치 Y 위치
    private const float DefaultBackgroundMoveX = -500f;// 배경 이동 X값
    private const float DefaultBackgroundMoveDuration = 15f;// 배경 이동 시간

    public float offScreenX = 3000f;// 화면 밖 X값
    public float batchSlideDuration = 3f;// 슬라이드 시간

    public EnemyRestSlideController enemyRestSlideController;// 적 휴식 슬라이드 컨트롤러

    private Transform backgroundTransform;// 배경 Transform
    private Vector3 backgroundStartPosition;// 배경 시작 위치

    void Awake()
    {
        playerBatchOriginScale = playerBatchUI.localScale;// 플레이어 배치 원래 스케일 저장
        enemyBatchOriginScale = enemyBatchUI.localScale;// 적 배치 원래 스케일 저장
        // 플레이어 대기 패널 원래 위치 저장
        waitingBattlePlayerOriginPos = waitingBattlePlayer.anchoredPosition;
        // 적 대기 패널 원래 위치 저장
        waitingBattleEnemyOriginPos = waitingBattleEnemy.anchoredPosition;
        // 버튼 원래 부모 저장
        fightButtonSOriginalParent = fightButtonS.transform.parent;
        // 플레이어 대기 패널 위치 초기화
        waitingBattlePlayerOriginPos = Vector2.zero;

        // 플레이어 배치 줌 스케일 계산
        playerBatchZoomScale = playerBatchOriginScale * 0.5f;
        // 적 배치 줌 스케일 계산
        enemyBatchZoomScale = enemyBatchOriginScale * 0.5f;

        // 플레이어 전투 위치
        waitingBattlePlayerBattlePos = new Vector2(-DefaultGridOffsetX, DefaultBattleY);
        // 적 전투 위치
        waitingBattleEnemyBattlePos = new Vector2(DefaultGridOffsetX, DefaultBattleY);

        battleStartButton.onClick.AddListener(OnBattleStart);
    }

    void Start()
    {
        var bgObj = FindObjectOfType<AreaBackgroundMap>();// 배경 오브젝트 찾기
        if (bgObj != null)
            backgroundTransform = bgObj.transform; // 배경 Transform 저장
    }

    // UI 상태 일괄 설정
    private void SetUIState(bool isBattle)
    {
        waitingBattleUI.SetActive(!isBattle);
        fightSceneUI.SetActive(isBattle);
        skillUI.SetActive(isBattle);
        enemyInfoMove.SetActive(!isBattle);
        summonSceneMovePlayer.SetActive(!isBattle);
        restMove.SetActive(!isBattle);
        summonSceneMoveMonster.SetActive(!isBattle);
        fightButtonS.SetActive(true);
        fightButton.SetActive(!isBattle);
    }

    // 배치 UI 스케일/위치 복원
    private void RestoreBatchUI()
    {
        playerBatchUI.DOKill();// 플레이어 배치 트윈 중지
        enemyBatchUI.DOKill(); // 적 배치 트윈 중지
        playerBatchUI.localScale = playerBatchOriginScale;// 플레이어 배치 스케일 복원
        enemyBatchUI.localScale = enemyBatchOriginScale;// 적 배치 스케일 복원
        playerBatchUI.anchoredPosition = Vector2.zero;// 플레이어 배치 위치 복원
        enemyBatchUI.anchoredPosition = Vector2.zero;// 적 배치 위치 복원
    }

    // 배경 위치 복원
    private void RestoreBackground()
    {
        if (backgroundTransform != null)
        {
            backgroundTransform.DOKill();// 배경 트윈 중지
            Vector3 currentPos = backgroundTransform.position;// 현재 위치
            // 복귀 위치
            Vector3 returnPos = new Vector3(backgroundStartPosition.x, currentPos.y, currentPos.z);
            // 배경 복귀 애니메이션
            backgroundTransform.DOMove(returnPos, 0.5f).SetEase(Ease.InOutCubic);
        }
    }

    // FightButtonS 부모/위치 복원
    private void RestoreFightButtonS()
    {
        fightButtonS.transform.SetParent(fightButtonSOriginalParent, true);// 부모 복원
        var fightButtonSRect = fightButtonS.GetComponent<RectTransform>();// RectTransform 가져오기
        if (fightButtonSRect != null)
            fightButtonSRect.anchoredPosition = Vector2.zero;// 위치 복원
    }

    // 플레이어/적 배치 오브젝트 활성화 상태 복원
    private void RestoreWaitingBattlePanels()
    {
        waitingBattlePlayer.gameObject.SetActive(true);
        waitingBattleEnemy.gameObject.SetActive(false);
    }

    // 전투 종료 시 호출
    public void OnBattleEnd()
    {
        PanelMultiSlideController.IsBattleActive = false;
        RestoreBackground();// 배경 복원
        SetUIState(false);// UI 상태 복원
        RestoreBatchUI();// 배치 UI 복원
        RestoreFightButtonS();// 버튼 복원
        RestoreWaitingBattlePanels();// 대기 패널 복원
    }

    public void OnBattleStart()
    {
        if (enemyRestSlideController != null)
            enemyRestSlideController.RestMoveToPlayer();// 적 휴식 패널 이동

        PanelMultiSlideController.IsBattleActive = true; // 전투 상태 활성화

        waitingBattlePlayer.DOKill();// 플레이어 배치 트윈 중지
        waitingBattleEnemy.DOKill();// 적 배치 트윈 중지

        waitingBattlePlayer.gameObject.SetActive(true);
        waitingBattleEnemy.gameObject.SetActive(true);

        SetUIState(true);// UI 상태 전투로 변경

        fightButtonS.transform.SetParent(null, true);// 버튼 부모 분리

        // 줌 애니메이션
        Sequence zoomSeq = DOTween.Sequence();
        // 플레이어 줌
        zoomSeq.Append(playerBatchUI.DOScale(playerBatchZoomScale, 0.3f).SetEase(Ease.InOutCubic));
        // 적 줌
        zoomSeq.Join(enemyBatchUI.DOScale(enemyBatchZoomScale, 0.3f).SetEase(Ease.InOutCubic));

        zoomSeq.OnComplete(() =>
        {
            float startY = playerBatchUI.anchoredPosition.y;// 현재 Y값
            float leftOutX = -offScreenX;// 왼쪽 밖 X값
            float rightOutX = offScreenX;// 오른쪽 밖 X값
            float centerX = waitingBattlePlayerOriginPos.x;// 플레이어 원래 X
            float enemyTargetX = waitingBattleEnemyOriginPos.x; // 적 원래 X

            // 플레이어 왼쪽 밖으로
            playerBatchUI.anchoredPosition = new Vector2(centerX, startY);
            // 적 중앙으로
            enemyBatchUI.anchoredPosition = new Vector2(rightOutX, startY);

            Sequence batchSlideSeq = DOTween.Sequence();
            batchSlideSeq.Append(playerBatchUI.DOAnchorPos(new Vector2(leftOutX, startY), batchSlideDuration).SetEase(Ease.InOutCubic));
            batchSlideSeq.Join(enemyBatchUI.DOAnchorPos(new Vector2(enemyTargetX, startY), batchSlideDuration).SetEase(Ease.InOutCubic));

            if (backgroundTransform != null)
            {
                backgroundStartPosition = backgroundTransform.position;
                // 배경 이동 위치
                Vector3 bgTargetPos = backgroundTransform.position + new Vector3(DefaultBackgroundMoveX, 0f, 0f);
                // 배경 이동
                backgroundTransform.DOMove(bgTargetPos, DefaultBackgroundMoveDuration).SetEase(Ease.InOutCubic);
            }

            batchSlideSeq.OnComplete(OnBattleEnd);// 슬라이드 끝나면 전투 종료
        });
    }

    public void MoveBatchAndBackground(Vector2 targetPos, float duration)
    {
        playerBatchUI.DOKill();
        playerBatchUI.DOAnchorPos(targetPos, duration).SetEase(Ease.InOutCubic);// 플레이어 배치 이동

        if (backgroundTransform != null)
        {
            // 배경 이동 위치
            Vector3 bgTargetPos = new Vector3(targetPos.x, backgroundTransform.position.y, backgroundTransform.position.z);
            // 배경 이동
            backgroundTransform.DOMove(bgTargetPos, duration).SetEase(Ease.InOutCubic);
        }
    }
}
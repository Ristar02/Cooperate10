
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyRestSlideController : MonoBehaviour
{
    public RectTransform waitingBattlePlayer;
    public RectTransform waitingBattleEnemy;
    public Button enemyInfoMoveButton;
    public Button restMoveButton;

    private Vector2 playerOriginPos;// 플레이어 패널 원래 위치
    private Vector2 enemyOriginPos;// 적 패널 원래 위치


    // 슬라이드 관련 상수
    private const float DefaultTopY = 33f;// 패널 Y 위치
    public float offScreenX = 800f;// 화면 밖 X 위치
    public float slideDuration = 0.4f;// 슬라이드 애니메이션 시간

    void Awake()
    {
        // 컴포넌트 초기화
        playerOriginPos = waitingBattlePlayer.anchoredPosition;// 플레이어 패널 원래 위치 저장
        enemyOriginPos = waitingBattleEnemy.anchoredPosition;// 적 패널 원래 위치 저장

        enemyInfoMoveButton.onClick.AddListener(OnEnemyInfoMove);// 적 정보 버튼 클릭 이벤트 등록
        restMoveButton.onClick.AddListener(OnRestMove);// 휴식 버튼 클릭 이벤트 등록
    }

    /// <summary>
    /// 패널 위치와 활성화 상태를 지정
    /// </summary>
    private void SetPanel(RectTransform panel, Vector2 pos, bool active)
    {
        panel.gameObject.SetActive(active);
        panel.anchoredPosition = pos;
    }

    /// <summary>
    /// 두 패널을 슬라이드 애니메이션으로 이동
    /// </summary>
    private void SlidePanels(Vector2 playerTarget, Vector2 enemyTarget)
    {
        waitingBattlePlayer.DOKill();// 플레이어 패널 트윈 중지
        waitingBattleEnemy.DOKill();// 적 패널 트윈 중지

        Sequence seq = DOTween.Sequence();
        // 적 패널 슬라이드
        seq.Append(waitingBattleEnemy.DOAnchorPos(enemyTarget, slideDuration).SetEase(Ease.OutCubic));
        // 플레이어 패널 슬라이드
        seq.Join(waitingBattlePlayer.DOAnchorPos(playerTarget, slideDuration).SetEase(Ease.OutCubic));
    }

    /// <summary>
    /// EnemyInfoMove 버튼 클릭 시
    /// - 플레이어 패널은 왼쪽 화면 밖으로, 적 패널은 중앙으로 슬라이드
    /// </summary>
    private void OnEnemyInfoMove()
    {
        // 플레이어 패널 중앙 위치 활성화
        SetPanel(waitingBattlePlayer, new Vector2(playerOriginPos.x, DefaultTopY), true);
        // 적 패널 오른쪽 밖 위치 활성화
        SetPanel(waitingBattleEnemy, new Vector2(offScreenX, DefaultTopY), true);

        SlidePanels(
            new Vector2(-offScreenX, DefaultTopY), // 플레이어: 왼쪽 밖으로
            new Vector2(enemyOriginPos.x, DefaultTopY) // 적: 중앙으로
        );
    }

    /// <summary>
    /// RestMove 버튼 클릭 시
    /// - 플레이어 패널은 중앙으로, 적 패널은 오른쪽 화면 밖으로 슬라이드
    /// </summary>
    private void OnRestMove()
    {
        SetPanel(waitingBattlePlayer, new Vector2(-offScreenX, DefaultTopY), true);// 플레이어 패널 왼쪽 밖 위치 활성화
        SetPanel(waitingBattleEnemy, new Vector2(enemyOriginPos.x, DefaultTopY), true);// 적 패널 중앙 위치 활성화

        SlidePanels(
            new Vector2(playerOriginPos.x, DefaultTopY), // 플레이어: 중앙으로
            new Vector2(offScreenX, DefaultTopY) // 적: 오른쪽 밖으로
        );
    }

    /// <summary>
    /// 외부에서 플레이어 대기 위치로 슬라이드할 때 호출
    /// </summary>
    public void RestMoveToPlayer()
    {
        OnRestMove();
    }
}
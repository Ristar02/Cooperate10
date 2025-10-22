using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BattleUISwitch : MonoBehaviour
{
    enum BattleUIType { MagicStone, Meter }

    [SerializeField] RectTransform _magicStonePanel;
    [SerializeField] RectTransform _meterPanel;
    [SerializeField] Ease _upEase = Ease.OutElastic;
    [SerializeField] Ease _downEase = Ease.InElastic;
    [SerializeField] float _duration = 0.5f;
    [SerializeField] float _offsetY = 500f;

    private BattleUIType battleUIType = BattleUIType.MagicStone;

    private bool _isMoving = false;
    private Vector2 _upPosition;
    private Vector2 _downPosition;

#if UNITY_EDITOR
    [Button("UI 변경하기")]
    public void SwitchUITest()
    {
        SwitchUI();
    }
#endif
    private void Awake()
    {
        _upPosition = new Vector2(0, 0);
        _downPosition = new Vector2(0, -_offsetY);

        _magicStonePanel.anchoredPosition = _upPosition;
        _meterPanel.anchoredPosition = _downPosition;

        UISetting().Forget();
    }

    public void SwitchUI()
    {
        if (_isMoving)
            return;

        SwitchUIAsync().Forget();
    }

    private async UniTask SwitchUIAsync()
    {
        if (battleUIType == BattleUIType.MagicStone)
        {
            battleUIType = BattleUIType.Meter;
        }
        else
        {
            battleUIType = BattleUIType.MagicStone;
        }
        
        await UISetting();
    }

    private async UniTask UISetting()
    {
        if (battleUIType == BattleUIType.MagicStone)
        {
            MoveDown(_meterPanel).Forget();
            await UniTask.WaitForSeconds(_duration);
            MoveUp(_magicStonePanel).Forget();
        }
        else
        {
            MoveDown(_magicStonePanel).Forget();
            await UniTask.WaitForSeconds(_duration);
            MoveUp(_meterPanel).Forget();
        }

        _isMoving = false;
    }

    private async UniTask MoveUp(RectTransform rectTransform)
    {
        _isMoving = true;

        await rectTransform.DOAnchorPos(_upPosition, _duration)
            .SetEase(_upEase)
            .SetLink(gameObject)            
            .AsyncWaitForCompletion();
    }

    private async UniTask MoveDown(RectTransform rectTransform)
    {
        _isMoving = true;

        await rectTransform.DOAnchorPos(_downPosition, _duration)
            .SetEase(_downEase)
            .SetLink(gameObject) 
            .AsyncWaitForCompletion();
    }
}
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PartySelectPanelController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject _stageSelectPanel;
    [SerializeField] private GameObject _partySelectPanel;
    [SerializeField] private BottomPanelController _bottomPanelCtrl;

    [Header("Button")]
    [SerializeField] private Button _gameStartButton;
    [SerializeField] private Button _arrangeButton;

    public Action OnSelectedIndexChanged;

    private void OnEnable()
    {
        _gameStartButton.onClick.AddListener(GameStart);
        _arrangeButton.onClick.AddListener(ArrangePreset);
        OnSelectedIndexChanged += ActivateGameStartButton;
        ActivateGameStartButton();
    }

    private void OnDisable()
    {
        OnSelectedIndexChanged -= ActivateGameStartButton;
    }

    public void SetSelectedPresetIndex(int presetIndex)
    {
        if (Manager.Data != null)
        {
            Manager.Data.PresetDB.SelectPresetIndex(presetIndex);
            OnSelectedIndexChanged?.Invoke();
        }
    }

    private void ActivateGameStartButton()
    { 
        if(Manager.Data != null && Manager.Data.PresetDB.SelectedPresetIndex != -1)
        {
            _gameStartButton.interactable = true;
        }
        else
        {
            _gameStartButton.interactable = false;
        }
    }

    private async void GameStart()
    {
        // 씬 전환
        await SceneChangeManager.Instance.LoadSceneAsync("GameScene", GameSceneInit);

        Debug.Log("게임 시작");
    }

    private async UniTask GameSceneInit()
    {
        await Manager.Resources.LoadLabel("Stage");

        // 나중에 스테이지가 결정되고 스테이지 1-2 같은 데이터 들이 저장이 되면 바꿀 것
        await Manager.Data.StageGridData.SetGridData(1, 1);
    }

    public void ArrangePreset()
    {
        _bottomPanelCtrl.SelectButton(3);
        _stageSelectPanel.SetActive(false);
        _partySelectPanel.SetActive(false);
    }
}

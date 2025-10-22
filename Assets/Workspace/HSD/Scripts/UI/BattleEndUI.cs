using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleEndUI : MonoBehaviour
{
    private void OnEnable()
    {
        BattleManager.OnPlayerDefeat += ShowPopup;
    }

    private void OnDisable()
    {
        BattleManager.OnPlayerDefeat -= ShowPopup;
    }

    private void ShowPopup()
    {
        Debug.Log("전투종료 팝업");
        PopupManager.Instance.ShowConfirmationPopup("전투가 종료되었습니다.\n로비로 돌아가시겠습니까?", async () =>
        {
            await WaitForClose();
        });
    }

    private async UniTask WaitForClose()
    {
        await SceneChangeManager.Instance.LoadSceneAsync("LobbyScene", Clear);

        await Manager.DB.SetTutorialCompleteAsync();
    }

    private async UniTask Clear()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        await UniTask.Delay(100);
    }
}

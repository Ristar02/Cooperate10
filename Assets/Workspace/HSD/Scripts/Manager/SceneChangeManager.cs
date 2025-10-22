using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SceneChangeManager : Singleton<SceneChangeManager>
{
    [SerializeField] FadeScreen _fadeScreen;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    public async UniTask LoadSceneAsync(string sceneName, System.Func<UniTask> init = null)
    {
        await LoadSceneAsyncTask(sceneName, init);
    }

    private async UniTask LoadSceneAsyncTask(string sceneName, System.Func<UniTask> init)
    {
        await _fadeScreen.FadeIn();

        if(init != null)
            await init.Invoke();

        await Addressables.LoadSceneAsync(sceneName);

        await UniTask.WaitForSeconds(.5f);

        await _fadeScreen.FadeOut();
    }
}

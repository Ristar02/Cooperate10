using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private static readonly int[] accelerates = { 1, 2, 4 };    
    public Property<float> CurrentAccelerate = new Property<float>();
    public Property<bool> IsPause = new Property<bool>();
    private int _currentIdx = 0;

    public void NextAccelerate()
    {
        _currentIdx = (_currentIdx + 1) % accelerates.Length;
        SetAccelerate(_currentIdx);
        CurrentAccelerate.Value = accelerates[_currentIdx];
    }

    public void ChangePause()
    {
        if(IsPause.Value)
        {
            IsPause.Value = false;
            SetAccelerate(_currentIdx);
        }
        else
        {
            IsPause.Value = true;
            SetTimeScale(0);
        }
    }

    private void SetAccelerate(int idx)
    {
        if (IsPause.Value)
            return;

        _currentIdx = idx;
        SetTimeScale(GetTimeScale());
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * scale;
    }

    public async UniTask SlowMotionAsync(float scale, float duration)
    {
        Time.timeScale = scale * GetTimeScale();
        
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration), true);

        SetTimeScale(GetTimeScale());
    }

    public async UniTask CameraDoMove(Vector2 targetPos, float duration, float zoomSize)
    {
        duration = duration / (Time.timeScale * GetTimeScale());

        Camera.main.transform.DOMove(new Vector3(targetPos.x, targetPos.y, -10), duration);
        await Camera.main.DOOrthoSize(zoomSize, duration).AsyncWaitForCompletion();
    }

    private float GetTimeScale()
    {
        return accelerates[_currentIdx];
    }
}

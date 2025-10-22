using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using DG.Tweening;

public class AddressablesDownloader : MonoBehaviour
{
    [Header("Download Settings")]
    public List<AssetLabelReference> LabelsToDownload;

    [Header("Progress Info")]
    public Property<float> DownloadProgress = new();
    [SerializeField] float _speed = 10;

    private Tween _progressTween;
    private Tween _downloadSizeTween;
    public long TotalFileSize;
    public Property<long> DownloadedSize = new();
    public bool IsDownloading;
    public bool IsChecking;

    public Action<long> OnNeedDownloading;
    public Action OnDontNeedDownloading;
    public Action OnDownloadEnded;
    public static bool IsDownloaded = false;

    public void Check()
    {
        if (LabelsToDownload == null || LabelsToDownload.Count == 0)
        {            
            Debug.LogWarning("[어드레서블] 다운로드할 라벨이 설정되지 않았습니다.");
            return;
        }

        CheckForDownloads().Forget();
    }
    #region Download
    /// <s$ummary>
    /// 어드레서블을 초기화하고 다운로드가 필요한지 체크만 함
    /// </summary>
    public async UniTask CheckForDownloads()
    {
        if (IsChecking || IsDownloading) return;

        IsChecking = true;

        try
        {
            await InitializeAddressables();
            await CheckCatalogUpdates();

            // 다운로드 필요한지 체크
            long downloadSize = await GetTotalDownloadSize();
            TotalFileSize = downloadSize;

            if (downloadSize > 0)
            {
                Debug.Log($"[어드레서블] 다운로드할 파일이 있습니다. 크기: {FormatBytes(downloadSize)}");
                OnNeedDownloading?.Invoke(downloadSize);
            }
            else
            {
                Debug.Log("[어드레서블] 다운로드할 파일이 없습니다. 모든 파일이 최신 상태입니다.");
                IsDownloaded = true;
                OnDontNeedDownloading?.Invoke();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[어드레서블] 체크 실패: {e.Message}");
        }
        finally
        {
            IsChecking = false;
        }
    }

    /// <summary>
    /// 실제 다운로드 실행 (OnNeedDownloading 이벤트 후 사용자가 승인하면 호출)
    /// </summary>
    public async UniTask StartDownload()
    {
        if (IsDownloading || IsChecking) return;

        try
        {
            await DownloadAllLabels();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[어드레서블] 다운로드 실패: {e.Message}");
        }
    }
    private async UniTask DownloadAllLabels()
    {
        IsDownloading = true;

        try
        {
            // 모든 라벨에 대한 다운로드 사이즈 계산
            await CalculateTotalDownloadSize();

            if (TotalFileSize == 0)
            {
                Debug.Log("[어드레서블] 다운로드할 파일이 없습니다.");
                return;
            }

            Debug.Log($"[어드레서블] 다운로드 시작 - 총 크기: {FormatBytes(TotalFileSize)}");

            List<UniTask> tasks = new List<UniTask>();

            // 각 라벨별로 다운로드
            foreach (var label in LabelsToDownload)
            {
                tasks.Add(DownloadLabel(label));
            }

            await UniTask.WhenAll(tasks);

            Debug.Log("[어드레서블] 모든 라벨 다운로드 완료");
        }
        finally
        {
            await UniTask.WaitForSeconds(1f);
            OnDownloadEnded?.Invoke();
            IsDownloading = false;
            IsDownloaded = true;
        }
    }
    private async UniTask DownloadLabel(AssetLabelReference label)
    {
        // 해당 라벨의 다운로드 사이즈 확인
        var sizeHandle = Addressables.GetDownloadSizeAsync(label.labelString);
        long labelSize = await sizeHandle.ToUniTask();
        Addressables.Release(sizeHandle);

        if (labelSize > 0)
        {
            Debug.Log($"[어드레서블] {label} 다운로드 시작");

            var downloadHandle = Addressables.DownloadDependenciesAsync(label.labelString, false);

            // 진행률 모니터링
            while (!downloadHandle.IsDone)
            {
                if (downloadHandle.IsValid())
                {
                    var status = downloadHandle.GetDownloadStatus();
                    float targetValue = downloadHandle.PercentComplete;

                    if (_progressTween != null && _progressTween.IsActive())
                        _progressTween.Kill();

                    _progressTween = DOTween.To(
                        () => DownloadProgress.Value,
                        x => DownloadProgress.Value = x,
                        targetValue,
                        _speed
                    ).SetEase(Ease.Linear)
                    .SetSpeedBased();

                    if (_downloadSizeTween != null && _downloadSizeTween.IsActive())
                        _downloadSizeTween.Kill();

                    _downloadSizeTween = DOTween.To(
                        () => DownloadedSize.Value,
                        x => DownloadedSize.Value = x,
                        (long)(TotalFileSize * DownloadProgress.Value),
                        .1f
                    ).SetEase(Ease.Linear);                    

                    Debug.Log($"[어드레서블] 다운로드 중 {label}: {DownloadProgress.Value:P2} - {FormatBytes(DownloadedSize.Value)}/{FormatBytes(TotalFileSize)}");
                }

                await UniTask.Yield();
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[어드레서블] {label} 다운로드 완료");
            }
            else
            {
                Debug.LogError($"[어드레서블] {label} 다운로드 실패: {downloadHandle.OperationException}");
            }

            Addressables.Release(downloadHandle);
        }
        else
        {
            Debug.Log($"[어드레서블] {label} 라벨은 이미 다운로드되어 있습니다.");
        }
    }
    private async UniTask CalculateTotalDownloadSize()
    {
        TotalFileSize = 0;

        foreach (var label in LabelsToDownload)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(label.labelString);
            long labelSize = await sizeHandle.ToUniTask();
            TotalFileSize += labelSize;

            if (labelSize > 0)
            {
                Debug.Log($"[어드레서블] '{label}' 라벨 다운로드 크기: {FormatBytes(labelSize)}");
            }

            Addressables.Release(sizeHandle);
        }

        Debug.Log($"[어드레서블] 전체 다운로드 크기: {FormatBytes(TotalFileSize)}");
    }
    public async UniTask<long> GetTotalDownloadSize()
    {
        long totalSize = 0;

        foreach (var label in LabelsToDownload)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(label.labelString);  
            long labelSize = await sizeHandle.ToUniTask();
            totalSize += labelSize;
            Debug.Log($"[어드레서블] '{label.labelString}' 라벨 다운로드 크기 {labelSize}...");
            Addressables.Release(sizeHandle);
        }

        return totalSize;
    }
    #endregion
    private async UniTask InitializeAddressables()
    {
        var initHandle = Addressables.InitializeAsync();
        await initHandle.ToUniTask();

        if (initHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("[어드레서블] 초기화 완료");
        }
        else
        {
            throw new System.Exception("[어드레서블] 초기화 실패");
        }

        Addressables.Release(initHandle);
    }

    private async UniTask CheckCatalogUpdates()
    {
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        var catalogsToUpdate = await checkHandle.ToUniTask();

        if (catalogsToUpdate.Count > 0)
        {
            Debug.Log($"[어드레서블] 업데이트할 카탈로그 수: {catalogsToUpdate.Count}");

            var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);
            await updateHandle.ToUniTask();

            Debug.Log("[어드레서블] 카탈로그 업데이트 완료");
            Addressables.Release(updateHandle);
        }
        else
        {
            Debug.Log("[어드레서블] 업데이트할 카탈로그 없음");
        }

        Addressables.Release(checkHandle);
    }

    public async UniTask<bool> CheckForUpdates()
    {
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        var catalogsToUpdate = await checkHandle.ToUniTask();
        bool hasUpdates = catalogsToUpdate.Count > 0;

        Addressables.Release(checkHandle);
        return hasUpdates;
    }


    public string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = (decimal)bytes;

        while (System.Math.Round(number / 1024) >= 1)
        {
            number = number / 1024;
            counter++;
        }

        return string.Format("{0:n1} {1}", number, suffixes[counter]);
    }

#region Test    
    [ContextMenu("Check for Downloads")]
    private async void TestCheckForDownloads()
    {
        await CheckForDownloads();
    }

    [ContextMenu("Start Download")]
    private async void TestStartDownload()
    {
        await StartDownload();
    }

    [ContextMenu("Get Download Size")]
    private async void TestGetDownloadSize()
    {
        long size = await GetTotalDownloadSize();
        Debug.Log($"전체 다운로드 크기: {FormatBytes(size)}");
    }
#endregion
}
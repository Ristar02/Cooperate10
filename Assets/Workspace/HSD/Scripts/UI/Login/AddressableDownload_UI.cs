using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddressableDownload_UI : UIBase
{
    [Header("UI")]
    [UIBind("DownloadSlider")] Slider _downloadSlider;
    [UIBind("DownloadCapacityAmount")] TMP_Text _capacityAmountText;

    [UIBind("Addressable_Downloader")] AddressablesDownloader _downloader;
    [UIBind("Download_Popup")] Download_Popup download_Popup;

    private void OnEnable()
    {
        Subscribe();
        Check();
    }

    private void OnDisable()
    {
        UnSubscribe();
    }

    #region Event
    private void Subscribe()
    {
        _downloader.OnNeedDownloading += (size) => download_Popup.Show(_downloader.FormatBytes(size), Download);

        _downloader.OnDownloadEnded += DownloadEnded;
        _downloader.DownloadProgress.AddEvent(DownloadProgressUpdate);
        _downloader.DownloadedSize.AddEvent(CurrentCapacityUpdate);
    }

    private void UnSubscribe()
    {
        _downloader.DownloadProgress.RemoveEvent(DownloadProgressUpdate);
        _downloader.DownloadedSize.RemoveEvent(CurrentCapacityUpdate);
    }
    #endregion

    [ContextMenu("CheckUpdate")]
    private void Check()
    {
        _downloader.Check();
    }

    private void Download()
    {
        _downloadSlider.gameObject.SetActive(true);
        _downloader.StartDownload().Forget();
    }

    private void DownloadEnded()
    {
        _downloadSlider.gameObject.SetActive(false);
    }    

    private void CurrentCapacityUpdate(long currentCapacity)
    {
        _capacityAmountText.text = $"{_downloader.FormatBytes(currentCapacity)} / {_downloader.FormatBytes(_downloader.TotalFileSize)}";
    }

    private void DownloadProgressUpdate(float progress)
    {
        _downloadSlider.value = progress;
    }
}

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Download_Popup : UIBase
{
    [TextArea] public string message;
    [UIBind("DownloadText")] TMP_Text _downloadText;
    [UIBind("ConfirmButton")] Button _confirmButton;
    [UIBind("CancelButton")] Button _cancelButton;
    [SerializeField] GameObject _popup;    

    private void OnEnable()
    {
        _cancelButton.onClick.AddListener(Cancel);
    }

    public void Show(string downloadSize, UnityAction confirm)
    {
        _confirmButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(Close);
        _confirmButton.onClick.AddListener(confirm);

        _downloadText.text = message.Replace("{downloadSize}", downloadSize.ToString());
        _popup.SetActive(true);
    }

    public void Close()
    {
        _popup.SetActive(false);
    }

    private void Cancel()
    {
        Application.Quit();
    }
}

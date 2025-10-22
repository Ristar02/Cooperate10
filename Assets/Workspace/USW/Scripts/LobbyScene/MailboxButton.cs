using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailboxButton : MonoBehaviour
{
    [SerializeField] Button button;
    
    void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OpenMailbox);
        }
    }
    
    public void OpenMailbox()
    {
        var popup = PopupManager.Instance;
        
        popup.ShowMailboxPopup(() => {
            Debug.Log("메일박스가 닫혔습니다!");
        });
    }
    
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OpenMailbox);
        }
    }
}
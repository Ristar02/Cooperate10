using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelCtrl : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingPanel;
    public GameObject couponPanel;
    public GameObject acountPanel;

    [Header("Buttons")]
    public Button couponBtn;
    public Button acountBtn;

    void Start()
    {
        // 버튼 이벤트 등록
        couponBtn.onClick.AddListener(OpenCoupon);
        acountBtn.onClick.AddListener(OpenAcount);
    }

    void OpenCoupon()
    {
        //settingPanel.SetActive(false);
        couponPanel.SetActive(true);
    }

    void OpenAcount()
    {
        //settingPanel.SetActive(false);
        acountPanel.SetActive(true);
    }

    

    
}

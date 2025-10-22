using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagicStoneUpgradeUnit : MonoBehaviour
{
    private UpgradeManager _manager;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(ShowPopup);
        _manager = GetComponentInParent<UpgradeManager>();
    }

    private void ShowPopup()
    {
        _manager.ShowStonePopUp();
    }
}

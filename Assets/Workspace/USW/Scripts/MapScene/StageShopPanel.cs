using Map;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace map
{
    public class StageShopPanel : MonoBehaviour
    {
        public static StageShopPanel Instance;

        public GameObject shopPanelUI; // Inspector에서 실제 패널 연결
        public Button _backButton;

        private void Awake()
        {
            Instance = this;
            Debug.Log("StageShopPanel Instance created!");
            // 여기서는 gameObject.SetActive(false) 하지 않음!
        }

        private void Start()
        {
            if (shopPanelUI != null)
            {
                shopPanelUI.SetActive(false); // 패널만 비활성화
            }
            
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(CloseShop);
            }
        }

        public void OpenShop()
        {
            Debug.Log("OpenShop called!");
            if (shopPanelUI != null)
            {
                shopPanelUI.SetActive(true);
                Debug.Log("Shop panel activated!");
            }
            else
            {
                Debug.LogError("shopPanelUI is not assigned!");
            }
        }

        public void CloseShop()
        {
            if (shopPanelUI != null)
            {
                shopPanelUI.SetActive(false);
            }

            if (MapPlayerTracker.Instance != null)
            {
                MapPlayerTracker.OnEventEnded?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(CloseShop);
            }
        }
    }
}
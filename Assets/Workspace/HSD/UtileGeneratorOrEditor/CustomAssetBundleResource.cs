using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement;
using System.ComponentModel;

namespace Custom.Addressables
{
    [DisplayName("CustomAssetBundle Provider")]
    public class CustomAssetBundleResource : IAssetBundleResource, IUpdateReceiver
    {
        private AssetBundle m_AssetBundle;
        private UnityEngine.AsyncOperation m_RequestOperation;

        public AssetBundle GetAssetBundle()
        {
            return m_AssetBundle;
        }

        public void Start(ProvideHandle provideHandle)
        {
            string path = provideHandle.ResourceManager.TransformInternalId(provideHandle.Location);
            m_RequestOperation = AssetBundle.LoadFromFileAsync(path);
            m_RequestOperation.completed += (op) =>
            {
                var req = op as AssetBundleCreateRequest;
                m_AssetBundle = req.assetBundle;
                provideHandle.Complete<CustomAssetBundleResource>(this, m_AssetBundle != null, null);
            };
        }

        // Addressable은 원래 Unload를 해버리면 생성된 인스턴스도 같이 내려가는 구조로 되어있음.
        // 하지만 이러면 계속 Load 되어 있어야하는 문제가 있음. 그래서 UnloadAsync(false)로 바꿔서 인스턴스는 유지되도록 변경.
        // 결국 필요한 Object Load를 하고 생성한 뒤 필요가 없어졌으면 Unload하는 방식으로 할 수 있음.
        // 상주하는 메모리가 줄어드는 결과를 가져옴
        // UnloadAsync(false) = 인스턴스 유지됨
        public bool Unload(out AssetBundleUnloadOperation unloadOp)
        {
            unloadOp = null;
            if (m_AssetBundle != null)
            {
                unloadOp = m_AssetBundle.UnloadAsync(false); // false
                m_AssetBundle = null;
            }
            m_RequestOperation = null;
            return unloadOp != null;
        }

        public void Update(float unscaledDeltaTime) { }
    }
}

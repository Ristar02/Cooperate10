using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ResourcesManager : Singleton<ResourcesManager>
{
    private SerializedDictionary<string, Object> _resources = new SerializedDictionary<string, Object>();    
    private static Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

    #region Get
    public async UniTask<T> LoadRefAsync<T>(AssetReference reference) where T : Object
    {
        string primaryKey = await GetPrimaryKey(reference);
        
        if (!_resources.ContainsKey(primaryKey))
            return null;

        return _resources[primaryKey] as T;
    }

    public T Load<T>(string address) where T : Object
    {
        if(string.IsNullOrEmpty(address)) return null;

        if (!_resources.ContainsKey(address))
        {
            Debug.LogWarning($"[AddressableSystem] {address} 주소의 에셋이 로드되지 않았습니다.");
            return null;
        }

        return _resources[address] as T;
    }

    public async UniTask<T> LoadAsync<T>(string address) where T : Object
    {
        if (string.IsNullOrEmpty(address)) return null;

        if (!_resources.ContainsKey(address))
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            var asset = await handle.Task;

            _resources.Add(address, asset);
        }

        return _resources[address] as T;
    }
    #endregion

    #region Sprite
    public Sprite SpriteGet(string address)
    {
        if(!_sprites.ContainsKey(address))
        {
            Debug.Log($"[스프라이트] {address}의 Sprite가 없습니다.");
            return null;
        }

        return _sprites[address];
    }

    public async UniTask SpriteLoadLable(string label)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        var locations = await locationsHandle.Task;

        List<UniTask> tasks = new List<UniTask>(100);

        foreach (var location in locations)
        {
            tasks.Add(SpriteLoadAndCache(location));
        }

        await UniTask.WhenAll(tasks);

        Addressables.Release(locationsHandle);
    }

    private async UniTask SpriteLoadAndCache(IResourceLocation location)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(location.PrimaryKey);
        var asset = await handle.Task;

        if (!_sprites.ContainsKey(location.PrimaryKey))
        {
            _sprites.Add(location.PrimaryKey, asset);
        }
    }
    #endregion

    #region Load&Unload Label
    public UniTask LoadAllLabel(AssetLabelReference[] labels)
    {
        List<UniTask> tasks = new List<UniTask>();
        foreach (var label in labels)
        {
            tasks.Add(LoadLabel(label));
        }

        return UniTask.WhenAll(tasks);
    }
    public UniTask UnloadAllLabel(AssetLabelReference[] labels)
    {
        List<UniTask> tasks = new List<UniTask>();
        foreach (var label in labels)
        {
            tasks.Add(UnloadLabel(label));
        }
        return UniTask.WhenAll(tasks);
    }
    #endregion

    #region Load
    public async UniTask LoadLabel(string label)
    {
        if (string.IsNullOrEmpty(label)) return;

        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        var locations = await locationsHandle.Task;

        List<UniTask> tasks = new List<UniTask>(100);

        foreach (var location in locations)
        {
            tasks.Add(LoadAndCache(location));
        }

        await UniTask.WhenAll(tasks);

        Addressables.Release(locationsHandle);
        Debug.Log($"[로드 성공] : {label}");
    }

    public async UniTask LoadLabel(AssetLabelReference label)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        var locations = await locationsHandle.Task;

        List<UniTask> tasks = new List<UniTask>(100);

        foreach (var location in locations)
        {
            tasks.Add(LoadAndCache(location));
        }

        await UniTask.WhenAll(tasks);

        Addressables.Release(locationsHandle);
    }

    private async UniTask LoadAndCache(IResourceLocation location)
    {
        var handle = Addressables.LoadAssetAsync<Object>(location);
        var asset = await handle.Task;

        if (!_resources.ContainsKey(location.PrimaryKey))
            _resources.Add(location.PrimaryKey, asset);
        else
            _resources[location.PrimaryKey] = asset;
    }

    public async UniTask<T[]> LoadAll<T>(string label) where T : Object
    {
        if (string.IsNullOrEmpty(label)) return null;

        var handle = Addressables.LoadAssetsAsync<T>(label, null);
        var result = await handle.Task;

        return result.ToArray();
    }

    public async UniTask<T> LoadAsync<T>(AssetReference reference) where T : Object
    {
        string primaryKey = await GetPrimaryKey(reference);

        if (!_resources.ContainsKey(primaryKey))
        {
            var handle = reference.LoadAssetAsync<T>();
            var asset = await handle.Task;
            _resources.Add(primaryKey, asset);
        }

        return _resources[primaryKey] as T;
    }
    #endregion

    #region Unload
    public async UniTask UnloadLabel(string label)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        var locations = await locationsHandle.Task;

        foreach (var location in locations)
        {
            if (_resources.ContainsKey(location.PrimaryKey))
            {
                Unload(location.PrimaryKey);
            }
        }

        Addressables.Release(locationsHandle);
    }

    public async UniTask UnloadLabel(AssetLabelReference label)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        var locations = await locationsHandle.Task;        

        foreach (var location in locations)
        {
            if (_resources.ContainsKey(location.PrimaryKey))
            {
                Unload(location.PrimaryKey);
            }
        }

        Addressables.Release(locationsHandle);
    }

    public void Unload(string path)
    {
        if (_resources.TryGetValue(path, out var asset))
        {
            Addressables.Release(asset);
            _resources.Remove(path);
        }
    }

    public void UnloadAll()
    {
        foreach (var kvp in _resources)
        {
            Addressables.Release(kvp.Value);
        }
        _resources.Clear();
    }
    #endregion

    private async UniTask<string> GetPrimaryKey(AssetReference reference)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(reference);
        var locations = await locationsHandle.Task;
        string primaryKey = locations.FirstOrDefault()?.PrimaryKey ?? reference.RuntimeKey.ToString();
        Addressables.Release(locationsHandle);
        return primaryKey;
    }

    #region Instantiate&Destroy
    public T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent, bool isPool = false) where T : Object
    {
        GameObject obj = original as GameObject;

        if (isPool)
            return Manager.Pool.Get(obj, position, rotation, parent) as T;
        else
            return Object.Instantiate(obj, position, rotation, parent) as T;
    }

    public T Instantiate<T>(T original, Vector3 position, Quaternion rotation, bool isPool = false) where T : Object
    {
        return Instantiate(original, position, rotation, null, isPool);
    }

    public T Instantiate<T>(T original, Vector3 position, bool isPool = false) where T : Object
    {
        return Instantiate(original, position, Quaternion.identity, null, isPool);
    }
    public T Instantiate<T>(T original, Transform parent, bool isPool = false) where T : Object
    {
        return Instantiate(original, Vector3.zero, Quaternion.identity, parent, isPool);
    }

    public T Instantiate<T>(string path, Vector3 position, Quaternion rotation, Transform parent, bool isPool = false) where T : Object
    {
        T obj = Load<T>(path);
        return Instantiate(obj, position, rotation, parent, isPool);
    }

    public T Instantiate<T>(string path, Vector3 position, Quaternion rotation, bool isPool = false) where T : Object
    {
        return Instantiate<T>(path, position, rotation, null, isPool);
    }

    public T Instantiate<T>(string path, Vector3 postion, bool isPool = false) where T : Object
    {
        return Instantiate<T>(path, postion, Quaternion.identity, null, isPool);
    }
    public T Instantiate<T>(string path, Transform parent, bool isPool = false) where T : Object
    {
        return Instantiate<T>(path, Vector3.zero, Quaternion.identity, parent, isPool);
    }

    public void Destroy(GameObject obj)
    {
        if (obj == null || !obj.activeSelf) return;

        if (Manager.Pool.ContainsKey(obj.name))
            Manager.Pool.Release(obj);
        else
            Object.Destroy(obj);
    }

    public void Destroy(GameObject obj, float delay)
    {
        if (Manager.Pool.ContainsKey(obj.name))
            Manager.Pool.Release(obj, delay);
        else
            Object.Destroy(obj, delay);
    }
    #endregion
}
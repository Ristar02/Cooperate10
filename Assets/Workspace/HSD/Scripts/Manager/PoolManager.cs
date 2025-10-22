using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<string, IObjectPool<GameObject>> _poolDic;
    private Dictionary<string, Transform> _parentDic;
    private Dictionary<string, float> _lastUseTimeDic;

    private Transform _parent;
    private Transform _uiParent;

    private const float _poolCleanupTime = 60;
    private const float _poolCleanupDelay = 30;

    public void Start()
    {
        ResetPool();
        PoolCleanupRoutine().Forget();

        SceneManager.sceneLoaded += OnSceneLoaded;        
    }

    public void ResetPool()
    {
        _poolDic = new();
        _parentDic = new();
        _lastUseTimeDic = new();

        _parent = new GameObject("Pools").transform;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetPool();
    }

    private async UniTask PoolCleanupRoutine()
    {
        while (true)
        {
            await UniTask.WaitForSeconds(_poolCleanupDelay); 

            float now = Time.time;
            List<string> removePoolKeys = new List<string>();

            foreach (var kvp in _poolDic)
            {
                string key = kvp.Key;

                if (_lastUseTimeDic.TryGetValue(key, out float lastTime))
                {
                    if (now - lastTime > _poolCleanupTime)
                    {
                        removePoolKeys.Add(key);
                    }
                }
            }

            foreach (var value in removePoolKeys)
            {
                _poolDic.Remove(value);

                if (_parentDic[value].gameObject != null)
                    Destroy(_parentDic[value].gameObject);

                _parentDic.Remove(value);
                _lastUseTimeDic.Remove(value);
            }
        }
    }

    private IObjectPool<GameObject> GetOrCreatePool(string name, GameObject prefab)
    {
        if(_poolDic.ContainsKey(name))
            return _poolDic[name];

        Transform root = new GameObject($"{name} Pool").transform;
        root.parent = _parent;
        _parentDic.Add(name, root);

        ObjectPool<GameObject> pool = new ObjectPool<GameObject>
        (
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                obj.name = name;
                obj.transform.SetParent(root, false);
                _lastUseTimeDic[name] = Time.time;
                return obj;
            },
            actionOnGet: (GameObject go) =>
            {
                go.transform.SetParent(null, false);
                _lastUseTimeDic[name] = Time.time;
            },
            actionOnRelease: (GameObject go) =>
            {
                go.transform.SetParent(root, false);
                go.SetActive(false);
            },
            actionOnDestroy: (GameObject go) =>
            {
                Destroy(go);
            },
            maxSize: 10
        );

        _poolDic.Add(name, pool);
        return pool;
    }

    //#region Popup
    //public void PopUpInit(Transform uiParent)
    //{
    //    _uiParent = uiParent;
    //    _damagePopUp = Manager.Resources.Get<GameObject>("DamagePopUp").GetComponent<DamagePopUp>();
    //    CreatePopUpPool();
    //}
    //private void CreatePopUpPool()
    //{
    //    _popUpPool = new ObjectPool<DamagePopUp>
    //    (
    //        createFunc: () =>
    //        {
    //            DamagePopUp obj = Instantiate(_damagePopUp);
    //            obj.name = "DamagePopUp";
    //            obj.transform.SetParent(_uiParent);
    //            _lastUseTimeDic[obj.name] = Time.time;
    //            return obj;
    //        },
    //        actionOnGet: (DamagePopUp damagePopUp) =>
    //        {
    //            damagePopUp.gameObject.SetActive(true);
    //            _lastUseTimeDic[damagePopUp.gameObject.name] = Time.time;
    //        },
    //        actionOnRelease: (DamagePopUp damagePopUp) =>
    //        {
    //            damagePopUp.gameObject.SetActive(false);
    //        },
    //        actionOnDestroy: (DamagePopUp damagePopUp) =>
    //        {
    //            Destroy(damagePopUp.gameObject);
    //        },
    //        maxSize: 10
    //    );
    //}

    //public DamagePopUp GetPopUp(Vector2 pos)
    //{
    //    DamagePopUp popUp = _popUpPool.Get();
    //    popUp.transform.position = pos;            

    //    return popUp;
    //}

    //public void ReleasePopUp(DamagePopUp popUp)
    //{
    //    if (popUp == null || !popUp.gameObject.activeSelf) return;

    //    _popUpPool.Release(popUp);
    //}
    //#endregion

    #region Get
    public T Get<T> (T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
    {
        if (original == null) return null;

        GameObject go = original as GameObject;
        string name = go.name;

        var pool = GetOrCreatePool(name, go);

        go = pool.Get();        

        if (parent != null)
            go.transform.SetParent(parent, false);

        go.transform.position = position;
        go.transform.rotation = rotation;

        go.SetActive(true);

        return go as T;
    }

    public T Get<T>(T original, Vector3 position, Quaternion rotation) where T : Object
    {
        return Get<T>(original, position, rotation, null);
    }

    public T Get<T>(T original, Vector3 position) where T : Object
    {
        return Get<T>(original, position, Quaternion.identity);
    }

    public T Get<T>(T original, Vector3 position, Transform parent) where T : Object
    {
        return Get<T>(original, position, Quaternion.identity, parent);
    }
    #endregion

    #region Release
    public void Release<T> (T original) where T : Object
    {
        GameObject obj = original as GameObject;
        string name = obj.name;

        if (!_poolDic.ContainsKey(name) && !obj.activeSelf)
            return;

        _poolDic[name].Release(obj);
    }

    public void Release<T>(T original, float delay) where T : Object
    {
        StartCoroutine(DelayRelease(original, delay));
    }

    private IEnumerator DelayRelease<T>(T original, float delay) where T : Object
    {
        yield return new WaitForSeconds(delay);

        GameObject obj = original as GameObject;

        if (obj == null || !obj.activeSelf) yield break;

        string name = obj.name;

        if (!_poolDic.ContainsKey(name) && !obj.activeSelf)
            yield break;

        _poolDic[name].Release(obj);
    }
    #endregion

    public bool ContainsKey(string name) => _poolDic.ContainsKey(name);
}

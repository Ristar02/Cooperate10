using UnityEngine;

public class CYH_Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _isShuttingDown = false;
    private static object _lock = new object();

    [Header("Singleton Settings")]
    [SerializeField] private bool _dontDestroyOnLoad = true;

    public static T Instance
    {
        get
        {
            if (_isShuttingDown)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();

                        var singletonComponent = _instance as CYH_Singleton<T>;
                        if (singletonComponent != null && singletonComponent._dontDestroyOnLoad)
                        {
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _isShuttingDown = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _isShuttingDown = true;
        }
    }
}

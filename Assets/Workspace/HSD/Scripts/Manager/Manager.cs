using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Manager
{
    public static ResourcesManager Resources => ResourcesManager.Instance;
    public static PoolManager Pool => PoolManager.Instance;
    public static GameManager Game => GameManager.Instance;
    public static DataManager Data => DataManager.Instance;
    public static FirebaseManager Firebase => FirebaseManager.Instance;
    public static AuthManager Auth => AuthManager.Instance;
    public static DBManager DB => DBManager.Instance;
    public static PopupManager Popup => PopupManager.Instance;
    public static IAPManager IAP => IAPManager.Instance;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        ResourcesManager.CreateInstance();
        DataManager.CreateInstance();
        PoolManager.CreateInstance();
        GameManager.CreateInstance();
        FirebaseManager.CreateInstance();
        AuthManager.CreateInstance();
        DBManager.CreateInstance();
        PopupManager.CreateInstance();
        IAPManager.CreateInstance();
    }
}

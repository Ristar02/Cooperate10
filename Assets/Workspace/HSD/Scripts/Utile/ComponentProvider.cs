using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentProvider
{
    private static readonly Dictionary<string, Component> _components = new Dictionary<string, Component>(5000);

    public static void Add<T>(GameObject obj, T component) where T : Component
    {
        string key = GenerateKey<T>(obj);

        if (!_components.ContainsKey(key))
        {            
            _components[key] = component;
        }        
    }

    public static void Remove<T>(GameObject obj) where T : Component
    {
        string key = GenerateKey<T>(obj);
        if (_components.ContainsKey(key))
        {
            _components.Remove(key);
        }
    }

    public static T Get<T>(GameObject obj) where T : Component
    {
        if(obj == null)
            return null;

        string key = GenerateKey<T>(obj);

        if (_components.TryGetValue(key, out Component component))
        {            
            return component as T;
        }
        return null;
    }

    private static string GenerateKey<T>(GameObject obj) where T : Component    
    {
        return $"{obj.GetInstanceID()}_{typeof(T).Name}";
    }
}

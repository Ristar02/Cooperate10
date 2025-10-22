using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public struct Modifier<T>
{
    public T Value;
    public string Source;

    public Modifier(T value, string source)
    {
        Value = value;
        Source = source;
    }
}

[Serializable]
public class Stat<T> where T : struct, IComparable, IEquatable<T>
{
    [SerializeField] private T _baseValue;
    [SerializeField] private List<Modifier<T>> _modifiers = new();

    private bool _isChanged = true;
    private T _lastValue;

    public Action<T> OnChanged;

    // 타입별 덧셈 연산 델리게이트
    private readonly Func<T, T, T> addFunc;

    public Stat()
    {
        if (typeof(T) == typeof(int))
            addFunc = (a, b) => (T)(object)((int)(object)a + (int)(object)b);
        else if (typeof(T) == typeof(float))
            addFunc = (a, b) => (T)(object)((float)(object)a + (float)(object)b);
        else
            throw new NotSupportedException(typeof(T).Name);        
    }

    public T Value
    {
        get
        {
            if (_isChanged)
            {
                _lastValue = _baseValue;
                foreach (var modifier in _modifiers)
                    _lastValue = addFunc(_lastValue, modifier.Value);
                _isChanged = false;
            }
            return _lastValue;
        }
    }

    public void SetBaseStat(T value)
    {
        _baseValue = value;
    }

    public void AddModifier(T value, string source)
    {
        _modifiers.Add(new Modifier<T>(value, source));
        _isChanged = true;
        OnChanged?.Invoke(Value);
    }

    public void RemoveModifier(string source)
    {
        _modifiers.RemoveAll(m => m.Source == source);
        _isChanged = true;
        OnChanged?.Invoke(Value);
    }

    public void ClearModifiers() => _modifiers.Clear();
}
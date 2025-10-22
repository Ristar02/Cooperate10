using System;

[Serializable]
public class Property<T>
{
    private T _value;

    [UnityEngine.SerializeField] public T Value
    {
        get => _value;
        set 
        {
            _value = value; 

            _onChanged?.Invoke(_value); 
        }
    }

    public Property()
    {
        Value = default;
    }

    private event Action<T> _onChanged;

    public void AddEvent(Action<T> action)
    {
        _onChanged += action;
    }

    public void RemoveEvent(Action<T> action)
    {
        _onChanged -= action;
    }
}

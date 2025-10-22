using System;

public class UIBindAttribute : Attribute
{
    public string Path { get; }

    public UIBindAttribute(string path = null)
    {
        Path = path;
    }
}

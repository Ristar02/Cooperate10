using System;
using UnityEngine;

public class ButtonAttribute : Attribute
{
    public string ButtonName;

    public ButtonAttribute(string buttonName = "")
    {
        ButtonName = buttonName;
    }
}
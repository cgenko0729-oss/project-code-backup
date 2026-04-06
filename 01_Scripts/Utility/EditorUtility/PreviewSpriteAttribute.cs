using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]

public class PreviewSpriteAttribute : PropertyAttribute
{
    // We don't need parameters, but we could add height control later
    public PreviewSpriteAttribute() { }
}


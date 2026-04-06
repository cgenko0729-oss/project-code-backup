using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]

public class ReadOnlyAttribute : PropertyAttribute 
{ 
    public readonly bool runtimeOnly;

    // Constructor: Allows [ReadOnly(true)] to only lock during Play Mode
    public ReadOnlyAttribute(bool runtimeOnly = false)
    {
        this.runtimeOnly = runtimeOnly;
    }

}
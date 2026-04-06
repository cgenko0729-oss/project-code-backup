using System;

[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class JPNameAttribute : Attribute
{
    public string Text { get; }
    public JPNameAttribute(string text) => Text = text;
}

using System;
using System.Reflection;

public static class EnumJP
{
    public static string ToJP<T>(this T value) where T : Enum
    {
        var fi = value.GetType().GetField(value.ToString());
        if (fi != null && Attribute.GetCustomAttribute(fi, typeof(JPNameAttribute)) is JPNameAttribute attr)
            return attr.Text;
        return value.ToString(); // fallback
    }
}

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class IFNMGProbe
{
    public static void Run()
    {
        Type t = typeof(ModelImporter);
        foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            if (m.Name.Contains("Extract"))
                Debug.Log("[Probe] METHOD " + m.Name + " static=" + m.IsStatic + " params=[" + string.Join(", ", Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name + " " + p.Name)) + "]");
        foreach (var n in t.GetNestedTypes())
        {
            Debug.Log("[Probe] NESTED " + n.Name + " enum=" + n.IsEnum);
            if (n.IsEnum) foreach (var v in Enum.GetNames(n)) Debug.Log("[Probe]   " + n.Name + "." + v);
        }
    }
}

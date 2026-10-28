using UnityEditor;
using UnityEngine;

public static class IFNMGProbeShader
{
    public static void Run()
    {
        Shader std = Shader.Find("Standard");
        Debug.Log("[ShaderProbe] Standard=" + (std != null));
        if (std != null)
        {
            int n = std.GetPropertyCount();
            Debug.Log("[ShaderProbe] props=" + n);
            for (int i = 0; i < n; i++)
                Debug.Log("[ShaderProbe]   " + std.GetPropertyName(i) + " type=" + std.GetPropertyType(i));
        }
        Material m = new Material(std);
        Debug.Log("[ShaderProbe] HasProperty(_Cull)=" + m.HasProperty("_Cull"));
        m.SetFloat("_Cull", 0f);
        Debug.Log("[ShaderProbe] _Cull agora=" + m.GetFloat("_Cull"));
        UnityEngine.Object.DestroyImmediate(m);
    }
}

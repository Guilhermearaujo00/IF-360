using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ConverterShadersBuiltin
{
    const string PackRoot = "Assets/BluBlu Games/Low Poly Forest Mini Pack";
    const string MaterialsFolder = PackRoot + "/Materials";
    const string BuiltinFoliagePath = PackRoot + "/Shaders/BluBlu_Builtin_Foliage.shader";
    const string BuiltinVertexColorPath = PackRoot + "/Shaders/BluBlu_Builtin_VertexColor.shader";
    const string BuiltinWaterPath = PackRoot + "/Shaders/BluBlu_Builtin_Water.shader";

    static readonly Dictionary<string, float> FloatMap = new Dictionary<string, float>
    {
        { "Vector1_7722A149", 0.05f },  // Small Wind Strength
        { "Vector1_AADD838F", 10f },    // Small Wind Density
        { "Vector1_C1D16DBE", 0.08f },  // Wind Strength
        { "Vector1_C39D93FF", 0.2f },   // Wind Speed
        { "Vector1_CE098D07", 0.1f },   // Wind Density
        { "Vector1_EBB5CF27", 1f },     // Wind Speed (legacy)
    };

    [MenuItem("Tools/IFNMG/Ambientação/Converter Materiais BluBlu para Built-in")]
    public static void Converter()
    {
        var shaderFolhagem = AssetDatabase.LoadAssetAtPath<Shader>(BuiltinFoliagePath);
        var shaderVertexColor = AssetDatabase.LoadAssetAtPath<Shader>(BuiltinVertexColorPath);
        var shaderWater = AssetDatabase.LoadAssetAtPath<Shader>(BuiltinWaterPath);
        if (shaderFolhagem == null || shaderVertexColor == null || shaderWater == null)
        {
            Debug.LogError("[ConverterShadersBuiltin] Shader Built-in não encontrado em " + BuiltinFoliagePath + " / " + BuiltinVertexColorPath);
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Material", new[] { MaterialsFolder });
        int convertidos = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            if (mat.shader == null) continue;
            var shaderName = mat.shader.name;
            bool isFoliage = shaderName.StartsWith("BluBlu/SG_");
            bool isVertexColor = shaderName == "BluBlu/VertexColor" || shaderName == "BluBlu/RockMoss";
            bool isSimpleCutout = shaderName == "Shader Graphs/SG_SimpleLitCutout";
            bool isWater = shaderName == "BluBlu/Water";
            if (!isFoliage && !isVertexColor && !isSimpleCutout && !isWater) continue;

            var tex = mat.HasProperty("Texture2D_CAD82441")
                ? mat.GetTexture("Texture2D_CAD82441")
                : mat.GetTexture("_BaseMap");
            var baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
            var tint = mat.HasProperty("Color_369F793F") ? mat.GetColor("Color_369F793F") : Color.white;
            var emission = mat.HasProperty("Color_FA85148A") ? mat.GetColor("Color_FA85148A") : Color.black;
            bool showVC = mat.HasProperty("_ShowVertexColor") && mat.GetFloat("_ShowVertexColor") > 0.5f;
            bool useEmissive = mat.HasProperty("_useTextureAsEmissive") && mat.GetFloat("_useTextureAsEmissive") > 0.5f;
            float cutoff = mat.HasProperty("_Cutoff") ? mat.GetFloat("_Cutoff") : 0.5f;
            float smooth = mat.HasProperty("_Smoothness") ? mat.GetFloat("_Smoothness") : 0f;
            float metal = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
            float alphaClip = mat.HasProperty("_AlphaClip") ? mat.GetFloat("_AlphaClip") : 0f;

            Undo.RecordObject(mat, "Converter BluBlu para Built-in");

            if (isFoliage)
            {
                var wind = new Dictionary<string, float>();
                foreach (var kv in FloatMap)
                    wind[kv.Key] = mat.HasProperty(kv.Key) ? mat.GetFloat(kv.Key) : kv.Value;

                mat.shader = shaderFolhagem;
                SetTexture(mat, "_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                SetColor(mat, "_BaseColor", baseColor);
                SetColor(mat, "_LeafColourTint", tint);
                SetColor(mat, "_EmissionColor", new Color(emission.r, emission.g, emission.b, 1f));
                SetFloat(mat, "_Cutoff", cutoff);
                SetFloat(mat, "_Smoothness", smooth);
                SetFloat(mat, "_Metallic", metal);
                SetFloat(mat, "_ShowVertexColor", showVC ? 1f : 0f);
                SetFloat(mat, "_useTextureAsEmissive", useEmissive ? 1f : 0f);
                SetFloat(mat, "_WindStrength", wind["Vector1_C1D16DBE"]);
                SetFloat(mat, "_WindSpeed", wind["Vector1_C39D93FF"]);
                SetFloat(mat, "_WindDensity", wind["Vector1_CE098D07"]);
                SetFloat(mat, "_SmallWindStrength", wind["Vector1_7722A149"]);
                SetFloat(mat, "_SmallWindSpeed", wind["Vector1_EBB5CF27"]);
                SetFloat(mat, "_SmallWindDensity", wind["Vector1_AADD838F"]);
            }
            else if (isWater)
            {
                var waterColor = mat.HasProperty("_WaterColor") ? mat.GetColor("_WaterColor") : new Color(0.33f, 0.70f, 0.96f, 1f);
                var absorption = mat.HasProperty("_AbsorptionColor") ? mat.GetColor("_AbsorptionColor") : Color.black;
                var foam = mat.HasProperty("_FoamColor") ? mat.GetColor("_FoamColor") : Color.white;

                mat.shader = shaderWater;
                SetColor(mat, "_WaterColor", waterColor);
                SetColor(mat, "_AbsorptionColor", absorption);
                SetColor(mat, "_FoamColor", foam);
                SetFloat(mat, "_Smoothness", smooth);
                SetFloat(mat, "_Metallic", metal);
            }
            else
            {
                var colorProp = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                var colorTint = mat.HasProperty("_ColorTint") ? mat.GetColor("_ColorTint") : Color.white;

                mat.shader = shaderVertexColor;
                SetTexture(mat, "_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                SetColor(mat, "_BaseColor", baseColor);
                SetColor(mat, "_Color", colorProp);
                SetColor(mat, "_ColorTint", colorTint);
                SetFloat(mat, "_Cutoff", cutoff);
                SetFloat(mat, "_Smoothness", smooth);
                SetFloat(mat, "_Metallic", metal);
                SetFloat(mat, "_UseVertexColor", isVertexColor ? 1f : 0f);
                SetFloat(mat, "_AlphaClip", isSimpleCutout ? 1f : 0f);
            }

            if (alphaClip > 0.5f)
            {
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.SetOverrideTag("RenderType", "TransparentCutout");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            }
            else
            {
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.renderQueue = -1;
            }

            EditorUtility.SetDirty(mat);
            convertidos++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ConverterShadersBuiltin] {convertidos} materiais do pack BluBlu convertidos para o shader Built-in.");
    }

    static void SetTexture(Material m, string p, Texture t) { if (m.HasProperty(p)) m.SetTexture(p, t); }
    static void SetColor(Material m, string p, Color c) { if (m.HasProperty(p)) m.SetColor(p, c); }
    static void SetFloat(Material m, string p, float v) { if (m.HasProperty(p)) m.SetFloat(p, v); }

    [MenuItem("Tools/IFNMG/Ambientação/Relatar Shaders Não-Built-in (Diagnóstico)")]
    public static void Relatar()
    {
        var guids = AssetDatabase.FindAssets("t:Material");
        var sb = new System.Text.StringBuilder();
        int total = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".mat")) continue;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null || mat.shader == null) continue;
            var n = mat.shader.name;
            bool builtin = n.StartsWith("Standard") || n.StartsWith("Legacy") || n.StartsWith("Mobile/") ||
                           n.StartsWith("Particles/") || n.StartsWith("Sprites/") || n.StartsWith("UI/") ||
                           n.StartsWith("Unlit/") || n.StartsWith("BluBlu/Builtin") || n.StartsWith("Hidden/");
            if (!builtin)
            {
                sb.AppendLine(n + "\t" + path);
                total++;
            }
        }
        var outPath = "Docs/MontagemCena/SHADERS_NAO_BUILTIN.txt";
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log($"[ConverterShadersBuiltin] {total} materiais com shader não-Built-in. Relatório em {outPath}");
    }
}

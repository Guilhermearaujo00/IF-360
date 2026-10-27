using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Setup dos modelos FBX IFNMG na Campus (v3):
///  - Posicoes dos predios vem das 15 zonas "building" de Assets/IFNMG360/Config/IFNMG_Mapa_Proto_v0_1.json
///    (pointsUvFlat + mapping do JSON: worldX=(u-0.5)*W, worldZ=(0.5-v)*D).
///  - Match de nome quando possivel (biblioteca->edificio_biblioteca_*, ead->edificio_ead_*);
///    sobras preenchem as zonas restantes em ordem.
///  - Escala auto: footprint da zona -> escala uniforme do modelo; base no terreno (Y=0).
///  - Materiais: Standard cull Off (two-sided), opaque, textura via Manifest.
/// Batch: -executeMethod IFNMGModelosSetup.Setup
/// </summary>
public static class IFNMGModelosSetup
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";
    private const string ModelsDir = "Assets/Models/IFNMG";
    private const string TexDir = "Assets/Models/IFNMG/Textures";
    private const string ManDir = "Assets/Models/IFNMG/Manifests";
    private const string JsonPath = "Assets/IFNMG360/Config/IFNMG_Mapa_Proto_v0_1.json";
    private const string GroupName = "Modelos_IFNMG";
    private const string MapaName = "Mapa_IFNMG_Prototipo";

    [Serializable] private class SlotMat { public string mat; public string tex; }
    [Serializable] private class ObjEntry { public string obj; public SlotMat[] slots; }
    [Serializable] private class MatTex { public string mat; public string tex; }
    [Serializable] private class Manifest { public string model; public ObjEntry[] objects; public MatTex[] matmap; }

    [Serializable] private class ZoneData { public string id; public string category; public bool enabled; public float[] pointsUvFlat; }
    [Serializable] private class WorldData { public float worldWidthMeters; public float worldDepthMeters; public float baseY; }
    [Serializable] private class MapFile { public WorldData world; public ZoneData[] zones; }

    public static void Setup()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        MapFile map = null;
        try { map = JsonUtility.FromJson<MapFile>(File.ReadAllText(JsonPath, Encoding.UTF8)); }
        catch (Exception e) { Debug.LogWarning("[Modelos] JSON de zonas ilegivel: " + e.Message); }

        float W = map != null && map.world != null ? map.world.worldWidthMeters : 499.6f;
        float D = map != null && map.world != null ? map.world.worldDepthMeters : 364.5f;
        float baseY = map != null && map.world != null ? map.world.baseY : 0f;

        List<SlotZone> zones = new List<SlotZone>();
        if (map != null && map.zones != null)
        {
            foreach (ZoneData z in map.zones)
            {
                if (z == null || z.category != "building" || !z.enabled) continue;
                if (z.pointsUvFlat == null || z.pointsUvFlat.Length < 6) continue;
                float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
                for (int i = 0; i + 1 < z.pointsUvFlat.Length; i += 2)
                {
                    float u = z.pointsUvFlat[i], v = z.pointsUvFlat[i + 1];
                    float x = (u - 0.5f) * W;
                    float zc = (0.5f - v) * D;
                    if (x < minX) minX = x; if (x > maxX) maxX = x;
                    if (zc < minZ) minZ = zc; if (zc > maxZ) maxZ = zc;
                }
                Vector3 center = new Vector3((minX + maxX) * 0.5f, baseY, (minZ + maxZ) * 0.5f);
                Vector2 size = new Vector2(maxX - minX, maxZ - minZ);
                if (size.x < 1f || size.y < 1f) continue;
                zones.Add(new SlotZone { id = z.id, core = CoreOf(z.id), center = center, size = size, used = false });
            }
        }
        Debug.Log("[Modelos] zonas de predio no JSON: " + zones.Count);
        foreach (SlotZone z in zones)
            Debug.Log($"[Modelos]   zona \"{z.id}\" center={z.center} footprint={z.size}");

        Scene campus = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);
        foreach (GameObject go in new List<GameObject>(campus.GetRootGameObjects()))
            if (go.name == GroupName) UnityEngine.Object.DestroyImmediate(go);

        GameObject mapa = null;
        foreach (GameObject go in campus.GetRootGameObjects())
            if (go.name == MapaName) { mapa = go; break; }
        if (mapa == null) { Debug.LogError("[Modelos] mapa prototipo nao encontrado"); return; }

        GameObject group = new GameObject(GroupName);
        group.transform.SetParent(mapa.transform, false);
        group.transform.localPosition = Vector3.zero;

        List<string> modelPaths = new List<string>();
        foreach (string g in AssetDatabase.FindAssets("t:Model", new[] { ModelsDir }))
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (p.ToLower().EndsWith(".fbx")) modelPaths.Add(p);
        }
        modelPaths.Sort();

        // ---- pareamento: 1) match de nome; 2) zonas restantes em ordem; 3) grade livre ----
        List<string> models = modelPaths.Select(p => Path.GetFileNameWithoutExtension(p)).ToList();
        Dictionary<string, string> assignment = new Dictionary<string, string>();
        List<string> unassigned = new List<string>();
        foreach (string model in models)
        {
            string mn = Norm(model);
            string found = null;
            if (mn.Length >= 3)
                foreach (SlotZone z in zones)
                    if (!z.used && !string.IsNullOrEmpty(z.core) && z.core.Contains(mn)) { z.used = true; found = z.id; break; }
            if (found != null) assignment[model] = found; else unassigned.Add(model);
        }
        foreach (string model in unassigned)
        {
            SlotZone z = zones.FirstOrDefault(s => !s.used);
            if (z != null) { z.used = true; assignment[model] = z.id; }
            else assignment[model] = "__GRID__";
        }

        int gridIdx = 0, placed = 0, matFixed = 0, texMissing = 0;
        foreach (string model in models)
        {
            string path = modelPaths[models.IndexOf(model)];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            Manifest man = LoadManifest(model);
            if (man == null) Debug.LogWarning("[Modelos] manifesto ausente para " + model);

            Vector3 targetCenter; Vector2 footprint = Vector2.zero;
            string slotId = assignment[model];
            if (slotId == "__GRID__")
            {
                targetCenter = new Vector3(-150f + gridIdx * 55f, baseY, -140f); gridIdx++;
            }
            else
            {
                SlotZone z = zones.First(s => s.id == slotId);
                targetCenter = z.center; footprint = z.size;
            }

            GameObject inst = PrefabUtility.InstantiatePrefab(prefab, group.transform) as GameObject;
            inst.name = model;
            if (inst == null) { Debug.LogWarning("[Modelos] falha ao instanciar " + model); continue; }

            Renderer[] rs = inst.GetComponentsInChildren<Renderer>();
            if (rs == null || rs.Length == 0) { Debug.LogWarning("[Modelos] sem renderers: " + model); continue; }
            Bounds mb = rs[0].bounds;
            for (int k = 1; k < rs.Length; k++) mb.Encapsulate(rs[k].bounds);

            float factor;
            if (footprint.x > 1f && mb.size.x > 0.01f && mb.size.z > 0.01f)
            {
                factor = Mathf.Clamp(Mathf.Min(footprint.x / mb.size.x, footprint.y / mb.size.z), 0.001f, 1000f);
            }
            else factor = 1f;
            inst.transform.localScale = inst.transform.localScale * factor;

            matFixed += FixRendererMaterials(inst, man, ref texMissing);

            rs = inst.GetComponentsInChildren<Renderer>();
            mb = rs[0].bounds;
            for (int k = 1; k < rs.Length; k++) mb.Encapsulate(rs[k].bounds);
            Vector3 pos = inst.transform.position;
            pos.x += targetCenter.x - mb.center.x;
            pos.z += targetCenter.z - mb.center.z;
            pos.y += baseY - mb.min.y;
            inst.transform.position = pos;

            Debug.Log($"[Modelos] {model} -> zona \"{slotId}\" center={targetCenter} footprint={footprint} escala={factor:0.####} tamanhoFinal={(mb.max - mb.min)}");
            Undo.RegisterCreatedObjectUndo(inst, "Modelo IFNMG");
            placed++;
        }

        EditorSceneManager.MarkSceneDirty(campus);
        EditorSceneManager.SaveScene(campus, CampusPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Modelos] FIM. instanciados={placed} materiaisFixados={matFixed} texturasAusentes={texMissing} zonas={zones.Count}");
    }

    private static Manifest LoadManifest(string safe)
    {
        string p = $"{ManDir}/{safe}.json";
        if (!File.Exists(p)) return null;
        try { return JsonUtility.FromJson<Manifest>(File.ReadAllText(p, Encoding.UTF8)); }
        catch (Exception e) { Debug.LogWarning("[Modelos] manifesto invalido " + p + ": " + e.Message); return null; }
    }

    private static int FixRendererMaterials(GameObject inst, Manifest man, ref int texMissing)
    {
        int fixedCount = 0;
        foreach (Renderer r in inst.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;
            Material[] fixedMats = new Material[mats.Length];
            ObjEntry objEntry = FindObject(man, r.gameObject.name);

            for (int i = 0; i < mats.Length; i++)
            {
                Material src = mats[i];
                if (src == null) { fixedMats[i] = null; continue; }

                Material m = new Material(src);
                string shaderName = src.shader != null ? src.shader.name : "(null)";
                if (src.shader == null || shaderName.Contains("Error") || shaderName.StartsWith("Hidden"))
                {
                    m.shader = Shader.Find("Standard");
                }
                else if (m.shader != null && m.shader.name == "Standard" && m.HasProperty("_Mode"))
                {
                    m.SetFloat("_Mode", 0f);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.DisableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                }
                if (m.HasProperty("_Cull")) m.SetFloat("_Cull", 0f);

                string texFile = null;
                if (objEntry != null && objEntry.slots != null && i < objEntry.slots.Length)
                    texFile = objEntry.slots[i].tex;
                if (string.IsNullOrEmpty(texFile) && man != null && man.matmap != null)
                {
                    foreach (MatTex mt in man.matmap)
                        if (mt.mat == src.name && !string.IsNullOrEmpty(mt.tex)) { texFile = mt.tex; break; }
                }
                if (!string.IsNullOrEmpty(texFile) && m.HasProperty("_MainTex"))
                {
                    Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexDir}/{man.model}/{texFile}");
                    if (t != null) m.SetTexture("_MainTex", t);
                    else { texMissing++; Debug.LogWarning($"[Modelos] textura nao importada: {texFile} (em {inst.name}/{r.gameObject.name})"); }
                }

                fixedMats[i] = m;
                fixedCount++;
            }
            r.materials = fixedMats;
        }
        return fixedCount;
    }

    private static ObjEntry FindObject(Manifest man, string goName)
    {
        if (man == null || man.objects == null) return null;
        foreach (ObjEntry o in man.objects) if (o.obj == goName) return o;
        string gn = Norm(goName);
        foreach (ObjEntry o in man.objects) if (Norm(o.obj) == gn) return o;
        return null;
    }

    private static string CoreOf(string zoneId)
    {
        string s = zoneId.ToLower();
        s = s.Replace("edificio_", "").Replace("edificios_", "").Replace("predio_", "").Replace("predio_de_", "");
        return Norm(s);
    }

    private static string Norm(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        string d = s.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();
        foreach (char c in d)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != UnicodeCategory.NonSpacingMark)
            {
                char lc = char.ToLower(c, CultureInfo.InvariantCulture);
                if (char.IsLetterOrDigit(lc)) sb.Append(lc);
            }
        }
        return sb.ToString();
    }

    private class SlotZone
    {
        public string id;
        public string core;
        public Vector3 center;
        public Vector2 size;
        public bool used;
    }
}

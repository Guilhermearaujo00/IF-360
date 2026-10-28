using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Setup dos modelos IFNMG na Campus (v5):
///  - Posicoes das zonas "building" do JSON calibrado.
///  - Materiais: copia -> albedo via Manifesto (forward-tracing do Blender) -> shader IFNMG/Standard Two-Sided (Cull Off).
///  - Match de nome tolerante a sufixos .001/.002 do Unity.
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

        Shader twoSided = Shader.Find("IFNMG/Standard Two-Sided");
        if (twoSided == null)
            Debug.LogError("[Modelos] shader IFNMG/Standard Two-Sided NAO encontrado - conferir Assets/IFNMG360/Shaders/");

        MapFile map = null;
        try { map = JsonUtility.FromJson<MapFile>(File.ReadAllText(JsonPath, Encoding.UTF8)); }
        catch (Exception e) { Debug.LogWarning("[Modelos] JSON ilegivel: " + e.Message); }

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
                    float x = (z.pointsUvFlat[i] - 0.5f) * W;
                    float zc = (0.5f - z.pointsUvFlat[i + 1]) * D;
                    if (x < minX) minX = x; if (x > maxX) maxX = x;
                    if (zc < minZ) minZ = zc; if (zc > maxZ) maxZ = zc;
                }
                Vector2 size = new Vector2(maxX - minX, maxZ - minZ);
                if (size.x < 1f || size.y < 1f) continue;
                zones.Add(new SlotZone { id = z.id, core = CoreOf(z.id), center = new Vector3((minX + maxX) * 0.5f, baseY, (minZ + maxZ) * 0.5f), size = size, used = false });
            }
        }

        Scene campus = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);

        // limpeza recursiva de grupos antigos (raiz OU dentro do mapa)
        foreach (GameObject go in campus.GetRootGameObjects())
        {
            List<Transform> found = new List<Transform>();
            if (go.name == GroupName) found.Add(go.transform);
            foreach (Transform c in go.transform) if (c.name == GroupName) found.Add(c);
            foreach (Transform t in found) if (t != null) UnityEngine.Object.DestroyImmediate(t.gameObject);
        }

        GameObject mapa = null;
        foreach (GameObject go in campus.GetRootGameObjects())
            if (go.name == MapaName) { mapa = go; break; }
        if (mapa == null) { Debug.LogError("[Modelos] mapa prototipo nao encontrado"); return; }

        foreach (Transform t in mapa.transform)
            if (t.name == "Estruturas") { t.gameObject.SetActive(false); break; }

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

        int gridIdx = 0, placed = 0, texApplied = 0, swapped = 0;
        foreach (string model in models)
        {
            string path = modelPaths[models.IndexOf(model)];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            Manifest man = LoadManifest(model);
            if (man == null) Debug.LogWarning("[Modelos] manifesto ausente: " + model);

            Vector3 targetCenter; Vector2 footprint = Vector2.zero;
            string slotId = assignment[model];
            if (slotId == "__GRID__") { targetCenter = new Vector3(-150f + gridIdx * 55f, baseY, -140f); gridIdx++; }
            else { SlotZone z = zones.First(s => s.id == slotId); targetCenter = z.center; footprint = z.size; }

            GameObject inst = PrefabUtility.InstantiatePrefab(prefab, group.transform) as GameObject;
            if (inst == null) continue;
            inst.name = model;

            Renderer[] rs = inst.GetComponentsInChildren<Renderer>();
            if (rs == null || rs.Length == 0) { Debug.LogWarning("[Modelos] sem renderers: " + model); continue; }
            Bounds mb = rs[0].bounds;
            for (int k = 1; k < rs.Length; k++) mb.Encapsulate(rs[k].bounds);

            float factor = (footprint.x > 1f && mb.size.x > 0.01f && mb.size.z > 0.01f)
                ? Mathf.Clamp(Mathf.Min(footprint.x / mb.size.x, footprint.y / mb.size.z), 0.001f, 1000f) : 1f;
            inst.transform.localScale = inst.transform.localScale * factor;

            int modelTex = 0, modelSwap = 0;
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

                    string texFile = null;
                    if (objEntry != null && objEntry.slots != null && i < objEntry.slots.Length)
                        texFile = objEntry.slots[i].tex;
                    if (string.IsNullOrEmpty(texFile) && man != null && man.matmap != null)
                    {
                        foreach (MatTex mt in man.matmap)
                            if (NormKey(mt.mat) == NormKey(src.name) && !string.IsNullOrEmpty(mt.tex)) { texFile = mt.tex; break; }
                    }
                    if (!string.IsNullOrEmpty(texFile) && m.HasProperty("_MainTex"))
                    {
                        Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexDir}/{man.model}/{texFile}");
                        if (t != null) { m.SetTexture("_MainTex", t); modelTex++; }
                        else Debug.LogWarning($"[Modelos] textura ausente no import: {texFile} ({model}/{r.gameObject.name})");
                    }

                    if (twoSided != null)
                    {
                        Texture texKeep = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null;
                        Color colKeep = m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;
                        m.shader = twoSided;
                        if (texKeep != null) m.SetTexture("_MainTex", texKeep);
                        if (m.HasProperty("_Color")) m.SetColor("_Color", colKeep);
                        modelSwap++;
                    }
                    fixedMats[i] = m;
                }
                r.materials = fixedMats;
            }

            rs = inst.GetComponentsInChildren<Renderer>();
            mb = rs[0].bounds;
            for (int k = 1; k < rs.Length; k++) mb.Encapsulate(rs[k].bounds);
            Vector3 pos = inst.transform.position;
            pos.x += targetCenter.x - mb.center.x;
            pos.z += targetCenter.z - mb.center.z;
            pos.y += baseY - mb.min.y;
            inst.transform.position = pos;

            texApplied += modelTex; swapped += modelSwap; placed++;
            Debug.Log($"[Modelos] {model} -> zona \"{slotId}\" escala={factor:0.####} materiais={modelSwap} comAlbedo={modelTex}");
        }

        EditorSceneManager.MarkSceneDirty(campus);
        EditorSceneManager.SaveScene(campus, CampusPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Modelos] FIM v5. modelos={placed} albedosAplicados={texApplied} shadersTwoSided={swapped}");
    }

    private static Manifest LoadManifest(string model)
    {
        foreach (string candidate in new[] { model, Sanitize(model) })
        {
            string p = $"{ManDir}/{candidate}.json";
            if (File.Exists(p))
            {
                try { return JsonUtility.FromJson<Manifest>(File.ReadAllText(p, Encoding.UTF8)); }
                catch (Exception e) { Debug.LogWarning("[Modelos] manifesto invalido " + p + ": " + e.Message); }
            }
        }
        return null;
    }

    private static ObjEntry FindObject(Manifest man, string goName)
    {
        if (man == null || man.objects == null) return null;
        foreach (ObjEntry o in man.objects) if (o.obj == goName) return o;
        string gn = NormKey(goName);
        foreach (ObjEntry o in man.objects) if (NormKey(o.obj) == gn) return o;
        return null;
    }

    private static string Sanitize(string s)
    {
        return Regex.Replace(s, "[<>:\"/\\\\|?*\\x00-\\x1f]", "_");
    }

    private static string CoreOf(string zoneId)
    {
        string s = zoneId.ToLower();
        s = s.Replace("edificio_", "").Replace("edificios_", "").Replace("predio_", "").Replace("predio_de_", "");
        return Norm(s);
    }

    private static string NormKey(string s)
    {
        string n = Norm(s);
        return Regex.Replace(n, @"\d{3}$", ""); // remove sufixo de duplicado do Unity (.001/.002 -> 001/002 normalizado)
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

    private class SlotZone { public string id; public string core; public Vector3 center; public Vector2 size; public bool used; }
}

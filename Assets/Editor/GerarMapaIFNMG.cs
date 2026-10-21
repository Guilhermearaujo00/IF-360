using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gera o mapa do IFNMG Campus Januária na cena Campus, a partir do JSON
/// Assets/Dados/MapaIFNMG/mapa_ifnmg.json (seções 1,3–6 do mapa de referência).
///
/// Menu: Tools/IFNMG/Gerar Mapa IFNMG (Januária)
/// Batch: -executeMethod GerarMapaIFNMG.GerarBatch
///
/// Hierarquia criada: Mapa_IFNMG/{Terrenos,Vias,Estruturas,Vegetacao}.
/// Cada objeto recebe o id da tabela no nome. Rodar de novo apaga e recria
/// apenas a raiz "Mapa_IFNMG", sem tocar no resto da cena.
/// </summary>
public static class GerarMapaIFNMG
{
    private const string MENU = "Tools/IFNMG/Gerar Mapa IFNMG (Januária)";
    private const string JSON_PATH = "Assets/Dados/MapaIFNMG/mapa_ifnmg.json";
    private const string RAIZ = "Mapa_IFNMG";
    private const string CENA = "Assets/Scenes/Campus.unity";
    private const string MAT_DIR = "Assets/Dados/MapaIFNMG/Materiais";
    private const string RELATORIO = "Docs/MAPA_IFNMG_RELATORIO.md";

    // ------------------------------------------------------------- dados
    [Serializable] public class Vec2 { public float x; public float y; }
    [Serializable] public class PaletteEntry { public string key; public string path; }
    [Serializable] public class Road { public string id; public string type; public string prefab; public float widthPx; public Vec2[] vertices; }
    [Serializable] public class TerrainArea { public string id; public string type; public string prefab; public Vec2[] polygon; }
    [Serializable] public class PlaceholderSpec { public float w; public float h; public float d; public string color; public float y; }
    [Serializable] public class Part
    {
        public string prefab;
        public string layout;
        public Vec2 dir;
        public Vec2[] dirs;
        public int count;
        public float startMeters;
        public float stepMeters;
        public float ringSide;
        public int ringCountPerSide;
        public int gridRows;
        public int gridCols;
        public float gridSpacingMeters;
        public Vec2 offsetPx;
        public float yawOffset;
        public float[] scale;
        public float y;
        public PlaceholderSpec placeholder;
    }
    [Serializable] public class Structure
    {
        public string id;
        public string type;
        public Vec2 center;
        public float rot;
        public string tint;
        public Vec2 bboxMin;
        public Vec2 bboxMax;
        public Part[] parts;
    }
    [Serializable] public class VegGroup
    {
        public string id;
        public Vec2 center;
        public float radiusPx;
        public int n;
        public bool hasBbox;
        public bool ignorarEstruturas;
        public Vec2 bboxMin;
        public Vec2 bboxMax;
    }
    [Serializable] public class MapaData
    {
        public float metersPerPixel;
        public Vec2 originPx;
        public bool aplicarTint;
        public float mataDensidadeM2;
        public int mataMaxArvores;
        public string[] treeKeys;
        public string[] bushKeys;
        public PaletteEntry[] palette;
        public Road[] roads;
        public TerrainArea[] terrains;
        public Structure[] structures;
        public VegGroup[] vegetation;
    }

    // ------------------------------------------------------------- estado
    private static MapaData D;
    private static Dictionary<string, string> Pal;
    private static readonly Dictionary<string, Vector3> SizeCache = new Dictionary<string, Vector3>();
    private static Transform fTerrenos, fVias, fEstruturas, fVegetacao;
    private static readonly List<Rect> Exclusao = new List<Rect>();
    private static readonly List<(Vector2 a, Vector2 b, float halfW)> ViasPx = new List<(Vector2, Vector2, float)>();
    private static readonly List<(string key, Vector2 pos)> Ocupadas = new List<(string, Vector2)>();
    private static readonly List<string> SemAsset = new List<string>();
    private static readonly List<string> Placeholders = new List<string>();
    private static readonly List<string> PrefabFaltando = new List<string>();

    // ------------------------------------------------------------- entrada
    [MenuItem(MENU)]
    public static void GerarMenu()
    {
        Scene cena = SceneManager.GetActiveScene();
        if (cena.name != "Campus")
        {
            if (!EditorUtility.DisplayDialog("Gerar Mapa IFNMG",
                "A cena ativa não é a Campus. Gerar na cena ativa mesmo assim?", "Gerar", "Cancelar"))
                return;
        }
        Gerar(cena, false);
    }

    /// <summary>Entrada para batchmode: abre a cena Campus, gera e salva.</summary>
    public static void GerarBatch()
    {
        Scene cena = EditorSceneManager.OpenScene(CENA, OpenSceneMode.Single);
        Gerar(cena, true);
    }

    private static void Gerar(Scene cena, bool batch)
    {
        var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(JSON_PATH);
        if (ta == null) { Debug.LogError($"[MapaIFNMG] JSON não encontrado: {JSON_PATH}"); return; }
        D = JsonUtility.FromJson<MapaData>(ta.text);
        Pal = new Dictionary<string, string>();
        foreach (var p in D.palette) Pal[p.key] = p.path;
        SizeCache.Clear();
        SemAsset.Clear(); Placeholders.Clear(); PrefabFaltando.Clear();
        Exclusao.Clear(); ViasPx.Clear(); Ocupadas.Clear();

        // raiz
        GameObject velha = GameObject.Find(RAIZ);
        if (velha != null) UnityEngine.Object.DestroyImmediate(velha);

        // exclusão: bboxes das estruturas (3 px de margem) e vias
        foreach (var s in D.structures)
        {
            Exclusao.Add(Rect.MinMaxRect(
                s.bboxMin.x - 3, s.bboxMin.y - 3,
                s.bboxMax.x + 3, s.bboxMax.y + 3));
        }
        foreach (var r in D.roads)
        {
            float half = r.widthPx * 0.5f;
            for (int i = 0; i < r.vertices.Length - 1; i++)
                ViasPx.Add((new Vector2(r.vertices[i].x, r.vertices[i].y),
                            new Vector2(r.vertices[i + 1].x, r.vertices[i + 1].y), half));
        }

        var raiz = new GameObject(RAIZ);
        fTerrenos = NovaPasta(raiz, "Terrenos");
        fVias = NovaPasta(raiz, "Vias");
        fEstruturas = NovaPasta(raiz, "Estruturas");
        fVegetacao = NovaPasta(raiz, "Vegetacao");

        // ordem: terrenos → vias → estruturas → vegetação
        GerarTerrenos();
        GerarVias();
        GerarEstruturas();
        GerarVegetacao();

        EditorSceneManager.MarkSceneDirty(cena);
        if (batch) EditorSceneManager.SaveScene(cena);

        EscreverRelatorio();
        Debug.Log($"[MapaIFNMG] Concluído. Objetos: {raiz.transform.childCount} grupos. " +
                  $"Prefabs faltando: {PrefabFaltando.Count}. Placeholders: {Placeholders.Count}. Veja {RELATORIO}");
    }

    private static Transform NovaPasta(GameObject raiz, string nome)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(raiz.transform, false);
        return go.transform;
    }

    // ------------------------------------------------------------ conversão
    private static Vector3 PxToWorld(float px, float py, float y = 0f)
    {
        return new Vector3((px - D.originPx.x) * D.metersPerPixel, y, -(py - D.originPx.y) * D.metersPerPixel);
    }

    private static Vector3 OffsetPxToWorld(Vec2 off, float y = 0f)
    {
        if (off == null) return new Vector3(0, y, 0);
        return new Vector3(off.x * D.metersPerPixel, y, -off.y * D.metersPerPixel);
    }

    private static Quaternion Yaw(float graus) => Quaternion.Euler(0f, graus, 0f);

    private static Vector3 Escala(Part p) =>
        (p.scale != null && p.scale.Length >= 3) ? new Vector3(p.scale[0], p.scale[1], p.scale[2]) : Vector3.one;

    // ------------------------------------------------------------ prefabs
    private static GameObject CarregarPrefab(string key)
    {
        if (string.IsNullOrEmpty(key) || !Pal.TryGetValue(key, out string path)) return null;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null && !PrefabFaltando.Contains(path)) PrefabFaltando.Add(path);
        return prefab;
    }

    private static Vector3 TamanhoPrefab(string key)
    {
        if (string.IsNullOrEmpty(key)) return Vector3.zero;
        if (SizeCache.TryGetValue(key, out Vector3 s)) return s;
        GameObject prefab = CarregarPrefab(key);
        if (prefab == null) { SizeCache[key] = Vector3.zero; return Vector3.zero; }
        GameObject inst = null;
        try
        {
            inst = UnityEngine.Object.Instantiate(prefab);
            bool tem = false; Bounds b = new Bounds();
            foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
                if (!tem) { b = r.bounds; tem = true; } else b.Encapsulate(r.bounds);
            }
            s = tem ? b.size : Vector3.zero;
        }
        finally { if (inst != null) UnityEngine.Object.DestroyImmediate(inst); }
        SizeCache[key] = s;
        return s;
    }

    private static GameObject Instanciar(string key, Vector3 pos, float yaw, Vector3 escala,
                                         Transform pai, string nome, string tint)
    {
        GameObject prefab = CarregarPrefab(key);
        if (prefab == null)
        {
            if (!SemAsset.Contains(key)) SemAsset.Add(key);
            return null;
        }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pai);
        go.transform.position = pos;
        go.transform.rotation = Yaw(yaw);
        go.transform.localScale = escala;
        go.name = nome;
        if (D.aplicarTint && !string.IsNullOrEmpty(tint)) AplicarTint(go, tint);
        return go;
    }

    private static void AplicarTint(GameObject go, string hex)
    {
        Material mat = MaterialCor(hex);
        if (mat == null) return;
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }

    private static readonly Dictionary<string, Material> MatCache = new Dictionary<string, Material>();
    private static Material MaterialCor(string hex)
    {
        string safe = (hex ?? "").Replace("#", "");
        if (MatCache.TryGetValue(safe, out Material m)) return m;
        if (!AssetDatabase.IsValidFolder(MAT_DIR))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Dados/MapaIFNMG"))
                AssetDatabase.CreateFolder("Assets/Dados", "MapaIFNMG");
            AssetDatabase.CreateFolder("Assets/Dados/MapaIFNMG", "Materiais");
        }
        string path = MAT_DIR + "/Cor_" + safe + ".mat";
        m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Standard"));
            if (ColorUtility.TryParseHtmlString("#" + safe, out Color c)) m.color = c;
            AssetDatabase.CreateAsset(m, path);
        }
        MatCache[safe] = m;
        return m;
    }

    // ------------------------------------------------------------ terrenos
    private static void GerarTerrenos()
    {
        int ordem = 0;
        int contador = 0;
        foreach (var t in D.terrains)
        {
            ordem++;
            Vector2 min, max;
            Bbox(t.polygon, out min, out max);

            if (t.type == "mata_densa")
            {
                ScatterMata(t, min, max);
                continue;
            }

            if (t.type == "lavoura" || string.IsNullOrEmpty(t.prefab))
            {
                float w = (max.x - min.x) * D.metersPerPixel;
                float d = (max.y - min.y) * D.metersPerPixel;
                Vector3 c = PxToWorld((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, 0.01f);
                var box = BoxPlaceholder(c, 0f, new Vector3(w, 0.05f, d), "#2F4A2A", t.id + "_lavoura", fTerrenos);
                if (box != null) { if (!Placeholders.Contains(t.id)) Placeholders.Add(t.id); }
                continue;
            }

            Vector3 tam = TamanhoPrefab(t.prefab);
            float tile = Mathf.Max(tam.x, tam.z);
            if (tile <= 0.01f) tile = 20f;
            float stepPx = tile / D.metersPerPixel;

            float yTerreno = 0.002f * ordem;
            int i = 0;
            for (float px = min.x + stepPx * 0.5f; px <= max.x; px += stepPx)
            {
                for (float py = min.y + stepPx * 0.5f; py <= max.y; py += stepPx)
                {
                    if (!DentroPoligono(t.polygon, px, py)) continue;
                    Vector3 pos = PxToWorld(px, py, yTerreno);
                    Instanciar(t.prefab, pos, 0f, Vector3.one, fTerrenos, $"{t.id}_t{i}", null);
                    i++; contador++;
                }
            }
        }
        Debug.Log($"[MapaIFNMG] Terrenos: {contador} tiles gerados.");
    }

    private static void ScatterMata(TerrainArea t, Vector2 min, Vector2 max)
    {
        float areaPx = AreaPoligono(t.polygon);
        float areaM2 = areaPx * D.metersPerPixel * D.metersPerPixel;
        int n = Mathf.Clamp(Mathf.RoundToInt(areaM2 / Mathf.Max(1f, D.mataDensidadeM2)), 0, D.mataMaxArvores);
        if (n <= 0) return;

        var rnd = new System.Random(Estavel(t.id));
        float minDistPx = 2.5f / D.metersPerPixel;
        int colocadas = 0;
        int tentativas = 0;
        int maxTent = n * 40 + 200;

        while (colocadas < n && tentativas < maxTent)
        {
            tentativas++;
            float px = Mathf.Lerp(min.x, max.x, (float)rnd.NextDouble());
            float py = Mathf.Lerp(min.y, max.y, (float)rnd.NextDouble());
            if (!DentroPoligono(t.polygon, px, py)) continue;
            if (Bloqueado(px, py)) continue;
            if (MuitoPerto(px, py, minDistPx, null)) continue;

            string key = EscolhaArvore(rnd);
            float escala = Mathf.Lerp(0.9f, 1.2f, (float)rnd.NextDouble());
            float yaw = (float)rnd.NextDouble() * 360f;
            Vector3 pos = PxToWorld(px, py, 0f);
            var go = Instanciar(key, pos, yaw, Vector3.one * escala, fVegetacao, $"{t.id}_arv{colocadas}", null);
            if (go != null) { Ocupadas.Add((t.id, new Vector2(px, py))); colocadas++; }
        }
        Debug.Log($"[MapaIFNMG] Mata {t.id}: {colocadas}/{n} árvores.");
    }

    // --------------------------------------------------------------- vias
    private static void GerarVias()
    {
        int total = 0;
        foreach (var r in D.roads)
        {
            Vector3 tam = TamanhoPrefab(r.prefab);
            if (tam == Vector3.zero) { SemAsset.Add(r.id); continue; }
            float larguraAlvo = r.widthPx * D.metersPerPixel;
            float baseTile = Mathf.Max(tam.x, tam.z);
            float escala = larguraAlvo / Mathf.Max(0.01f, baseTile);
            float passo = baseTile * escala * 0.95f;
            int i = 0;

            for (int s = 0; s < r.vertices.Length - 1; s++)
            {
                Vector3 a = PxToWorld(r.vertices[s].x, r.vertices[s].y, 0.05f);
                Vector3 b = PxToWorld(r.vertices[s + 1].x, r.vertices[s + 1].y, 0.05f);
                Vector3 dir = b - a;
                float len = dir.magnitude;
                if (len < 0.001f) continue;
                dir /= len;
                float yaw = Mathf.Atan2(r.vertices[s + 1].y - r.vertices[s].y,
                                        r.vertices[s + 1].x - r.vertices[s].x) * Mathf.Rad2Deg;

                for (float d = passo * 0.5f; d <= len; d += passo)
                {
                    Vector3 pos = a + dir * d;
                    Instanciar(r.prefab, pos, yaw, new Vector3(escala, 1f, escala), fVias, $"{r.id}_{i}", null);
                    i++; total++;
                }
            }
        }
        Debug.Log($"[MapaIFNMG] Vias: {total} tiles gerados.");
    }

    // ---------------------------------------------------------- estruturas
    private static void GerarEstruturas()
    {
        int total = 0;
        foreach (var s in D.structures)
        {
            Vector3 centro = PxToWorld(s.center.x, s.center.y, 0f);
            Quaternion rotQ = Yaw(s.rot);
            int parte = 0;
            foreach (var p in s.parts)
            {
                string sufixo = $"_{parte++}";
                switch (p.layout)
                {
                    case "single":
                    {
                        Vector3 pos = centro + OffsetPxToWorld(p.offsetPx, p.y);
                        if (Instanciar(p.prefab, pos, s.rot + p.yawOffset, Escala(p), fEstruturas, s.id + sufixo, s.tint) != null) total++;
                        break;
                    }
                    case "row":
                    {
                        Vector2 dir = Normaliza(p.dir);
                        Vector3 dirW = new Vector3(dir.x, 0f, -dir.y);
                        float yaw = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + p.yawOffset;
                        Vector3 inicio = centro + OffsetPxToWorld(p.offsetPx, p.y);
                        for (int i = 0; i < p.count; i++)
                        {
                            Vector3 pos = inicio + dirW * (p.startMeters + i * p.stepMeters);
                            if (Instanciar(p.prefab, pos, yaw, Escala(p), fEstruturas, $"{s.id}{sufixo}_{i}", s.tint) != null) total++;
                        }
                        break;
                    }
                    case "arms":
                    {
                        foreach (var dv in p.dirs)
                        {
                            Vector2 dir = Normaliza(dv);
                            Vector3 dirW = new Vector3(dir.x, 0f, -dir.y);
                            float yaw = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + p.yawOffset;
                            for (int i = 0; i < p.count; i++)
                            {
                                Vector3 pos = centro + dirW * (p.startMeters + i * p.stepMeters) + OffsetPxToWorld(p.offsetPx, p.y);
                                if (Instanciar(p.prefab, pos, yaw, Escala(p), fEstruturas, $"{s.id}{sufixo}_{i}", s.tint) != null) total++;
                            }
                        }
                        break;
                    }
                    case "ring":
                    {
                        Vector3 tam = TamanhoPrefab(p.prefab);
                        float side = p.ringSide;
                        int c = Mathf.Max(1, p.ringCountPerSide);
                        float step = side / c;
                        float sx = tam.x > 0.01f ? step / tam.x : 1f;
                        Vector3 sc = new Vector3(sx, Escala(p).y, Escala(p).z);
                        for (int lado = 0; lado < 4; lado++)
                        {
                            for (int i = 0; i < c; i++)
                            {
                                float along = -side * 0.5f + step * 0.5f + i * step;
                                Vector3 local; float yawLocal;
                                if (lado == 0) { local = new Vector3(along, 0, -side * 0.5f); yawLocal = 0f; }
                                else if (lado == 1) { local = new Vector3(along, 0, side * 0.5f); yawLocal = 0f; }
                                else if (lado == 2) { local = new Vector3(-side * 0.5f, 0, along); yawLocal = 90f; }
                                else { local = new Vector3(side * 0.5f, 0, along); yawLocal = 90f; }
                                Vector3 pos = centro + rotQ * local + new Vector3(0, p.y, 0);
                                if (Instanciar(p.prefab, pos, s.rot + yawLocal, sc, fEstruturas, $"{s.id}{sufixo}_{lado}_{i}", s.tint) != null) total++;
                            }
                        }
                        break;
                    }
                    case "grid":
                    {
                        int rows = Mathf.Max(1, p.gridRows), cols = Mathf.Max(1, p.gridCols);
                        for (int rr = 0; rr < rows; rr++)
                            for (int cc = 0; cc < cols; cc++)
                            {
                                float lx = (cc - (cols - 1) * 0.5f) * p.gridSpacingMeters;
                                float lz = (rr - (rows - 1) * 0.5f) * p.gridSpacingMeters;
                                Vector3 pos = centro + rotQ * new Vector3(lx, 0, lz) + new Vector3(0, p.y, 0);
                                if (Instanciar(p.prefab, pos, s.rot, Escala(p), fEstruturas, $"{s.id}{sufixo}_{rr}{cc}", s.tint) != null) total++;
                            }
                        break;
                    }
                    case "placeholder":
                    {
                        var ph = p.placeholder;
                        if (ph == null) break;
                        Vector3 pos = centro + rotQ * OffsetPxToWorld(p.offsetPx, 0f);
                        var go = BoxPlaceholder(pos, s.rot + p.yawOffset, new Vector3(ph.w, ph.h, ph.d), ph.color,
                                                $"{s.id}{sufixo}", fEstruturas, ph.y);
                        if (go != null) { total++; if (!Placeholders.Contains(s.id)) Placeholders.Add(s.id); }
                        break;
                    }
                    default:
                        Debug.LogWarning($"[MapaIFNMG] Layout desconhecido '{p.layout}' em {s.id}");
                        break;
                }
            }
        }
        Debug.Log($"[MapaIFNMG] Estruturas: {total} peças geradas.");
    }

    private static GameObject BoxPlaceholder(Vector3 basePos, float yaw, Vector3 tamanho, string cor,
                                             string nome, Transform pai, float yBase = 0f)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(pai, true);
        go.transform.position = new Vector3(basePos.x, yBase + tamanho.y * 0.5f, basePos.z);
        go.transform.rotation = Yaw(yaw);
        go.transform.localScale = tamanho;
        go.name = nome;
        Material m = MaterialCor(cor);
        if (m != null) go.GetComponent<Renderer>().sharedMaterial = m;
        return go;
    }

    // ---------------------------------------------------------- vegetação
    private static void GerarVegetacao()
    {
        int total = 0;
        foreach (var g in D.vegetation)
        {
            var rnd = new System.Random(Estavel(g.id));
            Vector2 min, max;
            if (g.hasBbox) { min = new Vector2(g.bboxMin.x, g.bboxMin.y); max = new Vector2(g.bboxMax.x, g.bboxMax.y); }
            else
            {
                min = new Vector2(g.center.x - g.radiusPx, g.center.y - g.radiusPx);
                max = new Vector2(g.center.x + g.radiusPx, g.center.y + g.radiusPx);
            }

            float areaPx = g.hasBbox ? (max.x - min.x) * (max.y - min.y) : Mathf.PI * g.radiusPx * g.radiusPx;
            float minDistPx = Mathf.Sqrt(areaPx / Mathf.Max(1, g.n)) * 0.75f;
            int colocadas = 0, tentativas = 0, maxTent = g.n * 80 + 100;

            while (colocadas < g.n && tentativas < maxTent)
            {
                tentativas++;
                float px, py;
                if (g.hasBbox)
                {
                    px = Mathf.Lerp(min.x, max.x, (float)rnd.NextDouble());
                    py = Mathf.Lerp(min.y, max.y, (float)rnd.NextDouble());
                }
                else
                {
                    float ang = (float)rnd.NextDouble() * Mathf.PI * 2f;
                    float rad = Mathf.Sqrt((float)rnd.NextDouble()) * g.radiusPx;
                    px = g.center.x + Mathf.Cos(ang) * rad;
                    py = g.center.y + Mathf.Sin(ang) * rad;
                }
                if (Bloqueado(px, py, g.ignorarEstruturas)) continue;
                if (MuitoPerto(px, py, minDistPx, g.id)) continue;

                string key = EscolhaArvore(rnd);
                float escala = Mathf.Lerp(0.85f, 1.15f, (float)rnd.NextDouble());
                float yaw = (float)rnd.NextDouble() * 360f;
                Vector3 pos = PxToWorld(px, py, 0f);
                var go = Instanciar(key, pos, yaw, Vector3.one * escala, fVegetacao, $"{g.id}_{colocadas}", null);
                if (go != null) { Ocupadas.Add((g.id, new Vector2(px, py))); colocadas++; total++; }
            }
            Debug.Log($"[MapaIFNMG] Vegetação {g.id}: {colocadas}/{g.n}.");
        }
        Debug.Log($"[MapaIFNMG] Vegetação: {total} árvores geradas.");
    }

    private static string EscolhaArvore(System.Random rnd)
    {
        if (D.treeKeys == null || D.treeKeys.Length == 0) return "tree_a";
        // última chave (maior árvore) entra com peso menor
        float soma = 0f;
        for (int i = 0; i < D.treeKeys.Length; i++) soma += (i == D.treeKeys.Length - 1) ? 0.15f : 1f;
        float alvo = (float)rnd.NextDouble() * soma;
        for (int i = 0; i < D.treeKeys.Length; i++)
        {
            alvo -= (i == D.treeKeys.Length - 1) ? 0.15f : 1f;
            if (alvo <= 0f) return D.treeKeys[i];
        }
        return D.treeKeys[0];
    }

    // ------------------------------------------------------------ utilidades
    private static bool Bloqueado(float px, float py, bool ignorarEstruturas = false)
    {
        Vector2 p = new Vector2(px, py);
        if (!ignorarEstruturas)
            foreach (var r in Exclusao) if (r.Contains(p)) return true;
        foreach (var v in ViasPx)
        {
            if (DistSegmento(p, v.a, v.b) < v.halfW + 2f) return true;
        }
        return false;
    }

    private static bool MuitoPerto(float px, float py, float minDistPx, string grupo)
    {
        Vector2 p = new Vector2(px, py);
        float d2 = minDistPx * minDistPx;
        foreach (var o in Ocupadas)
        {
            if (grupo != null && o.key != grupo) continue;
            if ((o.pos - p).sqrMagnitude < d2) return true;
        }
        return false;
    }

    private static float DistSegmento(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude);
        t = Mathf.Clamp01(t);
        return Vector2.Distance(p, a + ab * t);
    }

    private static Vector2 Normaliza(Vec2 v)
    {
        if (v == null) return new Vector2(1, 0);
        Vector2 r = new Vector2(v.x, v.y);
        return r.sqrMagnitude < 0.0001f ? new Vector2(1, 0) : r.normalized;
    }

    private static void Bbox(Vec2[] poly, out Vector2 min, out Vector2 max)
    {
        min = new Vector2(float.MaxValue, float.MaxValue);
        max = new Vector2(float.MinValue, float.MinValue);
        foreach (var v in poly)
        {
            min = Vector2.Min(min, new Vector2(v.x, v.y));
            max = Vector2.Max(max, new Vector2(v.x, v.y));
        }
    }

    private static float AreaPoligono(Vec2[] poly)
    {
        float a = 0f;
        for (int i = 0; i < poly.Length; i++)
        {
            var p = poly[i]; var q = poly[(i + 1) % poly.Length];
            a += p.x * q.y - q.x * p.y;
        }
        return Mathf.Abs(a) * 0.5f;
    }

    private static bool DentroPoligono(Vec2[] poly, float x, float y)
    {
        bool dentro = false;
        int j = poly.Length - 1;
        for (int i = 0; i < poly.Length; i++)
        {
            float xi = poly[i].x, yi = poly[i].y, xj = poly[j].x, yj = poly[j].y;
            if (((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi))
                dentro = !dentro;
            j = i;
        }
        return dentro;
    }

    private static int Estavel(string s)
    {
        unchecked
        {
            int h = 17;
            foreach (char c in s) h = h * 31 + c;
            return h;
        }
    }

    private static void EscreverRelatorio()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Mapa IFNMG — relatório de geração");
        sb.AppendLine();
        sb.AppendLine($"Fonte: `{JSON_PATH}` (metro/px = {D.metersPerPixel.ToString(CultureInfo.InvariantCulture)}).");
        sb.AppendLine();
        sb.AppendLine($"- JSON: `{JSON_PATH}`");
        sb.AppendLine($"- Aplicar tint nos prefabs: **{(D.aplicarTint ? "sim" : "não")}**");
        sb.AppendLine();
        sb.AppendLine("## Ids sem asset equivalente (placeholder)");
        sb.AppendLine();
        if (Placeholders.Count == 0) sb.AppendLine("Nenhum.");
        else foreach (var id in Placeholders) sb.AppendLine($"- `{id}`");
        sb.AppendLine();
        sb.AppendLine("## Prefabs não encontrados");
        sb.AppendLine();
        if (PrefabFaltando.Count == 0) sb.AppendLine("Nenhum.");
        else foreach (var p in PrefabFaltando) sb.AppendLine($"- `{p}`");
        sb.AppendLine();
        sb.AppendLine("## Avisos");
        sb.AppendLine();
        sb.AppendLine("- E01, E04, E05, E09, E16 têm arranjo aproximado (alas/anel) e podem precisar de ajuste manual.");
        sb.AppendLine("- T06 (lavoura) e E08 (quadra) são caixas placeholder coloridas.");
        sb.AppendLine("- Telhados não foram recoloridos automaticamente (aplicarTint=false); o campo `tint` fica no JSON.");

        string full = Path.GetFullPath(RELATORIO);
        Directory.CreateDirectory(Path.GetDirectoryName(full));
        File.WriteAllText(full, sb.ToString(), new UTF8Encoding(false));
        AssetDatabase.Refresh();
    }
}

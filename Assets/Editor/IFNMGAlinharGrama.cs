using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGAlinharGrama
{
    private const string CENA = "Assets/Scenes/Campus.unity";
    private const string PREFAB = "Assets/Vegetation_Stylized_Pack_ByLuxArtStudios/Prefabs/Bushes/S_Grass_01A.prefab";
    private const string PREFAB_TILE = "Assets/SimplePoly City - Low Poly Assets/Prefab/Natures/Natures_Grass Tile.prefab";
    private const string NOME_RAIZ = "Mapa_IFNMG_Prototipo";
    private const string NOME_CONTAINER = "Grama_Alinhada";

    private const int TARGET_TUFOS = 130000;
    private const float ESPACAMENTO_MIN = 1.0f;
    private const float ESPACAMENTO_MAX = 6.0f;
    private const float MARGEM_TILE = 0.3f;
    private const float MARGEM_PREDIO = 1.0f;
    private const float ALTURA_ALVO = 1.1f;
    private const int SEED = 3602026;

    private class TileGrama
    {
        public string nome;
        public Transform transform;
        public Bounds bounds;
        public Bounds localBounds;
        public bool usarLocal;
        public float topoY;
        public float area;
        public float tamanhoX;
        public float tamanhoZ;
        public float insetLocalX;
        public float insetLocalZ;
        public int colocados;
    }

    public static void Diagnostico()
    {
        Debug.Log("[Grama] Diagnostico OK. Projeto=" + Application.dataPath);
        EditorApplication.Exit(0);
    }

    public static void DiagnosticoTile()
    {
        string caminho = PREFAB_TILE;
        GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
        if (p == null)
        {
            Debug.LogError("[Grama] Prefab do tile nao encontrado: " + caminho);
            EditorApplication.Exit(1);
            return;
        }
        GameObject temp = (GameObject)PrefabUtility.InstantiatePrefab(p);
        Renderer[] rs = temp.GetComponentsInChildren<Renderer>(true);
        Bounds b = new Bounds(temp.transform.position, Vector3.zero);
        bool tem = false;
        for (int i = 0; i < rs.Length; i++)
        {
            if (!tem) { b = rs[i].bounds; tem = true; }
            else b.Encapsulate(rs[i].bounds);
            string mat = rs[i].sharedMaterial != null ? AssetDatabase.GetAssetPath(rs[i].sharedMaterial) : "(null)";
            Debug.Log("[Grama] TileRenderer[" + i + "] " + rs[i].gameObject.name + " " + rs[i].GetType().Name
                + " size=" + rs[i].bounds.size.ToString("0.###") + " mat=" + mat);
        }
        Debug.Log("[Grama] TilePrefab bounds size=" + b.size.ToString("0.###") + " center=" + b.center.ToString("0.###") + " renderers=" + rs.Length);
        UnityEngine.Object.DestroyImmediate(temp);
        EditorApplication.Exit(0);
    }

    [MenuItem("Tools/IFNMG 360/Alinhar grama nos gramados")]
    public static void Executar()
    {
        try
        {
            ExecutarInterno();
        }
        catch (Exception ex)
        {
            Debug.LogError("[Grama] FALHA: " + ex);
            EditorApplication.Exit(1);
        }
    }

    private static void ExecutarInterno()
    {
        string cenaFull = Path.GetFullPath(CENA);
        if (!File.Exists(cenaFull))
            throw new FileNotFoundException("Cena nao encontrada", cenaFull);

        string backupDir = Path.GetFullPath("Backups");
        Directory.CreateDirectory(backupDir);
        string backup = Path.Combine(backupDir, "Campus.before-grama-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity");
        File.Copy(cenaFull, backup, true);
        Debug.Log("[Grama] Backup criado: " + backup);

        Scene cena = EditorSceneManager.OpenScene(CENA, OpenSceneMode.Single);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB);
        if (prefab == null)
            throw new Exception("Prefab nao encontrado: " + PREFAB);
        string prefabPath = AssetDatabase.GetAssetPath(prefab);

        GameObject tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_TILE);
        if (tilePrefab == null)
            throw new Exception("Prefab de tile nao encontrado: " + PREFAB_TILE);
        string tilePrefabPath = AssetDatabase.GetAssetPath(tilePrefab);
        Bounds tileLocal = CalcularBoundsLocais(tilePrefab);
        Debug.Log("[Grama] Tile prefab bounds local: " + tileLocal.size.ToString("0.###"));

        Transform predios = EncontrarTransform(NOME_RAIZ + "/Modelos_IFNMG");
        string hashAntes = predios != null ? HashTransform(predios) : "";
        int gruposPredios = predios != null ? predios.childCount : 0;
        Debug.Log("[Grama] Predios: " + gruposPredios + " grupos. Hash antes: " + hashAntes);

        int removidas = RemoverGramaExistente(cena, prefabPath);
        Debug.Log("[Grama] Raizes de grama removidas: " + removidas);

        List<TileGrama> tiles = ColetarTiles(cena, tilePrefabPath, tileLocal);
        float areaTotal = 0f;
        for (int i = 0; i < tiles.Count; i++)
            areaTotal += tiles[i].area;

        Debug.Log("[Grama] Tiles de grama: " + tiles.Count + " | area total=" + areaTotal.ToString("0") + " m2");
        if (tiles.Count == 0)
            throw new Exception("Nenhum tile Natures_Grass Tile encontrado.");

        float espacamento = Mathf.Clamp(Mathf.Sqrt(areaTotal / TARGET_TUFOS), ESPACAMENTO_MIN, ESPACAMENTO_MAX);
        Debug.Log("[Grama] Espacamento calculado: " + espacamento.ToString("0.###") + " m (alvo " + TARGET_TUFOS + " tufos)");

        List<Bounds> boundsPredios = ColetarBoundsPredios(predios);
        Debug.Log("[Grama] AABBs de predios validos: " + boundsPredios.Count);

        Transform container = CriarContainer();
        float escalaBase = CalcularEscalaBase(prefab);
        Debug.Log("[Grama] Escala uniforme base: " + escalaBase.ToString("0.####"));

        int total = Semear(tiles, boundsPredios, container, prefab, escalaBase, espacamento);
        Debug.Log("[Grama] Total de tufos criados: " + total);

        string hashDepois = predios != null ? HashTransform(predios) : "";
        Debug.Log("[Grama] Hash depois: " + hashDepois);
        if (hashAntes != hashDepois)
            throw new Exception("Hash dos predios mudou. Abortando sem salvar a cena.");

        EditorSceneManager.MarkSceneDirty(cena);
        bool salvo = EditorSceneManager.SaveScene(cena, CENA);
        if (!salvo)
            throw new Exception("Falha ao salvar a cena.");
        AssetDatabase.SaveAssets();
        Debug.Log("[Grama] Cena salva: " + CENA + " | tiles=" + tiles.Count + " tufos=" + total);

        EscreverRelatorio(tiles, total, areaTotal, espacamento, hashAntes, hashDepois, gruposPredios, boundsPredios.Count, escalaBase, backup);
    }

    private static int RemoverGramaExistente(Scene cena, string prefabPath)
    {
        int removidas = 0;
        Transform detalhes = EncontrarTransform(NOME_RAIZ + "/Detalhes_Naturais");
        if (detalhes != null)
        {
            for (int i = detalhes.childCount - 1; i >= 0; i--)
            {
                Transform filho = detalhes.GetChild(i);
                if (filho == null) continue;
                GameObject go = filho.gameObject;
                bool ehGrama = go.name.StartsWith("S_Grass_01A", StringComparison.OrdinalIgnoreCase);
                if (go.name == NOME_CONTAINER || ehGrama || PrefabPathDe(go) == prefabPath)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                    removidas++;
                }
            }
        }

        List<GameObject> restantes = new List<GameObject>();
        foreach (GameObject raiz in cena.GetRootGameObjects())
            ColetarGramaRec(raiz.transform, prefabPath, restantes);

        for (int i = 0; i < restantes.Count; i++)
        {
            if (restantes[i] != null)
            {
                UnityEngine.Object.DestroyImmediate(restantes[i]);
                removidas++;
            }
        }
        return removidas;
    }

    private static void ColetarGramaRec(Transform t, string prefabPath, List<GameObject> lista)
    {
        GameObject go = t.gameObject;
        if (go.name == NOME_CONTAINER)
        {
            lista.Add(go);
            return;
        }
        if (PrefabUtility.IsAnyPrefabInstanceRoot(go) && PrefabPathDe(go) == prefabPath)
        {
            lista.Add(go);
            return;
        }
        for (int i = 0; i < t.childCount; i++)
            ColetarGramaRec(t.GetChild(i), prefabPath, lista);
    }

    private static string PrefabPathDe(GameObject go)
    {
        if (!PrefabUtility.IsAnyPrefabInstanceRoot(go)) return "";
        return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
    }

    private static List<TileGrama> ColetarTiles(Scene cena, string tilePrefabPath, Bounds tileLocal)
    {
        List<TileGrama> lista = new List<TileGrama>();
        foreach (GameObject raiz in cena.GetRootGameObjects())
            ColetarTilesRec(raiz.transform, tilePrefabPath, tileLocal, lista);
        return lista;
    }

    private static void ColetarTilesRec(Transform t, string tilePrefabPath, Bounds tileLocal, List<TileGrama> lista)
    {
        GameObject go = t.gameObject;
        if (go.name.IndexOf("Natures_Grass Tile", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Bounds b;
            bool ok = false;
            bool usarLocal = false;
            float area = 0f;

            if (PrefabUtility.IsAnyPrefabInstanceRoot(go) && PrefabPathDe(go) == tilePrefabPath)
            {
                b = TransformBounds(t, tileLocal);
                Vector3 ls = t.lossyScale;
                area = Mathf.Abs(ls.x * ls.z) * tileLocal.size.x * tileLocal.size.z;
                usarLocal = true;
                ok = true;
            }
            else
            {
                Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
                if (rs.Length > 0)
                {
                    b = rs[0].bounds;
                    for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
                    area = b.size.x * b.size.z;
                    ok = true;
                }
                else b = new Bounds();
            }

            if (ok && b.size.x > 0.5f && b.size.z > 0.5f)
            {
                Vector3 escala = t.lossyScale;
                lista.Add(new TileGrama
                {
                    nome = go.name,
                    transform = t,
                    bounds = b,
                    localBounds = tileLocal,
                    usarLocal = usarLocal,
                    topoY = t.TransformPoint(new Vector3(0f, tileLocal.max.y, 0f)).y,
                    area = area,
                    tamanhoX = usarLocal ? tileLocal.size.x * Mathf.Abs(escala.x) : b.size.x,
                    tamanhoZ = usarLocal ? tileLocal.size.z * Mathf.Abs(escala.z) : b.size.z,
                    insetLocalX = MARGEM_TILE / Mathf.Max(Mathf.Abs(escala.x), 0.0001f),
                    insetLocalZ = MARGEM_TILE / Mathf.Max(Mathf.Abs(escala.z), 0.0001f),
                    colocados = 0
                });
            }
        }

        for (int i = 0; i < t.childCount; i++)
            ColetarTilesRec(t.GetChild(i), tilePrefabPath, tileLocal, lista);
    }

    private static Bounds TransformBounds(Transform t, Bounds local)
    {
        Vector3 centro = t.TransformPoint(local.center);
        Vector3 e = local.extents;
        Vector3 ax = t.TransformVector(new Vector3(e.x, 0f, 0f));
        Vector3 ay = t.TransformVector(new Vector3(0f, e.y, 0f));
        Vector3 az = t.TransformVector(new Vector3(0f, 0f, e.z));
        Vector3 ext = new Vector3(
            Mathf.Abs(ax.x) + Mathf.Abs(ay.x) + Mathf.Abs(az.x),
            Mathf.Abs(ax.y) + Mathf.Abs(ay.y) + Mathf.Abs(az.y),
            Mathf.Abs(ax.z) + Mathf.Abs(ay.z) + Mathf.Abs(az.z));
        return new Bounds(centro, ext * 2f);
    }

    private static Bounds CalcularBoundsLocais(GameObject prefab)
    {
        GameObject temp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        try
        {
            temp.transform.position = Vector3.zero;
            temp.transform.rotation = Quaternion.identity;
            temp.transform.localScale = Vector3.one;
            Renderer[] rs = temp.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) return new Bounds(Vector3.zero, Vector3.one);
            Bounds b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(temp);
        }
    }

    private static List<Bounds> ColetarBoundsPredios(Transform modelos)
    {
        List<Bounds> lista = new List<Bounds>();
        if (modelos == null) return lista;

        for (int i = 0; i < modelos.childCount; i++)
        {
            Transform ch = modelos.GetChild(i);
            Vector3 pos = ch.position;
            Renderer[] rs = ch.GetComponentsInChildren<Renderer>(true);
            bool tem = false;
            Bounds b = new Bounds();

            for (int j = 0; j < rs.Length; j++)
            {
                Renderer r = rs[j];
                if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
                Bounds rb = r.bounds;
                if (rb.size.x > 150f || rb.size.z > 150f) continue;
                if (rb.size.y < 0.1f && (rb.size.x > 25f || rb.size.z > 25f)) continue;
                if (Mathf.Abs(rb.center.x - pos.x) > 150f || Mathf.Abs(rb.center.z - pos.z) > 150f) continue;

                if (!tem) { b = rb; tem = true; }
                else b.Encapsulate(rb);
            }

            if (!tem)
                b = new Bounds(new Vector3(pos.x, pos.y, pos.z), new Vector3(20f, 2f, 20f));

            lista.Add(b);
            Debug.Log("[Grama] Predio '" + ch.name + "' AABB=" + b.size.ToString("0.0") + " centro=" + b.center.ToString("0.0"));
        }
        return lista;
    }

    private static Transform CriarContainer()
    {
        Transform detalhes = EncontrarTransform(NOME_RAIZ + "/Detalhes_Naturais");
        GameObject go = new GameObject(NOME_CONTAINER);
        Transform t = go.transform;
        if (detalhes != null)
            t.SetParent(detalhes, false);
        t.position = Vector3.zero;
        t.rotation = Quaternion.identity;
        t.localScale = Vector3.one;
        return t;
    }

    private static float CalcularEscalaBase(GameObject prefab)
    {
        Bounds b = CalcularBoundsLocais(prefab);
        float h = b.size.y;
        if (h < 0.0001f) return 1f;
        return Mathf.Clamp(ALTURA_ALVO / h, 0.01f, 50f);
    }

    private static int Semear(List<TileGrama> tiles, List<Bounds> predios, Transform container, GameObject prefab, float escalaBase, float espacamento)
    {
        System.Random rng = new System.Random(SEED);
        HashSet<long> celulas = new HashSet<long>();
        int total = 0;

        for (int ti = 0; ti < tiles.Count; ti++)
        {
            TileGrama tile = tiles[ti];
            float minX = tile.bounds.min.x;
            float maxX = tile.bounds.max.x;
            float minZ = tile.bounds.min.z;
            float maxZ = tile.bounds.max.z;
            if (maxX <= minX || maxZ <= minZ) continue;

            int ix0 = Mathf.CeilToInt(minX / espacamento);
            int ix1 = Mathf.FloorToInt(maxX / espacamento);
            int iz0 = Mathf.CeilToInt(minZ / espacamento);
            int iz1 = Mathf.FloorToInt(maxZ / espacamento);

            for (int ix = ix0; ix <= ix1; ix++)
            {
                float x = ix * espacamento;
                for (int iz = iz0; iz <= iz1; iz++)
                {
                    float z = iz * espacamento;

                    if (tile.usarLocal)
                    {
                        Vector3 local = tile.transform.InverseTransformPoint(new Vector3(x, tile.topoY, z));
                        if (local.x < tile.localBounds.min.x + tile.insetLocalX || local.x > tile.localBounds.max.x - tile.insetLocalX) continue;
                        if (local.z < tile.localBounds.min.z + tile.insetLocalZ || local.z > tile.localBounds.max.z - tile.insetLocalZ) continue;
                    }
                    else
                    {
                        if (x < minX + MARGEM_TILE || x > maxX - MARGEM_TILE) continue;
                        if (z < minZ + MARGEM_TILE || z > maxZ - MARGEM_TILE) continue;
                    }

                    if (DentroPredio(x, z, predios)) continue;

                    long chave = ((long)ix << 32) ^ (uint)iz;
                    if (!celulas.Add(chave)) continue;

                    float yaw = (float)(rng.NextDouble() * 360.0);
                    float variacao = 0.9f + (float)rng.NextDouble() * 0.2f;

                    GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, container);
                    Transform tr = go.transform;
                    tr.position = new Vector3(x, tile.topoY + 0.01f, z);
                    tr.rotation = Quaternion.Euler(0f, yaw, 0f);
                    tr.localScale = Vector3.one * (escalaBase * variacao);
                    go.name = "S_Grass_01A_" + (total + 1).ToString("00000");

                    tile.colocados++;
                    total++;
                }
            }

            Debug.Log("[Grama] Tile '" + tile.nome + "': " + tile.colocados + " tufos | area "
                + tile.area.ToString("0.#") + " m2");
        }

        return total;
    }

    private static bool DentroPredio(float x, float z, List<Bounds> predios)
    {
        for (int i = 0; i < predios.Count; i++)
        {
            Bounds b = predios[i];
            if (x >= b.min.x - MARGEM_PREDIO && x <= b.max.x + MARGEM_PREDIO &&
                z >= b.min.z - MARGEM_PREDIO && z <= b.max.z + MARGEM_PREDIO)
                return true;
        }
        return false;
    }

    private static Transform EncontrarTransform(string caminho)
    {
        GameObject go = GameObject.Find(caminho);
        if (go != null) return go.transform;

        Scene cena = SceneManager.GetActiveScene();
        foreach (GameObject raiz in cena.GetRootGameObjects())
        {
            Transform t = EncontrarRec(raiz.transform, caminho.Substring(caminho.LastIndexOf('/') + 1));
            if (t != null) return t;
        }
        return null;
    }

    private static Transform EncontrarRec(Transform t, string nome)
    {
        if (t.name == nome) return t;
        for (int i = 0; i < t.childCount; i++)
        {
            Transform f = EncontrarRec(t.GetChild(i), nome);
            if (f != null) return f;
        }
        return null;
    }

    private static string HashTransform(Transform raiz)
    {
        StringBuilder sb = new StringBuilder();
        Stack<Transform> pilha = new Stack<Transform>();
        pilha.Push(raiz);
        while (pilha.Count > 0)
        {
            Transform t = pilha.Pop();
            sb.Append(t.name).Append('|')
              .Append(t.localPosition.x.ToString("R")).Append(',')
              .Append(t.localPosition.y.ToString("R")).Append(',')
              .Append(t.localPosition.z.ToString("R")).Append('|')
              .Append(t.localRotation.x.ToString("R")).Append(',')
              .Append(t.localRotation.y.ToString("R")).Append(',')
              .Append(t.localRotation.z.ToString("R")).Append(',')
              .Append(t.localRotation.w.ToString("R")).Append('|')
              .Append(t.localScale.x.ToString("R")).Append(',')
              .Append(t.localScale.y.ToString("R")).Append(',')
              .Append(t.localScale.z.ToString("R")).Append('\n');

            for (int i = t.childCount - 1; i >= 0; i--)
                pilha.Push(t.GetChild(i));
        }

        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }

    private static void EscreverRelatorio(List<TileGrama> tiles, int total, float areaTotal, float espacamento,
        string hashAntes, string hashDepois, int gruposPredios, int boundsPredios, float escalaBase, string backup)
    {
        string dir = Path.GetFullPath("Library");
        Directory.CreateDirectory(dir);
        string caminho = Path.Combine(dir, "grama_alinhada_report.json");

        StringBuilder sb = new StringBuilder();
        sb.Append("{\n");
        sb.Append("  \"cena\": \"").Append(CENA.Replace("\\", "/")).Append("\",\n");
        sb.Append("  \"data\": \"").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("\",\n");
        sb.Append("  \"backup\": \"").Append(backup.Replace("\\", "/")).Append("\",\n");
        sb.Append("  \"area_total_m2\": ").Append(areaTotal.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)).Append(",\n");
        sb.Append("  \"espacamento_m\": ").Append(espacamento.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)).Append(",\n");
        sb.Append("  \"margem_tile_m\": ").Append(MARGEM_TILE.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)).Append(",\n");
        sb.Append("  \"margem_predio_m\": ").Append(MARGEM_PREDIO.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)).Append(",\n");
        sb.Append("  \"altura_alvo_m\": ").Append(ALTURA_ALVO.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)).Append(",\n");
        sb.Append("  \"escala_base\": ").Append(escalaBase.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture)).Append(",\n");
        sb.Append("  \"tiles\": ").Append(tiles.Count).Append(",\n");
        sb.Append("  \"tufos\": ").Append(total).Append(",\n");
        sb.Append("  \"grupos_predios\": ").Append(gruposPredios).Append(",\n");
        sb.Append("  \"aabb_predios\": ").Append(boundsPredios).Append(",\n");
        sb.Append("  \"hash_predios_antes\": \"").Append(hashAntes).Append("\",\n");
        sb.Append("  \"hash_predios_depois\": \"").Append(hashDepois).Append("\",\n");
        sb.Append("  \"detalhes_tiles\": [\n");
        for (int i = 0; i < tiles.Count; i++)
        {
            TileGrama t = tiles[i];
            sb.Append("    {\"nome\": \"").Append(t.nome.Replace("\"", "'")).Append("\", \"x\": ")
              .Append(t.bounds.center.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
              .Append(", \"z\": ")
              .Append(t.bounds.center.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
              .Append(", \"tamanho_x\": ")
              .Append(t.tamanhoX.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
              .Append(", \"tamanho_z\": ")
              .Append(t.tamanhoZ.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
              .Append(", \"tufos\": ").Append(t.colocados).Append("}");
            if (i < tiles.Count - 1) sb.Append(",");
            sb.Append("\n");
        }
        sb.Append("  ]\n");
        sb.Append("}\n");

        File.WriteAllText(caminho, sb.ToString());
        Debug.Log("[Grama] Relatorio: " + caminho);
    }
}

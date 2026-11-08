#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGCampusV2Generator
{
    private const string DataPath = "Assets/IFNMG360/Config/ifnmg_cena_v2.json";
    private const string RootName = "CampusGenerated";
    private const float Cell = 5f;
    private const float OriginX = -215f;
    private const float OriginZ = 157.5f;

    [Serializable] private class Root { public Meta meta; public Grid grade; public Grid grid; public Building[] edificacoes; public Road[] vias; }
    [Serializable] private class Meta { public World mundo; }
    [Serializable] private class World { public float largura_x_m; public float profundidade_z_m; }
    [Serializable] private class Grid { public int linhas; public int colunas; public string[] texto; }
    [Serializable] private class Building { public string id; public float[] centro_xz; public float comprimento_m; public float largura_m; public float yaw_graus; public string papel; }
    [Serializable] private class Road { public string id; public float largura_m; public float[] pontos_xz; }

    private static readonly Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();
    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    public static void FixBiologyVisibility()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        GameObject structure = GameObject.Find("Estrutura_Biologia");
        if (structure == null) { Debug.LogError("[IFNMG Biology] Instância não encontrada."); return; }
        structure.transform.localScale = Vector3.one * 10f;
        structure.transform.position = new Vector3(30f, 0f, 30f);
        foreach (Renderer renderer in structure.GetComponentsInChildren<Renderer>(true)) renderer.enabled = true;
        foreach (MeshFilter filter in structure.GetComponentsInChildren<MeshFilter>(true))
        {
            MeshCollider collider = filter.gameObject.GetComponent<MeshCollider>();
            if (collider == null) collider = filter.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = filter.sharedMesh;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[IFNMG Biology] Visibilidade corrigida: escala 10, posição (30,0,30).");
    }

    public static void AuditBiologyStructure()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        GameObject structure = GameObject.Find("Estrutura_Biologia");
        if (structure == null) { Debug.LogError("[IFNMG Biology] Instância não encontrada na cena."); return; }
        Renderer[] renderers = structure.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = new Bounds(structure.transform.position, Vector3.zero);
        bool initialized = false;
        foreach (Renderer renderer in renderers)
        {
            if (!initialized) { bounds = renderer.bounds; initialized = true; } else bounds.Encapsulate(renderer.bounds);
            renderer.enabled = true;
            Debug.Log("[IFNMG Biology] renderer=" + renderer.name + " active=" + renderer.gameObject.activeInHierarchy + " enabled=" + renderer.enabled + " bounds=" + renderer.bounds);
        }
        Debug.Log("[IFNMG Biology] active=" + structure.activeInHierarchy + " position=" + structure.transform.position + " scale=" + structure.transform.lossyScale + " renderers=" + renderers.Length + " bounds=" + bounds);
    }

    public static void AddBiologyStructure()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        GameObject existing = GameObject.Find("Estrutura_Biologia");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing);
        GameObject oldV2 = GameObject.Find("Estrutura_Biologia_V2");
        if (oldV2 != null) UnityEngine.Object.DestroyImmediate(oldV2);
        GameObject oldV3 = GameObject.Find("Estrutura_Biologia_V3");
        if (oldV3 != null) UnityEngine.Object.DestroyImmediate(oldV3);
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/IFNMG_Imported/Estrutura_Biologia_V3.fbx");
        if (model == null) { Debug.LogError("[IFNMG] Estrutura_Biologia.fbx não encontrada."); return; }
        GameObject structure = (GameObject)PrefabUtility.InstantiatePrefab(model);
        structure.name = "Estrutura_Biologia";
        structure.transform.position = new Vector3(30f, 0f, 30f);
        structure.transform.rotation = Quaternion.identity;
        structure.transform.localScale = Vector3.one;
        foreach (MeshRenderer renderer in structure.GetComponentsInChildren<MeshRenderer>(true)) renderer.enabled = true;
        foreach (MeshFilter filter in structure.GetComponentsInChildren<MeshFilter>(true))
        {
            MeshCollider collider = filter.gameObject.GetComponent<MeshCollider>();
            if (collider == null) collider = filter.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = filter.sharedMesh;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[IFNMG] Estrutura de biologia adicionada em " + structure.transform.position);
    }

    public static void RestorePlayerVisual()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        GameObject player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
        if (player == null) { Debug.LogError("[IFNMG Player] Player não encontrado."); return; }
        List<GameObject> oldVisuals = new List<GameObject>();
        foreach (Transform child in player.transform.GetComponentsInChildren<Transform>(true))
        {
            if (child == player.transform) continue;
            if (child.GetComponent<Renderer>() != null || child.GetComponentsInChildren<Renderer>(true).Length > 0)
                oldVisuals.Add(child.gameObject);
        }
        foreach (GameObject oldVisual in oldVisuals)
            if (oldVisual != null) UnityEngine.Object.DestroyImmediate(oldVisual);
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Modelos/Skate/Skate_V2.fbx");
        if (model == null) { Debug.LogError("[IFNMG Player] Modelo Skate_V2.fbx não encontrado."); return; }
        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
        visual.name = "PlayerVisual_Skate_V2";
        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;
        foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true)) renderer.enabled = true;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[IFNMG Player] Visual restaurado: " + visual.name + " renderers=" + visual.GetComponentsInChildren<Renderer>(true).Length);
    }

    public static void AuditPlayer()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        GameObject player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
        if (player == null) { Debug.LogError("[IFNMG Player] Player não encontrado."); return; }
        Bounds bounds = new Bounds(player.transform.position, Vector3.zero);
        bool hasRenderer = false;
        foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>(true))
        {
            if (!hasRenderer) { bounds = renderer.bounds; hasRenderer = true; } else bounds.Encapsulate(renderer.bounds);
            Debug.Log("[IFNMG Player] Renderer " + renderer.name + " enabled=" + renderer.enabled + " active=" + renderer.gameObject.activeInHierarchy + " bounds=" + renderer.bounds);
        }
        Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>();
        Debug.Log("[IFNMG Player] active=" + player.activeInHierarchy + " position=" + player.transform.position + " scale=" + player.transform.lossyScale + " bounds=" + bounds + " renderers=" + player.GetComponentsInChildren<Renderer>(true).Length + " cameras=" + cameras.Length);
    }

    public static void MeasureCampus()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        Bounds total = new Bounds();
        bool hasBounds = false;
        Transform generated = GameObject.Find(RootName)?.transform;
        foreach (Renderer renderer in UnityEngine.Object.FindObjectsByType<Renderer>())
        {
            if (generated != null && renderer.transform.IsChildOf(generated)) continue;
            if (!hasBounds) { total = renderer.bounds; hasBounds = true; } else total.Encapsulate(renderer.bounds);
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log("[IFNMG Measure] bounds cena: center=" + total.center + " size=" + total.size + " player=" + (player != null ? player.transform.position.ToString() : "ausente"));
    }

    [MenuItem("Tools/IFNMG 360/Campus v2/Gerar na cena Campus")]
    public static void Generate()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Campus.unity")
            scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(DataPath);
        if (asset == null) { Debug.LogError("JSON v2 ausente em " + DataPath); return; }
        Root data;
        try { data = JsonUtility.FromJson<Root>(asset.text); }
        catch (Exception e) { Debug.LogError("Falha ao ler JSON v2: " + e.Message); return; }
        if (data == null) { Debug.LogError("JSON v2 inválido."); return; }
        if (data.grid == null || data.grid.linhas <= 0) data.grid = data.grade;
        if (data.grid == null) { Debug.LogError("JSON v2 sem grade."); return; }
        GameObject originalGround = GameObject.Find("TerrenoBase_500x364m");
        if (originalGround != null)
        {
            foreach (Renderer renderer in originalGround.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
            foreach (Collider collider in originalGround.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        }
        GameObject old = GameObject.Find(RootName);
        if (old != null) Undo.DestroyObjectImmediate(old);
        Prefabs.Clear(); Materials.Clear();
        GameObject root = new GameObject(RootName);
        Transform bases = Group("Bases", root.transform);
        Transform roads = Group("Vias", root.transform);
        Transform buildings = Group("Edificacoes_Existentes_Nao_Geradas", root.transform);
        Transform nature = Group("Ambientacao_Removida", root.transform);
        float mapScaleX = data.meta != null && data.meta.mundo != null && data.meta.mundo.largura_x_m > 0f ? data.meta.mundo.largura_x_m / 430f : 1f;
        float mapScaleZ = data.meta != null && data.meta.mundo != null && data.meta.mundo.profundidade_z_m > 0f ? data.meta.mundo.profundidade_z_m / 315f : 1f;
        bases.localScale = new Vector3(mapScaleX, 1f, mapScaleZ);
        roads.localScale = new Vector3(mapScaleX, 1f, mapScaleZ);
        nature.localScale = new Vector3(mapScaleX, 1f, mapScaleZ);
        GenerateGrid(data.grid, bases, null);
        GenerateRoads(data.vias, roads);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[IFNMG] Campus v2 gerado na cena principal. Células processadas: " + (data.grid.linhas * data.grid.colunas));
    }

    [MenuItem("Tools/IFNMG 360/Campus v2/Limpar geração")]
    public static void Clear()
    {
        GameObject root = GameObject.Find(RootName);
        if (root != null) { Undo.DestroyObjectImmediate(root); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); }
    }

    [MenuItem("Tools/IFNMG 360/Campus v2/Remover vegetação e mobiliário")]
    public static void RemoveVegetationAndFurniture()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Campus.unity")
            scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        List<GameObject> remove = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == RootName) continue;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                string name = child.name.ToLowerInvariant();
                if (name.Contains("arvore") || name.Contains("tree_") || name.Contains("tree ") || name.Contains("grass") || name.Contains("grama") || name.Contains("tronco") || name.Contains("trunk") || name.Contains("log") || name.Contains("poste") || name.Contains("lamp"))
                    remove.Add(child.gameObject);
            }
        }
        HashSet<GameObject> unique = new HashSet<GameObject>(remove);
        foreach (GameObject go in unique)
            if (go != null && go.transform.parent != null && !IsChildOfGenerated(go.transform))
                Undo.DestroyObjectImmediate(go);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[IFNMG] Vegetação e mobiliário removidos: " + unique.Count);
    }

    [MenuItem("Tools/IFNMG 360/Campus v2/Remover árvores gramas troncos postes")]
    public static void RemoveTreesGrassLogsPosts()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Campus.unity")
            scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        Transform generated = GameObject.Find(RootName)?.transform;
        List<GameObject> candidates = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (generated != null && child.IsChildOf(generated)) continue;
                string name = child.name.ToLowerInvariant();
                if (name.Contains("arvore") || name.Contains("tree_") || name.Contains("tree ") || name.Contains("grass") || name.Contains("grama") || name.Contains("tronco") || name.Contains("trunk") || name.Contains("log") || name.Contains("poste") || name.Contains("lamp"))
                    candidates.Add(child.gameObject);
            }
        }
        HashSet<GameObject> unique = new HashSet<GameObject>(candidates);
        foreach (GameObject go in unique)
            if (go != null && go.transform.parent != null && !HasMatchingAncestor(go.transform, unique))
                Undo.DestroyObjectImmediate(go);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[IFNMG] Árvores, gramas, troncos e postes removidos: " + unique.Count);
    }

    [MenuItem("Tools/IFNMG 360/Campus v2/Remover árvores arbustos troncos pedras cogumelos")]
    public static void RemoveTreesShrubsRocksMushrooms()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Campus.unity")
            scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        Transform generated = GameObject.Find(RootName)?.transform;
        List<GameObject> candidates = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (generated != null && child.IsChildOf(generated)) continue;
                string name = child.name.ToLowerInvariant();
                if (name.Contains("arvore") || name.Contains("tree_") || name.Contains("tree ") || name.Contains("arbusto") || name.Contains("bush") || name.Contains("foliage") || name.Contains("tronco") || name.Contains("trunk") || name.Contains("log") || name.Contains("rock") || name.Contains("rocha") || name.Contains("pedra") || name.Contains("mushroom") || name.Contains("cogumelo") || name.Contains("fungus") || name.Contains("fungo"))
                    candidates.Add(child.gameObject);
            }
        }
        HashSet<GameObject> unique = new HashSet<GameObject>(candidates);
        foreach (GameObject go in unique)
            if (go != null && go.transform.parent != null && !HasMatchingAncestor(go.transform, unique))
                Undo.DestroyObjectImmediate(go);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[IFNMG] Elementos naturais removidos: " + unique.Count);
    }

    [MenuItem("Tools/IFNMG 360/Campus v2/Remover pedras e cogumelos")]
    public static void RemoveRocksAndMushrooms()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Campus.unity")
            scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        Transform generated = GameObject.Find(RootName)?.transform;
        List<GameObject> candidates = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (generated != null && child.IsChildOf(generated)) continue;
                string name = child.name.ToLowerInvariant();
                if (name.Contains("rock") || name.Contains("rocha") || name.Contains("pedra") || name.Contains("mushroom") || name.Contains("cogumelo") || name.Contains("fungus") || name.Contains("fungo"))
                    candidates.Add(child.gameObject);
            }
        }
        HashSet<GameObject> unique = new HashSet<GameObject>(candidates);
        foreach (GameObject go in unique)
            if (go != null && go.transform.parent != null && !HasMatchingAncestor(go.transform, unique))
                Undo.DestroyObjectImmediate(go);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[IFNMG] Pedras e cogumelos removidos: " + unique.Count);
    }

    private static bool HasMatchingAncestor(Transform current, HashSet<GameObject> matches)
    {
        Transform parent = current.parent;
        while (parent != null)
        {
            if (matches.Contains(parent.gameObject)) return true;
            parent = parent.parent;
        }
        return false;
    }

    private static bool IsChildOfGenerated(Transform current)
    {
        while (current != null)
        {
            if (current.name == RootName) return true;
            current = current.parent;
        }
        return false;
    }

    private static void GenerateGrid(Grid grid, Transform bases, Transform nature)
    {
        for (int r = 0; r < grid.linhas && r < grid.texto.Length; r++)
        {
            string row = grid.texto[r];
            for (int c = 0; c < grid.colunas && c < row.Length; c++)
            {
                char type = row[c];
                Vector3 p = new Vector3(OriginX + c * Cell + Cell * .5f, 0f, OriginZ - r * Cell - Cell * .5f);
                string material = type == 'S' ? "IFNMG_Terra" : type == 'V' ? "IFNMG_Via" : type == 'A' ? "IFNMG_Pavimento" : "IFNMG_Pavimento";
                CreateTile("Cell_" + r + "_" + c, p, Cell, GetMaterial(material, ColorFor(type)), bases, material);

            }
        }
    }

    private static void GenerateRoads(Road[] roads, Transform parent)
    {
        if (roads == null) return;
        foreach (Road road in roads)
        {
            if (road == null || road.pontos_xz == null || road.pontos_xz.Length < 4) continue;
            for (int i = 0; i + 3 < road.pontos_xz.Length; i += 2)
            {
                Vector3 a = new Vector3(road.pontos_xz[i], .08f, road.pontos_xz[i + 1]);
                Vector3 b = new Vector3(road.pontos_xz[i + 2], .08f, road.pontos_xz[i + 3]);
                Vector3 d = b - a; float length = new Vector2(d.x, d.z).magnitude;
                if (length < .5f) continue;
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = road.id + "_Segment_" + i / 2;
                tile.transform.SetParent(parent, false); tile.transform.position = (a + b) * .5f + Vector3.up * 0.18f;
                tile.transform.rotation = Quaternion.Euler(0, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, 0);
                tile.transform.localScale = new Vector3(road.largura_m, .12f, length);
                SetMaterial(tile, GetMaterial("IFNMG_Via", new Color(.18f, .2f, .21f)));
            }
        }
    }

    private static void GenerateBuildings(Building[] buildings, Transform parent)
    {
        if (buildings == null) return;
        foreach (Building b in buildings)
        {
            if (b == null || b.centro_xz == null || b.centro_xz.Length < 2) continue;
            string key = b.papel ?? "casa_predio_pequeno";
            GameObject prefab = FindPrefab(key);
            Vector3 p = new Vector3(b.centro_xz[0], .1f, b.centro_xz[1]);
            if (prefab != null)
            {
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = b.id + "_" + key; go.transform.SetParent(parent, false); go.transform.position = p; go.transform.rotation = Quaternion.Euler(0, b.yaw_graus, 0);
                Bounds bounds = RenderBounds(go);
                if (bounds.size.x > .01f && bounds.size.z > .01f) go.transform.localScale = new Vector3(Mathf.Clamp(b.comprimento_m / bounds.size.x, .7f, 1.4f), 1f, Mathf.Clamp(b.largura_m / bounds.size.z, .7f, 1.4f));
            }
            else
            {
                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube); block.name = b.id + "_Placeholder"; block.transform.SetParent(parent, false); block.transform.position = p + Vector3.up * 3f; block.transform.rotation = Quaternion.Euler(0, b.yaw_graus, 0); block.transform.localScale = new Vector3(b.comprimento_m, 6f, b.largura_m); SetMaterial(block, GetMaterial("IFNMG_Paredes", new Color(.55f, .55f, .52f)));
            }
        }
    }

    private static void GenerateCampusDetails(Grid grid, Transform parent)
    {
        Spawn("bench", new Vector3(-25, 0, 35), parent, 1f);
        Spawn("bin", new Vector3(18, 0, 30), parent, 1f);
        Spawn("bench", new Vector3(45, 0, 10), parent, 1f);
        Spawn("bin", new Vector3(70, 0, -25), parent, 1f);
    }

    private static Transform Group(string name, Transform parent) { GameObject go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
    private static void CreateTile(string name, Vector3 p, float size, Material mat, Transform parent, string key) { GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name + "_" + key; go.transform.SetParent(parent, false); go.transform.position = p + Vector3.up * 0.06f; go.transform.localScale = new Vector3(size, .12f, size); SetMaterial(go, mat); }
    private static Color ColorFor(char type) { return type == 'S' ? new Color(.48f, .32f, .18f) : type == 'V' ? new Color(.2f, .22f, .23f) : type == 'A' ? new Color(.12f, .3f, .14f) : new Color(.3f, .52f, .22f); }
    private static void SetMaterial(GameObject go, Material mat) { foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat; }
    private static Material GetMaterial(string name, Color color) { if (Materials.TryGetValue(name, out Material m)) return m; m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); m.name = name; m.color = color; Materials[name] = m; return m; }
    private static Bounds RenderBounds(GameObject go) { Renderer[] rs = go.GetComponentsInChildren<Renderer>(); Bounds b = new Bounds(go.transform.position, Vector3.zero); foreach (Renderer r in rs) b.Encapsulate(r.bounds); return b; }

    private static GameObject FindPrefab(string role)
    {
        string key = role.ToLowerInvariant(); if (Prefabs.TryGetValue(key, out GameObject cached)) return cached;
        string[] guids = AssetDatabase.FindAssets("t:Prefab"); GameObject best = null; int score = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid).ToLowerInvariant(); if (!path.Contains("assets/")) continue;
            int s = 0;
            if (key.Contains("tree") || key.Contains("arvore")) s = path.Contains("tree") ? 5 : 0;
            else if (key.Contains("bush") || key.Contains("arbusto")) s = path.Contains("bush") ? 5 : 0;
            else if (key.Contains("lamp") || key.Contains("poste")) s = path.Contains("lamp") ? 5 : 0;
            else if (key.Contains("bench") || key.Contains("banco")) s = path.Contains("bench") ? 5 : 0;
            else if (key.Contains("bin") || key.Contains("lixeira")) s = path.Contains("bin") ? 5 : 0;
            else if (key.Contains("galpao")) s = path.Contains("warehouse") || path.Contains("factory") ? 5 : 0;
            else if (key.Contains("bloco") || key.Contains("casa")) s = path.Contains("building") || path.Contains("house") ? 3 : 0;
            if (s > score) { best = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)); score = s; }
        }
        Prefabs[key] = best; return best;
    }

    private static void Spawn(string role, Vector3 p, Transform parent, float scale)
    {
        GameObject prefab = FindPrefab(role); if (prefab == null) return; GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab); go.transform.SetParent(parent, false); go.transform.position = p; go.transform.localScale = Vector3.one * scale;
    }
}
#endif

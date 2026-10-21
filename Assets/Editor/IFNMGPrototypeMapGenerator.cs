#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Generates a reversible, coordinate-driven IFNMG campus blockout in a separate scene.
/// All geometry comes from IFNMG_Mapa_Proto_v0_1.json. Image interpretation is not used.
/// </summary>
public static class IFNMGPrototypeMapGenerator
{
    private const string DataPath = "Assets/IFNMG360/Config/IFNMG_Mapa_Proto_v0_1.json";
    private const string ScenePath = "Assets/IFNMG360/Scenes/IFNMG_Mapa_Prototype_v0_1.unity";
    private const string MaterialsPath = "Assets/IFNMG360/Generated/Materials";
    private const string RootName = "IFNMG_Mapa_Prototype_v0_1";

    [Serializable] private class MapData
    {
        public int schemaVersion;
        public string title;
        public string status;
        public WorldData world;
        public GenerationData generation;
        public AssetGroup[] assetGroups;
        public ZoneData[] zones;
    }
    [Serializable] private class WorldData
    {
        public float worldWidthMeters = 440f;
        public float worldDepthMeters = 310f;
        public float metersPerUnityUnit = 1f;
        public float baseY = 0f;
        public string northAxis = "+Z";
    }
    [Serializable] private class GenerationData
    {
        public int seed = 3602026;
        public bool spawnTrees = true;
        public bool spawnDecorativeNaturalAssets = true;
        public bool spawnBuildingBlockouts = true;
        public bool spawnRoadMeshes = true;
    }
    [Serializable] private class AssetGroup
    {
        public string category;
        public string[] paths;
        public float tileSizeMeters;
    }
    [Serializable] private class ZoneData
    {
        public string id;
        public string category;
        public string geometryType;
        public string confidence;
        public bool enabled = true;
        public string notes;
        public float[] pointsUvFlat;
        public float widthMeters = 0f;
        public float buildingHeightMeters = 7f;
        public string roofStyle = "orange";
        public int treeCount;
        public int bushCount;
        public int grassDetailCount;
        public int rockCount;
        public int mushroomCount;
    }

    private class ZoneRuntime
    {
        public ZoneData data;
        public Vector2[] worldPoints;
        public Bounds2 bounds;
    }
    private struct Bounds2
    {
        public float minX, maxX, minY, maxY;
        public float Width { get { return maxX - minX; } }
        public float Height { get { return maxY - minY; } }
        public bool Contains(Vector2 p) { return p.x >= minX && p.x <= maxX && p.y >= minY && p.y <= maxY; }
    }

    private static readonly Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();
    private static readonly Dictionary<string, List<GameObject>> PrefabCache = new Dictionary<string, List<GameObject>>();
    private static readonly HashSet<string> WarnedMissingPaths = new HashSet<string>();
    private static MapData CurrentData;
    private static readonly Color GrassColor = new Color(0.32f, 0.53f, 0.24f, 1f);
    private static readonly Color ForestColor = new Color(0.12f, 0.28f, 0.16f, 1f);
    private static readonly Color SoilColor = new Color(0.55f, 0.40f, 0.24f, 1f);
    private static readonly Color RoadColor = new Color(0.34f, 0.39f, 0.42f, 1f);
    private static readonly Color WallColor = new Color(0.68f, 0.65f, 0.58f, 1f);
    private static readonly Color OrangeRoof = new Color(0.72f, 0.30f, 0.18f, 1f);
    private static readonly Color GrayRoof = new Color(0.42f, 0.45f, 0.45f, 1f);
    private static readonly Color BlueRoof = new Color(0.30f, 0.44f, 0.56f, 1f);

    [MenuItem("Tools/IFNMG 360/1 - Gerar mapa protótipo (cena separada)")]
    public static void GenerateSeparatePrototypeScene()
    {
        MapData data = ReadData();
        if (data == null) return;

        EnsureAssetFolder("Assets/IFNMG360");
        EnsureAssetFolder("Assets/IFNMG360/Scenes");
        EnsureAssetFolder("Assets/IFNMG360/Generated");
        EnsureAssetFolder(MaterialsPath);

        // Em batchmode não é possível criar/abrir cenas de forma aditiva a partir de uma cena
        // "untitled" não salva; nem exibir diálogos. Nesse modo, abre/cria a cena do protótipo
        // como Single (a cena principal permanece intacta em disco, não é salva nem alterada).
        bool batch = Application.isBatchMode;
        Scene scene;
        if (File.Exists(ScenePath))
        {
            if (!batch && !EditorUtility.DisplayDialog("Atualizar mapa protótipo", "A cena de protótipo já existe. Deseja abri-la e regenerar somente o mapa gerado? A cena principal do jogo não será alterada.", "Atualizar protótipo", "Cancelar"))
                return;
            scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(ScenePath, batch ? OpenSceneMode.Single : OpenSceneMode.Additive);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, batch ? NewSceneMode.Single : NewSceneMode.Additive);
        }

        SceneManager.SetActiveScene(scene);
        GameObject oldRoot = FindRootInScene(scene);
        if (oldRoot != null)
            Undo.DestroyObjectImmediate(oldRoot);

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Criar mapa protótipo IFNMG");
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;

        Generate(data, root.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[IFNMG Prototype] Cena gerada em " + ScenePath + ". A versão é um blockout aproximado e editável; veja os avisos no Console.");
        if (!batch)
            EditorUtility.DisplayDialog("Protótipo criado", "O mapa protótipo foi salvo em:\n" + ScenePath + "\n\nEle foi gerado por coordenadas do JSON. Revise a cena e o Console antes de reutilizar qualquer posição na cena principal.", "OK");
    }

    [MenuItem("Tools/IFNMG 360/2 - Limpar mapa gerado da cena ativa")]
    public static void ClearGeneratedMapFromActiveScene()
    {
        GameObject root = FindRootInScene(SceneManager.GetActiveScene());
        if (root == null)
        {
            EditorUtility.DisplayDialog("Nenhum mapa gerado", "A cena ativa não contém uma raiz chamada " + RootName + ". Nenhum objeto foi removido.", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Remover mapa protótipo", "Isso removerá somente a raiz gerada " + RootName + " da cena ativa. Outros objetos não serão removidos.", "Remover raiz gerada", "Cancelar")) return;
        Undo.DestroyObjectImmediate(root);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static MapData ReadData()
    {
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(DataPath);
        if (textAsset == null)
        {
            Debug.LogError("[IFNMG Prototype] JSON não encontrado em " + DataPath + ". Copie o arquivo do pacote para esse caminho.");
            return null;
        }
        try
        {
            MapData data = JsonUtility.FromJson<MapData>(textAsset.text);
            if (data == null || data.zones == null || data.world == null)
                throw new Exception("O JSON não contém world/zones no formato esperado.");
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError("[IFNMG Prototype] Não foi possível ler o JSON: " + ex.Message);
            return null;
        }
    }

    private static GameObject FindRootInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded) return null;
        foreach (GameObject go in scene.GetRootGameObjects())
            if (go.name == RootName) return go;
        return null;
    }

    private static void Generate(MapData data, Transform root)
    {
        MaterialCache.Clear();
        PrefabCache.Clear();
        WarnedMissingPaths.Clear();
        CurrentData = data;
        Transform terrainRoot = MakeGroup("Terrenos", root);
        Transform roadRoot = MakeGroup("Vias", root);
        Transform buildingRoot = MakeGroup("Estruturas", root);
        Transform vegetationRoot = MakeGroup("Vegetacao", root);
        Transform decorRoot = MakeGroup("Detalhes_Naturais", root);

        // Data geometry is relative to a centered perimeter. This plane is only the prototype's base.
        GameObject basePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        basePlane.name = "TerrenoBase_" + Mathf.RoundToInt(data.world.worldWidthMeters) + "x" + Mathf.RoundToInt(data.world.worldDepthMeters) + "m";
        basePlane.transform.SetParent(terrainRoot, false);
        basePlane.transform.localPosition = new Vector3(0f, data.world.baseY - 0.15f, 0f);
        basePlane.transform.localScale = new Vector3(data.world.worldWidthMeters / 10f, 1f, data.world.worldDepthMeters / 10f);
        SetRendererMaterial(basePlane, GetMaterial("IFNMG_Grama_Base", GrassColor));
        basePlane.layer = 0;

        List<ZoneRuntime> zones = BuildRuntimeZones(data);
        List<ZoneRuntime> buildingZones = new List<ZoneRuntime>();
        List<ZoneRuntime> pavedZones = new List<ZoneRuntime>();
        List<ZoneRuntime> treeZones = new List<ZoneRuntime>();
        foreach (ZoneRuntime zr in zones)
        {
            if (zr.data.category == "building") buildingZones.Add(zr);
            if (zr.data.category == "paved") pavedZones.Add(zr);
            if (zr.data.category == "trees") treeZones.Add(zr);
        }

        System.Random rngTiles = new System.Random((data.generation != null ? data.generation.seed : 3602026) ^ 0x7A17);

        if (data.generation == null || data.generation.spawnRoadMeshes || true)
        {
            // Superfícies agora são preenchidas com PREFABS reais (ladrilhamento), em vez de malhas de cor sólida.
            // A malha de mata (trees) permanece como base escura sob a vegetação densa.
            foreach (ZoneRuntime zr in zones)
            {
                if (!zr.data.enabled || zr.data.geometryType != "polygon") continue;
                if (zr.data.category == "grass")
                    TilePolygon(zr.data.id, zr.worldPoints, "grassSurface", data.world.baseY, terrainRoot, rngTiles);
                else if (zr.data.category == "soil")
                    TilePolygon(zr.data.id, zr.worldPoints, "soilSurface", data.world.baseY, terrainRoot, rngTiles);
                else if (zr.data.category == "trees")
                    CreatePolygonSurface(zr.data.id, zr.worldPoints, data.world.baseY - 0.04f, GetMaterial("IFNMG_Mata_Base", ForestColor), terrainRoot, false);
                else if (zr.data.category == "paved")
                    TilePolygon(zr.data.id, zr.worldPoints, "pavementSurface", data.world.baseY, roadRoot, rngTiles);
            }
        }

        if (data.generation == null || data.generation.spawnRoadMeshes)
        {
            foreach (ZoneRuntime zr in zones)
            {
                if (!zr.data.enabled || zr.data.category != "paved" || zr.data.geometryType != "line") continue;
                CreateRoadRibbon(zr.data.id, zr.worldPoints, Mathf.Max(2.0f, zr.data.widthMeters), data.world.baseY + 0.025f, GetMaterial("IFNMG_Via", RoadColor), roadRoot);
            }
        }

        if (data.generation == null || data.generation.spawnBuildingBlockouts)
        {
            foreach (ZoneRuntime zr in buildingZones)
            {
                if (!zr.data.enabled || zr.data.geometryType != "polygon") continue;
                CreateBuildingBlockout(zr, data.world.baseY, buildingRoot);
            }
        }

        System.Random rng = new System.Random(data.generation != null ? data.generation.seed : 3602026);
        HashSet<string> missingAssets = new HashSet<string>();
        int treeTotal = 0, bushTotal = 0, rockTotal = 0, mushroomTotal = 0, grassTotal = 0;
        if (data.generation == null || data.generation.spawnTrees || data.generation.spawnDecorativeNaturalAssets)
        {
            foreach (ZoneRuntime zr in treeZones)
            {
                if (!zr.data.enabled || zr.data.geometryType != "polygon") continue;
                List<Vector2> usedTrees = new List<Vector2>();
                List<Vector2> usedBushes = new List<Vector2>();
                List<Vector2> usedRocks = new List<Vector2>();
                List<Vector2> usedMushrooms = new List<Vector2>();
                List<Vector2> usedGrass = new List<Vector2>();
                if (data.generation == null || data.generation.spawnTrees)
                {
                    for (int i = 0; i < zr.data.treeCount; i++)
                    {
                        Vector2 p;
                        if (!TrySample(zr.worldPoints, buildingZones, pavedZones, usedTrees, 3.8f, rng, out p)) break;
                        usedTrees.Add(p);
                        SpawnAssetOrFallback("tree", p, vegetationRoot, rng, missingAssets, zr.data.id + "_Arvore_" + (i + 1).ToString("000"));
                        treeTotal++;
                    }
                }
                if (data.generation == null || data.generation.spawnDecorativeNaturalAssets)
                {
                    for (int i = 0; i < zr.data.bushCount; i++)
                    {
                        Vector2 p;
                        if (!TrySample(zr.worldPoints, buildingZones, pavedZones, usedBushes, 1.3f, rng, out p)) break;
                        usedBushes.Add(p);
                        SpawnAssetOrFallback("bush", p, vegetationRoot, rng, missingAssets, zr.data.id + "_Arbusto_" + (i + 1).ToString("000"));
                        bushTotal++;
                    }
                    for (int i = 0; i < zr.data.grassDetailCount; i++)
                    {
                        Vector2 p;
                        if (!TrySample(zr.worldPoints, buildingZones, pavedZones, usedGrass, 0.35f, rng, out p)) break;
                        usedGrass.Add(p);
                        SpawnAssetOrFallback("grassDetail", p, vegetationRoot, rng, missingAssets, zr.data.id + "_GramaDetalhe_" + (i + 1).ToString("000"));
                        grassTotal++;
                    }
                    for (int i = 0; i < zr.data.rockCount; i++)
                    {
                        Vector2 p;
                        if (!TrySample(zr.worldPoints, buildingZones, pavedZones, usedRocks, 1.0f, rng, out p)) break;
                        usedRocks.Add(p);
                        SpawnAssetOrFallback("rock", p, decorRoot, rng, missingAssets, zr.data.id + "_Rocha_" + (i + 1).ToString("000"));
                        rockTotal++;
                    }
                    for (int i = 0; i < zr.data.mushroomCount; i++)
                    {
                        Vector2 p;
                        if (!TrySample(zr.worldPoints, buildingZones, pavedZones, usedMushrooms, 0.45f, rng, out p)) break;
                        usedMushrooms.Add(p);
                        SpawnAssetOrFallback("mushroom", p, decorRoot, rng, missingAssets, zr.data.id + "_Fungo_" + (i + 1).ToString("000"));
                        mushroomTotal++;
                    }
                }
            }
        }

        ReportOverlaps(buildingZones, pavedZones);
        Debug.Log("[IFNMG Prototype] Resultado: zonas=" + zones.Count + ", footprints de prédios=" + buildingZones.Count + ", árvores=" + treeTotal + ", arbustos=" + bushTotal + ", detalhes de grama=" + grassTotal + ", rochas=" + rockTotal + ", fungos=" + mushroomTotal + ".");
        foreach (string path in missingAssets)
            Debug.LogWarning("[IFNMG Prototype] Prefab não encontrado; foi usado fallback simples: " + path);
    }

    private static List<ZoneRuntime> BuildRuntimeZones(MapData data)
    {
        List<ZoneRuntime> list = new List<ZoneRuntime>();
        foreach (ZoneData z in data.zones)
        {
            if (z == null || z.pointsUvFlat == null || z.pointsUvFlat.Length < 4 || z.pointsUvFlat.Length % 2 != 0)
            {
                Debug.LogWarning("[IFNMG Prototype] Zona ignorada por falta de pontos UV válidos: " + (z != null ? z.id : "null"));
                continue;
            }
            Vector2[] points = new Vector2[z.pointsUvFlat.Length / 2];
            float minX = float.PositiveInfinity, maxX = float.NegativeInfinity, minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;
            for (int i = 0; i < points.Length; i++)
            {
                float u = z.pointsUvFlat[i * 2];
                float v = z.pointsUvFlat[i * 2 + 1];
                float x = (u - 0.5f) * data.world.worldWidthMeters;
                float zz = (0.5f - v) * data.world.worldDepthMeters;
                points[i] = new Vector2(x, zz);
                minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                minZ = Mathf.Min(minZ, zz); maxZ = Mathf.Max(maxZ, zz);
            }
            list.Add(new ZoneRuntime { data = z, worldPoints = points, bounds = new Bounds2 { minX = minX, maxX = maxX, minY = minZ, maxY = maxZ } });
            if (!string.IsNullOrEmpty(z.confidence) && (z.confidence == "low" || z.confidence == "low-medium"))
                Debug.LogWarning("[IFNMG Prototype] Zona de confiança baixa; revise depois: " + z.id + " — " + z.notes);
            if (!z.enabled)
                Debug.Log("[IFNMG Prototype] Zona desativada pela regra de correção: " + z.id);
        }
        return list;
    }

    private static Transform MakeGroup(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void CreatePolygonSurface(string id, Vector2[] points, float y, Material material, Transform parent, bool addCollider)
    {
        List<int> triangles = Triangulate(points);
        if (triangles.Count < 3) { Debug.LogWarning("[IFNMG Prototype] Não foi possível triangular zona: " + id); return; }
        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++) vertices[i] = new Vector3(points[i].x, y, points[i].y);
        // The 2D triangulator returns CCW in X/Z; reverse winding so the top faces point upward in Unity.
        for (int i = 0; i < triangles.Count; i += 3)
        {
            int temp = triangles[i + 1]; triangles[i + 1] = triangles[i + 2]; triangles[i + 2] = temp;
        }
        GameObject go = new GameObject(id);
        go.transform.SetParent(parent, false);
        Mesh mesh = new Mesh { name = id + "_Mesh" };
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        if (addCollider) go.AddComponent<MeshCollider>().sharedMesh = mesh;
    }

    private static void CreateRoadRibbon(string id, Vector2[] points, float width, float y, Material material, Transform parent)
    {
        if (points.Length < 2) return;
        Vector2[] left = new Vector2[points.Length];
        Vector2[] right = new Vector2[points.Length];
        float half = width * 0.5f;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 prevDir = i == 0 ? (points[1] - points[0]).normalized : (points[i] - points[i - 1]).normalized;
            Vector2 nextDir = i == points.Length - 1 ? (points[i] - points[i - 1]).normalized : (points[i + 1] - points[i]).normalized;
            Vector2 n1 = new Vector2(-prevDir.y, prevDir.x);
            Vector2 n2 = new Vector2(-nextDir.y, nextDir.x);
            Vector2 miter = (n1 + n2).normalized;
            if (miter.sqrMagnitude < 0.001f) miter = n2;
            float denom = Mathf.Max(0.35f, Mathf.Abs(Vector2.Dot(miter, n2)));
            float miterLength = Mathf.Min(half / denom, half * 2.5f);
            left[i] = points[i] + miter * miterLength;
            right[i] = points[i] - miter * miterLength;
        }
        Vector3[] verts = new Vector3[points.Length * 2];
        for (int i = 0; i < points.Length; i++)
        {
            verts[i * 2] = new Vector3(left[i].x, y, left[i].y);
            verts[i * 2 + 1] = new Vector3(right[i].x, y, right[i].y);
        }
        int[] tris = new int[(points.Length - 1) * 6];
        int t = 0;
        for (int i = 0; i < points.Length - 1; i++)
        {
            int li = i * 2, ri = li + 1, ln = li + 2, rn = li + 3;
            // Winding chosen for upward-facing road surface.
            tris[t++] = li; tris[t++] = ln; tris[t++] = ri;
            tris[t++] = ln; tris[t++] = rn; tris[t++] = ri;
        }
        GameObject go = new GameObject(id);
        go.transform.SetParent(parent, false);
        Mesh mesh = new Mesh { name = id + "_RibbonMesh" };
        mesh.vertices = verts; mesh.triangles = tris; mesh.RecalculateNormals(); mesh.RecalculateBounds();
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        go.AddComponent<MeshCollider>().sharedMesh = mesh;
    }

    private static void CreateBuildingBlockout(ZoneRuntime zone, float baseY, Transform parent)
    {
        Vector2[] points = zone.worldPoints;
        if (points.Length < 3) return;
        float height = Mathf.Clamp(zone.data.buildingHeightMeters, 3f, 12f);
        string id = zone.data.id;
        Transform building = MakeGroup(id, parent);
        int n = points.Length;
        // Wall faces are generated double-sided to tolerate inconsistent polygon winding in hand-traced data.
        List<Vector3> wallVerts = new List<Vector3>();
        List<int> wallTris = new List<int>();
        for (int i = 0; i < n; i++)
        {
            Vector2 a = points[i]; Vector2 b = points[(i + 1) % n];
            int start = wallVerts.Count;
            wallVerts.Add(new Vector3(a.x, baseY, a.y));
            wallVerts.Add(new Vector3(b.x, baseY, b.y));
            wallVerts.Add(new Vector3(b.x, baseY + height, b.y));
            wallVerts.Add(new Vector3(a.x, baseY + height, a.y));
            wallTris.Add(start); wallTris.Add(start + 2); wallTris.Add(start + 1);
            wallTris.Add(start); wallTris.Add(start + 3); wallTris.Add(start + 2);
            wallTris.Add(start + 1); wallTris.Add(start + 2); wallTris.Add(start);
            wallTris.Add(start + 2); wallTris.Add(start + 3); wallTris.Add(start);
        }
        GameObject walls = new GameObject("Paredes");
        walls.transform.SetParent(building, false);
        Mesh wallMesh = new Mesh { name = id + "_Walls" };
        wallMesh.vertices = wallVerts.ToArray(); wallMesh.triangles = wallTris.ToArray(); wallMesh.RecalculateNormals(); wallMesh.RecalculateBounds();
        walls.AddComponent<MeshFilter>().sharedMesh = wallMesh;
        walls.AddComponent<MeshRenderer>().sharedMaterial = GetMaterial("IFNMG_Paredes", WallColor);
        walls.AddComponent<MeshCollider>().sharedMesh = wallMesh;

        // Roof polygon.
        List<int> roofTris = Triangulate(points);
        for (int i = 0; i < roofTris.Count; i += 3)
        {
            int tmp = roofTris[i + 1]; roofTris[i + 1] = roofTris[i + 2]; roofTris[i + 2] = tmp;
        }
        Vector3[] roofVerts = new Vector3[n];
        for (int i = 0; i < n; i++) roofVerts[i] = new Vector3(points[i].x, baseY + height, points[i].y);
        GameObject roof = new GameObject("Cobertura_" + zone.data.roofStyle);
        roof.transform.SetParent(building, false);
        Mesh roofMesh = new Mesh { name = id + "_Roof" };
        roofMesh.vertices = roofVerts; roofMesh.triangles = roofTris.ToArray(); roofMesh.RecalculateNormals(); roofMesh.RecalculateBounds();
        roof.AddComponent<MeshFilter>().sharedMesh = roofMesh;
        roof.AddComponent<MeshRenderer>().sharedMaterial = GetRoofMaterial(zone.data.roofStyle);
        roof.AddComponent<MeshCollider>().sharedMesh = roofMesh;
    }

    private static Material GetRoofMaterial(string style)
    {
        if (style == "blue") return GetMaterial("IFNMG_Telhado_Azul", BlueRoof);
        if (style == "gray") return GetMaterial("IFNMG_Telhado_Cinza", GrayRoof);
        return GetMaterial("IFNMG_Telhado_Laranja", OrangeRoof);
    }

    private static bool TrySample(Vector2[] polygon, List<ZoneRuntime> buildings, List<ZoneRuntime> paved, List<Vector2> used, float minDistance, System.Random rng, out Vector2 point)
    {
        point = Vector2.zero;
        Bounds2 b = GetBounds(polygon);
        if (b.Width <= 0f || b.Height <= 0f) return false;
        for (int attempt = 0; attempt < 160; attempt++)
        {
            Vector2 p = new Vector2((float)(b.minX + rng.NextDouble() * b.Width), (float)(b.minY + rng.NextDouble() * b.Height));
            if (!PointInPolygon(p, polygon)) continue;
            if (InsideAnyPolygon(p, buildings, "building")) continue;
            if (InsideAnyPolygon(p, paved, "paved")) continue;
            if (NearAnyRoad(p, paved, 2.0f)) continue;
            bool tooClose = false;
            for (int i = 0; i < used.Count; i++) if (Vector2.Distance(p, used[i]) < minDistance) { tooClose = true; break; }
            if (tooClose) continue;
            point = p; return true;
        }
        return false;
    }

    // Preenche um polígono com prefabs de superfície numa grade, mantendo só os tiles cujo CENTRO cai
    // dentro do polígono. Usa o tamanho real do prefab (tileSizeMeters); se ausente no JSON, tenta o bounds
    // do prefab; senão, cai para uma malha de cor sólida (fallback) e registra aviso.
    private static void TilePolygon(string id, Vector2[] points, string group, float baseY, Transform parent, System.Random rng)
    {
        if (points == null || points.Length < 3) return;
        GameObject prefab = GetRandomPrefab(group, rng);
        if (prefab == null)
        {
            Debug.LogWarning("[IFNMG Prototype] Sem prefab de superfície para '" + group + "'; usando malha sólida em " + id);
            Color c = group == "soilSurface" ? SoilColor : (group == "pavementSurface" ? RoadColor : GrassColor);
            CreatePolygonSurface(id, points, baseY - 0.02f, GetMaterial("IFNMG_Fallback_" + group, c), parent, false);
            return;
        }

        float tile = GetTileSize(group, prefab);
        if (tile <= 0.01f) tile = 10f;

        Bounds2 b = GetBounds(points);
        Transform container = MakeGroup(id, parent);

        // Fase do ladrilhamento: alinhado ao mínimo do polígono, com passo = tamanho do tile (encosta sem vão).
        // Rejeita o quadrado do tile se nenhum canto estiver dentro do polígono (aproximação rápida).
        int placed = 0;
        for (float x = b.minX; x < b.maxX; x += tile)
        {
            for (float z = b.minY; z < b.maxY; z += tile)
            {
                Vector2 centro = new Vector2(x + tile * 0.5f, z + tile * 0.5f);
                if (!PointInPolygon(centro, points)) continue;

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, container) as GameObject;
                if (instance == null) continue;
                instance.name = id + "_Tile_" + placed.ToString("000") + "__" + prefab.name;
                instance.transform.SetParent(container, true);
                instance.transform.position = new Vector3(centro.x, baseY, centro.y);
                instance.transform.rotation = Quaternion.identity;
                PlacePrefabBottomAtGround(instance, baseY);
                placed++;
            }
        }
        Debug.Log("[IFNMG Prototype] Superfície '" + id + "' ladrilhada com " + placed + " tiles de " + tile.ToString("0.#") + " m (" + prefab.name + ").");
    }

    private static float GetTileSize(string group, GameObject prefab)
    {
        if (CurrentData != null && CurrentData.assetGroups != null)
        {
            foreach (AssetGroup g in CurrentData.assetGroups)
            {
                if (g.category == group && g.tileSizeMeters > 0.01f) return g.tileSizeMeters;
            }
        }
        Renderer[] rs = prefab.GetComponentsInChildren<Renderer>();
        if (rs != null && rs.Length > 0)
        {
            Bounds b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return Mathf.Max(b.size.x, b.size.z);
        }
        return 0f;
    }

    private static void SpawnAssetOrFallback(string group, Vector2 p, Transform parent, System.Random rng, HashSet<string> missing, string instanceName)
    {
        GameObject prefab = GetRandomPrefab(group, rng);
        if (prefab != null)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance != null)
            {
                instance.name = instanceName + "__" + prefab.name;
                float scale = group == "tree" ? 0.90f + (float)rng.NextDouble() * 0.20f :
                              group == "bush" ? 0.85f + (float)rng.NextDouble() * 0.25f :
                              group == "grassDetail" ? 0.85f + (float)rng.NextDouble() * 0.25f :
                              group == "rock" ? 0.85f + (float)rng.NextDouble() * 0.45f :
                              group == "mushroom" ? 0.85f + (float)rng.NextDouble() * 0.35f : 1f;
                instance.transform.SetParent(parent, true);
                instance.transform.position = new Vector3(p.x, 0f, p.y);
                instance.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                instance.transform.localScale = instance.transform.localScale * scale;
                PlacePrefabBottomAtGround(instance, 0f);
                Undo.RegisterCreatedObjectUndo(instance, "Instanciar asset do mapa protótipo");
                return;
            }
        }
        missing.Add(group);
        CreateFallback(group, p, parent, instanceName, (float)rng.NextDouble() * 360f, rng);
    }

    private static GameObject GetRandomPrefab(string group, System.Random rng)
    {
        if (CurrentData == null || CurrentData.assetGroups == null) return null;
        if (!PrefabCache.TryGetValue(group, out List<GameObject> found))
        {
            found = new List<GameObject>();
            AssetGroup match = null;
            foreach (AssetGroup g in CurrentData.assetGroups) if (g.category == group) { match = g; break; }
            if (match != null && match.paths != null)
            {
                foreach (string path in match.paths)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (go != null) found.Add(go);
                    else if (!string.IsNullOrEmpty(path) && WarnedMissingPaths.Add(path))
                        Debug.LogWarning("[IFNMG Prototype] Asset listado mas não encontrado: " + path);
                }
            }
            PrefabCache[group] = found;
        }
        if (found.Count == 0) return null;
        return found[rng.Next(found.Count)];
    }

    private static void PlacePrefabBottomAtGround(GameObject go, float groundY)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0) return;
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        float delta = groundY - b.min.y;
        go.transform.position += new Vector3(0f, delta, 0f);
    }

    private static void CreateFallback(string group, Vector2 p, Transform parent, string name, float yaw, System.Random rng)
    {
        GameObject root = new GameObject(name + "__Fallback_" + group);
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(p.x, 0f, p.y);
        root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        switch (group)
        {
            case "tree":
                PrimitiveChild(root.transform, "Tronco", PrimitiveType.Cylinder, new Vector3(0f, 1.4f, 0f), new Vector3(0.28f, 1.4f, 0.28f), GetMaterial("IFNMG_Tronco", new Color(0.29f, 0.18f, 0.10f, 1f)));
                PrimitiveChild(root.transform, "Copa", PrimitiveType.Sphere, new Vector3(0f, 3.1f, 0f), new Vector3(2.2f, 2.7f, 2.2f), GetMaterial("IFNMG_Copa", new Color(0.18f, 0.38f, 0.18f, 1f)));
                break;
            case "bush":
                PrimitiveChild(root.transform, "Arbusto", PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), new Vector3(1.15f, 0.9f, 1.15f), GetMaterial("IFNMG_Arbusto", new Color(0.23f, 0.42f, 0.20f, 1f)));
                break;
            case "grassDetail":
                PrimitiveChild(root.transform, "Tufo", PrimitiveType.Sphere, new Vector3(0f, 0.12f, 0f), new Vector3(0.32f, 0.20f, 0.30f), GetMaterial("IFNMG_Tufo", new Color(0.38f, 0.60f, 0.25f, 1f)));
                break;
            case "rock":
                PrimitiveChild(root.transform, "Rocha", PrimitiveType.Sphere, new Vector3(0f, 0.35f, 0f), new Vector3(0.65f, 0.55f, 0.72f), GetMaterial("IFNMG_Rocha", new Color(0.43f, 0.43f, 0.40f, 1f)));
                break;
            case "mushroom":
                PrimitiveChild(root.transform, "Haste", PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(0.08f, 0.12f, 0.08f), GetMaterial("IFNMG_Fungo_Haste", new Color(0.83f, 0.78f, 0.66f, 1f)));
                PrimitiveChild(root.transform, "Chapeu", PrimitiveType.Sphere, new Vector3(0f, 0.28f, 0f), new Vector3(0.24f, 0.12f, 0.24f), GetMaterial("IFNMG_Fungo_Chapeu", new Color(0.66f, 0.27f, 0.20f, 1f)));
                break;
        }
    }

    private static GameObject PrimitiveChild(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = material;
        Collider c = go.GetComponent<Collider>();
        if (c != null) UnityEngine.Object.DestroyImmediate(c);
        return go;
    }

    private static bool InsideAnyPolygon(Vector2 p, List<ZoneRuntime> zones, string category)
    {
        foreach (ZoneRuntime z in zones)
        {
            if (!z.data.enabled || z.data.category != category || z.data.geometryType != "polygon") continue;
            if (z.bounds.Contains(p) && PointInPolygon(p, z.worldPoints)) return true;
        }
        return false;
    }

    private static bool NearAnyRoad(Vector2 p, List<ZoneRuntime> zones, float extraMargin)
    {
        foreach (ZoneRuntime z in zones)
        {
            if (!z.data.enabled || z.data.category != "paved") continue;
            if (z.data.geometryType == "line")
            {
                float threshold = Mathf.Max(2f, z.data.widthMeters * 0.5f) + extraMargin;
                for (int i = 0; i < z.worldPoints.Length - 1; i++)
                    if (DistancePointSegment(p, z.worldPoints[i], z.worldPoints[i + 1]) < threshold) return true;
            }
            else if (z.data.geometryType == "polygon" && z.bounds.Contains(p) && PointInPolygon(p, z.worldPoints)) return true;
        }
        return false;
    }

    private static float DistancePointSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = ab.sqrMagnitude > 0.0001f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
        return Vector2.Distance(p, a + ab * t);
    }

    private static Bounds2 GetBounds(Vector2[] points)
    {
        Bounds2 b = new Bounds2 { minX = float.PositiveInfinity, maxX = float.NegativeInfinity, minY = float.PositiveInfinity, maxY = float.NegativeInfinity };
        foreach (Vector2 p in points) { b.minX = Mathf.Min(b.minX, p.x); b.maxX = Mathf.Max(b.maxX, p.x); b.minY = Mathf.Min(b.minY, p.y); b.maxY = Mathf.Max(b.maxY, p.y); }
        return b;
    }

    private static bool PointInPolygon(Vector2 p, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            Vector2 a = polygon[i], b = polygon[j];
            bool intersects = ((a.y > p.y) != (b.y > p.y)) && (p.x < (b.x - a.x) * (p.y - a.y) / ((b.y - a.y) == 0f ? 0.000001f : (b.y - a.y)) + a.x);
            if (intersects) inside = !inside;
        }
        return inside;
    }

    // Ear-clipping triangulation for simple polygons. Results are CCW in (X,Z) coordinates.
    private static List<int> Triangulate(Vector2[] points)
    {
        List<int> result = new List<int>();
        if (points == null || points.Length < 3) return result;
        List<int> verts = new List<int>();
        for (int i = 0; i < points.Length; i++) verts.Add(i);
        if (SignedArea(points) < 0f) verts.Reverse();
        int guard = 0;
        while (verts.Count > 3 && guard < points.Length * points.Length)
        {
            guard++;
            bool earFound = false;
            for (int i = 0; i < verts.Count; i++)
            {
                int prev = verts[(i + verts.Count - 1) % verts.Count];
                int curr = verts[i];
                int next = verts[(i + 1) % verts.Count];
                if (Cross(points[prev], points[curr], points[next]) <= 0.00001f) continue;
                bool contains = false;
                for (int j = 0; j < verts.Count; j++)
                {
                    int test = verts[j];
                    if (test == prev || test == curr || test == next) continue;
                    if (PointInTriangle(points[test], points[prev], points[curr], points[next])) { contains = true; break; }
                }
                if (contains) continue;
                result.Add(prev); result.Add(curr); result.Add(next);
                verts.RemoveAt(i);
                earFound = true;
                break;
            }
            if (!earFound) break;
        }
        if (verts.Count == 3) { result.Add(verts[0]); result.Add(verts[1]); result.Add(verts[2]); }
        return result;
    }

    private static float SignedArea(Vector2[] p)
    {
        float area = 0f;
        for (int i = 0; i < p.Length; i++) { Vector2 a = p[i], b = p[(i + 1) % p.Length]; area += a.x * b.y - b.x * a.y; }
        return area * 0.5f;
    }
    private static float Cross(Vector2 a, Vector2 b, Vector2 c) { return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x); }
    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float c1 = Cross(a, b, p), c2 = Cross(b, c, p), c3 = Cross(c, a, p);
        return (c1 >= -0.0001f && c2 >= -0.0001f && c3 >= -0.0001f) || (c1 <= 0.0001f && c2 <= 0.0001f && c3 <= 0.0001f);
    }

    private static Material GetMaterial(string materialName, Color color)
    {
        if (MaterialCache.TryGetValue(materialName, out Material cached) && cached != null) return cached;
        string path = MaterialsPath + "/" + materialName + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            mat = new Material(shader) { name = materialName };
            AssetDatabase.CreateAsset(mat, path);
        }
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        EditorUtility.SetDirty(mat);
        MaterialCache[materialName] = mat;
        return mat;
    }

    private static void SetRendererMaterial(GameObject go, Material material)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
    }

    private static void EnsureAssetFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureAssetFolder(parent);
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, folder);
    }

    private static void ReportOverlaps(List<ZoneRuntime> buildings, List<ZoneRuntime> paved)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            if (!buildings[i].data.enabled) continue;
            for (int j = i + 1; j < buildings.Count; j++)
            {
                if (!buildings[j].data.enabled) continue;
                if (BoundsOverlap(buildings[i].bounds, buildings[j].bounds))
                    Debug.LogWarning("[IFNMG Prototype] Bounds de edificações se sobrepõem ou se aproximam: " + buildings[i].data.id + " / " + buildings[j].data.id + ". Coordenadas preliminares; revisar depois.");
            }
            foreach (ZoneRuntime road in paved)
            {
                if (!road.data.enabled) continue;
                if (road.data.geometryType == "polygon" && BoundsOverlap(buildings[i].bounds, road.bounds))
                    Debug.LogWarning("[IFNMG Prototype] Edificação cruza bounds de pavimento; edifício terá prioridade visual: " + buildings[i].data.id + " / " + road.data.id);
            }
        }
    }
    private static bool BoundsOverlap(Bounds2 a, Bounds2 b)
    {
        return a.minX <= b.maxX && a.maxX >= b.minX && a.minY <= b.maxY && a.maxY >= b.minY;
    }

}
#endif

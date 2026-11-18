// ============================================================
//  Lead Environment Artist
//  Populate Campus Vegetation (Trees, 3D Grass, Rocks)
//  Cena-alvo : Campus.unity
//  Menu      : Tools > AAA Fix > Populate Campus Vegetation
// ============================================================

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PopulateCampusVegetation
{
    public struct AssetCandidate
    {
        public string name;
        public string path;
        public GameObject prefab;
        public int vertexCount;
    }

    [MenuItem("Tools/AAA Fix/Populate Campus Vegetation")]
    public static string Run()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Populate Campus Vegetation Report ===");

        // ── 0. Validação do Terreno ───────────────────────────────────
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            terrain = UnityEngine.Object.FindAnyObjectByType<Terrain>();
        }
        if (terrain == null)
        {
            return "ERRO: Nenhum Terrain ativo encontrado na cena.";
        }

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Populate Campus Vegetation");
        int undoGroup = Undo.GetCurrentGroup();

        // ── 1. Destravar Leitura do Mapa ──────────────────────────────
        string mapPath = "Assets/Mapas/mapa_classificado.png";
        if (!File.Exists(mapPath) && File.Exists("Assets/Imagens/mapa_classificado.png"))
        {
            if (!Directory.Exists("Assets/Mapas")) Directory.CreateDirectory("Assets/Mapas");
            File.Copy("Assets/Imagens/mapa_classificado.png", mapPath, true);
            AssetDatabase.ImportAsset(mapPath);
        }

        TextureImporter mapImporter = AssetImporter.GetAtPath(mapPath) as TextureImporter;
        if (mapImporter != null && (!mapImporter.isReadable || mapImporter.textureCompression != TextureImporterCompression.Uncompressed))
        {
            mapImporter.isReadable = true;
            mapImporter.textureCompression = TextureImporterCompression.Uncompressed;
            mapImporter.SaveAndReimport();
        }

        Texture2D mapTex = AssetDatabase.LoadAssetAtPath<Texture2D>(mapPath);
        if (mapTex == null)
        {
            return "ERRO: Não foi possível carregar a textura do mapa em " + mapPath;
        }

        // ── 2. Busca e Ranqueamento de Assets 3D por Contagem de Vértices
        Func<GameObject, int> calculateVertexCount = (go) =>
        {
            int count = 0;
            var mfs = go.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in mfs)
            {
                if (mf.sharedMesh != null) count += mf.sharedMesh.vertexCount;
            }
            var smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in smrs)
            {
                if (smr.sharedMesh != null) count += smr.sharedMesh.vertexCount;
            }
            return count;
        };

        Func<GameObject, float> getMaxDimension = (go) =>
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return 0f;
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }
            return Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
        };

        string[] guids = AssetDatabase.FindAssets("t:Prefab t:Model", new[] { "Assets" });

        var treeKeywords = new[] { "tree", "arvore", "pine", "oak", "palm", "bush_large" };
        var rockKeywords = new[] { "rock", "stone", "pedra", "boulder", "pebble" };
        var grassKeywords = new[] { "grass", "grama", "plant", "weed", "fern", "bush" };

        var treeCandidates = new List<AssetCandidate>();
        var rockCandidates = new List<AssetCandidate>();
        var grassCandidates = new List<AssetCandidate>();

        foreach (var guid in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (!p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;

            string n = Path.GetFileNameWithoutExtension(p).ToLower();

            // Filtros de exclusão para evitar construções, móveis urbanos e placas
            if (n.Contains("sign") || n.Contains("street") || n.Contains("bench") || n.Contains("lamp") ||
                n.Contains("bin") || n.Contains("wall") || n.Contains("rocket") || n.Contains("trunk") ||
                n.Contains("fallen") || n.Contains("potted") || n.Contains("building") || n.Contains("infrastructure") ||
                n.Contains("nuclear") || n.Contains("power") || n.Contains("house") || n.Contains("prop_"))
            {
                continue;
            }

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go == null) continue;

            int verts = calculateVertexCount(go);
            if (verts < 10) continue;

            float maxDim = getMaxDimension(go);

            // Classificação Árvores
            bool isTree = false;
            foreach (var kw in treeKeywords)
            {
                if (n.Contains(kw)) { isTree = true; break; }
            }
            if (isTree)
            {
                treeCandidates.Add(new AssetCandidate { name = go.name, path = p, prefab = go, vertexCount = verts });
                continue;
            }

            // Classificação Pedras / Rochas (exclui blocos gigantescos de terreno > 7m)
            bool isRock = false;
            foreach (var kw in rockKeywords)
            {
                if (n.Contains(kw)) { isRock = true; break; }
            }
            if (isRock && maxDim > 0.1f && maxDim <= 6.5f)
            {
                rockCandidates.Add(new AssetCandidate { name = go.name, path = p, prefab = go, vertexCount = verts });
                continue;
            }

            // Classificação Grama 3D / Arbustos Baixos (dimensão máxima <= 3.5m)
            bool isGrass = false;
            foreach (var kw in grassKeywords)
            {
                if (n.Contains(kw)) { isGrass = true; break; }
            }
            if (isGrass && maxDim > 0.1f && maxDim <= 3.5f)
            {
                grassCandidates.Add(new AssetCandidate { name = go.name, path = p, prefab = go, vertexCount = verts });
            }
        }

        // Ordenar decrescente por contagem de vértices
        treeCandidates.Sort((a, b) => b.vertexCount.CompareTo(a.vertexCount));
        rockCandidates.Sort((a, b) => b.vertexCount.CompareTo(a.vertexCount));
        grassCandidates.Sort((a, b) => b.vertexCount.CompareTo(a.vertexCount));

        // Fallback para Árvores caso o projeto não tivesse nenhuma
        if (treeCandidates.Count == 0)
        {
            string fallbackDir = "Assets/Prefabs/Environment";
            if (!Directory.Exists(fallbackDir)) Directory.CreateDirectory(fallbackDir);
            string fallbackPath = fallbackDir + "/Tree_Fallback.prefab";

            var fallbackGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fallbackGO.name = "Tree_Fallback";
            var foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.transform.SetParent(fallbackGO.transform, false);
            foliage.transform.localPosition = new Vector3(0, 1.5f, 0);
            foliage.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(fallbackGO, fallbackPath);
            GameObject.DestroyImmediate(fallbackGO);
            treeCandidates.Add(new AssetCandidate
            {
                name = "Tree_Fallback",
                path = fallbackPath,
                prefab = savedPrefab,
                vertexCount = calculateVertexCount(savedPrefab)
            });
            report.AppendLine("[Fallback] Prefab de árvore estilizada criado em Assets/Prefabs/Environment/Tree_Fallback.prefab");
        }

        // Seleção dos Melhores Assets (Top Candidates)
        int numTreesToUse = Mathf.Min(5, treeCandidates.Count);
        var selectedTrees = treeCandidates.GetRange(0, numTreesToUse);

        int numRocksToUse = Mathf.Min(4, rockCandidates.Count);
        var selectedRocks = rockCandidates.GetRange(0, numRocksToUse);

        int numGrassToUse = Mathf.Min(4, grassCandidates.Count);
        var selectedGrass = grassCandidates.GetRange(0, numGrassToUse);

        report.AppendLine("\n[Passo 1] Assets 3D Mais Detalhados Selecionados:");
        report.AppendLine("--- ÁRVORES ---");
        foreach (var t in selectedTrees) report.AppendLine($"- {t.name} ({t.vertexCount:N0} vértices) @ {t.path}");

        report.AppendLine("--- PEDRAS ---");
        foreach (var r in selectedRocks) report.AppendLine($"- {r.name} ({r.vertexCount:N0} vértices) @ {r.path}");

        report.AppendLine("--- GRAMA 3D / ARBUSTOS ---");
        foreach (var g in selectedGrass) report.AppendLine($"- {g.name} ({g.vertexCount:N0} vértices) @ {g.path}");

        // ── 3. Registro no TerrainData ────────────────────────────────
        Undo.RegisterCompleteObjectUndo(terrainData, "Register Vegetation Prototypes");

        // Protótipos de Árvore
        var treePrototypes = new List<TreePrototype>();
        foreach (var t in selectedTrees)
        {
            var tp = new TreePrototype();
            tp.prefab = t.prefab;
            treePrototypes.Add(tp);
        }
        terrainData.treePrototypes = treePrototypes.ToArray();

        // Protótipos de Detalhe (Grama e Pedras)
        var detailPrototypes = new List<DetailPrototype>();
        foreach (var g in selectedGrass)
        {
            var dp = new DetailPrototype();
            dp.prototype = g.prefab;
            dp.usePrototypeMesh = true;
            dp.renderMode = DetailRenderMode.Grass;
            detailPrototypes.Add(dp);
        }
        foreach (var r in selectedRocks)
        {
            var dp = new DetailPrototype();
            dp.prototype = r.prefab;
            dp.usePrototypeMesh = true;
            dp.renderMode = DetailRenderMode.VertexLit;
            detailPrototypes.Add(dp);
        }
        terrainData.detailPrototypes = detailPrototypes.ToArray();
        report.AppendLine($"\n[Passo 2] Protótipos Registrados: {treePrototypes.Count} árvores e {detailPrototypes.Count} detalhes no TerrainData.");

        // ── 4. Distribuição Espacial Guiada pelo Mapa e Splatmap ────────
        int mw = mapTex.width;
        int mh = mapTex.height;
        int aw = terrainData.alphamapWidth;
        int ah = terrainData.alphamapHeight;
        float[,,] alphamaps = terrainData.GetAlphamaps(0, 0, aw, ah);

        var treeInstances = new List<TreeInstance>();

        // Limpeza de containers anteriores de props de vegetação
        GameObject envRoot = GameObject.Find("Terrain_Vegetation_Props");
        if (envRoot != null)
        {
            Undo.DestroyObjectImmediate(envRoot);
        }
        envRoot = new GameObject("Terrain_Vegetation_Props");
        Undo.RegisterCreatedObjectUndo(envRoot, "Create Vegetation Props Container");

        GameObject rocksContainer = new GameObject("Pedras");
        rocksContainer.transform.SetParent(envRoot.transform, false);

        GameObject grassContainer = new GameObject("Grama_3D");
        grassContainer.transform.SetParent(envRoot.transform, false);

        // ── 4.1 Árvores (Amostragem com passo de 6 pixels + Jitter) ────
        for (int py = 0; py < mh; py += 6)
        {
            for (int px = 0; px < mw; px += 6)
            {
                float jx = px + UnityEngine.Random.Range(-2.5f, 2.5f);
                float jy = py + UnityEngine.Random.Range(-2.5f, 2.5f);
                float normX = Mathf.Clamp01(jx / (mw - 1));
                float normZ = Mathf.Clamp01(jy / (mh - 1));

                int ax = Mathf.Clamp((int)(normX * (aw - 1)), 0, aw - 1);
                int ay = Mathf.Clamp((int)(normZ * (ah - 1)), 0, ah - 1);

                // Regra Crítica: Descartar se Asfalto (Canal 1) > 0.15f
                if (alphamaps[ay, ax, 1] > 0.15f) continue;
                if (alphamaps[ay, ax, 0] < 0.30f) continue;

                Color c = mapTex.GetPixelBilinear(normX, normZ);
                Color.RGBToHSV(c, out float H, out float S, out float V);

                // Áreas Verde-Escuras (Mata Densa)
                if (H > 0.18f && H <= 0.48f && S >= 0.18f && V < 0.42f)
                {
                    float scale = UnityEngine.Random.Range(0.85f, 1.45f);
                    var ti = new TreeInstance
                    {
                        position = new Vector3(normX, 0f, normZ),
                        widthScale = scale,
                        heightScale = scale,
                        rotation = UnityEngine.Random.Range(0f, Mathf.PI * 2f),
                        color = Color.white,
                        lightmapColor = Color.white,
                        prototypeIndex = UnityEngine.Random.Range(0, treePrototypes.Count)
                    };
                    treeInstances.Add(ti);
                }
            }
        }

        // ── 4.2 Pedras e Tufos de Grama 3D (Passo de 3 pixels + Jitter) ─
        int rocksGenerated = 0;
        int grassGenerated = 0;

        for (int py = 0; py < mh; py += 3)
        {
            for (int px = 0; px < mw; px += 3)
            {
                float jx = px + UnityEngine.Random.Range(-1.2f, 1.2f);
                float jy = py + UnityEngine.Random.Range(-1.2f, 1.2f);
                float normX = Mathf.Clamp01(jx / (mw - 1));
                float normZ = Mathf.Clamp01(jy / (mh - 1));

                int ax = Mathf.Clamp((int)(normX * (aw - 1)), 0, aw - 1);
                int ay = Mathf.Clamp((int)(normZ * (ah - 1)), 0, ah - 1);

                // Regra Crítica: Descartar se Asfalto > 0.15f
                if (alphamaps[ay, ax, 1] > 0.15f) continue;
                if (alphamaps[ay, ax, 0] < 0.30f) continue;

                Color c = mapTex.GetPixelBilinear(normX, normZ);
                Color.RGBToHSV(c, out float H, out float S, out float V);

                // Áreas Verde-Claras (Gramado Aberto)
                if (H > 0.18f && H <= 0.48f && S >= 0.18f && V >= 0.42f)
                {
                    float worldX = terrainPos.x + normX * terrainSize.x;
                    float worldZ = terrainPos.z + normZ * terrainSize.z;
                    Vector3 worldPos = new Vector3(worldX, 0f, worldZ);
                    float worldY = terrain.SampleHeight(worldPos) + terrainPos.y;
                    worldPos.y = worldY;

                    Vector3 normal = terrainData.GetInterpolatedNormal(normX, normZ);
                    Quaternion baseRot = Quaternion.FromToRotation(Vector3.up, normal);

                    float roll = UnityEngine.Random.value;

                    // Pedras Ocasionais (baixa densidade: ~2% dos pontos de gramado aberto)
                    if (roll < 0.02f && selectedRocks.Count > 0)
                    {
                        var rockAsset = selectedRocks[UnityEngine.Random.Range(0, selectedRocks.Count)];
                        var rockGO = UnityEngine.Object.Instantiate(rockAsset.prefab, rocksContainer.transform);
                        
                        // Assenta a pedra levemente no solo
                        rockGO.transform.position = new Vector3(worldPos.x, worldPos.y - 0.04f, worldPos.z);
                        rockGO.transform.rotation = baseRot * Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                        float rScale = UnityEngine.Random.Range(0.7f, 1.25f);
                        rockGO.transform.localScale = Vector3.one * rScale;
                        rocksGenerated++;
                    }
                    // Tufos de Grama 3D / Arbustos (densidade média: ~5.5% dos pontos)
                    else if (roll >= 0.02f && roll < 0.075f && selectedGrass.Count > 0)
                    {
                        var grassAsset = selectedGrass[UnityEngine.Random.Range(0, selectedGrass.Count)];
                        var grassGO = UnityEngine.Object.Instantiate(grassAsset.prefab, grassContainer.transform);

                        grassGO.transform.position = worldPos;
                        grassGO.transform.rotation = baseRot * Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                        float gScale = UnityEngine.Random.Range(0.8f, 1.3f);
                        grassGO.transform.localScale = Vector3.one * gScale;
                        grassGenerated++;
                    }
                }
            }
        }

        // ── 5. Aplicação no Terreno e Save ─────────────────────────────
        terrainData.SetTreeInstances(treeInstances.ToArray(), true);
        terrain.Flush();

        EditorUtility.SetDirty(terrainData);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        report.AppendLine("\n[Passo 3 e 4] Instâncias Geradas e Aplicadas:");
        report.AppendLine($"- Árvores (TreeInstances no Terreno)   : {treeInstances.Count:N0} instâncias");
        report.AppendLine($"- Pedras 3D (alinhadas à normal)       : {rocksGenerated:N0} instâncias");
        report.AppendLine($"- Tufos de Grama 3D / Arbustos (normal): {grassGenerated:N0} instâncias");
        report.AppendLine("\n[Sucesso] Undo registrado, terrain.Flush() executado e cena salva!");

        Debug.Log(report.ToString());
        return report.ToString();
    }
}

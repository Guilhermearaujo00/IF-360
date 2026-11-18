// ============================================================
//  Lead Environment Artist & Tool Programmer
//  Procedural Terrain Splatmap Painter
//  Cena-alvo : Campus.unity
//  Menu      : Tools > AAA Fix > Generate Procedural Splatmap
// ============================================================

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class ProceduralTerrainPainter
{
    public struct LayerResult
    {
        public string category;
        public Texture2D diffuse;
        public Texture2D normal;
        public string diffusePath;
        public string normalPath;
        public int width;
        public int height;
        public bool isFallback;
    }

    [MenuItem("Tools/AAA Fix/Generate Procedural Splatmap")]
    public static string Run()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Procedural Terrain Splatmap Generation ===");

        // ── 1. Destravar Leitura do Mapa ───────────────────────────
        string mapPath = "Assets/Mapas/mapa_classificado.png";
        if (!File.Exists(mapPath))
        {
            if (File.Exists("Assets/Imagens/mapa_classificado.png"))
            {
                if (!Directory.Exists("Assets/Mapas")) Directory.CreateDirectory("Assets/Mapas");
                File.Copy("Assets/Imagens/mapa_classificado.png", mapPath, true);
                AssetDatabase.ImportAsset(mapPath);
            }
            else
            {
                return "ERRO: Mapa de satélite não encontrado em 'Assets/Mapas/mapa_classificado.png'.";
            }
        }

        TextureImporter mapImporter = AssetImporter.GetAtPath(mapPath) as TextureImporter;
        if (mapImporter != null)
        {
            bool needReimport = false;
            if (!mapImporter.isReadable)
            {
                mapImporter.isReadable = true;
                needReimport = true;
            }
            if (mapImporter.textureCompression != TextureImporterCompression.Uncompressed)
            {
                mapImporter.textureCompression = TextureImporterCompression.Uncompressed;
                needReimport = true;
            }
            if (needReimport)
            {
                mapImporter.SaveAndReimport();
                report.AppendLine("[Passo 1] Importer do mapa reconfigurado (isReadable=true, Uncompressed).");
            }
            else
            {
                report.AppendLine("[Passo 1] Importer do mapa verificado (isReadable=true, Uncompressed).");
            }
        }

        Texture2D mapTex = AssetDatabase.LoadAssetAtPath<Texture2D>(mapPath);
        if (mapTex == null)
        {
            return "ERRO: Não foi possível carregar Texture2D em " + mapPath;
        }

        // ── 2. Busca Inteligente de Texturas de Alta Resolução ──────
        if (!AssetDatabase.IsValidFolder("Assets/TerrainLayers"))
        {
            AssetDatabase.CreateFolder("Assets", "TerrainLayers");
        }

        string[] allGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
        var allTextures = new List<Texture2D>();
        var pathMap = new Dictionary<Texture2D, string>();

        foreach (var guid in allGuids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.StartsWith("Assets/Mapas") || p.Contains("_Fallback")) continue;
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            if (t != null)
            {
                allTextures.Add(t);
                pathMap[t] = p;
            }
        }

        // Função auxiliar para encontrar Normal Map companheiro
        Func<Texture2D, Texture2D> findCompanionNormal = (diffuseTex) =>
        {
            if (diffuseTex == null) return null;
            string diffPath = pathMap[diffuseTex];
            string diffDir = Path.GetDirectoryName(diffPath);
            string diffBase = Path.GetFileNameWithoutExtension(diffPath).ToLower();
            string cleanStem = diffBase.Replace("diff", "").Replace("color", "").Replace("_bc", "").Trim('_', '-', '.');

            foreach (var tex in allTextures)
            {
                string p = pathMap[tex];
                string nName = Path.GetFileNameWithoutExtension(p).ToLower();
                string nDir = Path.GetDirectoryName(p);

                var imp = AssetImporter.GetAtPath(p) as TextureImporter;
                bool isNormalType = (imp != null && imp.textureType == TextureImporterType.NormalMap) ||
                                    nName.Contains("normal") || nName.Contains("_nor") || nName.EndsWith("_n") || nName.Contains("_n_");

                if (!isNormalType) continue;

                // Caso 1: Mesmo diretório e stem coincide
                if (nDir == diffDir && (nName.Contains(cleanStem) || (cleanStem.Length > 4 && nName.StartsWith(cleanStem.Substring(0, 4)))))
                {
                    if (imp != null && imp.textureType != TextureImporterType.NormalMap)
                    {
                        imp.textureType = TextureImporterType.NormalMap;
                        imp.SaveAndReimport();
                    }
                    return tex;
                }

                // Caso 2: TerrainLayers folder matching basepolygon
                if (diffDir.Contains("TerrainLayers") && nDir.Contains("TerrainLayers") && nName.Contains("basepolygon"))
                {
                    if (imp != null && imp.textureType != TextureImporterType.NormalMap)
                    {
                        imp.textureType = TextureImporterType.NormalMap;
                        imp.SaveAndReimport();
                    }
                    return tex;
                }
            }
            return null;
        };

        Func<string, string[], string, LayerResult> findCategory = (categoryName, keywords, fallbackHex) =>
        {
            Texture2D bestDiffuse = null;
            Texture2D bestNormal = null;
            int maxResolution = 0;

            foreach (var tex in allTextures)
            {
                string p = pathMap[tex];
                string lowerName = Path.GetFileNameWithoutExtension(p).ToLower();
                string lowerPath = p.ToLower();

                // Ignora mapas auxiliares e folhas/cutouts isolados
                if (lowerPath.Contains("alphas") || lowerPath.Contains("leafs") ||
                    lowerName.Contains("_normal") || lowerName.Contains("_nor") ||
                    lowerName.Contains("_n_") || lowerName.EndsWith("_n") ||
                    lowerName.Contains("sign") || lowerName.Contains("rough") ||
                    lowerName.Contains("disp") || lowerName.Contains("spec") ||
                    lowerName.Contains("metallic") || lowerName.Contains("ao") ||
                    lowerName.Contains("ambient"))
                {
                    continue;
                }

                bool matches = false;
                foreach (var kw in keywords)
                {
                    if (lowerName.Contains(kw.ToLower()))
                    {
                        matches = true;
                        break;
                    }
                }

                if (matches)
                {
                    int res = tex.width * tex.height;
                    Texture2D candidateNormal = findCompanionNormal(tex);

                    bool isBetter = false;
                    if (res > maxResolution)
                    {
                        isBetter = true;
                    }
                    else if (res == maxResolution)
                    {
                        // Desempate: prioriza quem possui Normal Map real
                        if (bestNormal == null && candidateNormal != null)
                        {
                            isBetter = true;
                        }
                    }

                    if (isBetter)
                    {
                        maxResolution = res;
                        bestDiffuse = tex;
                        bestNormal = candidateNormal;
                    }
                }
            }

            var resObj = new LayerResult { category = categoryName };
            if (bestDiffuse != null)
            {
                resObj.diffuse = bestDiffuse;
                resObj.normal = bestNormal;
                resObj.diffusePath = pathMap[bestDiffuse];
                resObj.normalPath = bestNormal != null ? pathMap[bestNormal] : null;
                resObj.width = bestDiffuse.width;
                resObj.height = bestDiffuse.height;
                resObj.isFallback = false;
            }
            else
            {
                // Fallback procedural 64x64
                Color col;
                ColorUtility.TryParseHtmlString(fallbackHex, out col);
                var fallback = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                var colors = new Color[64 * 64];
                for (int i = 0; i < colors.Length; i++) colors[i] = col;
                fallback.SetPixels(colors);
                fallback.Apply();
                string fallbackPath = "Assets/TerrainLayers/" + categoryName + "_Fallback.png";
                File.WriteAllBytes(fallbackPath, fallback.EncodeToPNG());
                AssetDatabase.ImportAsset(fallbackPath);
                
                resObj.diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(fallbackPath);
                resObj.normal = null;
                resObj.diffusePath = fallbackPath;
                resObj.normalPath = null;
                resObj.width = 64;
                resObj.height = 64;
                resObj.isFallback = true;
            }
            return resObj;
        };

        var grassRes = findCategory("Grama", new[] { "grass", "grama", "lawn", "turf", "foliage_ground" }, "#4E8A3E");
        var asphaltRes = findCategory("Asfalto", new[] { "asphalt", "asfalto", "road", "concrete", "pavement", "street" }, "#3A3D42");
        var dirtRes = findCategory("Terra", new[] { "dirt", "terra", "soil", "sand", "mud", "ground" }, "#B88A58");

        report.AppendLine("\n[Passo 2] Texturas Selecionadas no Projeto:");
        report.AppendLine($"- Grama  : {grassRes.diffuse.name} ({grassRes.width}x{grassRes.height}) | Normal: {(grassRes.normal != null ? grassRes.normal.name : "Nenhum")}");
        report.AppendLine($"- Asfalto: {asphaltRes.diffuse.name} ({asphaltRes.width}x{asphaltRes.height}) | Normal: {(asphaltRes.normal != null ? asphaltRes.normal.name : "Nenhum")}");
        report.AppendLine($"- Terra  : {dirtRes.diffuse.name} ({dirtRes.width}x{dirtRes.height}) | Normal: {(dirtRes.normal != null ? dirtRes.normal.name : "Nenhum")}");

        // ── 3. Criação dos TerrainLayers Persistentes ───────────────
        Func<string, LayerResult, TerrainLayer> createOrUpdateLayer = (assetPath, res) =>
        {
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(assetPath);
            bool isNew = false;
            if (layer == null)
            {
                layer = new TerrainLayer();
                isNew = true;
            }
            layer.diffuseTexture = res.diffuse;
            if (res.normal != null)
            {
                layer.normalMapTexture = res.normal;
            }
            else
            {
                layer.normalMapTexture = null;
            }
            layer.tileSize = new Vector2(10f, 10f);

            if (isNew)
            {
                AssetDatabase.CreateAsset(layer, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(layer);
            }
            return layer;
        };

        var layerGrama = createOrUpdateLayer("Assets/TerrainLayers/Layer_Grama.terrainlayer", grassRes);
        var layerAsfalto = createOrUpdateLayer("Assets/TerrainLayers/Layer_Asfalto.terrainlayer", asphaltRes);
        var layerTerra = createOrUpdateLayer("Assets/TerrainLayers/Layer_Terra.terrainlayer", dirtRes);
        AssetDatabase.SaveAssets();

        // Garantir que Terrain.activeTerrain exista na cena
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            terrain = UnityEngine.Object.FindAnyObjectByType<Terrain>();
        }

        if (terrain == null)
        {
            string tdPath = "Assets/TerrainLayers/Campus_TerrainData.asset";
            TerrainData td = AssetDatabase.LoadAssetAtPath<TerrainData>(tdPath);
            if (td == null)
            {
                td = new TerrainData();
                td.heightmapResolution = 513;
                td.size = new Vector3(1288.75f, 50f, 1143.39f);
                td.alphamapResolution = 1024;
                AssetDatabase.CreateAsset(td, tdPath);
                AssetDatabase.SaveAssets();
            }

            GameObject terrainGO = Terrain.CreateTerrainGameObject(td);
            terrainGO.name = "Terrain_Campus";
            terrainGO.transform.position = new Vector3(-286.59f, -0.10f, -907.48f);
            terrain = terrainGO.GetComponent<Terrain>();
            Undo.RegisterCreatedObjectUndo(terrainGO, "Create Terrain_Campus");
            report.AppendLine("\n[Passo 3] Terrain_Campus criado e posicionado com sucesso.");
        }

        TerrainData terrainData = terrain.terrainData;
        Undo.RegisterCompleteObjectUndo(terrainData, "Apply Detailed Splatmap");

        terrainData.terrainLayers = new TerrainLayer[] { layerGrama, layerAsfalto, layerTerra };
        report.AppendLine("[Passo 3] 3 TerrainLayers persistentes atribuídos a terrainData.terrainLayers.");

        // ── 4. Amostragem e Normalização do Splatmap ────────────────
        int w = terrainData.alphamapWidth;
        int h = terrainData.alphamapHeight;
        float[,,] splatmapData = new float[h, w, 3];

        int asphaltPixels = 0, dirtPixels = 0, grassPixels = 0;

        for (int y = 0; y < h; y++)
        {
            float v = (float)y / (h - 1);
            for (int x = 0; x < w; x++)
            {
                float u = (float)x / (w - 1);
                Color c = mapTex.GetPixelBilinear(u, v);

                Color.RGBToHSV(c, out float H, out float S, out float V);

                float wGrass = 0f;
                float wAsphalt = 0f;
                float wDirt = 0f;

                // Regras de Classificação:
                // 1. Cinza / Preto / Pouca Saturação -> Canal 1 (Asfalto)
                if (S < 0.18f || V < 0.18f)
                {
                    wAsphalt = 1f;
                    asphaltPixels++;
                }
                // 2. Amarelo / Laranja / Terroso -> Canal 2 (Terra)
                else if (H >= 0.05f && H <= 0.18f)
                {
                    wDirt = 1f;
                    dirtPixels++;
                }
                // 3. Verde -> Canal 0 (Grama)
                else if ((H > 0.18f && H <= 0.48f) || (c.g > c.r && c.g > c.b))
                {
                    wGrass = 1f;
                    grassPixels++;
                }
                // 4. Fallback
                else
                {
                    if (c.g > c.r && c.g > c.b) { wGrass = 1f; grassPixels++; }
                    else if (c.r > c.b) { wDirt = 1f; dirtPixels++; }
                    else { wAsphalt = 1f; asphaltPixels++; }
                }

                // Normalização dos Pesos (soma = 1.0f)
                float total = wGrass + wAsphalt + wDirt;
                if (total > 0.0001f)
                {
                    splatmapData[y, x, 0] = wGrass / total;
                    splatmapData[y, x, 1] = wAsphalt / total;
                    splatmapData[y, x, 2] = wDirt / total;
                }
                else
                {
                    splatmapData[y, x, 0] = 1f;
                    splatmapData[y, x, 1] = 0f;
                    splatmapData[y, x, 2] = 0f;
                }
            }
        }

        report.AppendLine($"\n[Passo 4] Splatmap processado ({w}x{h} amostras):");
        int totalPx = w * h;
        report.AppendLine($"- Grama (Canal 0) : {grassPixels} px ({grassPixels * 100f / totalPx:F1}%)");
        report.AppendLine($"- Asfalto (Canal 1): {asphaltPixels} px ({asphaltPixels * 100f / totalPx:F1}%)");
        report.AppendLine($"- Terra (Canal 2)  : {dirtPixels} px ({dirtPixels * 100f / totalPx:F1}%)");

        // ── 5. Aplicação e Salvamento ──────────────────────────────
        terrainData.SetAlphamaps(0, 0, splatmapData);
        terrain.Flush();
        EditorUtility.SetDirty(terrainData);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        report.AppendLine("\n[Passo 5] SetAlphamaps aplicado com sucesso e cena salva!");
        Debug.Log(report.ToString());
        return report.ToString();
    }
}

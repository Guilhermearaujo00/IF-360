using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Auditoria SOMENTE LEITURA da cena Campus e dos assets candidatos (Fases 1-2 do prompt v3).
/// Não altera nem salva a cena. Gera:
///   Docs/MontagemCena/AUDITORIA_CENA_ASSETS.md
///   Docs/MontagemCena/auditoria_assets.json
///
/// Menu: Tools/IFNMG/Ambientação/Auditar Cena e Assets
/// Batch: -executeMethod AuditarIFNMG.AuditarBatch
/// </summary>
public static class AuditarIFNMG
{
    private const string MENU = "Tools/IFNMG/Ambientação/Auditar Cena e Assets";
    private const string CENA = "Assets/Scenes/Campus.unity";
    private const string SAIDA_MD = "Docs/MontagemCena/AUDITORIA_CENA_ASSETS.md";
    private const string SAIDA_JSON = "Docs/MontagemCena/auditoria_assets.json";
    private const string RAIZ_GERADA = "Mapa_IFNMG";

    private static readonly (string nome, string path)[] Candidatos =
    {
        ("Road Concrete Tile", "Assets/SimplePoly City - Low Poly Assets/Prefab/Roads/Road Concrete Tile.prefab"),
        ("Road Concrete Tile Small", "Assets/SimplePoly City - Low Poly Assets/Prefab/Roads/Road Concrete Tile Small.prefab"),
        ("Road Sidewalk", "Assets/SimplePoly City - Low Poly Assets/Prefab/Roads/Road Sidewalk.prefab"),
        ("SM_LPC_Ground_WalkPavingPlain_01", "Assets/LowPolyCityFree/Prefabs/05_Ground_Paving/SM_LPC_Ground_WalkPavingPlain_01.prefab"),
        ("SM_LPC_Ground_WalkCornerExternal_01", "Assets/LowPolyCityFree/Prefabs/05_Ground_Paving/SM_LPC_Ground_WalkCornerExternal_01.prefab"),
        ("SM_LPC_Ground_GroundGrassFlat_01", "Assets/LowPolyCityFree/Prefabs/05_Ground_Paving/SM_LPC_Ground_GroundGrassFlat_01.prefab"),
        ("SM_LPC_Park_TreeBroadleafMature_01", "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_TreeBroadleafMature_01.prefab"),
        ("SM_LPC_Park_TreeBroadleafSpreading_01", "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_TreeBroadleafSpreading_01.prefab"),
        ("SM_LPC_Park_TreeBroadleafYoung_01", "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_TreeBroadleafYoung_01.prefab"),
        ("SM_LPC_Park_BushRoundMedium_01", "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_BushRoundMedium_01.prefab"),
        ("SM_LPC_Park_HedgeRunLow_01", "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_HedgeRunLow_01.prefab"),
        ("SM_LPC_Park_FlowerBedRound_01", "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_FlowerBedRound_01.prefab"),
        ("SM_LPC_Park_GrassTuftClump_01", "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_GrassTuftClump_01.prefab"),
        ("SM_Grass_01", "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Grass_01.prefab"),
        ("SM_FoliageBush_01", "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_FoliageBush_01.prefab"),
        ("SM_Flower_01", "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Flower_01.prefab"),
        ("HardRock_01", "Assets/Pandazole_Ultimate_Pack/Pandazole Nature Environment Pack/Prefabs/HardRock_01.prefab"),
        ("SoftRock_01", "Assets/Pandazole_Ultimate_Pack/Pandazole Nature Environment Pack/Prefabs/SoftRock_01.prefab"),
        ("SM_Mushroom_01", "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Mushroom_01.prefab"),
        ("SM_Tree_Green", "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Tree_Green.prefab"),
        ("SM_LPC_Street_BenchStreetSlat_01", "Assets/LowPolyCityFree/Prefabs/06_Street_Furniture/SM_LPC_Street_BenchStreetSlat_01.prefab"),
        ("SM_LPC_Street_LampStreetColumn_01", "Assets/LowPolyCityFree/Prefabs/06_Street_Furniture/SM_LPC_Street_LampStreetColumn_01.prefab"),
        ("SM_LPC_Street_StreetBinRound_01", "Assets/LowPolyCityFree/Prefabs/06_Street_Furniture/SM_LPC_Street_StreetBinRound_01.prefab"),
    };

    private static readonly string[] ArquivosReferencia =
    {
        "Docs/PREFABS_TABELA.md",
        "Docs/MontagemCena/IFNMG_Selecao_Assets_Ambientacao.md",
        "Docs/MontagemCena/IFNMG_Selecao_Assets_Ambientacao.json",
        "Docs/MontagemCena/IFNMG_Januaria_zonas_para_Unity.json",
        "Docs/MontagemCena/IFNMG_Plano_Espacial_Ambientacao.md",
        "Docs/MontagemCena/Prompt_OpenCode_Ambientacao_IFNMG_v3.md",
    };

    [MenuItem(MENU)]
    public static void AuditarMenu() => Auditar(SceneManager.GetActiveScene());

    public static void AuditarBatch()
    {
        Scene cena = EditorSceneManager.OpenScene(CENA, OpenSceneMode.Single);
        Auditar(cena);
    }

    private static void Auditar(Scene cena)
    {
        var sb = new StringBuilder();
        var linhasJson = new List<AssetInfo>();
        string raizProjeto = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        sb.AppendLine("# Auditoria da cena e dos assets (prompt v3)");
        sb.AppendLine();
        sb.AppendLine($"- Data: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"- Cena analisada: `{cena.path}` ({cena.name})");
        sb.AppendLine($"- Commit atual: `{GitHead(raizProjeto)}`");
        sb.AppendLine($"- Backup criado: `%TEMP%/opencode/backup_20261020/Campus.unity`");
        sb.AppendLine("- Modo: somente leitura; a cena **não** foi salva nem alterada.");
        sb.AppendLine("- Regra v3: nenhuma imagem foi interpretada visualmente. Tudo abaixo vem de hierarquia, transforms, bounds, metadados e arquivos de texto.");
        sb.AppendLine();

        // ------------------------------------------------------ disponibilidade
        sb.AppendLine("## 1. Arquivos de referência");
        sb.AppendLine();
        sb.AppendLine("| Arquivo | Status |");
        sb.AppendLine("|---|---|");
        foreach (var f in ArquivosReferencia)
            sb.AppendLine($"| `{f}` | {(File.Exists(Path.Combine(raizProjeto, f)) ? "presente" : "**AUSENTE**")} |");
        sb.AppendLine();
        sb.AppendLine("> O mapa antigo (`mapa_ifnmg.json`, IDs E/R/T/V, 0,516 m/px) é tratado como **obsoleto**; ver seção 7.");
        sb.AppendLine();

        // ------------------------------------------------------ cenas
        sb.AppendLine("## 2. Cenas do projeto");
        sb.AppendLine();
        sb.AppendLine("| Cena em Build Settings | Habilitada |");
        sb.AppendLine("|---|---|");
        foreach (var s in EditorBuildSettings.scenes)
            sb.AppendLine($"| `{s.path}` | {(s.enabled ? "sim" : "não")} |");
        var cenas = AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).OrderBy(x => x).ToList();
        sb.AppendLine();
        sb.AppendLine($"Cenas encontradas em `Assets/`: **{cenas.Count}**.");
        if (cenas.Count > 0) sb.AppendLine("- " + string.Join("\n- ", cenas));
        sb.AppendLine();

        // ------------------------------------------------------ eixos e limites
        sb.AppendLine("## 3. Eixos, escala e limites do mundo");
        sb.AppendLine();
        bool temCenaBounds = BoundsDaCena(cena, out Bounds cb);
        sb.AppendLine("- Convenção da Unity: **Y para cima**, X/Z no plano horizontal. O v3 não assume qual eixo é o norte; nenhuma rotação de mapa foi aplicada.");
        sb.AppendLine("- Escala assumida: **1 unidade Unity = 1 metro** (a confirmar contra um marco real da cena).");
        if (temCenaBounds)
        {
            sb.AppendLine($"- Limites dos renderers da cena: X [{cb.min.x:0.00}, {cb.max.x:0.00}], Y [{cb.min.y:0.00}, {cb.max.y:0.00}], Z [{cb.min.z:0.00}, {cb.max.z:0.00}] m.");
            sb.AppendLine($"- Dimensões: {cb.size.x:0.0} × {cb.size.y:0.0} × {cb.size.z:0.0} m.");
        }
        else sb.AppendLine("- Não foi possível calcular bounds da cena (sem renderers).");

        var terrenos = Terrain.activeTerrains;
        sb.AppendLine($"- Componentes `Terrain` ativos: **{terrenos.Length}**.");
        foreach (var t in terrenos)
            sb.AppendLine($"  - `{t.name}` pos ({t.transform.position.x:0.0}, {t.transform.position.y:0.0}, {t.transform.position.z:0.0}), tamanho {t.terrainData.size.x:0.0} × {t.terrainData.size.y:0.0} × {t.terrainData.size.z:0.0} m.");
        sb.AppendLine();

        // ------------------------------------------------------ hierarquia
        sb.AppendLine("## 4. Hierarquia e áreas ocupadas");
        sb.AppendLine();
        var roots = cena.GetRootGameObjects();
        sb.AppendLine($"Objetos raiz: **{roots.Length}**.");
        sb.AppendLine();
        sb.AppendLine("| Raiz | Filhos diretos | Renderers | Colliders | PrefabInstances | Bounds X×Y×Z (m) | Y mín–máx |");
        sb.AppendLine("|---|---:|---:|---:|---:|---|---|");
        foreach (var r in roots.OrderBy(x => x.name))
        {
            int filhos = r.transform.childCount;
            int rend = r.GetComponentsInChildren<Renderer>(true).Count(ContaRenderer);
            int col = r.GetComponentsInChildren<Collider>(true).Length;
            int pref = r.GetComponentsInChildren<Transform>(true).Count(t => PrefabUtility.IsPartOfPrefabInstance(t.gameObject));
            bool tem = BoundsDe(r, out Bounds b);
            string dims = tem ? $"{b.size.x:0.0} × {b.size.y:0.0} × {b.size.z:0.0}" : "—";
            string ys = tem ? $"{b.min.y:0.00} – {b.max.y:0.00}" : "—";
            sb.AppendLine($"| {r.name} | {filhos} | {rend} | {col} | {pref} | {dims} | {ys} |");
        }
        sb.AppendLine();

        GameObject mapa = roots.FirstOrDefault(r => r.name == RAIZ_GERADA);
        if (mapa != null)
        {
            sb.AppendLine($"### 4.1 `{RAIZ_GERADA}` — gerado a partir do mapa antigo (status: **obsoleto, não validado**)");
            sb.AppendLine();
            sb.AppendLine("| Grupo | Objetos | Renderers | Colliders | Extensão XZ (m) | Y mín–máx |");
            sb.AppendLine("|---|---:|---:|---:|---|---|");
            foreach (Transform g in mapa.transform)
            {
                var trs = g.GetComponentsInChildren<Transform>(true);
                int rend = g.GetComponentsInChildren<Renderer>(true).Count(ContaRenderer);
                int col = g.GetComponentsInChildren<Collider>(true).Length;
                Bounds b; bool tem = BoundsDe(g.gameObject, out b);
                string ext = tem ? $"{b.size.x:0.0} × {b.size.z:0.0}" : "—";
                string ys = tem ? $"{b.min.y:0.00} – {b.max.y:0.00}" : "—";
                sb.AppendLine($"| {g.name} | {trs.Length} | {rend} | {col} | {ext} | {ys} |");
            }
            sb.AppendLine();

            Transform est = mapa.transform.Find("Estruturas");
            if (est != null)
            {
                sb.AppendLine("#### Estruturas geradas (ids do mapa antigo)");
                sb.AppendLine();
                sb.AppendLine("| Id | Bounds X×Y×Z (m) | Centro (m) |");
                sb.AppendLine("|---|---|---|");
                foreach (Transform filho in est)
                {
                    if (!BoundsDe(filho.gameObject, out Bounds b)) continue;
                    sb.AppendLine($"| {filho.name} | {b.size.x:0.0} × {b.size.y:0.0} × {b.size.z:0.0} | ({b.center.x:0.0}, {b.center.y:0.0}, {b.center.z:0.0}) |");
                }
                sb.AppendLine();
            }
        }

        // ------------------------------------------------------ superfícies
        sb.AppendLine("## 5. Superfícies de piso e cobertura");
        sb.AppendLine();
        var superficies = new List<Renderer>();
        foreach (var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include))
        {
            if (!ContaRenderer(r)) continue;
            var b = r.bounds;
            if (b.size.y <= 1.5f && b.size.x >= 3f && b.size.z >= 3f && b.min.y >= -1.5f && b.min.y <= 2f)
                superficies.Add(r);
        }
        sb.AppendLine($"Renderers candidatos a piso (baixos, grandes, próximos de Y=0): **{superficies.Count}**.");
        if (superficies.Count > 0)
        {
            Bounds u = superficies[0].bounds;
            foreach (var r in superficies) u.Encapsulate(r.bounds);
            sb.AppendLine($"- Área coberta pelo conjunto (bounding box, não união real): {u.size.x:0.0} × {u.size.z:0.0} m; Y [{u.min.y:0.00}, {u.max.y:0.00}].");
            sb.AppendLine();
            sb.AppendLine("| Piso (amostra) | Bounds X×Y×Z (m) | Y mín |");
            sb.AppendLine("|---|---|---|");
            foreach (var r in superficies.OrderByDescending(x => x.bounds.size.x * x.bounds.size.z).Take(20))
                sb.AppendLine($"| {r.name} | {r.bounds.size.x:0.0} × {r.bounds.size.y:0.00} × {r.bounds.size.z:0.0} | {r.bounds.min.y:0.00} |");
            sb.AppendLine();

            // cobertura por amostragem (2 m) na caixa das superfícies
            var gaps = new List<Vector2>();
            int amostras = 0, cobertas = 0;
            for (float x = u.min.x; x <= u.max.x; x += 2f)
                for (float z = u.min.z; z <= u.max.z; z += 2f)
                {
                    amostras++;
                    bool ok = false;
                    foreach (var r in superficies)
                    {
                        var b = r.bounds;
                        if (x >= b.min.x && x <= b.max.x && z >= b.min.z && z <= b.max.z) { ok = true; break; }
                    }
                    if (ok) cobertas++;
                    else if (gaps.Count < 30) gaps.Add(new Vector2(x, z));
                }
            sb.AppendLine($"- Cobertura por amostragem de 2 m dentro da bounding box: **{cobertas}/{amostras}** pontos ({(amostras > 0 ? 100f * cobertas / amostras : 0f):0.0}%).");
            sb.AppendLine("- Lacunas são **estimativas por bounds**, não prova de vão real (módulos podem se sobrepor).");
            if (gaps.Count > 0)
            {
                sb.AppendLine($"- Primeiros pontos sem piso: {string.Join(", ", gaps.Take(12).Select(g => $"({g.x:0.0}, {g.y:0.0})"))}{(gaps.Count >= 30 ? ", …" : "")}.");
            }
            sb.AppendLine();
        }

        // ------------------------------------------------------ catálogo de prefabs
        sb.AppendLine("## 6. Catálogo de prefabs (metadados reais)");
        sb.AppendLine();
        var usados = PrefabsUsadosNaCena(roots);
        var caminhos = new List<string>(Candidatos.Select(c => c.path));
        foreach (var p in usados.Keys) if (!caminhos.Contains(p)) caminhos.Add(p);

        sb.AppendLine($"Prefabs distintos usados na cena: **{usados.Count}**. Candidatos da seleção: **{Candidatos.Length}**. Inspecionados: **{caminhos.Count}**.");
        sb.AppendLine();
        sb.AppendLine("Legenda de confiança: **alta** = caminho/nome + renderer coerentes; **média** = categoria inferida apenas por nome/pacote; **baixa** = ambíguo ou sem renderer → revisão humana.");
        sb.AppendLine();
        sb.AppendLine("| Prefab | Pacote | Usos na cena | Renderers | Bounds X×Y×Z (m) | Meshes (amostra) | Materiais (shader; cor; smooth/metallic) | Colliders | Categoria inferida | Confiança |");
        sb.AppendLine("|---|---|---:|---:|---|---|---|---|---|---|");

        foreach (var path in caminhos.OrderBy(x => x))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                sb.AppendLine($"| `{path}` | — | {usados.GetValueOrDefault(path)} | — | — | — | — | — | **AUSENTE** | — |");
                continue;
            }
            var info = InspecionarPrefab(path, prefab);
            info.usosNaCena = usados.GetValueOrDefault(path);
            linhasJson.Add(info);
            sb.AppendLine($"| {info.nome} | {info.pacote} | {info.usosNaCena} | {info.renderers} | {info.bounds} | {info.meshes} | {info.materiais} | {info.colliders} | {info.categoria} | {info.confianca} |");
        }
        sb.AppendLine();
        sb.AppendLine("> Categoria é **hipótese por nome/caminho**, não confirmação visual. Materiais mostram até 2 por prefab.");
        sb.AppendLine();

        // ------------------------------------------------------ dados obsoletos
        sb.AppendLine("## 7. Dados do mapa antigo: descartados / revalidados");
        sb.AppendLine();
        sb.AppendLine("| Dado antigo | Status | Motivo |");
        sb.AppendLine("|---|---|---|");
        sb.AppendLine("| `metersPerPixel = 0.516` | **descartado** | Válido apenas para a imagem 973×632 antiga; v3 proíbe reutilizar sem reconfirmar base e calibração. |");
        sb.AppendLine("| `originPx (486,316)` | **descartado** | Mesma calibração do item acima. |");
        sb.AppendLine("| IDs `R01–R08` (vias) | **obsoleto** | Classificação e coordenadas antigas; não revalidado. |");
        sb.AppendLine("| IDs `T01–T09` (terrenos) | **obsoleto** | Polígonos antigos; não revalidados. |");
        sb.AppendLine("| IDs `E01–E19` (estruturas) | **obsoleto** | Footprints/rotações antigos; não revalidados. |");
        sb.AppendLine("| IDs `V01–V10` (vegetação) | **obsoleto** | Centros/raios/n antigos; não revalidados. |");
        sb.AppendLine("| Áreas azuis classificadas como via | **corrigido pelo usuário** | São **prédios/edificações**; classificação antiga não deve ser usada. |");
        sb.AppendLine("| Traçado vermelho sob as árvores | **corrigido pelo usuário** | É **via**; falta o dado textual/vetorial de coordenadas para localizá-lo. |");
        sb.AppendLine("| Formato ID/tabela/confiança | **mantido como exemplo** | Só o formato documental; nunca os valores. |");
        sb.AppendLine();
        sb.AppendLine("Consequência: a raiz `Mapa_IFNMG` e `Assets/Dados/MapaIFNMG/mapa_ifnmg.json` ficam **congelados como não validados** até existir referência espacial atual.");
        sb.AppendLine();

        // ------------------------------------------------------ pendências
        sb.AppendLine("## 8. Pendências de dados (humano)");
        sb.AppendLine();
        sb.AppendLine("1. JSON/mapa atual com coordenadas, dimensão da imagem-base e sistema de referência (ou descrições textuais ancoradas em objetos reais).");
        sb.AppendLine("2. Coordenadas/descrição do traçado vermelho que é via.");
        sb.AppendLine("3. Lista das áreas azuis que são prédios (para não classificá-las como piso).");
        sb.AppendLine("4. Confirmação da escala real (marco da cena com distância conhecida).");
        sb.AppendLine("5. Definição de qual eixo corresponde ao norte (se houver mapa georreferenciado).");
        sb.AppendLine("6. Zona única, localizável com dados atuais, para a área de teste.");
        sb.AppendLine();

        // ------------------------------------------------------ escrita
        string raiz = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string md = Path.Combine(raiz, SAIDA_MD);
        string json = Path.Combine(raiz, SAIDA_JSON);
        Directory.CreateDirectory(Path.GetDirectoryName(md));
        File.WriteAllText(md, sb.ToString(), new UTF8Encoding(false));
        File.WriteAllText(json, JsonUtility.ToJson(new AssetInfoList { assets = linhasJson }, true), new UTF8Encoding(false));
        AssetDatabase.Refresh();
        Debug.Log($"[AuditarIFNMG] Relatório em {SAIDA_MD} e {SAIDA_JSON}");
    }

    private static string GitHead(string raiz)
    {
        try
        {
            var head = Path.Combine(raiz, ".git", "HEAD");
            if (!File.Exists(head)) return "sem git";
            string txt = File.ReadAllText(head).Trim();
            if (!txt.StartsWith("ref:")) return txt;
            var refPath = Path.Combine(raiz, ".git", txt.Substring(4).Trim().Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(refPath) ? File.ReadAllText(refPath).Trim() : txt;
        }
        catch { return "indisponível"; }
    }

    private static Dictionary<string, int> PrefabsUsadosNaCena(GameObject[] roots)
    {
        var dict = new Dictionary<string, int>();
        foreach (var t in roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(t.gameObject)) continue;
            string p = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject);
            if (string.IsNullOrEmpty(p)) continue;
            dict[p] = dict.GetValueOrDefault(p) + 1;
        }
        return dict;
    }

    private static AssetInfo InspecionarPrefab(string path, GameObject prefab)
    {
        var info = new AssetInfo
        {
            nome = prefab.name,
            path = path,
            pacote = Pacote(path),
            categoria = Categoria(path, prefab.name),
        };

        var renderers = prefab.GetComponentsInChildren<Renderer>(true).Where(ContaRenderer).ToList();
        info.renderers = renderers.Count;
        info.meshes = string.Join(", ", Meshes(prefab).Take(3));
        if (Meshes(prefab).Count > 3) info.meshes += $" (+{Meshes(prefab).Count - 3})";
        if (string.IsNullOrEmpty(info.meshes)) info.meshes = "—";

        GameObject inst = null;
        try
        {
            inst = UnityEngine.Object.Instantiate(prefab);
            Physics.SyncTransforms();

            bool tem = false; Bounds b = new Bounds();
            foreach (var r in inst.GetComponentsInChildren<Renderer>(true).Where(ContaRenderer))
            {
                if (!tem) { b = r.bounds; tem = true; } else b.Encapsulate(r.bounds);
            }
            info.bounds = tem ? $"{b.size.x:0.00} × {b.size.y:0.00} × {b.size.z:0.00}" : "—";

            var materiais = new List<string>();
            foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    string desc = DescreverMaterial(m);
                    if (!materiais.Contains(desc)) materiais.Add(desc);
                }
            info.materiais = materiais.Count == 0 ? "—" : string.Join("<br>", materiais.Take(2));
            if (materiais.Count > 2) info.materiais += $"<br>…(+{materiais.Count - 2})";

            var cols = inst.GetComponentsInChildren<Collider>(true);
            var tipos = cols.GroupBy(c => c.GetType().Name).Select(g => $"{g.Key}×{g.Count()}");
            info.colliders = cols.Length == 0 ? "nenhum" : $"{cols.Length} ({string.Join(", ", tipos)})";

            info.confianca = Confianca(info.categoria, info.renderers, path);
        }
        catch (Exception e)
        {
            info.bounds = "erro";
            info.materiais = e.Message;
            info.colliders = "—";
            info.confianca = "baixa";
        }
        finally { if (inst != null) UnityEngine.Object.DestroyImmediate(inst); }

        if (info.confianca != "alta") info.revisaoHumana = true;
        return info;
    }

    private static string DescreverMaterial(Material m)
    {
        string sh = m.shader != null ? m.shader.name : "<null shader>";
        string cor = "";
        if (m.HasProperty("_BaseColor")) cor = " BaseColor=" + m.GetColor("_BaseColor");
        else if (m.HasProperty("_Color")) cor = " Color=" + m.GetColor("_Color");
        float smooth = float.NaN, metal = float.NaN;
        if (m.HasProperty("_Smoothness")) smooth = m.GetFloat("_Smoothness");
        else if (m.HasProperty("_Glossiness")) smooth = m.GetFloat("_Glossiness");
        if (m.HasProperty("_Metallic")) metal = m.GetFloat("_Metallic");
        string extra = "";
        if (!float.IsNaN(smooth)) extra += $" smooth={smooth:0.00}";
        if (!float.IsNaN(metal)) extra += $" metal={metal:0.00}";
        if (m.HasProperty("_MainTex") && m.GetTexture("_MainTex") != null) extra += $" tex={m.GetTexture("_MainTex").name}";
        return $"{m.name} [{sh}]{cor}{extra}";
    }

    private static List<string> Meshes(GameObject go)
    {
        var nomes = new List<string>();
        foreach (var mf in go.GetComponentsInChildren<MeshFilter>(true))
            if (mf.sharedMesh != null && !nomes.Contains(mf.sharedMesh.name)) nomes.Add(mf.sharedMesh.name);
        foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            if (smr.sharedMesh != null && !nomes.Contains(smr.sharedMesh.name)) nomes.Add(smr.sharedMesh.name);
        return nomes;
    }

    private static string Pacote(string path)
    {
        var partes = path.Split('/');
        return partes.Length > 1 ? partes[1] : "—";
    }

    private static string Categoria(string path, string nome)
    {
        string s = System.Text.RegularExpressions.Regex.Replace((path + " " + nome).ToLowerInvariant(), "[^a-z0-9]+", " ");
        if (Tem(s, "road", "sidewalk", "pavement", "walk", "concrete", "ground", "tile", "paving")) return "Superfície";
        if (Tem(s, "tree", "arbusto", "bush", "hedge", "flower", "grass", "mushroom", "foliage", "plant")) return "Vegetação";
        if (Tem(s, "rock", "stone")) return "Rocha";
        if (Tem(s, "bench", "lamp", "bin", "sign", "street")) return "Mobiliário";
        if (Tem(s, "building", "house", "school", "museum", "industrial", "resident", "supermarket", "concert", "wall", "roof", "window", "door", "casement")) return "Edifício/Componente";
        return "Indefinido";
    }

    private static bool Tem(string s, params string[] termos)
    {
        foreach (var t in termos)
            if (System.Text.RegularExpressions.Regex.IsMatch(s, @"(^| )" + System.Text.RegularExpressions.Regex.Escape(t)))
                return true;
        return false;
    }

    private static string Confianca(string categoria, int renderers, string path)
    {
        if (renderers == 0 || categoria == "Indefinido") return "baixa";
        string s = System.Text.RegularExpressions.Regex.Replace(path.ToLowerInvariant(), "[^a-z0-9]+", " ");
        bool caminhoClaro = Tem(s, "road", "sidewalk", "ground", "parks", "plants", "street", "nature", "forest", "buildings");
        return caminhoClaro ? "alta" : "média";
    }

    private static bool Contem(string s, params string[] termos)
    {
        foreach (var t in termos) if (s.Contains(t)) return true;
        return false;
    }

    private static bool ContaRenderer(Renderer r)
    {
        return r != null && !(r is ParticleSystemRenderer) && !(r is TrailRenderer) && !(r is LineRenderer);
    }

    private static bool BoundsDe(GameObject go, out Bounds b)
    {
        bool tem = false;
        b = new Bounds();
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (!ContaRenderer(r)) continue;
            if (!tem) { b = r.bounds; tem = true; } else b.Encapsulate(r.bounds);
        }
        return tem;
    }

    private static bool BoundsDaCena(Scene cena, out Bounds b)
    {
        bool tem = false;
        b = new Bounds();
        foreach (var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include))
        {
            if (!ContaRenderer(r)) continue;
            if (!tem) { b = r.bounds; tem = true; } else b.Encapsulate(r.bounds);
        }
        return tem;
    }

    [Serializable]
    private class AssetInfo
    {
        public string nome;
        public string path;
        public string pacote;
        public int usosNaCena;
        public int renderers;
        public string bounds;
        public string meshes;
        public string materiais;
        public string colliders;
        public string categoria;
        public string confianca;
        public bool revisaoHumana;
    }

    [Serializable]
    private class AssetInfoList
    {
        public List<AssetInfo> assets;
    }
}

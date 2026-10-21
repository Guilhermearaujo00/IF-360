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
/// Ãrea-piloto de ambientaÃ§Ã£o do IFNMG 360Â°. Cria uma cena de prÃ©via isolada
/// (Assets/Scenes/IFNMG_AmbientacaoPreview.unity) com um terreno de teste que
/// exercita: grama modular, praÃ§a de concreto, calÃ§ada, Ã¡rvores, arbustos,
/// tufos de grama, rochas, fungos, banco, lixeira e poste.
///
/// Nada Ã© aplicado no campus real: "Aplicar no Campus" sÃ³ fica habilitado
/// quando a prÃ©via Ã© marcada como validada.
///
/// Menu: Tools/IFNMG/AmbientaÃ§Ã£o/â€¦
/// Batch: -executeMethod AmbientacaoIFNMG.PreviewBatch
/// </summary>
public static class AmbientacaoIFNMG
{
    public const string PREVIEW_SCENE = "Assets/Scenes/IFNMG_AmbientacaoPreview.unity";
    public const string CAMPUS = "Assets/Scenes/Campus.unity";
    public const string RAIZ = "IFNMG_Ambientacao_GERADA";
    public const string PREVIAS = "Docs/MontagemCena/previas";
    public const string VALIDACAO_MD = "Docs/MontagemCena/VALIDACAO_AREA_TESTE.md";
    public const string VALIDACAO_CSV = "Docs/MontagemCena/objetos_gerados.csv";
    private const int SEED = 20261020;

    // Sandbox sintÃ©tico, NÃƒO corresponde a nenhuma zona do campus.
    private static readonly Bounds ZonaSandbox = new Bounds(new Vector3(0, 1, 0), new Vector3(40, 4, 30));

    // ParÃ¢metros ajustÃ¡veis (o painel altera estes valores).
    public static int seed = SEED;
    public static float escalaArvores = 1.0f;
    public static int qtdTufosGrama = 45;
    public static int qtdRochas = 4;

    // ---------------------------------------------------------- assets reais
    private const string GRAMA = "Assets/LowPolyCityFree/Prefabs/05_Ground_Paving/SM_LPC_Ground_GroundGrassFlat_01.prefab";
    private const string PRACA = "Assets/SimplePoly City - Low Poly Assets/Prefab/Roads/Road Concrete Tile.prefab";
    private const string CALCADA = "Assets/SimplePoly City - Low Poly Assets/Prefab/Roads/Road Sidewalk.prefab";
    private const string ARV_MADURA = "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_TreeBroadleafMature_01.prefab";
    private const string ARV_AMPLA = "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_TreeBroadleafSpreading_01.prefab";
    private const string ARV_JOVEM = "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_TreeBroadleafYoung_01.prefab";
    private const string ARV_MATA = "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Tree_Green.prefab";
    private const string ARBUSTO = "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_BushRoundMedium_01.prefab";
    private const string CERCA_VIVA = "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_HedgeRunLow_01.prefab";
    private const string CANTEIRO = "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_FlowerBedRound_01.prefab";
    private const string Tufo = "Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_GrassTuftClump_01.prefab";
    private const string capim = "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Grass_01.prefab";
    private const string rochaDura = "Assets/Pandazole_Ultimate_Pack/Pandazole Nature Environment Pack/Prefabs/HardRock_01.prefab";
    private const string rochaMole = "Assets/Pandazole_Ultimate_Pack/Pandazole Nature Environment Pack/Prefabs/SoftRock_01.prefab";
    private const string fungo = "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Mushroom_01.prefab";
    private const string banco = "Assets/LowPolyCityFree/Prefabs/06_Street_Furniture/SM_LPC_Street_BenchStreetSlat_01.prefab";
    private const string poste = "Assets/LowPolyCityFree/Prefabs/06_Street_Furniture/SM_LPC_Street_LampStreetColumn_01.prefab";
    private const string lixeira = "Assets/LowPolyCityFree/Prefabs/06_Street_Furniture/SM_LPC_Street_StreetBinRound_01.prefab";

    private static Transform _raiz, _terreno, _arvores, _sub, _rochas, _mobiliario;

    [MenuItem("Tools/IFNMG/AmbientaÃ§Ã£o/Abrir Painel de AmbientaÃ§Ã£o")]
    public static void AbrirPainel() => EditorWindow.GetWindow<AmbientacaoIFNMGWindow>("AmbientaÃ§Ã£o IFNMG");

    // --------------------------------------------------------------- batch
    public static void PreviewBatch()
    {
        GerarPreview(true);
        RenderizarPrevias(true);
    }

    // --------------------------------------------------------------- preview
    [MenuItem("Tools/IFNMG/AmbientaÃ§Ã£o/Sandbox: Gerar/Rebuild (cena separada)")]
    public static void GerarPreviewMenu() => GerarPreview(false);

    public static void GerarPreview(bool batch)
    {
        Scene cena;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PREVIEW_SCENE) == null)
        {
            cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Scenes"));
            EditorSceneManager.SaveScene(cena, PREVIEW_SCENE);
        }
        else
        {
            cena = EditorSceneManager.OpenScene(PREVIEW_SCENE, OpenSceneMode.Single);
        }

        LimparGerado();
        Construir(Vector3.zero);
        ConfigurarAmbiente();

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        AssetDatabase.Refresh();
        Debug.Log($"[AmbientacaoIFNMG] PrÃ©via gerada em {PREVIEW_SCENE} (seed {seed}).");
    }

    [MenuItem("Tools/IFNMG/AmbientaÃ§Ã£o/Limpar Somente o Gerado")]
    public static void LimparGerado()
    {
        var go = GameObject.Find(RAIZ);
        if (go != null) UnityEngine.Object.DestroyImmediate(go);
    }

    /// <summary>
    /// Bloqueado pelo prompt v3: sÃ³ pode aplicar quando existir uma zona
    /// localizÃ¡vel com dados atuais e validaÃ§Ã£o humana. NÃ£o usa posiÃ§Ã£o arbitrÃ¡ria.
    /// </summary>
    public static void AplicarNoCampus(Vector3 origem)
    {
        Debug.LogError(
            "[AmbientacaoIFNMG] AplicaÃ§Ã£o no campus bloqueada: nenhuma zona validada com dados atuais. " +
            "O v3 proÃ­be posicionar em coordenadas arbitrÃ¡rias. A prÃ©via em cena separada continua disponÃ­vel.");
        EditorUtility.DisplayDialog("AplicaÃ§Ã£o bloqueada",
            "Nenhuma zona do campus foi validada com dados atuais (v3).\n\n" +
            "A aplicaÃ§Ã£o sÃ³ serÃ¡ habilitada depois que o plano espacial indicar uma zona localizÃ¡vel " +
            "e a prÃ©via for aprovada por vocÃª.", "OK");
    }

    // --------------------------------------------------------------- render
    [MenuItem("Tools/IFNMG/AmbientaÃ§Ã£o/PrÃ©via: Renderizar Imagens")]
    public static void RenderizarPreviasMenu() => RenderizarPrevias(false);

    public static void RenderizarPrevias(bool batch)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PREVIEW_SCENE) == null)
        {
            Debug.LogError("[AmbientacaoIFNMG] Gere a prÃ©via antes de renderizar.");
            return;
        }
        if (batch) EditorSceneManager.OpenScene(PREVIEW_SCENE, OpenSceneMode.Single);
        else if (SceneManager.GetActiveScene().path != PREVIEW_SCENE)
            EditorSceneManager.OpenScene(PREVIEW_SCENE, OpenSceneMode.Single);

        string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", PREVIAS));
        Directory.CreateDirectory(dir);

        var camTop = GameObject.Find("Cam_Topo")?.GetComponent<Camera>();
        var camPersp = GameObject.Find("Cam_Perspectiva")?.GetComponent<Camera>();
        var camDet = GameObject.Find("Cam_Transicao")?.GetComponent<Camera>();

        RenderCamera(camTop, Path.Combine(dir, "01_topo.png"), 1280, 900);
        RenderCamera(camPersp, Path.Combine(dir, "02_perspectiva.png"), 1280, 800);
        RenderCamera(camDet, Path.Combine(dir, "03_transicao.png"), 1280, 800);

        Debug.Log($"[AmbientacaoIFNMG] PrÃ©vias renderizadas em {PREVIAS}");
    }

    private static void RenderCamera(Camera cam, string path, int w, int h)
    {
        if (cam == null) { Debug.LogWarning($"[AmbientacaoIFNMG] CÃ¢mera ausente para {path}"); return; }
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        var anterior = RenderTexture.active;
        try
        {
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
        }
        finally
        {
            cam.targetTexture = null;
            RenderTexture.active = anterior;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);
        }
    }

    // ------------------------------------------------------------- ambiente
    private static void ConfigurarAmbiente()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.60f, 0.66f);
        RenderSettings.fog = false;

        var pasta = new GameObject("Cameras_e_Luz");
        pasta.transform.SetParent(_raiz, false);

        var luz = new GameObject("Luz_Direcional").AddComponent<Light>();
        luz.transform.SetParent(pasta.transform, false);
        luz.type = LightType.Directional;
        luz.intensity = 1.15f;
        luz.color = new Color(1f, 0.97f, 0.92f);
        luz.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        CriarCamera(pasta.transform, "Cam_Topo", new Vector3(0, 45, 0), Quaternion.Euler(90, 0, 0), true, 19f, 55f);
        CriarCamera(pasta.transform, "Cam_Perspectiva", new Vector3(20, 3.2f, 20), OlharPara(new Vector3(20, 3.2f, 20), new Vector3(0, 1, 0)), false, 0f, 50f);
        CriarCamera(pasta.transform, "Cam_Transicao", new Vector3(3.5f, 2.2f, 6.5f), OlharPara(new Vector3(3.5f, 2.2f, 6.5f), new Vector3(3.5f, 0.6f, -4f)), false, 0f, 55f);
    }

    private static Quaternion OlharPara(Vector3 de, Vector3 alvo)
    {
        return Quaternion.LookRotation((alvo - de).normalized, Vector3.up);
    }

    private static void CriarCamera(Transform pai, string nome, Vector3 pos, Quaternion rot, bool ortografica, float size, float fov)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        var cam = go.AddComponent<Camera>();
        cam.transform.position = pos;
        cam.transform.rotation = rot;
        cam.orthographic = ortografica;
        cam.orthographicSize = size;
        cam.fieldOfView = fov;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 300f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.62f, 0.76f, 0.90f);
        go.tag = "Untagged";
    }

    // ------------------------------------------------------------ construÃ§Ã£o
    private static void Construir(Vector3 origem)
    {
        var raiz = new GameObject(RAIZ);
        _raiz = raiz.transform;

        _terreno = Pasta("Terreno_e_Pavimentacao");
        _arvores = Pasta("Arvores");
        _sub = Pasta("Arbustos_e_Grama");
        _rochas = Pasta("Rochas_e_Fungos");
        _mobiliario = Pasta("Mobiliario");

        // base contÃ­nua (evita frestas entre tiles) + grama modular por cima
        CriarBaseContinua();
        CriarGramadoModular();
        CriarPracaECalcada();

        // vegetaÃ§Ã£o principal
        Arvore(ARV_MADURA, new Vector3(-12, 0, -6), 15, 1.10f);
        Arvore(ARV_AMPLA, new Vector3(-8, 0, -10), 80, 1.00f);
        Arvore(ARV_MADURA, new Vector3(-14, 0, 0), 200, 0.95f);
        Arvore(ARV_JOVEM, new Vector3(-5, 0, -4), 45, 1.15f);
        Arvore(ARV_MATA, new Vector3(-10, 0, -13), 300, 1.00f); // comparaÃ§Ã£o de estilo

        // arbustos e canteiros
        Colocar(ARBUSTO, new Vector3(-4, 0, -3), 0, 1.0f);
        Colocar(ARBUSTO, new Vector3(-6.2f, 0, -6.2f), 40, 1.1f);
        Colocar(ARBUSTO, new Vector3(-9, 0, -8.5f), 120, 0.9f);
        Colocar("Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_FoliageBush_01.prefab",
                new Vector3(-12, 0, -11), 0, 1.0f);
        Colocar(CERCA_VIVA, new Vector3(2.0f, 0, -2.5f), 90, 1.0f);
        Colocar(CANTEIRO, new Vector3(1.0f, 0, -5.5f), 0, 1.0f);

        // tufos de grama (seed fixa, agrupados nas bordas/sob Ã¡rvores)
        ScatterTufos();

        // rochas na borda natural (norte)
        var rochas = new[]
        {
            new Vector3(-16, 0, 12.5f), new Vector3(-14.5f, 0, 13.2f),
            new Vector3(-11, 0, 12.2f), new Vector3(-8, 0, 13.4f)
        };
        var rr = new System.Random(seed + 7);
        for (int i = 0; i < qtdRochas; i++)
        {
            Vector3 p = i < rochas.Length
                ? rochas[i]
                : new Vector3(Mathf.Lerp(-18f, 18f, (float)rr.NextDouble()), 0, Mathf.Lerp(11.5f, 13.8f, (float)rr.NextDouble()));
            Rocha(i % 2 == 0 ? rochaDura : rochaMole, p, (float)rr.NextDouble() * 360f, Mathf.Lerp(0.8f, 1.15f, (float)rr.NextDouble()));
        }

        // fungos sob as Ã¡rvores
        Colocar(fungo, new Vector3(-12.5f, 0, -6.5f), 0, 1.0f);
        Colocar(fungo, new Vector3(-11.6f, 0, -5.2f), 60, 0.9f);
        Colocar(fungo, new Vector3(-9.4f, 0, -9.1f), 120, 1.05f);

        // mobiliÃ¡rio perto da praÃ§a
        Colocar(banco, new Vector3(5.5f, 0, -3.2f), 0, 1.0f, _mobiliario);
        Colocar(lixeira, new Vector3(7.4f, 0, -3.0f), 0, 1.0f, _mobiliario);
        Colocar(poste, new Vector3(1.4f, 0, -1.2f), 0, 1.0f, _mobiliario);
        Colocar(poste, new Vector3(14.2f, 0, -12.2f), 180, 1.0f, _mobiliario);

        raiz.transform.position = origem;
        ColetarEValidar(raiz.transform);
    }

    // ------------------------------------------------- validaÃ§Ã£o por dados
    [Serializable]
    public class ObjGerado
    {
        public string prefab;
        public float x, y, z;
        public float rotY;
        public float escala;
        public float minY, maxY;
        public float minX, maxX, minZ, maxZ;
        public string zona;
        public string justificativa;
        public string avisos;
    }

    private static void ColetarEValidar(Transform raiz)
    {
        var itens = new List<ObjGerado>();
        foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(t.gameObject)) continue;
            if (PrefabUtility.GetOutermostPrefabInstanceRoot(t.gameObject) != t.gameObject) continue;
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject);
            if (string.IsNullOrEmpty(path)) continue;

            Bounds b = new Bounds(t.position, Vector3.zero);
            bool tem = false;
            foreach (var r in t.GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
                if (!tem) { b = r.bounds; tem = true; } else b.Encapsulate(r.bounds);
            }

            itens.Add(new ObjGerado
            {
                prefab = path,
                x = t.position.x, y = t.position.y, z = t.position.z,
                rotY = t.eulerAngles.y,
                escala = t.lossyScale.x,
                minY = tem ? b.min.y : t.position.y,
                maxY = tem ? b.max.y : t.position.y,
                minX = tem ? b.min.x : t.position.x,
                maxX = tem ? b.max.x : t.position.x,
                minZ = tem ? b.min.z : t.position.z,
                maxZ = tem ? b.max.z : t.position.z,
                zona = "sandbox_sintetico",
                justificativa = "ComposiÃ§Ã£o de teste (grama/praÃ§a/calÃ§ada/vegetaÃ§Ã£o/mobiliÃ¡rio).",
            });
        }

        var avisos = new List<string>();
        foreach (var g in itens) g.avisos = "";

        // 1) dentro da zona
        foreach (var g in itens)
        {
            var p = new Vector3(g.x, ZonaSandbox.center.y, g.z);
            if (!ZonaSandbox.Contains(p))
                AddAviso(g, avisos, "fora da zona do sandbox");
        }

        // 2) apoio no piso: compara o fundo do objeto com o piso logo abaixo (Y=0 no sandbox)
        foreach (var g in itens)
        {
            float baseLocal = PisoLocal(g.x, g.z);
            float fundo = g.minY;
            if (ParecePiso(g.prefab)) continue;
            if (ParecePoste(g.prefab)) continue;
            if (fundo < baseLocal - 0.8f) AddAviso(g, avisos, $"enterrado (fundo {fundo:0.00}, piso {baseLocal:0.00})");
            else if (fundo > baseLocal + 0.9f) AddAviso(g, avisos, $"flutuando (fundo {fundo:0.00}, piso {baseLocal:0.00})");
        }

        // 3) duplicatas exatas (mesmo prefab, mesma posiÃ§Ã£o XZ)
        for (int i = 0; i < itens.Count; i++)
            for (int j = i + 1; j < itens.Count; j++)
            {
                var a = itens[i]; var b = itens[j];
                if (a.prefab != b.prefab) continue;
                float d = new Vector2(a.x - b.x, a.z - b.z).magnitude;
                if (d < 0.4f && Mathf.Abs(a.y - b.y) < 0.4f)
                {
                    AddAviso(a, avisos, $"duplicata com {System.IO.Path.GetFileName(b.prefab)}");
                    AddAviso(b, avisos, $"duplicata com {System.IO.Path.GetFileName(a.prefab)}");
                }
            }

        // 4) espaÃ§amento entre Ã¡rvores (copa alta)
        var arvores = itens.Where(g => g.prefab.ToLowerInvariant().Contains("tree") && g.maxY > 2f).ToList();
        for (int i = 0; i < arvores.Count; i++)
            for (int j = i + 1; j < arvores.Count; j++)
            {
                float d = new Vector2(arvores[i].x - arvores[j].x, arvores[i].z - arvores[j].z).magnitude;
                if (d < 2.0f)
                {
                    AddAviso(arvores[i], avisos, $"Ã¡rvore a {d:0.00} m de outra");
                    AddAviso(arvores[j], avisos, $"Ã¡rvore a {d:0.00} m de outra");
                }
            }

        // 5) continuidade entre peÃ§as de pavimento
        var pav = itens.Where(g => g.prefab.ToLowerInvariant().Contains("road concrete") || g.prefab.ToLowerInvariant().Contains("road sidewalk")).ToList();
        var gaps = new List<string>();
        for (int i = 0; i < pav.Count; i++)
            for (int j = i + 1; j < pav.Count; j++)
            {
                float gap = DistanciaRetangulos(pav[i], pav[j]);
                if (gap > 0.01f && gap <= 0.35f)
                    gaps.Add($"{System.IO.Path.GetFileName(pav[i].prefab)} â†” {System.IO.Path.GetFileName(pav[j].prefab)}: {gap:0.00} m");
            }

        string raizProjeto = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string mdPath = Path.Combine(raizProjeto, VALIDACAO_MD);
        string csvPath = Path.Combine(raizProjeto, VALIDACAO_CSV);
        Directory.CreateDirectory(Path.GetDirectoryName(mdPath));

        var sb = new StringBuilder();
        sb.AppendLine("# Validação geométrica da área de teste (v3)");
        sb.AppendLine();
        sb.AppendLine($"- Data: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"- Raiz: `{RAIZ}` na cena `{SceneManager.GetActiveScene().path}`");
        sb.AppendLine($"- Zona do sandbox: centro (0, 1, 0), tamanho 40 × 4 × 30 m (**sintética, não é local do campus**).");
        sb.AppendLine($"- Objetos gerados (prefab instances): **{itens.Count}**.");
        sb.AppendLine("- Seed: `" + seed + "`.");
        sb.AppendLine("- **Isto não é aprovação visual.** As verificações são geométricas; a avaliação estética é humana.");
        sb.AppendLine();
        sb.AppendLine($"## Avisos: {avisos.Count}");
        sb.AppendLine();
        if (avisos.Count == 0) sb.AppendLine("Nenhum aviso geométrico.");
        else foreach (var a in avisos) sb.AppendLine($"- {a}");
        sb.AppendLine();
        sb.AppendLine("## Contagem por categoria inferida");
        sb.AppendLine();
        foreach (var grupo in itens.GroupBy(g => CategoriaDoPath(g.prefab)).OrderByDescending(g => g.Count()))
            sb.AppendLine($"- {grupo.Key}: **{grupo.Count()}**");
        sb.AppendLine();
        sb.AppendLine("## Pavimento (peças e folgas quase-encostadas)");
        sb.AppendLine();
        sb.AppendLine($"Peças de pavimento: **{pav.Count}**.");
        if (gaps.Count == 0) sb.AppendLine("Nenhuma folga entre 0,01 m e 0,35 m detectada.");
        else foreach (var g in gaps) sb.AppendLine($"- {g}");
        sb.AppendLine();
        sb.AppendLine("## Objetos gerados");
        sb.AppendLine();
        sb.AppendLine("Tabela completa em `objetos_gerados.csv` (separador `;`). Amostra:");
        sb.AppendLine();
        sb.AppendLine("| Prefab | X | Y | Z | Rot Y | Escala | Zona | Avisos |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|---|---|");
        foreach (var g in itens.Take(40))
            sb.AppendLine($"| {Path.GetFileNameWithoutExtension(g.prefab)} | {g.x:0.00} | {g.y:0.00} | {g.z:0.00} | {g.rotY:0.0} | {g.escala:0.00} | {g.zona} | {g.avisos} |");
        if (itens.Count > 40) sb.AppendLine($"| â€¦ | | | | | | | (+{itens.Count - 40}) |");
        File.WriteAllText(mdPath, sb.ToString(), new UTF8Encoding(false));

        var csv = new StringBuilder();
        csv.AppendLine("prefab;x;y;z;rotY;escala;minY;maxY;zona;justificativa;avisos");
        foreach (var g in itens)
            csv.AppendLine($"{g.prefab};{g.x:0.000};{g.y:0.000};{g.z:0.000};{g.rotY:0.0};{g.escala:0.000};{g.minY:0.000};{g.maxY:0.000};{g.zona};{g.justificativa};{g.avisos}");
        File.WriteAllText(csvPath, csv.ToString(), new UTF8Encoding(false));

        Debug.Log($"[AmbientacaoIFNMG] ValidaÃ§Ã£o: {itens.Count} objetos, {avisos.Count} avisos. Ver {VALIDACAO_MD}.");
    }

    private static void AddAviso(ObjGerado g, List<string> lista, string texto)
    {
        string item = $"{Path.GetFileNameWithoutExtension(g.prefab)} em ({g.x:0.0}, {g.z:0.0}): {texto}";
        lista.Add(item);
        g.avisos = string.IsNullOrEmpty(g.avisos) ? texto : g.avisos + "; " + texto;
    }

    private static float PisoLocal(float x, float z)
    {
        if (Physics.Raycast(new Vector3(x, 5f, z), Vector3.down, out RaycastHit hit, 20f))
            return hit.point.y;
        return 0f;
    }

    private static bool ParecePiso(string path)
    {
        string s = System.Text.RegularExpressions.Regex.Replace(path.ToLowerInvariant(), "[^a-z0-9]+", " ");
        return TemToken(s, "ground", "grass", "road", "sidewalk", "concrete", "paving");
    }

    private static bool ParecePoste(string path)
    {
        string s = System.Text.RegularExpressions.Regex.Replace(path.ToLowerInvariant(), "[^a-z0-9]+", " ");
        return TemToken(s, "lamp", "light", "pole");
    }

    private static float DistanciaRetangulos(ObjGerado a, ObjGerado b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.minX - b.maxX, b.minX - a.maxX));
        float dz = Mathf.Max(0f, Mathf.Max(a.minZ - b.maxZ, b.minZ - a.maxZ));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private static string CategoriaDoPath(string path)
    {
        string s = System.Text.RegularExpressions.Regex.Replace(path.ToLowerInvariant(), "[^a-z0-9]+", " ");
        if (TemToken(s, "road", "sidewalk", "ground", "paving")) return "Piso/Pavimentação";
        if (TemToken(s, "tree")) return "Árvore";
        if (TemToken(s, "bush", "hedge", "flower", "grass", "foliage")) return "Arbusto/Grama";
        if (TemToken(s, "rock")) return "Rocha";
        if (TemToken(s, "mushroom")) return "Fungo";
        if (TemToken(s, "bench", "lamp", "bin")) return "Mobiliário";
        return "Outro";
    }

    private static bool TemToken(string s, params string[] termos)
    {
        foreach (var t in termos)
            if (System.Text.RegularExpressions.Regex.IsMatch(s, @"(^| )" + System.Text.RegularExpressions.Regex.Escape(t)))
                return true;
        return false;
    }

    private static Transform Pasta(string nome)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(_raiz, false);
        return go.transform;
    }

    private static void CriarBaseContinua()
    {
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "Base_Gramado_Continua";
        plane.transform.SetParent(_terreno, false);
        plane.transform.position = new Vector3(0, -0.03f, 0);
        plane.transform.localScale = new Vector3(4f, 1f, 3f); // 40 x 30 m
        var mat = MaterialGrama();
        if (mat != null) plane.GetComponent<Renderer>().sharedMaterial = mat;
        Physics.SyncTransforms();
    }

    private static Material MaterialGrama()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GRAMA);
        if (prefab == null) return null;
        foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            if (r.sharedMaterial != null) return r.sharedMaterial;
        return null;
    }

    private static void CriarGramadoModular()
    {
        var p = AssetDatabase.LoadAssetAtPath<GameObject>(GRAMA);
        if (p == null) return;
        for (float x = -18; x <= 18; x += 4)
            for (float z = -14; z <= 14; z += 4)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(p, _terreno);
                go.transform.position = new Vector3(x, 0f, z);
                go.transform.rotation = Quaternion.identity;
                go.name = $"Grama_{x:0}_{z:0}";
            }
    }

    private static void CriarPracaECalcada()
    {
        var praca = AssetDatabase.LoadAssetAtPath<GameObject>(PRACA);
        if (praca != null)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(praca, _terreno);
            go.transform.position = new Vector3(10, 0.12f, -7);
            go.transform.localScale = new Vector3(0.6f, 1f, 0.6f); // 12 x 12 m
            go.name = "Praca_Concreto";
        }

        var calc = AssetDatabase.LoadAssetAtPath<GameObject>(CALCADA);
        if (calc != null)
        {
            var a = (GameObject)PrefabUtility.InstantiatePrefab(calc, _terreno);
            a.transform.position = new Vector3(3.5f, 0.2f, -7f);
            a.transform.rotation = Quaternion.identity; // 3 x 10
            a.name = "Calcada_NorteSul";

            var b = (GameObject)PrefabUtility.InstantiatePrefab(calc, _terreno);
            b.transform.position = new Vector3(10f, 0.2f, -1f);
            b.transform.rotation = Quaternion.Euler(0, 90, 0); // 10 x 3
            b.name = "Calcada_LesteOeste";
        }
    }

    private static void Arvore(string path, Vector3 pos, float yaw, float escala)
    {
        Colocar(path, pos, yaw, escala * escalaArvores, _arvores);
    }

    private static void Rocha(string path, Vector3 pos, float yaw, float escala)
    {
        Colocar(path, pos, yaw, escala, _rochas);
    }

    private static GameObject Colocar(string path, Vector3 pos, float yaw, float escala, Transform pai = null)
    {
        var p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (p == null)
        {
            Debug.LogWarning($"[AmbientacaoIFNMG] Prefab ausente: {path}");
            return null;
        }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(p, pai != null ? pai : _sub);
        go.transform.position = new Vector3(pos.x, BaseY(pos.x, pos.z), pos.z);
        go.transform.rotation = Quaternion.Euler(0, yaw, 0);
        go.transform.localScale = Vector3.one * escala;
        go.name = p.name;
        return go;
    }

    private static float BaseY(float x, float z)
    {
        if (Physics.Raycast(new Vector3(x, 50, z), Vector3.down, out RaycastHit hit, 200f))
            return hit.point.y;
        return 0f;
    }

    private static void ScatterTufos()
    {
        var rnd = new System.Random(seed);
        var usados = new List<Vector3>();
        var pontosArvores = new[] {
            new Vector3(-12,0,-6), new Vector3(-8,0,-10), new Vector3(-14,0,0), new Vector3(-5,0,-4), new Vector3(-10,0,-13)
        };

        int feitos = 0, tent = 0;
        while (feitos < qtdTufosGrama && tent < qtdTufosGrama * 60)
        {
            tent++;
            float x = Mathf.Lerp(-19f, 19f, (float)rnd.NextDouble());
            float z = Mathf.Lerp(-14f, 14f, (float)rnd.NextDouble());

            // fora da praÃ§a/calÃ§ada
            if (x > 2.5f && x < 16.5f && z > -13f && z < -1f) continue;
            if (x > 2.0f && x < 5.2f && z > -12.5f && z < -1.5f) continue;
            if (x > 4.5f && x < 15.5f && z > -2.8f && z < 0.8f) continue;
            // perto de Ã¡rvores ou bordas = bom
            bool perto = false;
            foreach (var a in pontosArvores) if ((new Vector2(a.x - x, a.z - z)).sqrMagnitude < 9f) perto = true;
            bool borda = Mathf.Abs(x) > 15f || Mathf.Abs(z) > 11f;
            if (!perto && !borda && rnd.NextDouble() < 0.55) continue;

            if (MuitoPerto(usados, x, z, 1.1f)) continue;
            usados.Add(new Vector3(x, 0, z));

            string path = (rnd.NextDouble() < 0.6) ? Tufo : capim;
            Colocar(path, new Vector3(x, 0, z), (float)rnd.NextDouble() * 360f,
                    Mathf.Lerp(0.8f, 1.2f, (float)rnd.NextDouble()),
                    _sub);
            feitos++;
        }
        Debug.Log($"[AmbientacaoIFNMG] Tufos de grama: {feitos}.");
    }

    private static bool MuitoPerto(List<Vector3> usados, float x, float z, float min)
    {
        foreach (var u in usados) if ((new Vector2(u.x - x, u.z - z)).sqrMagnitude < min * min) return true;
        return false;
    }
}

public class AmbientacaoIFNMGWindow : EditorWindow
{
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Sandbox de ambientaÃ§Ã£o â€” IFNMG 360Â°", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Gera uma cena de teste ISOLADA (IFNMG_AmbientacaoPreview) com grama, praÃ§a, calÃ§ada, Ã¡rvores, " +
            "arbustos, tufos, rochas, fungos e mobiliÃ¡rio. Esta cena Ã© um sandbox sintÃ©tico: NÃƒO corresponde a " +
            "nenhuma zona do campus. Pelo prompt v3, nada Ã© aplicado no campus atÃ© existir zona validada por dados atuais.",
            MessageType.Info);

        AmbientacaoIFNMG.seed = EditorGUILayout.IntField("Seed", AmbientacaoIFNMG.seed);
        AmbientacaoIFNMG.escalaArvores = EditorGUILayout.Slider("Escala das Ã¡rvores", AmbientacaoIFNMG.escalaArvores, 0.6f, 1.6f);
        AmbientacaoIFNMG.qtdTufosGrama = EditorGUILayout.IntSlider("Tufos de grama", AmbientacaoIFNMG.qtdTufosGrama, 0, 150);
        AmbientacaoIFNMG.qtdRochas = EditorGUILayout.IntSlider("Rochas", AmbientacaoIFNMG.qtdRochas, 0, 12);

        EditorGUILayout.Space();
        if (GUILayout.Button("Gerar/Rebuild do sandbox (cena separada)"))
            AmbientacaoIFNMG.GerarPreview(false);
        if (GUILayout.Button("Renderizar prÃ©vias (topo / perspectiva / transiÃ§Ã£o)"))
            AmbientacaoIFNMG.RenderizarPrevias(false);
        if (GUILayout.Button("Limpar somente o gerado"))
            AmbientacaoIFNMG.LimparGerado();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Aplicar no campus: BLOQUEADO. Falta uma zona localizÃ¡vel com dados atuais (JSON/mapa ou descriÃ§Ã£o " +
            "ancorada na cena). O v3 proÃ­be posicionar em coordenadas arbitrÃ¡rias.",
            MessageType.Warning);
        using (new EditorGUI.DisabledScope(true))
            GUILayout.Button("Aplicar no Campus (bloqueado atÃ© validaÃ§Ã£o)");
    }
}

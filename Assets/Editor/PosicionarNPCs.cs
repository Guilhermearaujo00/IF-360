using IF360;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Etapa 2 do plano: posiciona os 12 NPCs na Campus.unity, em frente aos prédios
/// corretos, usando os limites REAIS dos prédios (os modelos FBX têm as posições
/// gravadas nos vértices — as instâncias ficam em (0,0,0)).
///
/// O que este script faz (idempotente — nada é duplicado):
///   - Abre a Campus.unity;
///   - Localiza cada prédio pelo nome (busca normalizada, sem acentos);
///   - Calcula o ponto na frente do prédio voltado para a estrada mais próxima
///     (Road_014), com a altura do terreno via raycast;
///   - Instancia o modelo FBX riggado (Assets/NPCs/Modelos) sob o grupo "NPCs";
///   - Adiciona NPCBaseNovo + SphereCollider trigger (2.5 m) + NPCAnimacao;
///   - Configura missões (ids reais: missao_ensino_medio, missao_administracao,
///     missao_biblioteca, missao_nucleo, missao_refeitorio, missao_assistencia)
///     e as VARIANTES de diálogo por estado, ligando os assets criados por
///     'Tools > IFNMG 360 > 1 - Criar Dialogos dos NPCs';
///   - Garante MinijogoFios e MinijogoCompilacao no objeto 'Systems';
///   - Salva a cena.
///
/// REESTRUTURAÇÃO DAS MISSÕES: este wiring legado é mantido para compatibilidade
/// — os NPCs já existem na cena e este script os PULA (idempotente). Os campos
/// (variantes, idMissao, chamarMinijogo, minijogoComoParteDaMissao) continuam
/// válidos: o NPCBaseNovo resolve o perfil no catálogo central pelo nomeNPC e
/// a abertura de minijogos agora exige AUTORIZAÇÃO do QuestManager central.
/// NPC de fora do catálogo (ex.: Secretaria com missaoRequisito=missao_diretor)
/// conversa normalmente, mas nunca recebe autorização de minijogo.
///
/// Observação: não há modelo de Laboratório de Química no projeto; o JP é
/// posicionado ao lado do Laboratório de Física (âncora), com aviso no log.
///
/// Executar via: Tools > IFNMG 360 > 2 - Posicionar NPCs no Campus
/// </summary>
public static class PosicionarNPCs
{
    private const string Menu = "Tools/IFNMG 360/2 - Posicionar NPCs no Campus";
    private const string CaminhoCampus = "Assets/Scenes/Campus.unity";
    private const string RaioTrigger = "2.5"; // documentação; valor aplicado abaixo

    // ---------- Definições dos 12 NPCs ----------

    private sealed class DefinicaoNPC
    {
        public string nomeExibicao;
        public string arquivoModelo;
        public string[] chavesPredio;
        public bool deslocarLateral; // âncora alternativa (ex.: JP perto da Física)

        public string idMissao;
        public string missaoRequisito = "";
        public string idFlagConversa = "";
        public bool completarMissaoAoTerminar;
        public bool usarModeloEtapas;
        public bool concluirApenasNaPrimeiraConversa;
        public bool chamarMinijogo;
        public bool minijogoComoParteDaMissao;
        public string idMinijogo = "fios";
        public string assetFallback;

        // (asset, requisitoMissao, requisitoFlag, requisitoEtapaMinima) — em ordem.
        public (string asset, string reqMissao, string reqFlag, int etapaMin)[] variantes;
    }

    private static readonly DefinicaoNPC[] Definicoes =
    {
        new DefinicaoNPC
        {
            nomeExibicao = "Ananda", arquivoModelo = "ANANDA_mixamo",
            chavesPredio = new[] { "ensino medio", "ensinomedio" },
            idMissao = "missao_ensino_medio", missaoRequisito = "missao_administracao",
            completarMissaoAoTerminar = true, chamarMinijogo = false,
            assetFallback = "Dialogo_Ananda_Pre",
            variantes = new[]
            {
                ("Dialogo_Ananda_Pos_Refeitorio", "missao_refeitorio", "", 2),
                ("Dialogo_Ananda_Pos_Nucleo", "missao_nucleo", "", 5),
                ("Dialogo_Ananda_Pos_Biblioteca", "missao_biblioteca", "", 4),
                ("Dialogo_Ananda_Pos_Matricula", "missao_administracao", "", -1)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "Roney", arquivoModelo = "RONEY_mixamo",
            chavesPredio = new[] { "administracao" },
            idMissao = "missao_administracao",
            // chamarMinijogo=false: sem reabertura pós-missão (evita farm de pontos);
            // o desafio abre pela branch 'minijogoComoParteDaMissao' em NPCBaseNovo.
            chamarMinijogo = false, minijogoComoParteDaMissao = true, idMinijogo = "fios",
            assetFallback = "Dialogo_Roney_Pre",
            variantes = new[]
            {
                ("Dialogo_Roney_Pos", "missao_administracao", "", -1)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "Thiago", arquivoModelo = "THIAGO_mixamo",
            chavesPredio = new[] { "biblioteca" },
            idMissao = "missao_biblioteca",
            completarMissaoAoTerminar = true, usarModeloEtapas = true,
            concluirApenasNaPrimeiraConversa = true,
            assetFallback = "Dialogo_Thiago_Pre",
            variantes = new[]
            {
                ("Dialogo_Thiago_Pos", "missao_biblioteca", "", 4),
                ("Dialogo_Thiago_Durante", "missao_biblioteca", "", 1)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "JP", arquivoModelo = "JP_mixamo",
            chavesPredio = new[] { "quimica", "predio de fisica" }, deslocarLateral = true,
            idMissao = "missao_biblioteca", idFlagConversa = "conversou_jp",
            completarMissaoAoTerminar = true, usarModeloEtapas = true,
            concluirApenasNaPrimeiraConversa = true,
            assetFallback = "Dialogo_JP_Pre",
            variantes = new[]
            {
                ("Dialogo_JP_Pos", "", "conversou_jp", -1)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "Gleice", arquivoModelo = "GLEICE_mixamo",
            chavesPredio = new[] { "predio de fisica" },
            idMissao = "missao_biblioteca", idFlagConversa = "conversou_gleice",
            completarMissaoAoTerminar = true, usarModeloEtapas = true,
            concluirApenasNaPrimeiraConversa = true,
            assetFallback = "Dialogo_Gleice_Pre",
            variantes = new[]
            {
                ("Dialogo_Gleice_Pos", "", "conversou_gleice", -1)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "Rosy", arquivoModelo = "ROSY_mixamo",
            chavesPredio = new[] { "biologia" },
            idMissao = "missao_biblioteca", idFlagConversa = "conversou_rosy",
            completarMissaoAoTerminar = true, usarModeloEtapas = true,
            concluirApenasNaPrimeiraConversa = true,
            assetFallback = "Dialogo_Rosy_Pre",
            variantes = new[]
            {
                ("Dialogo_Rosy_Pos", "", "conversou_rosy", -1)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "Josy", arquivoModelo = "JOSY_mixamo",
            chavesPredio = new[] { "nucleo melhorado", "gabinete" },
            idMissao = "missao_nucleo",
            completarMissaoAoTerminar = true, usarModeloEtapas = true,
            concluirApenasNaPrimeiraConversa = true,
            assetFallback = "Dialogo_Josy_Pre",
            variantes = new[]
            {
                ("Dialogo_Josy_Pos", "missao_nucleo", "", 5),
                ("Dialogo_Josy_Durante", "missao_nucleo", "", 2)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "David", arquivoModelo = "DAVI_mixamo",
            chavesPredio = new[] { "ead", "bridge" },
            idMissao = "missao_nucleo", idFlagConversa = "conversou_david",
            completarMissaoAoTerminar = true, usarModeloEtapas = true,
            concluirApenasNaPrimeiraConversa = true,
            assetFallback = "Dialogo_David_Pre",
            variantes = new[]
            {
                ("Dialogo_David_Pos", "", "conversou_david", -1)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "Márcio", arquivoModelo = "MARCO_mixamo",
            chavesPredio = new[] { "predio do superior" },
            idMissao = "missao_nucleo",
            // chamarMinijogo=false: idem Roney (sem replay pós-missão).
            chamarMinijogo = false, minijogoComoParteDaMissao = true, idMinijogo = "compilacao",
            assetFallback = "Dialogo_Marcio_Pre",
            variantes = new[]
            {
                ("Dialogo_Marcio_Pos", "missao_nucleo", "", -1)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "João", arquivoModelo = "JOAO_mixamo",
            chavesPredio = new[] { "refeitorio" },
            idMissao = "missao_refeitorio",
            completarMissaoAoTerminar = true, usarModeloEtapas = true,
            concluirApenasNaPrimeiraConversa = true,
            assetFallback = "Dialogo_Joao_Pre",
            variantes = new[]
            {
                ("Dialogo_Joao_Pos", "missao_refeitorio", "", 2),
                ("Dialogo_Joao_Durante", "missao_refeitorio", "", 1)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "Cláudio", arquivoModelo = "CLAUDIO_mixamo",
            chavesPredio = new[] { "posto de vendas" },
            idMissao = "missao_refeitorio", idFlagConversa = "conversou_claudio",
            completarMissaoAoTerminar = true, usarModeloEtapas = true,
            concluirApenasNaPrimeiraConversa = true,
            assetFallback = "Dialogo_Claudio_Pre",
            variantes = new[]
            {
                ("Dialogo_Claudio_Pos", "", "conversou_claudio", -1)
            }
        },
        new DefinicaoNPC
        {
            nomeExibicao = "Alice", arquivoModelo = "ALICE_mixamo",
            chavesPredio = new[] { "assistencia estudantil", "assistencia" },
            idMissao = "missao_assistencia",
            completarMissaoAoTerminar = true, usarModeloEtapas = true,
            concluirApenasNaPrimeiraConversa = true,
            assetFallback = "Dialogo_Alice_Pre",
            variantes = new[]
            {
                ("Dialogo_Alice_Pos", "missao_assistencia", "", 1)
            }
        }
    };

    [MenuItem(Menu)]
    public static void Posicionar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Posicionar NPCs",
                "Saia do Play Mode antes de posicionar os NPCs.", "OK");
            return;
        }

        // Garante que FBX novos (Models/NPCs) e assets de diálogo sejam vistos
        // pelo AssetDatabase antes de carregá-los.
        AssetDatabase.Refresh();

        Scene cena = EditorSceneManager.OpenScene(CaminhoCampus, OpenSceneMode.Single);
        Transform[] todos = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);

        Transform grupoNpcs = ObterOuCriarGrupo("NPCs", cena);

        int criados = 0, pulados = 0;
        foreach (DefinicaoNPC def in Definicoes)
        {
            string nomeObjeto = "NPC_" + def.nomeExibicao;
            if (BuscarPorNome(todos, nomeObjeto) != null)
            {
                pulados++;
                Debug.Log($"[IFNMG 360] {nomeObjeto} já existe na cena — nada feito.");
                continue;
            }

            Transform predio = BuscarPredio(todos, def.chavesPredio);
            if (predio == null)
            {
                Debug.LogError($"[IFNMG 360] Prédio não encontrado para {def.nomeExibicao} " +
                               $"(chaves: {string.Join(", ", def.chavesPredio)}). NPC NÃO criado.");
                continue;
            }

            // Âncora primária ausente (ex.: não há Lab de Química no projeto):
            // avisa que está usando a âncora alternativa.
            if (!Normalizar(predio.name).Contains(def.chavesPredio[0]))
            {
                Debug.LogWarning($"[IFNMG 360] {def.nomeExibicao}: prédio '{def.chavesPredio[0]}' não existe " +
                                  $"na cena — usando '{predio.name}' como âncora alternativa.");
            }

            Bounds limites = CalcularBounds(predio);
            List<Transform> estradas = ColetarEstradas(todos);
            Vector3 direcao;
            Vector3 posicao = CalcularPosicaoFrente(limites, estradas, def.deslocarLateral, out direcao);

            GameObject npc = InstanciarModelo(def, grupoNpcs);
            npc.name = nomeObjeto;
            npc.transform.position = posicao;
            npc.transform.rotation = Quaternion.LookRotation(direcao); // olha para a circulação

            ConfigurarColisor(npc);
            ConfigurarNPC(npc, def);

            criados++;
            Debug.Log($"[IFNMG 360] {nomeObjeto}: prédio '{predio.name}' | missão '{def.idMissao}' | " +
                      $"posição ({posicao.x:F1}, {posicao.y:F2}, {posicao.z:F1}).");
        }

        GarantirMinijogos();

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);

        Debug.Log($"[IFNMG 360] Posicionamento concluído: {criados} criados, {pulados} já existentes. " +
                  "Trigger de interação: raio " + RaioTrigger + " m. Próximo passo: " +
                  "'Tools > IFNMG 360 > 3 - Preparar Animacoes dos NPCs'.");
    }

    // ---------- Helpers de cena ----------

    private static Transform ObterOuCriarGrupo(string nome, Scene cena)
    {
        Transform existente = GameObject.Find(nome)?.transform;
        if (existente != null) return existente;

        GameObject grupo = new GameObject(nome);
        Undo.RegisterCreatedObjectUndo(grupo, "Criar grupo NPCs");
        SceneManager.MoveGameObjectToScene(grupo, cena);
        return grupo.transform;
    }

    private static Transform BuscarPorNome(Transform[] todos, string nome)
    {
        foreach (Transform t in todos)
        {
            if (t.name == nome) return t;
        }
        return null;
    }

    private static Transform BuscarPredio(Transform[] todos, string[] chaves)
    {
        Transform melhor = null;
        int melhorProfundidade = int.MaxValue;

        foreach (Transform t in todos)
        {
            string normalizado = Normalizar(t.name);
            bool bate = false;
            foreach (string chave in chaves)
            {
                if (normalizado.Contains(chave)) { bate = true; break; }
            }
            if (!bate) continue;

            // Prédios têm renderers; ignora coincidências sem malha (ex.: pastas).
            if (t.GetComponentsInChildren<Renderer>(true).Length == 0) continue;

            int profundidade = 0;
            for (Transform p = t.parent; p != null; p = p.parent) profundidade++;
            if (profundidade < melhorProfundidade)
            {
                melhorProfundidade = profundidade;
                melhor = t;
            }
        }

        return melhor;
    }

    private static List<Transform> ColetarEstradas(Transform[] todos)
    {
        var estradas = new List<Transform>();
        foreach (Transform t in todos)
        {
            // Rodovias nomeadas Road_001..Road_064 e duplicatas Road_014 (N).
            if (t.name.StartsWith("Road_")) estradas.Add(t);
        }
        return estradas;
    }

    private static Bounds CalcularBounds(Transform predio)
    {
        Bounds limites = new Bounds(predio.position, Vector3.zero);
        bool primeiro = true;
        foreach (Renderer r in predio.GetComponentsInChildren<Renderer>(true))
        {
            if (primeiro) { limites = r.bounds; primeiro = false; }
            else limites.Encapsulate(r.bounds);
        }
        return limites;
    }

    private static Vector3 CalcularPosicaoFrente(Bounds limites, List<Transform> estradas,
                                                 bool deslocarLateral, out Vector3 direcao)
    {
        Vector3 centro = limites.center;
        centro.y = 0f;

        Vector3 alvo = Vector3.zero; // fallback: centro do campus
        float melhorDist = float.MaxValue;
        foreach (Transform estrada in estradas)
        {
            Vector3 p = estrada.position;
            float d = (p - centro).sqrMagnitude;
            if (d < melhorDist) { melhorDist = d; alvo = p; }
        }

        direcao = alvo - centro;
        direcao.y = 0f;
        direcao = direcao.sqrMagnitude < 0.01f ? Vector3.back : direcao.normalized;

        // Ponto de apoio da caixa alinhada aos eixos na direção da estrada.
        float suporte = Mathf.Abs(direcao.x) * limites.extents.x +
                        Mathf.Abs(direcao.z) * limites.extents.z;
        Vector3 posicao = centro + direcao * (suporte + 3.5f);

        if (deslocarLateral)
        {
            Vector3 lateral = Vector3.Cross(Vector3.up, direcao).normalized;
            posicao += lateral * 15f;
        }

        posicao.y = AlturaDoTerreno(posicao);
        return posicao;
    }

    private static float AlturaDoTerreno(Vector3 posicao)
    {
        if (Physics.Raycast(posicao + Vector3.up * 120f, Vector3.down, out RaycastHit acerto, 300f))
        {
            return acerto.point.y;
        }
        return 0f; // campus praticamente plano em y ~ 0
    }

    private static GameObject InstanciarModelo(DefinicaoNPC def, Transform grupo)
    {
        string caminho = $"Assets/NPCs/Modelos/{def.arquivoModelo}.fbx";
        GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);

        if (modelo != null)
        {
            GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(modelo, grupo);
            Undo.RegisterCreatedObjectUndo(instancia, "Criar " + def.nomeExibicao);
            return instancia;
        }

        Debug.LogError($"[IFNMG 360] Modelo não encontrado: {caminho}. Criando objeto vazio " +
                       "(atribua o modelo manualmente no Inspector).");
        GameObject vazio = new GameObject("NPC_" + def.nomeExibicao);
        Undo.RegisterCreatedObjectUndo(vazio, "Criar " + def.nomeExibicao);
        vazio.transform.SetParent(grupo, false);
        return vazio;
    }

    private static void ConfigurarColisor(GameObject npc)
    {
        SphereCollider colisor = npc.GetComponent<SphereCollider>();
        if (colisor == null) colisor = npc.AddComponent<SphereCollider>();
        colisor.isTrigger = true;
        colisor.radius = 2.5f;   // perímetro de interação (tecla E)
        colisor.center = new Vector3(0f, 1f, 0f);
    }

    private static void ConfigurarNPC(GameObject npc, DefinicaoNPC def)
    {
        NPCBaseNovo comp = npc.GetComponent<NPCBaseNovo>();
        if (comp == null) comp = Undo.AddComponent<NPCBaseNovo>(npc);
        Undo.RecordObject(comp, "Configurar " + def.nomeExibicao);

        comp.nomeNPC = def.nomeExibicao;
        comp.idMissao = def.idMissao;
        comp.missaoRequisito = def.missaoRequisito;
        comp.idFlagConversa = def.idFlagConversa;
        comp.completarMissaoAoTerminar = def.completarMissaoAoTerminar;
        comp.usarModeloEtapas = def.usarModeloEtapas;
        comp.concluirApenasNaPrimeiraConversa = def.concluirApenasNaPrimeiraConversa;
        comp.chamarMinijogo = def.chamarMinijogo;
        comp.minijogoComoParteDaMissao = def.minijogoComoParteDaMissao;
        comp.idMinijogo = def.idMinijogo;
        comp.registrarConversa = true;
        comp.dialogoData = CarregarDialogo(def.assetFallback);
        comp.falaApresentacao = $"Olá! Sou {def.nomeExibicao}.";

        comp.variantes = new VarianteDialogo[def.variantes.Length];
        for (int i = 0; i < def.variantes.Length; i++)
        {
            comp.variantes[i] = new VarianteDialogo
            {
                dialogo = CarregarDialogo(def.variantes[i].asset),
                requisitoMissao = def.variantes[i].reqMissao,
                requisitoFlag = def.variantes[i].reqFlag,
                requisitoEtapaMinima = def.variantes[i].etapaMin
            };
        }

        if (npc.GetComponent<NPCAnimacao>() == null)
        {
            Undo.AddComponent<NPCAnimacao>(npc);
        }

        EditorUtility.SetDirty(comp);
    }

    private static DialogoData CarregarDialogo(string nomeAsset)
    {
        DialogoData asset = AssetDatabase.LoadAssetAtPath<DialogoData>(
            $"Assets/Dados/Dialogos/{nomeAsset}.asset");
        if (asset == null)
        {
            Debug.LogWarning($"[IFNMG 360] DialogoData '{nomeAsset}' não encontrado. " +
                             "Rode 'Tools > IFNMG 360 > 1 - Criar Dialogos dos NPCs' primeiro.");
        }
        return asset;
    }

    private static void GarantirMinijogos()
    {
        GameObject systems = GameObject.Find("Systems");
        if (systems == null)
        {
            systems = new GameObject("Systems");
            Undo.RegisterCreatedObjectUndo(systems, "Criar Systems");
        }

        if (Object.FindAnyObjectByType<MinijogoFios>() == null)
        {
            Undo.AddComponent<MinijogoFios>(systems);
            Debug.Log("[IFNMG 360] MinijogoFios adicionado em 'Systems' (desafio dos 3 fios).");
        }

        if (Object.FindAnyObjectByType<MinijogoCompilacao>() == null)
        {
            Undo.AddComponent<MinijogoCompilacao>(systems);
            Debug.Log("[IFNMG 360] MinijogoCompilacao adicionado em 'Systems' (desafio dos padrões).");
        }
    }

    private static string Normalizar(string texto)
    {
        string semAcentos = texto.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (char c in semAcentos)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().ToLowerInvariant().Replace('_', ' ');
    }
}

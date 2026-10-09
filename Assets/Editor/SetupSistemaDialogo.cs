using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Configura a cena atual com o sistema de diálogo em UMA execução:
///   - "Systems" + DialogueManager;
///   - "Canvas" (Screen Space Overlay, 1920x1080 com Scale With Screen Size);
///     "DialogoUI" -> "CaixaDialogo" com textos TMP (NpcNome, TextoFala, DicaContinuar);
///   - Player: tag "Player" + componente PlayerInteraction.
///
/// A ferramenta é REUTILIZÁVEL: na segunda execução ela não duplica nada — reconstrói
/// a hierarquia de UI com RectTransform correto (caixa ancorada na PARTE INFERIOR)
/// e preserva componentes já existentes (incluindo o Font Asset atribuído).
///
/// RODAR DEPOIS (uma vez, por cena): Window > TextMeshPro > Import TMP Essential
/// Resources e, se a fonte não aparecer, atribuir um Font Asset nos três textos.
/// </summary>
public static class SetupSistemaDialogo
{
    private const string MENU = "Tools/IFNMG/Configurar Sistema de Diálogo";
    private const string MENU_NPC_MISSAO = "Tools/IFNMG/Criar NPC Diretor (teste de missão)";
    private const string MENU_HUD = "Tools/IFNMG/Criar HUD de Missão";

    [MenuItem(MENU)]
    public static void ConfigurarCena()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("SetupSistemaDialogo",
                "Saia do Play Mode antes de configurar a cena. " +
                "Objetos criados durante o Play seriam descartados ao parar.",
                "OK");
            return;
        }

        CriarSystems();
        CriarCanvas();
        ConfigurarPlayer();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("SetupSistemaDialogo: sistema de diálogo configurado. " +
                  "Caixa ancorada na parte inferior. Se os textos aparecerem vazios, " +
                  "atribua um Font Asset (LiberationSans SDF).");
    }

    [MenuItem(MENU_NPC_MISSAO)]
    public static void CriarNPCDeTeste()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("SetupSistemaDialogo",
                "Saia do Play Mode antes de criar o NPC.", "OK");
            return;
        }

        GameObject existente = GameObject.Find("NPC_Diretor");
        if (existente != null)
        {
            Selection.activeGameObject = existente;
            Debug.Log("SetupSistemaDialogo: 'NPC_Diretor' já existe na cena — selecionado (nada foi duplicado).");
            return;
        }

        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = "NPC_Diretor";
        Undo.RegisterCreatedObjectUndo(npc, "Criar NPC_Diretor");

        GameObject secretaria = GameObject.Find("NPC_Secretaria");
        Transform basePos = secretaria != null ? secretaria.transform : null;
        npc.transform.position = basePos != null
            ? basePos.position + basePos.right * -3f
            : Vector3.up;
        npc.transform.rotation = Quaternion.identity;

        CapsuleCollider col = npc.GetComponent<CapsuleCollider>();
        col.isTrigger = true;
        col.radius = 0.6f;

        NPCBaseNovo npcComp = Undo.AddComponent<NPCBaseNovo>(npc);
        npcComp.nomeNPC = "Diretor";
        npcComp.idMissao = "missao_diretor";
        npcComp.chamarMinijogo = false;
        npcComp.completarMissaoAoTerminar = true;
        npcComp.dialogoData = EncontrarDialogoDataPorNome("Dialogo_Diretor_Pre");
        npcComp.falaApresentacao = "Olá! Sou o Diretor. (NPC de teste de missão)";

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Selection.activeGameObject = npc;

        Debug.Log("SetupSistemaDialogo: 'NPC_Diretor' criado — idMissao 'missao_diretor' completa a etapa ao " +
                  "terminar a conversa. Configure o 'Missão Requisito' da NPC_Secretaria como 'missao_diretor'.");

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
    }

    [MenuItem(MENU_HUD)]
    public static void CriarHUD()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("SetupSistemaDialogo",
                "Saia do Play Mode antes de criar o HUD.", "OK");
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("SetupSistemaDialogo",
                "Nenhum Canvas na cena. Rode 'Tools > IFNMG > Configurar Sistema de Diálogo' primeiro.", "OK");
            return;
        }

        GameObject painel = ObterOuCriarFilho(canvas.gameObject, "PainelMissao");
        if (painel.transform.parent != canvas.transform)
        {
            painel.transform.SetParent(canvas.transform, false);
        }
        PrepararPainelHUD(painel);

        TMP_Text titulo = ObterOuCriarTMP(painel, "Titulo");
        TMP_Text objetivo = ObterOuCriarTMP(painel, "Objetivo");
        TMP_Text progresso = ObterOuCriarTMP(painel, "Progresso");

        PosicionarTituloHUD(titulo);
        PosicionarObjetivoHUD(objetivo);
        PosicionarProgressoHUD(progresso);

        HUDMissao comp = painel.GetComponent<HUDMissao>();
        if (comp == null) comp = Undo.AddComponent<HUDMissao>(painel);

        SerializedObject so = new SerializedObject(comp);
        so.FindProperty("textoTitulo").objectReferenceValue = titulo;
        so.FindProperty("textoObjetivo").objectReferenceValue = objetivo;
        so.FindProperty("textoProgresso").objectReferenceValue = progresso;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Selection.activeGameObject = painel;

        Debug.Log("SetupSistemaDialogo: HUD de missão pronto no canto superior esquerdo do Canvas. " +
                  "Preencha 'Nomes de Missões' no HUDMissao para exibir nomes amigáveis.");
    }

    private static void PrepararPainelHUD(GameObject painel)
    {
        Image fundo = painel.GetComponent<Image>();
        if (fundo == null)
        {
            fundo = painel.AddComponent<Image>();
            fundo.color = new Color(0.1f, 0.1f, 0.14f, 0.8f);
        }

        RectTransform rect = painel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(20f, -20f);
        rect.sizeDelta = new Vector2(440f, 150f);
    }

    private static void PosicionarTituloHUD(TMP_Text titulo)
    {
        RectTransform rect = titulo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(16f, -42f);
        rect.offsetMax = new Vector2(-16f, -10f);

        titulo.fontSize = 26f;
        titulo.alignment = TextAlignmentOptions.TopLeft;
        titulo.color = Color.white;
        titulo.text = "Jornada";
    }

    private static void PosicionarObjetivoHUD(TMP_Text objetivo)
    {
        RectTransform rect = objetivo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(16f, -98f);
        rect.offsetMax = new Vector2(-16f, -46f);

        objetivo.fontSize = 20f;
        objetivo.alignment = TextAlignmentOptions.TopLeft;
        objetivo.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        objetivo.text = "Fale com os NPCs para conhecer o campus.";
    }

    private static void PosicionarProgressoHUD(TMP_Text progresso)
    {
        RectTransform rect = progresso.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(16f, -136f);
        rect.offsetMax = new Vector2(-16f, -102f);

        progresso.fontSize = 18f;
        progresso.alignment = TextAlignmentOptions.TopLeft;
        progresso.color = new Color(0.7f, 0.85f, 1f, 1f);
        progresso.text = "0 missões concluídas";
    }

    private static DialogoData EncontrarDialogoData()
        string[] guids = AssetDatabase.FindAssets("t:DialogoData");
        foreach (string guid in guids)
        {
            DialogoData so = AssetDatabase.LoadAssetAtPath<DialogoData>(AssetDatabase.GUIDToAssetPath(guid));
            if (so != null && so.falas != null && so.falas.Length > 0)
            {
                return so;
            }
        }
        return null;
    }

    private static DialogoData EncontrarDialogoDataPorNome(string nome)
    {
        string[] guids = AssetDatabase.FindAssets("t:DialogoData");
        foreach (string guid in guids)
        {
            DialogoData so = AssetDatabase.LoadAssetAtPath<DialogoData>(AssetDatabase.GUIDToAssetPath(guid));
            if (so != null && so.name == nome)
            {
                return so;
            }
        }
        return null;
    }

    private static void CriarSystems()
    {
        GameObject systems = GameObject.Find("Systems");
        if (systems == null)
        {
            systems = new GameObject("Systems");
            Undo.RegisterCreatedObjectUndo(systems, "Criar Systems");
        }

        if (Object.FindAnyObjectByType<DialogueManager>() == null)
        {
            systems.AddComponent<DialogueManager>();
            Debug.Log("SetupSistemaDialogo: DialogueManager adicionado em 'Systems'.");
        }
        else
        {
            Debug.Log("SetupSistemaDialogo: DialogueManager já existe na cena. Pulando.");
        }

        GarantirQuestManager(systems);
        GarantirSistemasExtras(systems);
    }

    private static void GarantirSistemasExtras(GameObject systems)
    {
        if (Object.FindAnyObjectByType<SaveManager>() == null)
        {
            systems.AddComponent<SaveManager>();
            Debug.Log("SetupSistemaDialogo: SaveManager adicionado em 'Systems' (salva pontos, colecionáveis e missões).");
        }

        if (Object.FindAnyObjectByType<AudioManager>() == null)
        {
            // RequireComponent adiciona o AudioSource automaticamente; atribua os AudioClips no Inspector.
            systems.AddComponent<AudioManager>();
            Debug.Log("SetupSistemaDialogo: AudioManager adicionado em 'Systems' (atribua os AudioClips: digitar/confirmar/fechar).");
        }

        if (Object.FindAnyObjectByType<InventoryManager>() == null)
        {
            systems.AddComponent<InventoryManager>();
            Debug.Log("SetupSistemaDialogo: InventoryManager adicionado em 'Systems'.");
        }
    }

    private static void GarantirQuestManager(GameObject systems)
    {
        if (Object.FindAnyObjectByType<QuestManager>() != null)
        {
            Debug.Log("SetupSistemaDialogo: QuestManager já existe na cena. Pulando.");
            return;
        }

        // Fica no MESMO objeto do GameManager (que é DontDestroyOnLoad) para o
        // estado das missões sobreviver à troca de cena, junto da fachada.
        GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
        GameObject host = gameManager != null ? gameManager.gameObject : systems;

        host.AddComponent<QuestManager>();

        if (gameManager == null)
        {
            Debug.LogWarning("SetupSistemaDialogo: GameManager não encontrado — QuestManager foi adicionado " +
                             "em 'Systems' e NÃO persistirá entre cenas. Coloque-o no objeto do GameManager.");
        }
        else
        {
            Debug.Log($"SetupSistemaDialogo: QuestManager adicionado em '{host.name}' (persistente).");
        }
    }

    private static void CriarCanvas()
    {
        Canvas canvasExistente = Object.FindAnyObjectByType<Canvas>();
        GameObject canvasGo;

        if (canvasExistente != null)
        {
            canvasGo = canvasExistente.gameObject;
            Debug.Log("SetupSistemaDialogo: Canvas existente encontrado — reutilizado.");
        }
        else
        {
            canvasGo = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGo, "Criar Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // "DialogoUI" é apenas o host do componente DialogueUI (é um nó simples,
        // não é renderizado). A caixa NÃO fica filha dele: fica DIRETAMENTE do
        // Canvas (que já é RectTransform), senão as âncoras de "inferior" seriam
        // ignoradas e a caixa apareceria centralizada.
        GameObject uiGo = ObterOuCriarFilho(canvasGo, "DialogoUI");
        if (uiGo.GetComponent<DialogueUI>() == null) uiGo.AddComponent<DialogueUI>();

        GameObject caixa = ObterOuCriarFilho(canvasGo, "CaixaDialogo");
        if (caixa.transform.parent != canvasGo.transform)
        {
            caixa.transform.SetParent(canvasGo.transform, false);
        }
        PrepararCaixa(caixa);

        TMP_Text nome = ObterOuCriarTMP(caixa, "NpcNome");
        TMP_Text texto = ObterOuCriarTMP(caixa, "TextoFala");
        TMP_Text dica = ObterOuCriarTMP(caixa, "DicaContinuar");

        PosicionarNome(nome);
        PosicionarTextoFala(texto);
        PosicionarDica(dica);

        PreencherReferenciasDialogoUI(uiGo, caixa, nome, texto, dica.gameObject);
        PreencherReferenciaManager(uiGo);
    }

    private static void PrepararCaixa(GameObject caixa)
    {
        Image fundo = caixa.GetComponent<Image>();
        if (fundo == null)
        {
            fundo = caixa.AddComponent<Image>();
            fundo.color = new Color(0.16f, 0.16f, 0.2f, 0.94f);
        }

        if (caixa.GetComponent<Outline>() == null) caixa.AddComponent<Outline>();

        RectTransform rect = caixa.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 40f);
        rect.sizeDelta = new Vector2(1600f, 260f);

        caixa.SetActive(false);
    }

    private static void PosicionarNome(TMP_Text nome)
    {
        RectTransform rect = nome.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(20f, -12f);
        rect.sizeDelta = new Vector2(1560f, 40f);

        nome.fontSize = 34f;
        nome.alignment = TextAlignmentOptions.TopLeft;
        nome.color = Color.white;
        nome.text = string.Empty;
    }

    private static void PosicionarTextoFala(TMP_Text texto)
    {
        RectTransform rect = texto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(20f, -230f);
        rect.offsetMax = new Vector2(-20f, -52f);

        texto.fontSize = 26f;
        texto.alignment = TextAlignmentOptions.TopLeft;
        texto.textWrappingMode = TextWrappingModes.Normal;
        texto.color = Color.white;
        texto.text = string.Empty;
    }

    private static void PosicionarDica(TMP_Text dica)
    {
        RectTransform rect = dica.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-20f, 12f);
        rect.sizeDelta = new Vector2(500f, 30f);

        dica.fontSize = 24f;
        dica.alignment = TextAlignmentOptions.BottomRight;
        dica.color = new Color(1f, 1f, 1f, 0.85f);
        dica.text = "Pressione [E] para continuar";
    }

    // ---------- Helpers de objetos (reutilizáveis, sem duplicar) ----------

    private static Transform BuscarFilho(Transform pai, string nome)
    {
        foreach (Transform filho in pai)
        {
            if (filho.name == nome) return filho;

            Transform neto = BuscarFilho(filho, nome);
            if (neto != null) return neto;
        }
        return null;
    }

    private static GameObject ObterOuCriarFilho(GameObject pai, string nome)
    {
        Transform existente = BuscarFilho(pai.transform, nome);
        if (existente != null) return existente.gameObject;

        GameObject go = new GameObject(nome);
        Undo.RegisterCreatedObjectUndo(go, "Criar " + nome);
        go.transform.SetParent(pai.transform, false);
        return go;
    }

    private static TMP_Text ObterOuCriarTMP(GameObject pai, string nome)
    {
        GameObject go = ObterOuCriarFilho(pai, nome);
        TMP_Text tmp = go.GetComponent<TMP_Text>();
        if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
        return tmp;
    }

    private static void PreencherReferenciasDialogoUI(GameObject uiGo, GameObject caixa,
                                                      TMP_Text nome, TMP_Text texto, GameObject dica)
    {
        DialogueUI dialogoUi = uiGo.GetComponent<DialogueUI>();
        if (dialogoUi == null) return;

        SerializedObject so = new SerializedObject(dialogoUi);
        so.FindProperty("caixa").objectReferenceValue = caixa;
        so.FindProperty("nome").objectReferenceValue = nome;
        so.FindProperty("texto").objectReferenceValue = texto;
        so.FindProperty("dica").objectReferenceValue = dica;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void PreencherReferenciaManager(GameObject uiGo)
    {
        DialogueManager manager = Object.FindAnyObjectByType<DialogueManager>();
        if (manager == null)
        {
            Debug.LogWarning("SetupSistemaDialogo: DialogueManager não encontrado para vincular 'ui'. " +
                             "Atribua manualmente no Inspector.");
            return;
        }

        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("ui").objectReferenceValue = uiGo.GetComponent<DialogueUI>();
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) so.FindProperty("playerController").objectReferenceValue = pc;
        }

        CameraController cc = Object.FindAnyObjectByType<CameraController>();
        if (cc != null)
        {
            so.FindProperty("cameraController").objectReferenceValue = cc;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigurarPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
            if (player != null) player.tag = "Player";
        }

        if (player == null)
        {
            Debug.LogWarning("SetupSistemaDialogo: nenhum GameObject com tag 'Player' ou nome 'Player' " +
                             "encontrado. Adicione o PlayerInteraction manualmente.");
            return;
        }

        if (player.GetComponent<PlayerInteraction>() == null)
        {
            Undo.AddComponent<PlayerInteraction>(player);
        }

        Debug.Log($"SetupSistemaDialogo: Player '{player.name}' com PlayerInteraction pronto " +
                  "(tag usada pelos NPCs: 'Player').");
    }
}
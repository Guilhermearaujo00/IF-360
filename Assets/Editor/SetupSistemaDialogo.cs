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

    private static void CriarSystems()
    {
        if (Object.FindAnyObjectByType<DialogueManager>() != null)
        {
            Debug.Log("SetupSistemaDialogo: DialogueManager já existe na cena. Pulando Systems.");
            return;
        }

        GameObject systems = new GameObject("Systems");
        Undo.RegisterCreatedObjectUndo(systems, "Criar Systems");
        systems.AddComponent<DialogueManager>();
        Debug.Log("SetupSistemaDialogo: 'Systems' criado (a referência 'ui' será vinculada quando o Canvas for criado abaixo).");
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
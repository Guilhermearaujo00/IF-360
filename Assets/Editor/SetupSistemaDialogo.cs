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
/// RODAR DEPOIS (uma vez, por cena): Window > TextMeshPro > Import TMP Essential
/// Resources e, se a fonte não aparecer, atribuir um Font Asset nos três textos.
/// </summary>
public static class SetupSistemaDialogo
{
    private const string MENU = "Tools/IFNMG/Configurar Sistema de Diálogo";

    [MenuItem(MENU)]
    public static void ConfigurarCena()
    {
        CriarSystems();
        CriarCanvas();
        ConfigurarPlayer();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("SetupSistemaDialogo: sistema de diálogo configurado. " +
                  "Lembre-se de importar os recursos essenciais do TextMeshPro " +
                  "(Window > TextMeshPro > Import TMP Essential Resources) e atribuir " +
                  "Font Assets aos textos se eles aparecerem vazios.");
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
        Debug.Log("SetupSistemaDialogo: 'Systems' criado (não esqueça de atribuir a referência 'ui' " +
                  "após a criação do Canvas abaixo).");
    }

    private static void CriarCanvas()
    {
        Canvas canvasExistente = Object.FindFirstObjectByType<Canvas>();
        Canvas canvas = canvasExistente;
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
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        GameObject uiGo = CriarFilho(canvasGo, "DialogoUI");
        if (uiGo.GetComponent<DialogueUI>() == null) uiGo.AddComponent<DialogueUI>();

        GameObject caixa = CriarCaixaDialogo(uiGo);
        TMP_Text nome = CriarTexto(caixa, "NpcNome", new Vector2(0f, 1f), new Vector2(0f, 1f),
                                   new Vector2(20f, -12f), 34, TextAlignmentOptions.TopLeft);
        RectTransform rectNome = nome.GetComponent<RectTransform>();
        rectNome.sizeDelta = new Vector2(1560f, 40f);

        TMP_Text texto = CriarTexto(caixa, "TextoFala", new Vector2(0f, 1f), new Vector2(1f, 1f),
                                    new Vector2(20f, -58f), 26, TextAlignmentOptions.TopLeft);
        RectTransform rectTexto = texto.GetComponent<RectTransform>();
        rectTexto.offsetMin = new Vector2(20f, -230f);
        rectTexto.offsetMax = new Vector2(-20f, -52f);
        texto.enableWordWrapping = true;
        GameObject dica = CriarTextoCeil(caixa, "DicaContinuar");

        TMP_Text dicaTexto = dica.GetComponent<TMP_Text>();
        dicaTexto.alignment = TextAlignmentOptions.BottomRight;
        dicaTexto.text = "Pressione [E] para continuar";

        RectTransform rectDica = dica.GetComponent<RectTransform>();
        rectDica.anchorMin = new Vector2(1f, 0f);
        rectDica.anchorMax = new Vector2(1f, 0f);
        rectDica.pivot = new Vector2(1f, 0f);
        rectDica.anchoredPosition = new Vector2(-20f, 12f);
        rectDica.sizeDelta = new Vector2(500f, 30f);

        PreencherReferenciasDialogoUI(uiGo, caixa, nome, texto, dica);
        PreencherReferenciaManager(uiGo);
    }

    private static GameObject CriarCaixaDialogo(GameObject pai)
    {
        GameObject caixa = CriarFilho(pai, "CaixaDialogo");

        Image fundo = caixa.GetComponent<Image>();
        if (fundo == null)
        {
            fundo = caixa.AddComponent<Image>();
            fundo.color = new Color(0.16f, 0.16f, 0.2f, 0.94f);
        }

        RectTransform rect = caixa.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 40f);
        rect.sizeDelta = new Vector2(1600f, 260f);

        caixa.AddComponent<Outline>();
        caixa.SetActive(false);

        return caixa;
    }

    private static TMP_Text CriarTexto(GameObject pai, string nome, Vector2 anchorMin, Vector2 anchorMax,
                                       Vector2 posicao, int tamanho, TextAlignmentOptions alinhamento)
    {
        GameObject go = CriarFilho(pai, nome);
        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = posicao;
        rect.sizeDelta = new Vector2(200f, 40f);

        tmp.fontSize = tamanho;
        tmp.alignment = alinhamento;
        tmp.color = Color.white;
        tmp.text = string.Empty;

        return tmp;
    }

    private static GameObject CriarTextoCeil(GameObject pai, string nome)
    {
        GameObject go = CriarFilho(pai, nome);
        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.pivot = new Vector2(1f, 0f);

        tmp.fontSize = 24f;
        tmp.color = new Color(1f, 1f, 1f, 0.85f);
        tmp.text = string.Empty;

        return go;
    }

    private static GameObject CriarFilho(GameObject pai, string nome)
    {
        GameObject go = new GameObject(nome);
        Undo.RegisterCreatedObjectUndo(go, "Criar " + nome);
        go.transform.SetParent(pai.transform, false);
        return go;
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
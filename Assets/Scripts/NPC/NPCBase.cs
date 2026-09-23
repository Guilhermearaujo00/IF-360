using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NPCBase : MonoBehaviour
{
    private const string TAG_PLAYER = "Player";

    public string nomeNPC = "NPC";
    [TextArea] public string falaApresentacao = "Olá!";
    public string idMissao = "missao_padrao";
    public bool chamarMinijogo = true;

    [Header("Balão")]
    public float alturaBalao = 2.0f;
    public float escalaBalao = 1.0f;
    public float tempoExibicao = -1f;
    public float velDigitacao = 0.03f;

    private PlayerInputActions input;
    private bool jogadorPorPerto;
    private bool digitando;
    private Coroutine coroutineDigitacao;

    private GameObject objetoBalao;
    private Transform pontoBalao;
    private Text textoNome;
    private Text textoFala;
    private string textoCompleto;
    private float tempoAtual;
    private bool minijogoTentado;

    private void Awake()
    {
        CriarBalao();
    }

    private void OnEnable()
    {
        if (input == null) input = new PlayerInputActions();
        input.Enable();
        input.Player.Interact.performed += Interagir;
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.Player.Interact.performed -= Interagir;
            input.Disable();
        }
    }

    private void OnDestroy()
    {
        if (input != null)
        {
            input.Player.Interact.performed -= Interagir;
            input.Dispose();
        }
    }

    private void Update()
    {
        if (balaoVisivel() && Camera.main != null)
        {
            objetoBalao.transform.rotation = Camera.main.transform.rotation;
        }

        if (balaoVisivel() && tempoExibicao > 0f)
        {
            tempoAtual += Time.deltaTime;
            if (tempoAtual >= tempoExibicao && !digitando)
            {
                EsconderBalao();
                DispararMinijogoSeNecessario();
            }
        }
    }

    private void CriarBalao()
    {
        GameObject ponto = new GameObject("PontoBalão");
        ponto.transform.SetParent(transform, false);
        ponto.transform.localPosition = new Vector3(0f, alturaBalao, 0f);
        pontoBalao = ponto.transform;

        GameObject raiz = new GameObject("BalãoDialogo");
        raiz.transform.SetParent(pontoBalao, false);
        raiz.transform.localPosition = Vector3.zero;
        raiz.transform.localScale = Vector3.one * (escalaBalao * 2f / 1920f);
        objetoBalao = raiz;

        Canvas canvas = raiz.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 30;

        CanvasScaler scaler = raiz.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        RectTransform rtRaiz = raiz.GetComponent<RectTransform>();
        rtRaiz.sizeDelta = new Vector2(1920f, 1080f);

        GameObject fundoObj = new GameObject("Fundo");
        fundoObj.transform.SetParent(raiz.transform, false);
        Image fundo = fundoObj.AddComponent<Image>();
        fundo.color = new Color(1f, 1f, 1f, 0.95f);

        RectTransform rtFundo = fundo.GetComponent<RectTransform>();
        rtFundo.anchorMin = Vector2.zero;
        rtFundo.anchorMax = Vector2.one;
        rtFundo.offsetMin = Vector2.zero;
        rtFundo.offsetMax = Vector2.zero;

        GameObject nomeObj = new GameObject("TextoNome");
        nomeObj.transform.SetParent(raiz.transform, false);
        textoNome = nomeObj.AddComponent<Text>();
        ConfigurarFonte(textoNome);
        textoNome.fontSize = 46;
        textoNome.fontStyle = FontStyle.Bold;
        textoNome.color = new Color(0.1f, 0.1f, 0.1f);
        textoNome.alignment = TextAnchor.MiddleLeft;

        RectTransform rtNome = textoNome.rectTransform;
        rtNome.anchorMin = new Vector2(0f, 1f);
        rtNome.anchorMax = new Vector2(1f, 1f);
        rtNome.pivot = new Vector2(0.5f, 1f);
        rtNome.anchoredPosition = new Vector2(50f, -45f);
        rtNome.sizeDelta = new Vector2(-100f, 90f);

        GameObject falaObj = new GameObject("TextoFala");
        falaObj.transform.SetParent(raiz.transform, false);
        textoFala = falaObj.AddComponent<Text>();
        ConfigurarFonte(textoFala);
        textoFala.fontSize = 70;
        textoFala.color = new Color(0.1f, 0.1f, 0.1f);
        textoFala.alignment = TextAnchor.UpperLeft;
        textoFala.horizontalOverflow = HorizontalWrapMode.Wrap;
        textoFala.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rtFala = textoFala.rectTransform;
        rtFala.anchorMin = new Vector2(0f, 0f);
        rtFala.anchorMax = new Vector2(1f, 1f);
        rtFala.offsetMin = new Vector2(55f, 130f);
        rtFala.offsetMax = new Vector2(-55f, -170f);

        GameObject dicaObj = new GameObject("TextoDica");
        dicaObj.transform.SetParent(raiz.transform, false);
        Text dica = dicaObj.AddComponent<Text>();
        ConfigurarFonte(dica);
        dica.fontSize = 34;
        dica.color = new Color(0.35f, 0.35f, 0.35f, 0.8f);
        dica.text = "E para continuar";
        dica.alignment = TextAnchor.MiddleRight;

        RectTransform rtDica = dica.rectTransform;
        rtDica.anchorMin = new Vector2(0f, 0f);
        rtDica.anchorMax = new Vector2(1f, 0f);
        rtDica.pivot = new Vector2(1f, 0.5f);
        rtDica.anchoredPosition = new Vector2(-40f, 65f);
        rtDica.sizeDelta = new Vector2(400f, 55f);

        raiz.SetActive(false);
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag(TAG_PLAYER))
        {
            jogadorPorPerto = true;
            minijogoTentado = false;
            MostrarBalao();
        }
    }

    private void OnTriggerExit(Collider outro)
    {
        if (outro.CompareTag(TAG_PLAYER))
        {
            jogadorPorPerto = false;
            EsconderBalao();
        }
    }

    private void Interagir(InputAction.CallbackContext contexto)
    {
        if (!jogadorPorPerto) return;

        if (!balaoVisivel())
        {
            MostrarBalao();
            return;
        }

        if (digitando)
        {
            CompletarTexto();
            return;
        }

        EsconderBalao();
        DispararMinijogoSeNecessario();
    }

    private void MostrarBalao()
    {
        tempoAtual = 0f;
        textoNome.text = nomeNPC;
        textoCompleto = falaApresentacao;
        textoFala.text = "";
        objetoBalao.SetActive(true);
        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        coroutineDigitacao = StartCoroutine(RevelarTexto());
    }

    private System.Collections.IEnumerator RevelarTexto()
    {
        digitando = true;
        foreach (char c in textoCompleto)
        {
            textoFala.text += c;
            yield return new WaitForSecondsRealtime(velDigitacao);
        }
        digitando = false;
        coroutineDigitacao = null;
    }

    private void CompletarTexto()
    {
        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        textoFala.text = textoCompleto;
        digitando = false;
        coroutineDigitacao = null;
    }

    private void EsconderBalao()
    {
        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        if (objetoBalao != null) objetoBalao.SetActive(false);
        coroutineDigitacao = null;
        digitando = false;
    }

    private bool balaoVisivel()
    {
        return objetoBalao != null && objetoBalao.activeInHierarchy;
    }

    private void DispararMinijogoSeNecessario()
    {
        if (chamarMinijogo && !minijogoTentado)
        {
            minijogoTentado = true;
            IniciarMinijogo();
        }
    }

    protected virtual void IniciarMinijogo()
    {
        Debug.Log("Iniciando minijogo padrão: " + idMissao);
    }

    private static void ConfigurarFonte(Text texto)
    {
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (texto.font == null)
        {
            texto.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
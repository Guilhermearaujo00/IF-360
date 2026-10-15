namespace IF360
{
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Tela de carregamento para transições de cena.
/// </summary>
public class TelaCarregamento : MonoBehaviour
{
    public static TelaCarregamento Instancia { get; private set; }

    private GameObject painel;
    private TMP_Text textoStatus;
    private TMP_Text textoProgresso;
    private Canvas canvas;
    private Coroutine rotinaCarregamento;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarInstancia() => Instancia = null;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null) ConstruirUI();
    }

    private void OnLevelWasLoaded(int level)
    {
        // Reencontra canvas ao trocar de cena
        canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null && painel == null) ConstruirUI();
    }

    private void ConstruirUI()
    {
        if (painel != null) return;
        if (canvas == null) return;

        painel = new GameObject("TelaCarregamento", typeof(RectTransform), typeof(Image));
        painel.transform.SetParent(canvas.transform, false);
        var rt = painel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(1920f, 1080f);
        painel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.95f);

        // Texto "Carregando..."
        var goTexto = new GameObject("TextoCarregando", typeof(RectTransform));
        goTexto.transform.SetParent(painel.transform, false);
        var rtTexto = goTexto.GetComponent<RectTransform>();
        rtTexto.anchorMin = rtTexto.anchorMax = new Vector2(0.5f, 0.5f);
        rtTexto.pivot = new Vector2(0.5f, 0.5f);
        rtTexto.anchoredPosition = new Vector2(0f, 40f);
        rtTexto.sizeDelta = new Vector2(800f, 100f);
        var txt = goTexto.AddComponent<TextMeshProUGUI>();
        txt.text = "CARREGANDO";
        txt.fontSize = 48f;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;

        // Progresso / Dicas
        var goProg = new GameObject("Progresso", typeof(RectTransform));
        goProg.transform.SetParent(painel.transform, false);
        var rtProg = goProg.GetComponent<RectTransform>();
        rtProg.anchorMin = rtProg.anchorMax = new Vector2(0.5f, 0.5f);
        rtProg.pivot = new Vector2(0.5f, 0.5f);
        rtProg.anchoredPosition = new Vector2(0f, -60f);
        rtProg.sizeDelta = new Vector2(800f, 60f);
        textoProgresso = goProg.AddComponent<TextMeshProUGUI>();
        textoProgresso.text = "";
        textoProgresso.fontSize = 28f;
        textoProgresso.alignment = TextAlignmentOptions.Center;
        textoProgresso.color = new Color(0.8f, 0.8f, 0.8f);

        painel.SetActive(false);
    }

    /// <summary>
    /// Carrega uma cena de forma assíncrona mostrando a tela de carregamento.
    /// </summary>
    public void CarregarCena(string nomeCena, float tempoMinimo = 1f)
    {
        if (rotinaCarregamento != null) StopCoroutine(rotinaCarregamento);
        rotinaCarregamento = StartCoroutine(CarregarCenaRoutine(nomeCena, tempoMinimo));
    }

    private IEnumerator CarregarCenaRoutine(string nomeCena, float tempoMinimo)
    {
        // Garante UI construída
        if (painel == null)
        {
            canvas = Object.FindAnyObjectByType<Canvas>();
            ConstruirUI();
        }

        painel.SetActive(true);
        float tempoInicio = Time.realtimeSinceStartup;

        // Animação de "carregando..."
        var rotinaTexto = StartCoroutine(AnimarTextoCarregando());

        AsyncOperation async = SceneManager.LoadSceneAsync(nomeCena);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            float progresso = Mathf.Clamp01(async.progress / 0.9f);
            textoProgresso.text = $"Progresso: {progresso * 100f:F0}%";

            if (async.progress >= 0.9f)
            {
                float tempoDecorrido = Time.realtimeSinceStartup - tempoInicio;
                if (tempoDecorrido >= tempoMinimo)
                {
                    async.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        StopCoroutine(rotinaTexto);
        textoProgresso.text = "Concluído!";
        yield return new WaitForSecondsRealtime(0.3f);
        painel.SetActive(false);
    }

    private IEnumerator AnimarTextoCarregando()
    {
        string[] frames = { "CARREGANDO", "CARREGANDO.", "CARREGANDO..", "CARREGANDO..." };
        int idx = 0;
        var txtObj = painel?.transform.Find("TextoCarregando")?.GetComponent<TMP_Text>();

        while (painel != null && painel.activeSelf)
        {
            if (txtObj != null)
                txtObj.text = frames[idx];
            idx = (idx + 1) % frames.Length;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    public static void Carregar(string cena, float tempoMinimo = 1f)
    {
        if (Instancia != null) Instancia.CarregarCena(cena, tempoMinimo);
    }
}
}
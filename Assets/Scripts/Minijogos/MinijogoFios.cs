namespace IF360
{
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Minijogo "ligar os 3 fios" (vertical slice).
///
/// Autocontido: constrói a própria UI em runtime (reaproveitando o Canvas da cena)
/// e lê o teclado diretamente via Input System, sem alterar o PlayerInputActions.
///
/// Fluxo:
///   - Inscreve-se em NPCBaseNovo.MinijogoSolicitado; quando um NPC pede um
///     minijogo (IniciarMinijogo), a tela abre.
///   - 3 fios (coloridos) à esquerda e 3 tomadas (coloridas) à direita, em ordem
///     embaralhada. Escolha o fio com ←/→ (ou A/D) e ligue à tomada 1/2/3.
///   - Fio na cor certa -> trava a ligação; as 3 certas -> vitória (+pontos).
///   - Esc cancela.
///
/// Enquanto aberto, trava movimento/câmera (como o diálogo) e bloqueia o E do
/// PlayerInteraction (evita reabrir diálogo durante o desafio).
/// </summary>
public class MinijogoFios : MonoBehaviour
{
    public static MinijogoFios Instancia { get; private set; }

    [Header("Regras")]
    [Tooltip("Pontos concedidos ao concluir o minijogo.")]
    [SerializeField] private int premioPontos = 100;

    private static readonly Color[] Cores =
    {
        new Color(0.85f, 0.22f, 0.22f), // vermelho
        new Color(0.22f, 0.72f, 0.32f), // verde
        new Color(0.25f, 0.50f, 0.95f)  // azul
    };

    private readonly int[] tomadaDoFio = { 0, 1, 2 }; // tomada correta de cada fio
    private readonly int[] fioConectadoEm = { -1, -1, -1 }; // -1 = não conectado
    private int fioAtual;
    private bool ativo;
    private bool concluido;
    private float fimConclusao = -1f;
    private bool recompensaConcedida;

    private GameObject painel;
    private RectTransform[] quadradosFio;
    private RectTransform[] quadradosTomada;
    private TMP_Text textoResultado;

    public bool Ativo => ativo;

    // ---------- Ciclo de vida ----------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarEstatico() => Instancia = null;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(this);
            return;
        }

        Instancia = this;
    }

    private void OnEnable() => NPCBaseNovo.MinijogoSolicitado += Abrir;
    private void OnDisable() => NPCBaseNovo.MinijogoSolicitado -= Abrir;
    private void OnDestroy() { if (Instancia == this) Instancia = null; }

    private void Update()
    {
        if (!ativo) return;

        if (fimConclusao > 0f && Time.time >= fimConclusao)
        {
            Fechar(true);
            return;
        }

        if (concluido) return;

        Keyboard teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado.escapeKey.wasPressedThisFrame)
        {
            Fechar(false);
            return;
        }

        if (teclado.leftArrowKey.wasPressedThisFrame || teclado.aKey.wasPressedThisFrame)
        {
            MoverFio(-1);
        }
        else if (teclado.rightArrowKey.wasPressedThisFrame || teclado.dKey.wasPressedThisFrame)
        {
            MoverFio(1);
        }

        if (teclado.digit1Key.wasPressedThisFrame) Conectar(0);
        else if (teclado.digit2Key.wasPressedThisFrame) Conectar(1);
        else if (teclado.digit3Key.wasPressedThisFrame) Conectar(2);
    }

    // ---------- Entrada/saída do minijogo ----------

    private void Abrir(NPCBaseNovo npc) => Abrir();

    public void Abrir()
    {
        if (ativo) return;
        if (!ConstruirUI()) return;

        for (int i = 0; i < 3; i++) fioConectadoEm[i] = -1;
        fioAtual = 0;
        concluido = false;
        recompensaConcedida = false;
        fimConclusao = -1f;
        EmbaralharTomadas();
        AplicarCores();
        AtualizarVisual();

        textoResultado.text = "Escolha o fio e a tomada da mesma cor.";
        textoResultado.color = new Color(0.85f, 0.85f, 0.85f);

        painel.SetActive(true);
        ativo = true;
        TravarJogador(true);
    }

    private void Fechar(bool vitoria)
    {
        if (painel != null) painel.SetActive(false);
        ativo = false;
        concluido = false;
        fimConclusao = -1f;

        if (vitoria && recompensaConcedida)
        {
            GameManager.Instance?.AdicionarPontos(premioPontos);
        }

        TravarJogador(false);
    }

    // ---------- Regras ----------

    private void EmbaralharTomadas()
    {
        for (int i = 0; i < 3; i++) tomadaDoFio[i] = i;

        for (int i = 2; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (tomadaDoFio[i], tomadaDoFio[j]) = (tomadaDoFio[j], tomadaDoFio[i]);
        }

        // Garante que não seja idêntica à ordem original (senão é trivial 1-1-1).
        if (tomadaDoFio[0] == 0 && tomadaDoFio[1] == 1 && tomadaDoFio[2] == 2)
        {
            (tomadaDoFio[0], tomadaDoFio[1]) = (tomadaDoFio[1], tomadaDoFio[0]);
        }
    }

    private void Conectar(int indiceTomada)
    {
        if (fioConectadoEm[fioAtual] >= 0) return;

        if (tomadaDoFio[fioAtual] == indiceTomada)
        {
            fioConectadoEm[fioAtual] = indiceTomada;
            textoResultado.text = "Fio ligado!";
            textoResultado.color = new Color(0.4f, 0.95f, 0.4f);

            if (TodosConectados())
            {
                concluido = true;
                recompensaConcedida = true;
                fimConclusao = Time.time + 1.2f;
                textoResultado.text = $"Tudo ligado! +{premioPontos} pontos";
            }
            else
            {
                AvancarParaProximoFioLivre();
            }

            AtualizarVisual();
        }
        else
        {
            textoResultado.text = "Tomada errada — tente outra cor.";
            textoResultado.color = new Color(0.95f, 0.5f, 0.35f);
        }
    }

    private bool TodosConectados()
    {
        for (int i = 0; i < 3; i++)
        {
            if (fioConectadoEm[i] < 0) return false;
        }
        return true;
    }

    private void MoverFio(int direcao)
    {
        for (int passo = 0; passo < 3; passo++)
        {
            fioAtual = (fioAtual + direcao + 3) % 3;
            if (fioConectadoEm[fioAtual] < 0) break;
        }
        AtualizarVisual();
    }

    private void AvancarParaProximoFioLivre()
    {
        for (int passo = 0; passo < 3; passo++)
        {
            fioAtual = (fioAtual + 1) % 3;
            if (fioConectadoEm[fioAtual] < 0) break;
        }
    }

    // ---------- Visual ----------

    private void AplicarCores()
    {
        for (int i = 0; i < 3; i++)
        {
            Pintar(quadradosFio[i], Cores[i]);
        }

        for (int j = 0; j < 3; j++)
        {
            Pintar(quadradosTomada[j], Cores[tomadaDoFio[j]]);
        }
    }

    private void AtualizarVisual()
    {
        for (int i = 0; i < 3; i++)
        {
            bool ligado = fioConectadoEm[i] >= 0;
            float escala = (!ligado && i == fioAtual) ? 1.18f : 1f;
            quadradosFio[i].localScale = Vector3.one * escala;

            Image imagem = quadradosFio[i].GetComponent<Image>();
            Color cor = Cores[i];
            if (ligado) cor *= 0.55f;
            cor.a = 1f;
            if (imagem != null) imagem.color = cor;
        }

        for (int j = 0; j < 3; j++)
        {
            Image imagem = quadradosTomada[j].GetComponent<Image>();
            if (imagem == null) continue;
            Color cor = Cores[tomadaDoFio[j]];
            if (EstaTomadaLigada(j)) cor *= 0.55f;
            cor.a = 1f;
            imagem.color = cor;
        }
    }

    private bool EstaTomadaLigada(int tomada)
    {
        for (int i = 0; i < 3; i++)
        {
            if (fioConectadoEm[i] == tomada) return true;
        }
        return false;
    }

    private static void Pintar(RectTransform rt, Color cor)
    {
        Image imagem = rt.GetComponent<Image>();
        if (imagem != null) imagem.color = cor;
    }

    // ---------- Travamento de controle ----------

    private static void TravarJogador(bool travar)
    {
        PlayerController jogador = Object.FindAnyObjectByType<PlayerController>();
        if (jogador != null) jogador.TravarControle(travar);

        CameraController camera = Object.FindAnyObjectByType<CameraController>();
        if (camera != null) camera.TravarControle(travar);
    }

    // ---------- Construção da UI ----------

    private bool ConstruirUI()
    {
        if (painel != null) return true;

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("MinijogoFios: nenhum Canvas encontrado na cena.", this);
            return false;
        }

        painel = new GameObject("MinijogoFios", typeof(RectTransform), typeof(Image));
        painel.transform.SetParent(canvas.transform, false);

        RectTransform rt = painel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(760f, 480f);
        painel.GetComponent<Image>().color = new Color(0.07f, 0.07f, 0.11f, 0.98f);

        CriarTexto(painel.transform, "Titulo", "Ligue os 3 fios",
            new Vector2(0f, 196f), new Vector2(700f, 54f), 38f, TextAlignmentOptions.Center);

        CriarTexto(painel.transform, "RotuloFios", "FIO",
            new Vector2(-250f, 132f), new Vector2(160f, 34f), 22f, TextAlignmentOptions.Center);

        CriarTexto(painel.transform, "RotuloTomadas", "TOMADA",
            new Vector2(250f, 132f), new Vector2(200f, 34f), 22f, TextAlignmentOptions.Center);

        quadradosFio = new RectTransform[3];
        for (int i = 0; i < 3; i++)
        {
            quadradosFio[i] = CriarQuadrado(painel.transform, "Fio" + (i + 1),
                new Vector2(-250f, 72f - (i * 84f)), Cores[i]);
        }

        quadradosTomada = new RectTransform[3];
        for (int j = 0; j < 3; j++)
        {
            quadradosTomada[j] = CriarQuadrado(painel.transform, "Tomada" + (j + 1),
                new Vector2(250f, 72f - (j * 84f)), Cores[j]);
            CriarTexto(quadradosTomada[j], "Numero", (j + 1).ToString(),
                new Vector2(60f, 0f), new Vector2(40f, 40f), 26f, TextAlignmentOptions.Center);
        }

        CriarTexto(painel.transform, "Instrucao",
            "←/→ (ou A/D) escolhe o fio  •  1/2/3 liga à tomada  •  Esc cancela",
            new Vector2(0f, -204f), new Vector2(720f, 34f), 20f, TextAlignmentOptions.Center);

        textoResultado = CriarTexto(painel.transform, "Resultado", string.Empty,
            new Vector2(0f, -168f), new Vector2(720f, 40f), 24f, TextAlignmentOptions.Center);

        painel.SetActive(false);
        return true;
    }

    private static RectTransform CriarQuadrado(Transform pai, string nome, Vector2 pos, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(pai, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(96f, 60f);
        go.GetComponent<Image>().color = cor;
        return rt;
    }

    private static TMP_Text CriarTexto(Transform pai, string nome, string conteudo,
        Vector2 pos, Vector2 tamanho, float fontSize, TextAlignmentOptions alinhamento)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tamanho;

        TextMeshProUGUI texto = go.AddComponent<TextMeshProUGUI>();
        texto.text = conteudo;
        texto.fontSize = fontSize;
        texto.alignment = alinhamento;
        texto.color = Color.white;
        return texto;
    }
}
}

namespace IF360
{
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Minijogo "compilação do jogo" (Márcio, prédio do Ensino Superior — missão
/// do Núcleo, Q03 do plano original).
///
/// Autocontido: constrói a própria UI em runtime (reaproveitando o Canvas da
/// cena) e lê o teclado diretamente via Input System, no padrão do MinijogoFios.
///
/// Fluxo:
///   - Inscreve-se em NPCBaseNovo.MinijogoSolicitado; só abre para NPCs com
///     idMinijogo == "compilacao" (o NPC só pede o desafio com AUTORIZAÇÃO
///     central do QuestManager — Etapa 9 do plano);
///   - 3 estágios: uma fileira de quadrados coloridos segue um CICLO que se
///     repete; um quadrado está oculto ("?") e o jogador deduz a cor que falta
///     escolhendo entre 5 alternativas (teclas 1–5);
///   - Cronômetro por estágio (30/25/20 s). Tempo esgotado refaz o padrão do
///     estágio atual (sem punição permanente);
///   - Vitória final (3 estágios): REPORTE CENTRAL — o resultado é enviado ao
///     GameManager.RegistrarResultadoMinijogo, que valida missão/objetivo com o
///     QuestManager; pontos só quando o objetivo é aceito. Derrota/Esc não
///     registra nada (objetivo de etapa de minijogo só é concluído pela
///     vitória — fix do fluxo Márcio);
///   - Sempre começa do estágio 1: no modelo central este desafio corresponde
///     a UMA etapa única (Q03_COMPILACAO); não há mais retomada por passo;
///     Esc cancela.
///
/// Enquanto aberto, trava movimento/câmera (como o diálogo e o MinijogoFios).
/// </summary>
public class MinijogoCompilacao : MonoBehaviour
{
    public static MinijogoCompilacao Instancia { get; private set; }

    [Header("Regras")]
    [Tooltip("Pontos concedidos ao concluir os 3 estágios.")]
    [SerializeField] private int premioPontos = 150;

    // 6 cores distintas para os padrões e as alternativas.
    private static readonly Color[] Paleta =
    {
        new Color(0.85f, 0.22f, 0.22f), // vermelho
        new Color(0.22f, 0.72f, 0.32f), // verde
        new Color(0.25f, 0.50f, 0.95f), // azul
        new Color(0.95f, 0.80f, 0.20f), // amarelo
        new Color(0.20f, 0.80f, 0.85f), // ciano
        new Color(0.80f, 0.35f, 0.85f)  // magenta
    };

    // Cada estágio: (tamanho do ciclo de cores, quadrados exibidos, segundos).
    private static readonly (int ciclo, int slots, float segundos)[] Estagios =
    {
        (3, 6, 30f),
        (4, 8, 25f),
        (4, 8, 20f)
    };

    private NPCBaseNovo npcOrigem;
    private int estagio;
    private int[] padrao;           // índice de cor de cada quadrado
    private int slotOculto;         // posição do quadrado escondido
    private int corOculta;          // resposta correta (índice na Paleta)
    private int[] alternativas;     // 5 índices de cor, embaralhados
    private float fimDoTempo;       // instante do tempo limite do estágio
    private bool ativo;
    private bool concluido;
    private float fimConclusao = -1f;
    private bool recompensaConcedida;

    private GameObject painel;
    private Transform containerSlots;
    private readonly List<RectTransform> slotsAtuais = new List<RectTransform>();
    private RectTransform[] quadradosAlternativa;
    private TMP_Text textoEtapa;
    private TMP_Text textoTempo;
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

        if (Time.time >= fimDoTempo)
        {
            textoResultado.text = "Tempo esgotado! O padrão foi refeito — observe com calma.";
            textoResultado.color = new Color(0.95f, 0.5f, 0.35f);
            GerarPadrao();
            AtualizarVisual();
            return;
        }

        AtualizarTempoVisual();

        if (teclado.digit1Key.wasPressedThisFrame) Responder(0);
        else if (teclado.digit2Key.wasPressedThisFrame) Responder(1);
        else if (teclado.digit3Key.wasPressedThisFrame) Responder(2);
        else if (teclado.digit4Key.wasPressedThisFrame) Responder(3);
        else if (teclado.digit5Key.wasPressedThisFrame) Responder(4);
    }

    // ---------- Entrada/saída do minijogo ----------

    private void Abrir(NPCBaseNovo npc)
    {
        // Roteamento: este minijogo só abre para NPCs que pedem o desafio 'compilacao'.
        if (npc == null || npc.idMinijogo != "compilacao") return;

        npcOrigem = npc;
        Abrir();
    }

    public void Abrir()
    {
        if (ativo) return;
        if (!ConstruirUI()) return;

        concluido = false;
        recompensaConcedida = false;
        fimConclusao = -1f;

        // Sempre começa do primeiro estágio: no modelo central este desafio
        // corresponde a UMA etapa única da missão (Q03_COMPILACAO) — não há
        // mais retomada por "passo" da missão.
        estagio = 0;

        GerarPadrao();
        AtualizarVisual();

        textoResultado.text = "Complete o padrão com a cor que falta.";
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

        if (vitoria && recompensaConcedida && GameManager.Instance != null)
        {
            // REPORTE CENTRAL (Etapa 9): a vitória é registrada na autoridade
            // central (QuestManager/motor) — pontos só se o objetivo for aceito.
            // O minijogo NÃO conclui etapas nem missões por conta própria.
            var autorizacao = npcOrigem != null ? npcOrigem.AutorizacaoMinijogoAtual : null;
            if (autorizacao != null && autorizacao.Autorizado)
            {
                string motivo;
                if (GameManager.Instance.RegistrarResultadoMinijogo(
                        autorizacao.MissionId, autorizacao.ObjectiveId, true, out motivo))
                {
                    GameManager.Instance.AdicionarPontos(premioPontos);
                }
                else
                {
                    Debug.Log("[MISSION] Vitória da compilação não registrada: " + motivo);
                }
            }
            else
            {
                Debug.LogWarning("[MISSION] MinijogoCompilacao fechado com vitória sem autorização central — " +
                                 "pontos não concedidos.");
            }
        }

        npcOrigem = null;
        TravarJogador(false);
    }

    // ---------- Regras ----------

    private void Responder(int indice)
    {
        if (alternativas == null || indice >= alternativas.Length) return;

        if (alternativas[indice] == corOculta)
        {
            if (estagio >= Estagios.Length - 1)
            {
                concluido = true;
                recompensaConcedida = true;
                fimConclusao = Time.time + 1.5f;
                textoResultado.text = $"Compilação concluída! +{premioPontos} pontos";
                textoResultado.color = new Color(0.4f, 0.95f, 0.4f);
            }
            else
            {
                estagio++;
                GerarPadrao();
                AtualizarVisual();
                textoResultado.text = "Correto! Próxima etapa...";
                textoResultado.color = new Color(0.4f, 0.95f, 0.4f);
            }
        }
        else
        {
            textoResultado.text = "Não é essa — observe o ciclo de cores e tente de novo.";
            textoResultado.color = new Color(0.95f, 0.5f, 0.35f);
        }
    }

    private void GerarPadrao()
    {
        (int ciclo, int slots, float segundos) cfg = Estagios[estagio];

        // Embaralha a paleta; o ciclo usa as primeiras 'ciclo' cores.
        List<int> paleta = new List<int>();
        for (int i = 0; i < Paleta.Length; i++) paleta.Add(i);
        for (int i = paleta.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (paleta[i], paleta[j]) = (paleta[j], paleta[i]);
        }

        padrao = new int[cfg.slots];
        for (int i = 0; i < cfg.slots; i++)
        {
            padrao[i] = paleta[i % cfg.ciclo];
        }

        // Esconde um quadrado qualquer — a cor continua dedutível pelo ciclo
        // (cada cor aparece ao menos 2x na fileira).
        slotOculto = Random.Range(0, cfg.slots);
        corOculta = padrao[slotOculto];

        // 5 alternativas: a correta + 4 cores distintas, embaralhadas.
        List<int> opcoes = new List<int>();
        for (int i = 0; i < paleta.Count && opcoes.Count < 4; i++)
        {
            if (paleta[i] != corOculta) opcoes.Add(paleta[i]);
        }
        opcoes.Insert(Random.Range(0, opcoes.Count + 1), corOculta);
        alternativas = opcoes.ToArray();

        ReconstruirSlots(cfg.slots);
        fimDoTempo = Time.time + cfg.segundos;
    }

    // ---------- Visual ----------

    private void AtualizarVisual()
    {
        textoEtapa.text = $"Etapa {estagio + 1} de {Estagios.Length}";

        for (int i = 0; i < slotsAtuais.Count && i < padrao.Length; i++)
        {
            bool oculto = i == slotOculto;

            Image imagem = slotsAtuais[i].GetComponent<Image>();
            if (imagem != null)
            {
                imagem.color = oculto ? new Color(0.13f, 0.13f, 0.17f) : Paleta[padrao[i]];
            }

            TMP_Text rotulo = slotsAtuais[i].GetComponentInChildren<TMP_Text>();
            if (rotulo != null) rotulo.text = oculto ? "?" : "";
        }

        if (quadradosAlternativa == null) return;
        for (int j = 0; j < quadradosAlternativa.Length && j < alternativas.Length; j++)
        {
            Image imagem = quadradosAlternativa[j].GetComponent<Image>();
            if (imagem != null) imagem.color = Paleta[alternativas[j]];
        }
    }

    private void AtualizarTempoVisual()
    {
        float restante = Mathf.Max(0f, fimDoTempo - Time.time);
        textoTempo.text = $"Tempo: {Mathf.CeilToInt(restante)}s";
        textoTempo.color = restante <= 5f
            ? new Color(0.95f, 0.4f, 0.35f)
            : new Color(0.85f, 0.85f, 0.85f);
    }

    private void ReconstruirSlots(int quantidade)
    {
        for (int i = slotsAtuais.Count - 1; i >= 0; i--)
        {
            if (slotsAtuais[i] != null) Destroy(slotsAtuais[i].gameObject);
        }
        slotsAtuais.Clear();

        for (int i = 0; i < quantidade; i++)
        {
            float x = (i - (quantidade - 1) / 2f) * 80f;
            RectTransform rt = CriarQuadrado(containerSlots, "Slot" + (i + 1),
                new Vector2(x, 0f), 70f, 70f, Color.white);
            CriarTexto(rt, "Rotulo", "?", new Vector2(0f, 0f), new Vector2(60f, 60f), 44f,
                TextAlignmentOptions.Center);
            slotsAtuais.Add(rt);
        }
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
            Debug.LogError("MinijogoCompilacao: nenhum Canvas encontrado na cena.", this);
            return false;
        }

        painel = new GameObject("MinijogoCompilacao", typeof(RectTransform), typeof(Image), typeof(Outline));
        painel.transform.SetParent(canvas.transform, false);

        RectTransform rt = painel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(760f, 480f);
        var painelImg = painel.GetComponent<Image>();
        painelImg.color = new Color(0.10f, 0.10f, 0.14f, 0.95f); // #1A1A24F2
        painelImg.raycastTarget = false;

        var outline = painel.GetComponent<Outline>();
        outline.effectColor = new Color(0.19f, 0.19f, 0.27f, 0.90f); // #303045E6
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        outline.useGraphicAlpha = true;

        CriarTexto(painel.transform, "Titulo", "Compilação do Jogo",
            new Vector2(0f, 196f), new Vector2(700f, 54f), 38f, TextAlignmentOptions.Center);

        textoEtapa = CriarTexto(painel.transform, "Etapa", "Etapa 1 de 3",
            new Vector2(0f, 148f), new Vector2(700f, 34f), 24f, TextAlignmentOptions.Center);

        textoTempo = CriarTexto(painel.transform, "Tempo", string.Empty,
            new Vector2(0f, 110f), new Vector2(700f, 34f), 24f, TextAlignmentOptions.Center);

        GameObject container = new GameObject("Padrao", typeof(RectTransform));
        container.transform.SetParent(painel.transform, false);
        RectTransform crt = container.GetComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = new Vector2(0f, 30f);
        crt.sizeDelta = new Vector2(720f, 80f);
        containerSlots = container.transform;

        quadradosAlternativa = new RectTransform[5];
        for (int j = 0; j < 5; j++)
        {
            float x = (j - 2) * 104f;
            quadradosAlternativa[j] = CriarQuadrado(painel.transform, "Alternativa" + (j + 1),
                new Vector2(x, -110f), 84f, 64f, Color.white);
            CriarTexto(quadradosAlternativa[j], "Numero", (j + 1).ToString(),
                new Vector2(0f, -50f), new Vector2(40f, 34f), 22f, TextAlignmentOptions.Center);
        }

        CriarTexto(painel.transform, "Instrucao",
            "1–5 escolhe a cor que falta  •  Esc cancela",
            new Vector2(0f, -204f), new Vector2(720f, 34f), 20f, TextAlignmentOptions.Center);

        textoResultado = CriarTexto(painel.transform, "Resultado", string.Empty,
            new Vector2(0f, -160f), new Vector2(720f, 40f), 24f, TextAlignmentOptions.Center);

        painel.SetActive(false);
        return true;
    }

    private static RectTransform CriarQuadrado(Transform pai, string nome, Vector2 pos,
                                               float largura, float altura, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(pai, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(largura, altura);
        var img = go.GetComponent<Image>();
        img.color = cor;
        img.raycastTarget = false;
        return rt;
    }

    private static TMP_Text CriarTexto(Transform pai, string nome, string conteudo,
                                       Vector2 pos, Vector2 tamanho, float fontSize,
                                       TextAlignmentOptions alinhamento)
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
        texto.raycastTarget = false;

        if (texto.fontSharedMaterial != null)
        {
            var mat = new Material(texto.fontSharedMaterial);
            mat.name = texto.fontSharedMaterial.name + "_Underlay";
            mat.EnableKeyword("UNDERLAY_ON");
            mat.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.6f));
            mat.SetFloat("_UnderlayOffsetX", 1f);
            mat.SetFloat("_UnderlayOffsetY", -1f);
            mat.SetFloat("_UnderlayDilate", 0.15f);
            mat.SetFloat("_UnderlaySoftness", 0.25f);
            texto.fontMaterial = mat;
        }

        return texto;
    }
}
}

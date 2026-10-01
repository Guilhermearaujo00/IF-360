using System.Collections;
using UnityEngine;

/// <summary>
/// Controla TODA a conversa. Não mostra nada na tela (quem desenha é a
/// DialogueUI). Recebe os NPCs via evento estático NPCBaseNovo.DialogoSolicitado.
///
/// Máquina de estados:
///   NONE    -> sem diálogo aberto;
///   TYPING  -> uma fala está sendo digitada (E completa a digitação);
///   COMPLETE-> fala inteira visível (E avança para a próxima ou encerra);
///   CLOSING -> momento final, antes de voltar para NONE.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    private enum Estado
    {
        NONE,
        TYPING,
        COMPLETE,
        CLOSING
    }

    public static DialogueManager Instancia { get; private set; }

    [Header("UI (obrigatório)")]
    [Tooltip("Referência para a DialogueUI. Sem ela o diálogo não é exibido.")]
    [SerializeField] private DialogueUI ui;

    [Header("Extras (opcional)")]
    [Tooltip("Trava o movimento do jogador enquanto houver diálogo. Se vazio, é procurado automaticamente.")]
    [SerializeField] private PlayerController playerController;

    private NPCBaseNovo npcEmConversa;
    private DialogoData.FalaDialogo[] falas;
    private int indiceFala;
    private float velDigitacao = 0.03f;
    private Coroutine coroutineDigitacao;
    private Estado estado = Estado.NONE;

    public bool DialogoAtivo => estado != Estado.NONE;

    // Estratégia de acesso: singleton único por cena. Vários componentes não
    // relacionados entre si precisam chegar aqui (PlayerInteraction, futuras
    // UIs e NPCs), e cada cena carrega o seu próprio DialogueManager, então
    // um único Instancia com guarda anti-duplicidade é a solução mais simples.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarInstanciaEstatica()
    {
        Instancia = null;
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Debug.LogWarning("DialogueManager duplicado na cena — o segundo foi destruído. " +
                             "Mantenha um único por cena (criado pela ferramenta SetupSistemaDialogo).", this);
            if (ui != null) ui.Esconder();
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        NPCBaseNovo.DialogoSolicitado += IniciarDialogo;
    }

    private void OnDestroy()
    {
        NPCBaseNovo.DialogoSolicitado -= IniciarDialogo;
        if (Instancia == this) Instancia = null;
    }

    // ---------- Abertura ----------

    private void IniciarDialogo(NPCBaseNovo npc)
    {
        if (npc == null)
        {
            Debug.LogError("DialogueManager: IniciarDialogo recebeu um NPC nulo.", this);
            return;
        }

        if (estado != Estado.NONE)
        {
            Debug.LogWarning($"DialogueManager: '{npc.name}' pediu diálogo, mas já existe uma conversa ativa " +
                             $"com '{npcEmConversa?.name}'. Pedido ignorado.", this);
            return;
        }

        if (ui == null)
        {
            Debug.LogError("DialogueManager: referência 'ui' vazia. Atribua a DialogueUI no Inspector " +
                           "(ou rode Tools > IFNMG > Configurar Sistema de Diálogo).", this);
            return;
        }

        if (!npc.TemDialogoValido)
        {
            Debug.LogWarning($"DialogueManager: '{npc.name}' não tem diálogo válido. Pedido ignorado.", npc);
            return;
        }

        DialogoData.FalaDialogo[] lista = ResolverFalas(npc);
        if (lista == null || lista.Length == 0)
        {
            Debug.LogWarning($"DialogueManager: '{npc.name}' resolveu para zero falas. Pedido ignorado.", npc);
            return;
        }

        DialogoData dados = npc.dialogoData;
        if (dados != null)
        {
            // 0 = texto instantâneo, ou o valor escolhido no Inspector do SO.
            velDigitacao = dados.velDigitacao;
        }
        else
        {
            // Fala de reserva (sem ScriptableObject): mantém a digitação padrão.
            velDigitacao = 0.03f;
        }

        npcEmConversa = npc;
        falas = lista;
        indiceFala = 0;

        TravarJogador(true);
        ui.Mostrar();
        ui.MostrarIndicador(true);
        ExibirFalaAtual();
    }

    private void TravarJogador(bool travado)
    {
        if (playerController == null)
        {
            playerController = Object.FindAnyObjectByType<PlayerController>();
        }

        if (playerController != null)
        {
            playerController.TravarControle(travado);
        }
        else
        {
            Debug.LogWarning("DialogueManager: não encontrou PlayerController para " +
                             $"{(travado ? "travar" : "destravar")} o movimento durante a conversa.", this);
        }
    }

    private static DialogoData.FalaDialogo[] ResolverFalas(NPCBaseNovo npc)
    {
        DialogoData dados = npc.dialogoData;
        if (dados != null && dados.falas != null && dados.falas.Length > 0)
        {
            return dados.falas;
        }

        if (!string.IsNullOrWhiteSpace(npc.falaApresentacao))
        {
            DialogoData.FalaDialogo reserva = new DialogoData.FalaDialogo
            {
                nomeFalante = npc.nomeNPC,
                textoFala = npc.falaApresentacao
            };
            return new[] { reserva };
        }

        return null;
    }

    private string NomeDoFalante(DialogoData.FalaDialogo fala)
    {
        if (fala != null && !string.IsNullOrWhiteSpace(fala.nomeFalante))
        {
            return fala.nomeFalante;
        }

        return npcEmConversa != null ? npcEmConversa.nomeNPC : string.Empty;
    }

    // ---------- Avanço ----------

    public void Avancar()
    {
        if (estado == Estado.NONE) return;

        switch (estado)
        {
            case Estado.TYPING:
                CompletarDigitacao();
                break;

            case Estado.COMPLETE:
                ProximaFalaOuEncerrar();
                break;

            case Estado.CLOSING:
            default:
                // Durante o fechamento o avanço é ignorado (evita chamadas duplas).
                break;
        }
    }

    private void ProximaFalaOuEncerrar()
    {
        indiceFala++;

        if (indiceFala < falas.Length)
        {
            ExibirFalaAtual();
            return;
        }

        EncerrarDialogo();
    }

    // ---------- Digitação ----------

    private void ExibirFalaAtual()
    {
        if (coroutineDigitacao != null)
        {
            StopCoroutine(coroutineDigitacao);
            coroutineDigitacao = null;
        }

        estado = Estado.TYPING;

        DialogoData.FalaDialogo falaAgora = falas[indiceFala];
        ui.DefinirNome(NomeDoFalante(falaAgora));
        coroutineDigitacao = StartCoroutine(DigitarTexto(falaAgora.textoFala));
    }

    private IEnumerator DigitarTexto(string textoCompleto)
    {
        ui.DefinirTexto(string.Empty);
        ui.MostrarIndicador(false);

        int total = textoCompleto.Length;

        if (velDigitacao <= 0f)
        {
            ui.DefinirTexto(textoCompleto);
            FinalizarDigitacao();
            yield break;
        }

        for (int i = 1; i <= total; i++)
        {
            ui.DefinirTexto(textoCompleto.Substring(0, i));
            yield return new WaitForSeconds(velDigitacao);
        }

        FinalizarDigitacao();
    }

    private void CompletarDigitacao()
    {
        if (coroutineDigitacao != null)
        {
            StopCoroutine(coroutineDigitacao);
            coroutineDigitacao = null;
        }

        string falaAtual = falas[indiceFala].textoFala;
        ui.DefinirTexto(falaAtual);
        FinalizarDigitacao();
    }

    private void FinalizarDigitacao()
    {
        coroutineDigitacao = null;
        estado = Estado.COMPLETE;
        ui.MostrarIndicador(true);
    }

    // ---------- Encerramento ----------

    private void EncerrarDialogo()
    {
        if (estado == Estado.NONE) return;

        estado = Estado.CLOSING;

        if (coroutineDigitacao != null)
        {
            StopCoroutine(coroutineDigitacao);
            coroutineDigitacao = null;
        }

        ui.Esconder();
        ui.MostrarIndicador(false);

        NPCBaseNovo encerrado = npcEmConversa;
        npcEmConversa = null;
        falas = null;
        indiceFala = 0;

        estado = Estado.NONE;

        TravarJogador(false);

        // Notifica o NPC para o gancho AoEncerrarDialogo (chamarMinijogo e cia).
        // A checagem != null também cobre o caso do NPC ter sido destruído
        // contra Unity (objeto destruído compara com null).
        if (encerrado != null)
        {
            encerrado.NotificarDialogoEncerrado();
        }
    }
}
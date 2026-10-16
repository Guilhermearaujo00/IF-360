namespace IF360
{
using System;
using UnityEngine;

/// <summary>
/// Uma variante de conversa com condições. O NPC usa a PRIMEIRA variante cujas
/// condições forem satisfeitas. Deixe uma variante sem condições no fim para
/// servir de "fallback" (o diálogo que se repete).
/// </summary>
[Serializable]
public class VarianteDialogo
{
    [Tooltip("Diálogo mostrado quando as condições abaixo forem satisfeitas.")]
    public DialogoData dialogo;

    [Tooltip("Missão que precisa estar COMPLETA. Vazio = ignora esta condição.")]
    public string requisitoMissao = "";

    [Tooltip("Flag que precisa estar marcada (ex.: conversou_com_diretor). Vazio = ignora.")]
    public string requisitoFlag = "";

    [Tooltip("Passo mínimo exigido na missão (a 'requisitoMissao' ou o idMissao do NPC). -1 = ignora.")]
    public int requisitoEtapaMinima = -1;
}

/// <summary>
/// Representa UM NPC: quem ele é, qual é o seu diálogo, se ele está
/// disponível para interação e o que ele faz quando a conversa termina.
///
/// Este script NÃO cria interface, NÃO lê input e NÃO controla a conversa.
/// Ele se comunica com o mundo externo apenas por eventos estáticos:
///   - JogadorEntrouNaArea / JogadorSaiuDaArea -> o sistema de interação
///     do jogador sabe quais NPCs estão disponíveis;
///   - DialogoSolicitado -> o sistema de diálogo recebe este NPC e lê
///     nome/dados dele para abrir a conversa.
///
/// DEPENDÊNCIA: o tipo DialogoData precisa existir no projeto.
/// </summary>
public class NPCBaseNovo : MonoBehaviour, IInteragivel
{
    private const string TAG_PLAYER = "Player";

    // ---------- Comunicação com sistemas externos ----------

    /// <summary>Disparado quando o jogador entra na área de interação deste NPC.</summary>
    public static event Action<NPCBaseNovo> JogadorEntrouNaArea;

    /// <summary>Disparado quando o jogador sai da área (ou quando o NPC é desativado com o jogador dentro).</summary>
    public static event Action<NPCBaseNovo> JogadorSaiuDaArea;

    /// <summary>Disparado por Interagir(): o sistema de diálogo deve abrir a conversa deste NPC.</summary>
    public static event Action<NPCBaseNovo> DialogoSolicitado;

    /// <summary>Disparado por IniciarMinijogo(): o sistema de minijogo deve abrir o desafio.</summary>
    public static event Action<NPCBaseNovo> MinijogoSolicitado;

    // ---------- Dados configuráveis no Inspector ----------

    [Header("Identidade")]
    public string nomeNPC = "NPC";
    public string idMissao = "missao_padrao";
    [Tooltip("Missão que precisa estar COMPLETA para este NPC mostrar 'dialogoDataPosMissao'. Vazio = usa o próprio idMissao.")]
    public string missaoRequisito = "";

    [Header("Dados do diálogo")]
    [Tooltip("Dados do diálogo deste NPC (preferencial).")]
    public DialogoData dialogoData;
    [Tooltip("Conversa exibida DEPOIS que a missão (idMissao) deste NPC estiver completa.")]
    public DialogoData dialogoDataPosMissao;
    [Tooltip("Completar a missão (idMissao) automaticamente quando esta conversa terminar (1x).")]
    public bool completarMissaoAoTerminar;
    [Tooltip("Fala de reserva, usada enquanto o NPC ainda não tem DialogoData.")]
    [TextArea] public string falaApresentacao = "Olá!";

    [Header("Variantes de diálogo por condição (avaliadas em ordem)")]
    [Tooltip("Cada variante tem um diálogo + condições (missão/flag/etapa). " +
             "Vence a PRIMEIRA cujas condições baterem. Use uma última sem condições como fallback.")]
    public VarianteDialogo[] variantes;

    [Header("Registro de conversa (flags)")]
    [Tooltip("Ao terminar uma conversa, marca automaticamente a flag 'conversou_<idMissao>' " +
             "(ou o idFlagConversa abaixo). Outros NPCs podem exigir essa flag.")]
    public bool registrarConversa = true;
    [Tooltip("Nome da flag gravada ao conversar. Vazio = 'conversou_' + idMissao (ou nomeNPC).")]
    public string idFlagConversa = "";

    [Header("Ação após o diálogo")]
    public bool chamarMinijogo = true;

    [Tooltip("Se true, o minijogo é chamado como PARTE da missão (antes de completá-la). " +
             "Se false (padrão), o minijogo só é chamado APÓS a missão estar completa (ex.: Secretaria).")]
    public bool minijogoComoParteDaMissao = false;

    [Header("Modelo de missões por etapas (opcional)")]
    [Tooltip("Se true, ao terminar o diálogo conclui uma ETAPA da missão (GameManager.ConcluirEtapa) em vez de completar a missão inteira.")]
    public bool usarModeloEtapas = false;

    // ---------- Estado interno ----------

    private bool jogadorPorPerto;
    private bool minijogoTentado;

    public bool JogadorPorPerto => jogadorPorPerto;

    // IInteragivel: usado pelo PlayerInteraction para escolher o alvo mais próximo.
    public Transform Transform => transform;

    public bool TemDialogoValido =>
        DialogoParaConversa != null || !string.IsNullOrWhiteSpace(falaApresentacao);

    /// <summary>
    /// Conversa que o NPC deve mostrar AGORA, conforme o estado do jogo.
    /// Ordem de prioridade:
    ///   1) Primeira variante (de 'variantes') cujas condições estejam satisfeitas;
    ///   2) Retrocompatível: 'dialogoDataPosMissao' se a missão requisito estiver completa;
    ///   3) 'dialogoData' (fallback).
    /// </summary>
    public DialogoData DialogoParaConversa
    {
        get
        {
            if (variantes != null)
            {
                foreach (VarianteDialogo v in variantes)
                {
                    if (v == null || v.dialogo == null) continue;
                    if (CondicoesSatisfeitas(v)) return v.dialogo;
                }
            }

            if (dialogoDataPosMissao != null && MissaoRequisitoCompletaNoJogo)
            {
                return dialogoDataPosMissao;
            }

            return dialogoData;
        }
    }

    /// <summary>Flag gravada quando o jogador termina uma conversa com este NPC.</summary>
    public string IdFlagConversa
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(idFlagConversa)) return idFlagConversa;
            string baseId = !string.IsNullOrWhiteSpace(idMissao) ? idMissao : nomeNPC;
            return "conversou_" + baseId;
        }
    }

    private bool CondicoesSatisfeitas(VarianteDialogo v)
    {
        if (!string.IsNullOrWhiteSpace(v.requisitoMissao) && !MissaoCompletaJogo(v.requisitoMissao))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(v.requisitoFlag) && !FlagMarcadaNoJogo(v.requisitoFlag))
        {
            return false;
        }

        if (v.requisitoEtapaMinima >= 0)
        {
            string idEtapa = !string.IsNullOrWhiteSpace(v.requisitoMissao) ? v.requisitoMissao : idMissao;
            int passo = GameManager.Instance != null ? GameManager.Instance.ObterPassoMissao(idEtapa) : 0;
            if (passo < v.requisitoEtapaMinima) return false;
        }

        return true;
    }

    private static bool MissaoCompletaJogo(string id)
    {
        return GameManager.Instance != null && GameManager.Instance.MissaoCompleta(id);
    }

    private static bool FlagMarcadaNoJogo(string id)
    {
        return GameManager.Instance != null && GameManager.Instance.TemFlag(id);
    }

    /// <summary>Requisito para a conversa do "pós". Sem missaoRequisito, usa o próprio idMissao.</summary>
    private bool MissaoRequisitoCompletaNoJogo
    {
        get
        {
            string idVerificar = string.IsNullOrWhiteSpace(missaoRequisito) ? idMissao : missaoRequisito;
            return GameManager.Instance != null && GameManager.Instance.MissaoCompleta(idVerificar);
        }
    }

    private bool MissaoCompletaNoJogo =>
        GameManager.Instance != null && GameManager.Instance.MissaoCompleta(idMissao);

    // Evita eventos "fantasmas" quando o Enter Play Mode Options está com
    // Domain Reload desativado (os eventos estáticos sobreviveriam ao Play).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarEventosEstaticos()
    {
        JogadorEntrouNaArea = null;
        JogadorSaiuDaArea = null;
        DialogoSolicitado = null;
        MinijogoSolicitado = null;
    }

    private void Awake()
    {
        bool temTrigger = false;
        foreach (Collider c in GetComponents<Collider>())
        {
            if (c.isTrigger) { temTrigger = true; break; }
        }

        if (!temTrigger)
        {
            Debug.LogWarning($"NPCBaseNovo '{name}': nenhum Collider com 'Is Trigger' neste objeto — " +
                             "o jogador nunca será detectado como próximo.", this);
        }

        if (!TemDialogoValido)
        {
            Debug.LogWarning($"NPCBaseNovo '{name}': sem DialogoData e sem falaApresentacao — " +
                             "a interação será recusada.", this);
        }
    }

    // ---------- Proximidade ----------

    private void OnTriggerEnter(Collider outro)
    {
        if (!outro.CompareTag(TAG_PLAYER) || jogadorPorPerto) return;

        jogadorPorPerto = true;
        minijogoTentado = false;
        JogadorEntrouNaArea?.Invoke(this);
        Interacao.NotificarEntrada(this);
    }

    private void OnTriggerExit(Collider outro)
    {
        if (!outro.CompareTag(TAG_PLAYER) || !jogadorPorPerto) return;

        jogadorPorPerto = false;
        JogadorSaiuDaArea?.Invoke(this);
        Interacao.NotificarSaida(this);
    }

    private void OnDisable()
    {
        // NPC desativado/destruído com o jogador dentro: avisa quem estava
        // acompanhando este NPC para não manter uma referência inválida.
        if (!jogadorPorPerto) return;

        jogadorPorPerto = false;
        JogadorSaiuDaArea?.Invoke(this);
        Interacao.NotificarSaida(this);
    }

    // ---------- Interação ----------

    /// <summary>
    /// Ação de "conversar com este NPC". Chamada pelo sistema de interação
    /// do jogador. Só valida e solicita; quem abre e conduz a conversa é
    /// o sistema de diálogo, inscrito em DialogoSolicitado.
    /// </summary>
    public void Interagir()
    {
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning($"NPCBaseNovo '{name}': Interagir() chamado com o NPC desativado.", this);
            return;
        }

        if (!jogadorPorPerto)
        {
            Debug.LogWarning($"NPCBaseNovo '{name}': Interagir() chamado sem o jogador na área de interação.", this);
            return;
        }

        if (!TemDialogoValido)
        {
            Debug.LogError($"NPCBaseNovo '{name}': não há diálogo para exibir (DialogoData e falaApresentacao vazios).", this);
            return;
        }

        if (DialogoSolicitado == null)
        {
            Debug.LogError($"NPCBaseNovo '{name}': nenhum sistema de diálogo está inscrito em NPCBaseNovo.DialogoSolicitado.", this);
            return;
        }

        DialogoSolicitado.Invoke(this);
    }

    // ---------- Após o diálogo ----------

    /// <summary>
    /// Chamado pelo sistema de diálogo quando a conversa com este NPC
    /// termina normalmente. Repassa para o gancho protegido abaixo.
    /// </summary>
    public void NotificarDialogoEncerrado()
    {
        // Registra o "já conversei com este NPC" como flag do mundo, para que
        // outras variantes de outros NPCs possam exigir isso como condição.
        if (registrarConversa)
        {
            GameManager.Instance?.MarcarFlag(IdFlagConversa);
        }

        AoEncerrarDialogo();
    }

    /// <summary>Ação específica do NPC ao fim da conversa (sobrescreva em NPCs especiais).</summary>
    protected virtual void AoEncerrarDialogo()
    {
        bool missaoCompleta = MissaoCompletaNoJogo;

        // 1) Minijogo como PARTE da missão (antes de completá-la) — ex.: Administração, Núcleo.
        if (minijogoComoParteDaMissao && !missaoCompleta && !minijogoTentado)
        {
            minijogoTentado = true;
            IniciarMinijogo();
            return;
        }

        // 2) Minijogo pós-missão (padrão anterior) — ex.: Secretaria.
        if (chamarMinijogo && missaoCompleta && !minijogoTentado)
        {
            minijogoTentado = true;
            IniciarMinijogo();
            return;
        }

        // Conclui a própria missão apenas na "entrega": quando o requisito já está
        // cumprido (corrente de missões) ou quando não há requisito (pré -> pós direto).
        bool emEntrega = !string.IsNullOrWhiteSpace(missaoRequisito)
            ? MissaoRequisitoCompletaNoJogo
            : true;

        if (completarMissaoAoTerminar && !missaoCompleta && emEntrega)
        {
            if (usarModeloEtapas)
            {
                GameManager.Instance?.ConcluirEtapa(idMissao);
            }
            else
            {
                GameManager.Instance?.CompletarMissao(idMissao);
            }
            minijogoTentado = false;
        }
    }

    protected virtual void IniciarMinijogo()
    {
        if (MinijogoSolicitado == null)
        {
            Debug.LogWarning($"NPCBaseNovo '{name}': nenhum sistema de minijogo inscrito em NPCBaseNovo.MinijogoSolicitado.", this);
            return;
        }

        MinijogoSolicitado.Invoke(this);
    }
}
}

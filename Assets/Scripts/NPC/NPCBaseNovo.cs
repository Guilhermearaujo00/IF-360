using System;
using UnityEngine;

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
public class NPCBaseNovo : MonoBehaviour
{
    private const string TAG_PLAYER = "Player";

    // ---------- Comunicação com sistemas externos ----------

    /// <summary>Disparado quando o jogador entra na área de interação deste NPC.</summary>
    public static event Action<NPCBaseNovo> JogadorEntrouNaArea;

    /// <summary>Disparado quando o jogador sai da área (ou quando o NPC é desativado com o jogador dentro).</summary>
    public static event Action<NPCBaseNovo> JogadorSaiuDaArea;

    /// <summary>Disparado por Interagir(): o sistema de diálogo deve abrir a conversa deste NPC.</summary>
    public static event Action<NPCBaseNovo> DialogoSolicitado;

    // ---------- Dados configuráveis no Inspector ----------

    [Header("Identidade")]
    public string nomeNPC = "NPC";
    public string idMissao = "missao_padrao";

    [Header("Dados do diálogo")]
    [Tooltip("Dados do diálogo deste NPC (preferencial).")]
    public DialogoData dialogoData;
    [Tooltip("Fala de reserva, usada enquanto o NPC ainda não tem DialogoData.")]
    [TextArea] public string falaApresentacao = "Olá!";

    [Header("Ação após o diálogo")]
    public bool chamarMinijogo = true;

    // ---------- Estado interno ----------

    private bool jogadorPorPerto;
    private bool minijogoTentado;

    public bool JogadorPorPerto => jogadorPorPerto;

    public bool TemDialogoValido =>
        dialogoData != null || !string.IsNullOrWhiteSpace(falaApresentacao);

    // Evita eventos "fantasmas" quando o Enter Play Mode Options está com
    // Domain Reload desativado (os eventos estáticos sobreviveriam ao Play).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarEventosEstaticos()
    {
        JogadorEntrouNaArea = null;
        JogadorSaiuDaArea = null;
        DialogoSolicitado = null;
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
    }

    private void OnTriggerExit(Collider outro)
    {
        if (!outro.CompareTag(TAG_PLAYER) || !jogadorPorPerto) return;

        jogadorPorPerto = false;
        JogadorSaiuDaArea?.Invoke(this);
    }

    private void OnDisable()
    {
        // NPC desativado/destruído com o jogador dentro: avisa quem estava
        // acompanhando este NPC para não manter uma referência inválida.
        if (!jogadorPorPerto) return;

        jogadorPorPerto = false;
        JogadorSaiuDaArea?.Invoke(this);
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
        AoEncerrarDialogo();
    }

    /// <summary>Ação específica do NPC ao fim da conversa (sobrescreva em NPCs especiais).</summary>
    protected virtual void AoEncerrarDialogo()
    {
        if (!chamarMinijogo || minijogoTentado) return;

        minijogoTentado = true;
        IniciarMinijogo();
    }

    protected virtual void IniciarMinijogo()
    {
        Debug.Log("Iniciando minijogo padrão: " + idMissao);
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ÚNICO ouvinte da ação "Interagir" (E) de toda a cena.
///
/// Não decide o que é um diálogo, não é um NPC e não possui Trigger próprio:
///   - Os NPCs (NPCBaseNovo) avisam por evento estático quando o jogador sai
///     ou entra na área de interação deles;
///   - Ao apertar E com diálogo ativo -> repassa para o DialogueManager;
///   - Ao apertar E sem diálogo ativo -> conversa com o NPC ELEGÍVEL mais
///     próximo do jogador (se houver).
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    private readonly List<NPCBaseNovo> npcsElegiveis = new List<NPCBaseNovo>();
    private PlayerInputActions input;

    private void Awake()
    {
        input = new PlayerInputActions();
        input.Player.Interact.performed += AoPressionarInteragir;
    }

    private void OnEnable()
    {
        NPCBaseNovo.JogadorEntrouNaArea += AdicionarNpc;
        NPCBaseNovo.JogadorSaiuDaArea += RemoverNpc;

        if (input != null) input.Enable();
    }

    private void OnDisable()
    {
        NPCBaseNovo.JogadorEntrouNaArea -= AdicionarNpc;
        NPCBaseNovo.JogadorSaiuDaArea -= RemoverNpc;

        if (input != null) input.Disable();
        npcsElegiveis.Clear();
    }

    private void OnDestroy()
    {
        if (input == null) return;

        input.Player.Interact.performed -= AoPressionarInteragir;
        input.Dispose();
        input = null;
    }

    // ---------- Eventos dos NPCs ----------

    private void AdicionarNpc(NPCBaseNovo npc)
    {
        if (npc == null || npcsElegiveis.Contains(npc)) return;
        npcsElegiveis.Add(npc);
    }

    private void RemoverNpc(NPCBaseNovo npc)
    {
        if (npc == null) return;
        npcsElegiveis.Remove(npc);
    }

    // ---------- Input ----------

    private void AoPressionarInteragir(InputAction.CallbackContext contexto)
    {
        LimparInvalidos();

        DialogueManager dialogo = DialogueManager.Instancia;

        // Diálogo aberto: E é sempre do diálogo (avançar/fechar), nunca do NPC.
        if (dialogo != null && dialogo.DialogoAtivo)
        {
            dialogo.Avancar();
            return;
        }

        NPCBaseNovo alvo = NpcElegivelMaisProximo();
        if (alvo == null)
        {
            Debug.Log("Não há NPC próximo para interagir.");
            return;
        }

        alvo.Interagir();
    }

    // ---------- Seleção do alvo ----------

    private void LimparInvalidos()
    {
        for (int i = npcsElegiveis.Count - 1; i >= 0; i--)
        {
            if (npcsElegiveis[i] == null || !npcsElegiveis[i].JogadorPorPerto)
            {
                npcsElegiveis.RemoveAt(i);
            }
        }
    }

    private NPCBaseNovo NpcElegivelMaisProximo()
    {
        if (npcsElegiveis.Count == 0) return null;

        NPCBaseNovo maisProximo = null;
        float menorDistancia = float.MaxValue;

        foreach (NPCBaseNovo npc in npcsElegiveis)
        {
            if (npc == null) continue;

            float delta = Vector3.Distance(transform.position, npc.transform.position);
            if (delta < menorDistancia)
            {
                menorDistancia = delta;
                maisProximo = npc;
            }
        }

        return maisProximo;
    }
}
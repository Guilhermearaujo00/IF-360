namespace IF360
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ÚNICO ouvinte da ação "Interagir" (E) de toda a cena.
///
/// Não decide o que é um diálogo, não é um NPC e não possui Trigger próprio:
///   - NPCs e placas informativas (IInteragivel) avisam pelo canal estático
///     Interacao quando o jogador entra ou sai da área de interação deles;
///   - Ao apertar E com diálogo ativo -> repassa para o DialogueManager;
///   - Ao apertar E sem diálogo ativo -> interage com o alvo ELEGÍVEL mais
///     próximo do jogador (se houver).
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    private readonly List<IInteragivel> alvosElegiveis = new List<IInteragivel>();
    private PlayerInputActions input;

    private void Awake()
    {
        input = new PlayerInputActions();
        input.Player.Interact.performed += AoPressionarInteragir;
    }

    private void OnEnable()
    {
        Interacao.JogadorEntrouNaArea += AdicionarAlvo;
        Interacao.JogadorSaiuDaArea += RemoverAlvo;

        if (input != null) input.Enable();
    }

    private void OnDisable()
    {
        Interacao.JogadorEntrouNaArea -= AdicionarAlvo;
        Interacao.JogadorSaiuDaArea -= RemoverAlvo;

        if (input != null) input.Disable();
        alvosElegiveis.Clear();
    }

    private void OnDestroy()
    {
        if (input == null) return;

        input.Player.Interact.performed -= AoPressionarInteragir;
        input.Dispose();
        input = null;
    }

    // ---------- Eventos de proximidade ----------

    private void AdicionarAlvo(IInteragivel alvo)
    {
        if (AlvoNulo(alvo) || alvosElegiveis.Contains(alvo)) return;
        alvosElegiveis.Add(alvo);
    }

    private void RemoverAlvo(IInteragivel alvo)
    {
        if (alvo == null) return;
        alvosElegiveis.Remove(alvo);
    }

    // ---------- Input ----------

    private void AoPressionarInteragir(InputAction.CallbackContext contexto)
    {
        LimparInvalidos();

        // Minijogo aberto: E não deve interagir com o mundo (evita reabrir diálogo).
        if (MinijogoFios.Instancia != null && MinijogoFios.Instancia.Ativo)
        {
            return;
        }

        DialogueManager dialogo = DialogueManager.Instancia;

        // Diálogo aberto: E é sempre do diálogo (avançar/fechar), nunca do alvo.
        if (dialogo != null && dialogo.DialogoAtivo)
        {
            dialogo.Avancar();
            return;
        }

        IInteragivel alvo = AlvoElegivelMaisProximo();
        if (alvo == null)
        {
            Debug.Log("Não há nada próximo para interagir.");
            return;
        }

        alvo.Interagir();
    }

    // ---------- Seleção do alvo ----------

    private void LimparInvalidos()
    {
        for (int i = alvosElegiveis.Count - 1; i >= 0; i--)
        {
            if (AlvoNulo(alvosElegiveis[i]) || !alvosElegiveis[i].JogadorPorPerto)
            {
                alvosElegiveis.RemoveAt(i);
            }
        }
    }

    private IInteragivel AlvoElegivelMaisProximo()
    {
        if (alvosElegiveis.Count == 0) return null;

        IInteragivel maisProximo = null;
        float menorDistancia = float.MaxValue;

        foreach (IInteragivel alvo in alvosElegiveis)
        {
            if (AlvoNulo(alvo)) continue;

            float delta = Vector3.Distance(transform.position, alvo.Transform.position);
            if (delta < menorDistancia)
            {
                menorDistancia = delta;
                maisProximo = alvo;
            }
        }

        return maisProximo;
    }

    // Interface não usa a sobrecarga de == do UnityEngine.Object; um MonoBehaviour
    // destruído ainda "não seria null" via interface, então checamos explicitamente.
    private static bool AlvoNulo(IInteragivel alvo)
    {
        if (alvo == null) return true;
        return alvo is Object obj && obj == null;
    }
}
}

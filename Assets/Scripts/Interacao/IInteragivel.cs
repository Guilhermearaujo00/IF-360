namespace IF360
{
using UnityEngine;

/// <summary>
/// Algo com que o jogador pode interagir (E) quando está por perto:
/// NPCs, placas informativas etc. O PlayerInteraction é o único que lê o E
/// e conversa com os alvos apenas por esta abstração (ele não conhece NPCs).
/// </summary>
public interface IInteragivel
{
    /// <summary>True enquanto o jogador está dentro da área de interação.</summary>
    bool JogadorPorPerto { get; }

    /// <summary>Usado para escolher o alvo mais próximo quando há vários.</summary>
    Transform Transform { get; }

    /// <summary>Ação executada ao apertar E (ex.: abrir a conversa).</summary>
    void Interagir();
}
}

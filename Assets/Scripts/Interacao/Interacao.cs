namespace IF360
{
using System;
using UnityEngine;

/// <summary>
/// Canal estático de proximidade para interação. NPCs e placas avisam por
/// aqui quando o jogador entra/sai da área; o PlayerInteraction é o ÚNICO
/// inscrito e o único que lê o botão E.
/// </summary>
public static class Interacao
{
    public static event System.Action<IInteragivel> JogadorEntrouNaArea;
    public static event System.Action<IInteragivel> JogadorSaiuDaArea;

    public static void NotificarEntrada(IInteragivel alvo)
    {
        if (alvo != null) JogadorEntrouNaArea?.Invoke(alvo);
    }

    public static void NotificarSaida(IInteragivel alvo)
    {
        if (alvo != null) JogadorSaiuDaArea?.Invoke(alvo);
    }

    // Evita eventos "fantasmas" com o Domain Reload desativado.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarEventosEstaticos()
    {
        JogadorEntrouNaArea = null;
        JogadorSaiuDaArea = null;
    }
}
}

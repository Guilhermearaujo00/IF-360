namespace IF360
{
using System;
using UnityEngine;

/// <summary>
/// Define uma missão completa com suas etapas.
/// Cada etapa tem um id curto, nome exibível e descrição opcional.
/// </summary>
[CreateAssetMenu(fileName = "MissaoData", menuName = "IFNMG/Missão")]
public class MissaoData : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("ID único da missão (ex.: missao_administracao).")]
    public string idMissao;

    [Tooltip("Nome amigável mostrado no HUD.")]
    public string nome;

    [TextArea(2, 4)]
    [Tooltip("Descrição geral da missão.")]
    public string descricao;

    [Header("Etapas")]
    [Tooltip("Lista ordenada de etapas. O progresso avança de 0 até Length-1.")]
    public EtapaMissao[] etapas;

    /// <summary>
    /// Retorna a etapa atual com base no índice de progresso.
    /// </summary>
    public EtapaMissao ObterEtapaAtual(int indiceProgresso)
    {
        if (etapas == null || etapas.Length == 0) return default;
        int idx = Mathf.Clamp(indiceProgresso, 0, etapas.Length - 1);
        return etapas[idx];
    }

    /// <summary>
    /// Verifica se a missão está completa (todas as etapas feitas).
    /// </summary>
    public bool EstaCompleta(int indiceProgresso)
    {
        return etapas != null && indiceProgresso >= etapas.Length;
    }
}

[Serializable]
public struct EtapaMissao
{
    [Tooltip("ID curto da etapa (ex.: 'ligar_fios', 'entregar_quimica').")]
    public string idEtapa;

    [Tooltip("Nome exibido no HUD (ex.: 'Ligar 3 fios').")]
    public string nome;

    [TextArea(1, 3)]
    [Tooltip("Descrição do que o jogador deve fazer nesta etapa.")]
    public string descricao;
}
}
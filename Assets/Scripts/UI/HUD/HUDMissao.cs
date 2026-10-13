namespace IF360
{
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// HUD/jornada de missão: mostra o progresso (quantas missões já foram
/// concluídas) e, a cada conclusão, o nome da missão concluída.
/// Agora também exibe o passo atual da missão (modelo por etapas).
/// </summary>
public class HUDMissao : MonoBehaviour
{
    [Header("Textos (TextMeshPro)")]
    [Tooltip("Título fixo do painel (ex.: 'Jornada').")]
    [SerializeField] private TMP_Text textoTitulo;

    [Tooltip("Objetivo atual / último evento de missão.")]
    [SerializeField] private TMP_Text textoObjetivo;

    [Tooltip("Contador de progresso (ex.: '2 missões concluídas').")]
    [SerializeField] private TMP_Text textoProgresso;

    [Tooltip("Exibe a etapa atual da missão ativa (ex.: 'Etapa 2 de 3: Ligar 3 fios').")]
    [SerializeField] private TMP_Text textoEtapaAtual;

    [Header("Nomes amigáveis por id de missão")]
    [Tooltip("Traduz o idMissao (ex.: missao_diretor) para um nome exibido (ex.: 'Falar com o Diretor').")]
    [SerializeField] private List<EntradaMissao> nomesMissoes = new List<EntradaMissao>();

    private void OnEnable()
    {
        QuestManager.MissaoCompletada += AoCompletarMissao;
        QuestManager.EtapaConcluida += AoConcluirEtapa;
        AtualizarProgresso();
        AtualizarEtapaAtual(string.Empty);
    }

    private void OnDisable()
    {
        QuestManager.MissaoCompletada -= AoCompletarMissao;
        QuestManager.EtapaConcluida -= AoConcluirEtapa;
    }

    private void AoCompletarMissao(string idMissao)
    {
        if (textoObjetivo != null)
        {
            textoObjetivo.text = "Concluída: " + NomeDe(idMissao);
        }

        AtualizarProgresso();
        // Ao completar missão, limpa a etapa atual
        AtualizarEtapaAtual(string.Empty);
    }

    private void AoConcluirEtapa(string idMissao, int passoAtual)
    {
        if (textoEtapaAtual == null) return;

        string nomeMissao = NomeDe(idMissao);
        string nomeEtapa = string.Empty;

        // Tenta buscar o nome da etapa no MissaoData se houver referência
        var missaoData = BuscarMissaoData(idMissao);
        if (missaoData != null && missaoData.etapas != null && passoAtual > 0 && passoAtual <= missaoData.etapas.Length)
        {
            var etapa = missaoData.ObterEtapaAtual(passoAtual - 1);
            if (!string.IsNullOrEmpty(etapa.nome))
                nomeEtapa = etapa.nome;
        }

        if (!string.IsNullOrEmpty(nomeEtapa))
        {
            textoEtapaAtual.text = $"{nomeMissao} — Etapa {passoAtual}: {nomeEtapa}";
        }
        else
        {
            textoEtapaAtual.text = $"{nomeMissao} — Etapa {passoAtual}";
        }
    }

    private void AtualizarProgresso()
    {
        if (textoProgresso == null) return;

        int total = QuestManager.Instancia != null
            ? QuestManager.Instancia.QuantidadeMissoesCompletas()
            : 0;

        textoProgresso.text = total + (total == 1 ? " missão concluída" : " missões concluídas");
    }

    private void AtualizarEtapaAtual(string idMissao)
    {
        if (textoEtapaAtual == null) return;

        if (string.IsNullOrEmpty(idMissao))
        {
            textoEtapaAtual.text = string.Empty;
            return;
        }

        int passo = QuestManager.Instancia != null ? QuestManager.Instancia.ObterPassoAtual(idMissao) : 0;
        if (passo > 0)
        {
            AoConcluirEtapa(idMissao, passo);
        }
    }

    private string NomeDe(string idMissao)
    {
        foreach (EntradaMissao entrada in nomesMissoes)
        {
            if (entrada != null && entrada.idMissao == idMissao)
            {
                return string.IsNullOrWhiteSpace(entrada.nome) ? idMissao : entrada.nome;
            }
        }

        return idMissao;
    }

    private MissaoData BuscarMissaoData(string idMissao)
    {
        // Busca simples via Resources (assumindo que os assets estão em Resources/Missoes)
        // Para evitar alocação a cada frame, cache seria melhor, mas para HUD é aceitável.
        var all = Resources.LoadAll<MissaoData>("Missoes");
        foreach (var md in all)
        {
            if (md != null && md.idMissao == idMissao)
                return md;
        }
        return null;
    }

    [System.Serializable]
    public class EntradaMissao
    {
        public string idMissao;
        public string nome;
    }
}
}

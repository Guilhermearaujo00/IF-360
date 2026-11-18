namespace IF360
{
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using IF360.Missoes;

/// <summary>
/// HUD/jornada de missão — consome o ResumoProgresso da AUTORIDADE CENTRAL
/// (QuestManager), sem lógica de progressão própria:
///   - contador do catálogo central: "X de N missões concluídas" (N = Q01–Q04);
///   - missão ativa: etapa atual (ordem/total + descrição), pendentes da etapa
///     com o "faltam X de N" dinâmico (ex.: "Faltam 2 de 3: Gleice e Rosy.")
///     e a próxima ação orientada;
///   - sem missão ativa: aponta o iniciador da próxima missão do catálogo;
///   - eventos legados mantidos (MissaoIniciada/EtapaConcluida/
///     MissaoCompletada) — agora apenas acionam a atualização visual.
/// </summary>
public class HUDMissao : MonoBehaviour
{
    [Header("Textos (TextMeshPro)")]
    [Tooltip("Título fixo do painel (ex.: 'Jornada').")]
    [SerializeField] private TMP_Text textoTitulo;

    [Tooltip("Objetivo atual / último evento de missão (pendentes + próxima ação).")]
    [SerializeField] private TMP_Text textoObjetivo;

    [Tooltip("Contador de progresso (ex.: '2 de 4 missões concluídas').")]
    [SerializeField] private TMP_Text textoProgresso;

    [Tooltip("Exibe a etapa atual da missão ativa (ex.: 'Matrícula — Etapa 2 de 4: Ligar os fios').")]
    [SerializeField] private TMP_Text textoEtapaAtual;

    [Header("Nomes amigáveis por id de missão (fallback para ids legados)")]
    [Tooltip("Traduz um idMissao para um nome exibido. O catálogo central é consultado antes.")]
    [SerializeField] private List<EntradaMissao> nomesMissoes = new List<EntradaMissao>();

    private void OnEnable()
    {
        QuestManager.MissaoIniciada += AoIniciarMissao;
        QuestManager.MissaoCompletada += AoCompletarMissao;
        QuestManager.EtapaConcluida += AoConcluirEtapa;
        Atualizar();
    }

    private void OnDisable()
    {
        QuestManager.MissaoIniciada -= AoIniciarMissao;
        QuestManager.MissaoCompletada -= AoCompletarMissao;
        QuestManager.EtapaConcluida -= AoConcluirEtapa;
    }

    // ---------- Eventos (apenas re-exibem o estado central) ----------

    private void AoIniciarMissao(string idMissao)
    {
        if (textoObjetivo != null)
        {
            textoObjetivo.text = "Nova missão: " + NomeDe(idMissao);
        }
        Atualizar();
    }

    private void AoCompletarMissao(string idMissao)
    {
        if (textoObjetivo != null)
        {
            textoObjetivo.text = "Concluída: " + NomeDe(idMissao);
        }
        Atualizar();
    }

    private void AoConcluirEtapa(string idMissao, int ordemEtapa)
    {
        Atualizar();
    }

    // ---------- Atualização (lê o ResumoProgresso central) ----------

    private void Atualizar()
    {
        AtualizarProgresso();
        AtualizarMissaoAtiva();
    }

    private void AtualizarProgresso()
    {
        if (textoProgresso == null) return;

        var quest = QuestManager.Instancia;
        int concluidas = quest != null ? quest.QuantidadeMissoesCompletas() : 0;
        int total = quest != null ? quest.TotalMissoesCatalogo : 0;

        // "X de N" — N = missões estruturadas no catálogo central (Q01–Q04).
        textoProgresso.text = concluidas + " de " + total + " missões concluídas";
    }

    private void AtualizarMissaoAtiva()
    {
        var quest = QuestManager.Instancia;
        if (quest == null) return;

        var resumo = quest.ObterResumoProgresso();

        if (!resumo.TemMissaoAtiva)
        {
            if (textoObjetivo != null) textoObjetivo.text = "Nenhuma missão ativa.";
            if (textoEtapaAtual != null) textoEtapaAtual.text = MontarDicaProximaMissao(quest);
            return;
        }

        if (textoEtapaAtual != null)
        {
            textoEtapaAtual.text = resumo.MissionName + " — Etapa " + resumo.StepOrder +
                                   " de " + resumo.TotalEtapas + ": " + resumo.StepDescricao;
        }

        if (textoObjetivo != null)
        {
            string pendentes = string.IsNullOrEmpty(resumo.DescricaoPendentes)
                ? ""
                : resumo.DescricaoPendentes + "\n";
            textoObjetivo.text = pendentes + resumo.ProximaAcao;
        }
    }

    /// <summary>
    /// Sem missão ativa: aponta o INICIADOR da próxima missão não concluída do
    /// catálogo (a cadeia é linear Q01 → Q04, então a primeira incompleta é a
    /// próxima). Todas concluídas → mensagem de jornada completa.
    /// </summary>
    private string MontarDicaProximaMissao(QuestManager quest)
    {
        var missoes = quest.Motor.Catalogo.Missoes;
        for (int i = 0; i < missoes.Count; i++)
        {
            if (quest.ObterEstadoMissao(missoes[i].MissionId) == EstadoMissao.Completed) continue;

            var perfil = quest.ObterPerfil(missoes[i].StartingNPCId);
            string nome = perfil != null ? perfil.DisplayName : missoes[i].StartingNPCId;
            string local = perfil != null ? perfil.LocationName : "";

            return "Fale com " + nome + (string.IsNullOrEmpty(local) ? "" : " (" + local + ")") +
                   " para começar: " + missoes[i].MissionName + ".";
        }
        return "Jornada completa! Todas as missões foram concluídas.";
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

        // Fallback: nome da missão no catálogo central.
        var quest = QuestManager.Instancia;
        if (quest != null)
        {
            var defM = quest.Motor.Catalogo.ObterMissao(idMissao);
            if (defM != null) return defM.MissionName;
        }

        return idMissao;
    }

    [System.Serializable]
    public class EntradaMissao
    {
        public string idMissao;
        public string nome;
    }
}
}

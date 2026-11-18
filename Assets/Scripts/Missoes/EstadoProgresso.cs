using System;
using System.Collections.Generic;

namespace IF360.Missoes
{
    /// <summary>
    /// Formato v2 do estado de progresso (serializável pelo JsonUtility —
    /// somente List/campos simples, sem Dictionary).
    ///
    /// Repositório de verdade do progresso:
    ///   - missaoAtiva: id da única missão em andamento (vazio = nenhuma);
    ///   - missoesCompletas: ids definitivamente concluídos (inclui ids
    ///     legados preservados, ex.: missao_diretor);
    ///   - objetivosConcluidos: conjunto de objectiveIds concluídos — as
    ///     etapas concluídas e o estado de cada missão são DERIVADOS daqui;
    ///   - flags: flags do mundo (conversou_*) mantidas por compatibilidade
    ///     das variantes de diálogo.
    /// </summary>
    [Serializable]
    public class DadosProgressoSalvos
    {
        public int versao = 2;
        public string missaoAtiva = "";
        public List<string> missoesCompletas = new List<string>();
        public List<string> objetivosConcluidos = new List<string>();
        public List<string> flags = new List<string>();
    }

    /// <summary>
    /// Formato ANTIGO de save (QuestManager legado), usado apenas pela
    /// migração: flat + passo + flags → objetivos do modelo novo.
    /// </summary>
    [Serializable]
    public class DadosLegado
    {
        public List<string> idsCompletas = new List<string>();
        public List<string> missaoIds = new List<string>();
        public List<int> passoAtual = new List<int>();
        public List<string> idsFlags = new List<string>();
    }

    /// <summary>
    /// Consulta viva do progresso da missão ativa — alimenta o HUD e os
    /// diálogos dinâmicos (Etapa 5.1: dados centralizados e editáveis).
    /// </summary>
    public class ResumoProgresso
    {
        public bool TemMissaoAtiva;
        public string MissionId = "";
        public string MissionName = "";
        public EstadoMissao Estado = EstadoMissao.Locked;
        public string StepId = "";
        public string StepDescricao = "";
        public int StepOrder;
        public int TotalEtapas;

        /// <summary>Donos dos objetivos pendentes da etapa atual (nomes exibidos).</summary>
        public List<string> NomesNPCsPendentes = new List<string>();

        /// <summary>Localizações dos NPCs pendentes.</summary>
        public List<string> LocalizacoesPendentes = new List<string>();

        public int ObjetivosPendentes;
        public int ObjetivosTotaisEtapa;

        /// <summary>Ex.: "Faltam 2 de 3 lembretes: Gleice e Rosy."</summary>
        public string DescricaoPendentes = "";

        /// <summary>Ex.: "Converse com Roney na Administração." / "Retorne a Ananda."</summary>
        string _proximaAcao = "";
        public string ProximaAcao => _proximaAcao;

        public void DefinirProximaAcao(string texto) { _proximaAcao = texto ?? ""; }
    }

    /// <summary>Resultado do registro do encerramento de um diálogo (Evento 1 → 2 → 3 da Etapa 7).</summary>
    public class ResultadoEncerramento
    {
        public ResultadoInteracao Resultado = ResultadoInteracao.InformationalNpc;
        public bool MissaoIniciada;
        public string MissaoIniciadaId = "";
        public bool ObjetivoRegistrado;
        public string ObjetivoRegistradoId = "";
        public bool EtapaConcluida;
        public string EtapaConcluidaId = "";
        public bool MissaoFinalizada;
        public string MissaoFinalizadaId = "";
    }

    /// <summary>
    /// Autorização central para abrir um minijogo (Etapa 9). Sem isto, o
    /// minijogo permanece bloqueado; o resultado reportado só é aceito se
    /// a autorização corresponder à etapa atual.
    /// </summary>
    public class AutorizacaoMinijogo
    {
        public bool Autorizado;
        public string Motivo = "";
        public string NPCId = "";
        public string MissionId = "";
        public string StepId = "";
        public string ObjectiveId = "";
        public string IdMinijogo = "";
    }

    /// <summary>Uma fala gerada em runtime (diálogos dinâmicos), consumida pelo DialogueManager.</summary>
    [Serializable]
    public class FalaDinamica
    {
        public string NomeFalante = "";
        public string Texto = "";

        public FalaDinamica() { }

        public FalaDinamica(string nomeFalante, string texto)
        {
            NomeFalante = nomeFalante ?? "";
            Texto = texto ?? "";
        }
    }
}

using System.Collections.Generic;

namespace IF360.Missoes
{
    /// <summary>
    /// Um objetivo dentro de uma etapa (Etapa 4.2 do plano). Em etapas com
    /// objetivos paralelos, cada objetivo tem dono próprio e é contabilizado 1x.
    /// O estado "concluído" NÃO vive aqui: vive no conjunto central de
    /// objetivos concluídos (EstadoProgresso/MotorRegrasMissao), único
    /// repositório de verdade — o que garante consistência no salvamento.
    /// </summary>
    public class DefinicaoObjetivo
    {
        public string ObjectiveId { get; }
        public string OwnerNPCId { get; }
        public bool Required { get; }
        public string Descricao { get; }

        public DefinicaoObjetivo(string objectiveId, string ownerNPCId, bool required, string descricao)
        {
            ObjectiveId = objectiveId;
            OwnerNPCId = ownerNPCId;
            Required = required;
            Descricao = descricao ?? "";
        }
    }

    /// <summary>Regra que define quando a etapa está cumprida (Etapa 4.2).</summary>
    public enum RegraConclusaoEtapa
    {
        /// <summary>Todos os objetivos obrigatórios, em qualquer ordem (etapas simples e paralelas).</summary>
        AllObjectives = 0,

        /// <summary>Pelo menos um objetivo (reservado para missões futuras).</summary>
        AnyObjective = 1
    }

    /// <summary>
    /// Uma etapa da missão (Etapa 4). Etapas simples têm um NPC e um objetivo;
    /// etapas com objetivos paralelos têm vários donos simultaneamente
    /// autorizados enquanto a etapa estiver ativa.
    /// </summary>
    public class DefinicaoEtapa
    {
        public string StepId { get; }
        public string MissionId { get; }
        public int StepOrder { get; }

        /// <summary>Lista de NPCs autorizados a atuar nesta etapa (etapa simples: 1 NPC).</summary>
        public List<string> ExpectedNPCIds { get; }

        /// <summary>Como o objetivo da etapa é cumprido (diálogo ou minijogo).</summary>
        public TipoInteracaoEtapa ExpectedInteractionType { get; }

        /// <summary>Id do minijogo da etapa ('fios'/'compilacao'); vazio para etapas de diálogo.</summary>
        public string IdMinijogo { get; }

        /// <summary>Objetivos da etapa (etapas simples: 1; paralelas: N).</summary>
        public List<DefinicaoObjetivo> Objectives { get; }

        public RegraConclusaoEtapa CompletionRule { get; }

        /// <summary>Objetivos obrigatórios para concluir a etapa (derivado de Objectives quando vazio).</summary>
        public List<string> RequiredObjectives { get; }

        /// <summary>Id do evento disparado na conclusão da etapa (documentação/logs).</summary>
        public string CompletionEvent { get; }

        /// <summary>Próxima etapa (vazio/null na última etapa — a de finalização).</summary>
        public string NextStepId { get; }

        /// <summary>Descrição amigável usada em HUD e diálogos dinâmicos.</summary>
        public string Descricao { get; }

        public DefinicaoEtapa(string stepId, string missionId, int stepOrder,
                              IEnumerable<string> expectedNPCIds,
                              TipoInteracaoEtapa tipoInteracao,
                              string idMinijogo,
                              IEnumerable<DefinicaoObjetivo> objetivos,
                              RegraConclusaoEtapa regra = RegraConclusaoEtapa.AllObjectives,
                              string completionEvent = "",
                              string nextStepId = "",
                              string descricao = "")
        {
            StepId = stepId;
            MissionId = missionId;
            StepOrder = stepOrder;
            ExpectedNPCIds = new List<string>(expectedNPCIds ?? new string[0]);
            ExpectedInteractionType = tipoInteracao;
            IdMinijogo = idMinijogo ?? "";
            Objectives = new List<DefinicaoObjetivo>(objetivos ?? new DefinicaoObjetivo[0]);
            CompletionRule = regra;
            CompletionEvent = completionEvent ?? "";
            NextStepId = nextStepId ?? "";
            Descricao = descricao ?? "";

            RequiredObjectives = new List<string>();
            foreach (var obj in Objectives)
            {
                if (obj.Required) RequiredObjectives.Add(obj.ObjectiveId);
            }
        }
    }

    /// <summary>
    /// Definição completa de uma missão (Etapa 8 — dados de missão).
    /// A ÚLTIMA etapa de toda missão deve ser a etapa de FINALIZAÇÃO,
    /// pertencente ao NPC iniciador (só ele pode concluí-la definitivamente).
    /// </summary>
    public class DefinicaoMissao
    {
        public string MissionId { get; }
        public string MissionName { get; }
        public string Descricao { get; }
        public string StartingNPCId { get; }
        public string FinalNPCId { get; }

        /// <summary>Missões que precisam estar concluídas para esta ficar Available.</summary>
        public List<string> RequiredMissions { get; }

        /// <summary>Etapas ordenadas (StepOrder 1..N).</summary>
        public List<DefinicaoEtapa> Steps { get; }

        public DefinicaoMissao(string missionId, string missionName, string descricao,
                               string startingNPCId, string finalNPCId,
                               IEnumerable<string> requiredMissions,
                               IEnumerable<DefinicaoEtapa> etapas)
        {
            MissionId = missionId;
            MissionName = missionName;
            Descricao = descricao ?? "";
            StartingNPCId = startingNPCId;
            FinalNPCId = finalNPCId;
            RequiredMissions = new List<string>(requiredMissions ?? new string[0]);
            Steps = new List<DefinicaoEtapa>(etapas ?? new DefinicaoEtapa[0]);
        }

        /// <summary>Última etapa (a de finalização, do NPC iniciador).</summary>
        public DefinicaoEtapa UltimaEtapa => Steps.Count > 0 ? Steps[Steps.Count - 1] : null;
    }
}

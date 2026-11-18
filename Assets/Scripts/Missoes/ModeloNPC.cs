using System.Collections.Generic;

namespace IF360.Missoes
{
    /// <summary>
    /// Perfil de um NPC do catálogo (Etapa 2.4 do plano). Classe C# pura,
    /// sem UnityEngine, para ser testável em modo Edit (Etapa 8 — Testabilidade).
    ///
    /// A classificação é validada por identificadores (NPCId), nunca por
    /// comparação de nomes exibidos na interface.
    /// </summary>
    public class PerfilNPC
    {
        /// <summary>Identificador estável (ex.: NPC_MARCIO).</summary>
        public string NPCId { get; }

        /// <summary>Nome exibido na cena (ex.: "Márcio") — usado só para resolver o NPCId em runtime.</summary>
        public string DisplayName { get; }

        /// <summary>Localização narrativa (ex.: "Prédio do Ensino Superior").</summary>
        public string LocationName { get; }

        /// <summary>Categoria hierárquica (iniciador / subordinado / informativo).</summary>
        public TipoNPC Tipo { get; }

        /// <summary>Missão à qual o NPC pertence (vazio para informativos).</summary>
        public string AssociatedMissionId { get; }

        /// <summary>Ids de etapas nas quais este NPC pode atuar.</summary>
        public List<string> OwnedStepIds { get; }

        /// <summary>Ids de objetivos pertencentes a este NPC (etapas com objetivos paralelos).</summary>
        public List<string> OwnedObjectiveIds { get; }

        /// <summary>Somente iniciadores.</summary>
        public bool CanStartMission => Tipo == TipoNPC.MissionInitiator;

        /// <summary>Somente o iniciador da missão finaliza.</summary>
        public bool CanFinalizeMission => Tipo == TipoNPC.MissionInitiator;

        public PerfilNPC(string npcId, string displayName, string locationName, TipoNPC tipo,
                         string associatedMissionId,
                         IEnumerable<string> ownedStepIds = null,
                         IEnumerable<string> ownedObjectiveIds = null)
        {
            NPCId = npcId;
            DisplayName = displayName;
            LocationName = locationName;
            Tipo = tipo;
            AssociatedMissionId = associatedMissionId ?? "";
            OwnedStepIds = new List<string>(ownedStepIds ?? new string[0]);
            OwnedObjectiveIds = new List<string>(ownedObjectiveIds ?? new string[0]);
        }
    }
}

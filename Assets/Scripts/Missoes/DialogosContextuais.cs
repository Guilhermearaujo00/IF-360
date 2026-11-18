using System;
using System.Collections.Generic;
using System.Text;

namespace IF360.Missoes
{
    /// <summary>
    /// Diálogos dinâmicos (Situações A/B/C + MissingRequirements) gerados em
    /// runtime a partir do estado central do MotorRegrasMissao — C# puro,
    /// sem UnityEngine (Etapa 5.2 do plano).
    ///
    /// Quando usar diálogo DINÂMICO x VARIANTE legada:
    ///   - Dinâmico: NoActiveMission (C), AnotherMissionActive (A),
    ///     WrongNPCForCurrentStep (B), MissingRequirements;
    ///   - Variante legada (DialogoParaConversa): InteractionAllowed,
    ///     MissionReadyToTurnIn, MissionAlreadyCompleted,
    ///     ObjectiveAlreadyCompleted, InformationalNpc.
    ///
    /// Tokens substituíveis nos modelos abaixo:
    ///   {Jogador} {NPC} {ProximaAcao} {MissaoNPC} {Iniciador} {LocalIniciador}
    ///   {RequisitosPendentes} {Pendentes}
    /// </summary>
    public class DialogosContextuais
    {
        /// <summary>Nome do jogador usado nos diálogos existentes do projeto.</summary>
        public const string NomeJogador = "Caio";

        private readonly MotorRegrasMissao _motor;

        public DialogosContextuais(MotorRegrasMissao motor)
        {
            _motor = motor ?? throw new ArgumentNullException("motor");
        }

        /// <summary>Este resultado deve exibir diálogo dinâmico (em vez da variante legada)?</summary>
        public static bool PrecisaDialogoDinamico(ResultadoInteracao resultado)
        {
            switch (resultado)
            {
                case ResultadoInteracao.NoActiveMission:
                case ResultadoInteracao.AnotherMissionActive:
                case ResultadoInteracao.WrongNPCForCurrentStep:
                case ResultadoInteracao.MissingRequirements:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gera as falas dinâmicas do NPC para o resultado avaliado. Devolve null
        /// quando o resultado não usa diálogo dinâmico (quem chama deve recorrer
        /// à variante legada do NPC).
        /// </summary>
        public List<FalaDinamica> GerarFalas(ResultadoInteracao resultado, string npcId)
        {
            switch (resultado)
            {
                case ResultadoInteracao.AnotherMissionActive:
                    return SituacaoAOutraMissaoAtiva(npcId);
                case ResultadoInteracao.WrongNPCForCurrentStep:
                    return SituacaoBNPCErradoParaEtapa(npcId);
                case ResultadoInteracao.NoActiveMission:
                    return SituacaoCSemMissaoAtiva(npcId);
                case ResultadoInteracao.MissingRequirements:
                    return FalasRequisitosPendentes(npcId);
                default:
                    return null;
            }
        }

        // =====================================================================
        // Situação A — outra missão ativa
        // =====================================================================

        private List<FalaDinamica> SituacaoAOutraMissaoAtiva(string npcId)
        {
            var npc = _motor.Catalogo.ObterNPC(npcId);
            var resumo = _motor.ObterResumoProgresso();
            var valores = MontarValores(npc, resumo);

            var falas = new List<FalaDinamica>();
            falas.Add(new FalaDinamica("", "Oi, {Jogador}! Agora não é uma boa hora — você já está ocupado com outra atividade."));
            falas.Add(new FalaDinamica(NomeJogador, "É mesmo, desculpe!"));
            falas.Add(new FalaDinamica("", "Termine primeiro o que começou: {ProximaAcao} Quando terminar, volte para falarmos de {MissaoNPC}."));
            return Substituir(falas, valores);
        }

        // =====================================================================
        // Situação B — NPC errado para a etapa atual
        // =====================================================================

        private List<FalaDinamica> SituacaoBNPCErradoParaEtapa(string npcId)
        {
            var npc = _motor.Catalogo.ObterNPC(npcId);
            var resumo = _motor.ObterResumoProgresso();
            var valores = MontarValores(npc, resumo);

            var falas = new List<FalaDinamica>();
            falas.Add(new FalaDinamica("", "Isso não é comigo agora, {Jogador}."));
            if (!string.IsNullOrEmpty(resumo.ProximaAcao))
                falas.Add(new FalaDinamica("", "{ProximaAcao}"));
            falas.Add(new FalaDinamica(NomeJogador, "Certo! Já volto aqui depois."));
            return Substituir(falas, valores);
        }

        // =====================================================================
        // Situação C — subordinado sem missão ativa
        // =====================================================================

        private List<FalaDinamica> SituacaoCSemMissaoAtiva(string npcId)
        {
            var npc = _motor.Catalogo.ObterNPC(npcId);
            var valores = MontarValores(npc, null);

            var falas = new List<FalaDinamica>();
            if (npc != null)
            {
                var defM = _motor.Catalogo.ObterMissao(npc.AssociatedMissionId);
                if (defM != null)
                {
                    var estado = _motor.ObterEstadoMissao(defM.MissionId);
                    if (estado == EstadoMissao.Locked)
                    {
                        falas.Add(new FalaDinamica("", "Ainda não, {Jogador}... {Iniciador} está esperando você resolver outras atividades antes."));
                        falas.Add(new FalaDinamica("", "Continue explorando o campus e volte mais tarde, por favor."));
                    }
                    else
                    {
                        falas.Add(new FalaDinamica("", "Oi, {Jogador}! Agora não posso te ajudar com isso."));
                        falas.Add(new FalaDinamica("", "Procure primeiro {Iniciador} ({LocalIniciador}) — ele vai te explicar tudo."));
                    }
                }
            }
            if (falas.Count == 0)
                falas.Add(new FalaDinamica("", "Oi, {Jogador}! Não tenho nada para você agora."));
            falas.Add(new FalaDinamica(NomeJogador, "Combinado!"));
            return Substituir(falas, valores);
        }

        // =====================================================================
        // MissingRequirements — iniciador com cadeia incompleta
        // =====================================================================

        private List<FalaDinamica> FalasRequisitosPendentes(string npcId)
        {
            var npc = _motor.Catalogo.ObterNPC(npcId);
            var valores = MontarValores(npc, null);

            if (npc != null)
            {
                var defM = _motor.Catalogo.ObterMissao(npc.AssociatedMissionId);
                if (defM != null)
                {
                    valores["RequisitosPendentes"] = ListarRequisitosPendentes(defM);
                }
            }
            if (valores.ContainsKey("RequisitosPendentes") && string.IsNullOrEmpty(valores["RequisitosPendentes"]))
                valores["RequisitosPendentes"] = "há atividades pendentes no campus";

            var falas = new List<FalaDinamica>();
            falas.Add(new FalaDinamica("", "Olá, {Jogador}! Ainda não podemos conversar sobre isso."));
            falas.Add(new FalaDinamica("", "Você tem pendências antes: {RequisitosPendentes}."));
            falas.Add(new FalaDinamica(NomeJogador, "Combinado! Volto depois."));
            return Substituir(falas, valores);
        }

        /// <summary>Lista amigável das missões-requisito ainda não concluídas.</summary>
        private string ListarRequisitosPendentes(DefinicaoMissao defM)
        {
            var texto = new StringBuilder();
            for (int i = 0; i < defM.RequiredMissions.Count; i++)
            {
                var requisito = _motor.Catalogo.ObterMissao(defM.RequiredMissions[i]);
                if (requisito == null || _motor.MissaoCompleta(requisito.MissionId)) continue;
                if (texto.Length > 0) texto.Append(" e ");
                texto.Append(requisito.MissionName).Append(" (com ").Append(NomeDoNPC(requisito.StartingNPCId)).Append(")");
            }
            return texto.ToString();
        }

        // =====================================================================
        // Tokens
        // =====================================================================

        private Dictionary<string, string> MontarValores(PerfilNPC npc, ResumoProgresso resumo)
        {
            var valores = new Dictionary<string, string>();
            valores["Jogador"] = NomeJogador;
            valores["NPC"] = npc != null ? npc.DisplayName : "";
            valores["ProximaAcao"] = resumo != null && !string.IsNullOrEmpty(resumo.ProximaAcao)
                ? resumo.ProximaAcao.TrimEnd('.') + "."
                : "";

            if (npc != null)
            {
                var defM = _motor.Catalogo.ObterMissao(npc.AssociatedMissionId);
                if (defM != null)
                {
                    valores["MissaoNPC"] = defM.MissionName;
                    valores["Iniciador"] = NomeDoNPC(defM.StartingNPCId);
                    valores["LocalIniciador"] = LocalDoNPC(defM.StartingNPCId);
                }
            }
            return valores;
        }

        private string NomeDoNPC(string npcId)
        {
            var perfil = _motor.Catalogo.ObterNPC(npcId);
            return perfil != null ? perfil.DisplayName : npcId;
        }

        private string LocalDoNPC(string npcId)
        {
            var perfil = _motor.Catalogo.ObterNPC(npcId);
            return perfil != null ? perfil.LocationName : "";
        }

        private static List<FalaDinamica> Substituir(List<FalaDinamica> falas, Dictionary<string, string> valores)
        {
            for (int i = 0; i < falas.Count; i++)
            {
                var texto = falas[i].Texto;
                foreach (var par in valores)
                    texto = texto.Replace("{" + par.Key + "}", par.Value);
                falas[i].Texto = texto.Trim();
            }
            return falas;
        }
    }
}

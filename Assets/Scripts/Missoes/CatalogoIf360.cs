using System.Collections.Generic;

namespace IF360.Missoes
{
    /// <summary>
    /// Catálogo de missões e NPCs em memória (consultas por id e por nome exibido).
    /// Construído por CatalogoIf360.Criar(); testes podem montar catálogos
    /// próprios para validar regras sem depender de dados do jogo.
    /// </summary>
    public class CatalogoMissoes
    {
        public List<DefinicaoMissao> Missoes { get; } = new List<DefinicaoMissao>();
        public List<PerfilNPC> NPCs { get; } = new List<PerfilNPC>();

        private readonly Dictionary<string, DefinicaoMissao> _missoesPorId = new Dictionary<string, DefinicaoMissao>();
        private readonly Dictionary<string, PerfilNPC> _npcsPorId = new Dictionary<string, PerfilNPC>();
        private readonly Dictionary<string, PerfilNPC> _npcsPorNome = new Dictionary<string, PerfilNPC>();

        public void AdicionarMissao(DefinicaoMissao missao)
        {
            if (missao == null || _missoesPorId.ContainsKey(missao.MissionId)) return;
            Missoes.Add(missao);
            _missoesPorId[missao.MissionId] = missao;
        }

        public void AdicionarNPC(PerfilNPC npc)
        {
            if (npc == null || _npcsPorId.ContainsKey(npc.NPCId)) return;
            NPCs.Add(npc);
            _npcsPorId[npc.NPCId] = npc;
            if (!_npcsPorNome.ContainsKey(npc.DisplayName)) _npcsPorNome[npc.DisplayName] = npc;
        }

        public DefinicaoMissao ObterMissao(string missionId)
        {
            return !string.IsNullOrEmpty(missionId) && _missoesPorId.TryGetValue(missionId, out var m) ? m : null;
        }

        public PerfilNPC ObterNPC(string npcId)
        {
            return !string.IsNullOrEmpty(npcId) && _npcsPorId.TryGetValue(npcId, out var n) ? n : null;
        }

        /// <summary>Resolve pelo nome exibido na cena (nomeNPC) — match exato.</summary>
        public PerfilNPC ObterNPCPorNome(string nomeExibicao)
        {
            return !string.IsNullOrEmpty(nomeExibicao) && _npcsPorNome.TryGetValue(nomeExibicao, out var n) ? n : null;
        }

        public bool ConheceMissao(string missionId) => ObterMissao(missionId) != null;
        public bool ConheceNPC(string npcId) => ObterNPC(npcId) != null;
    }

    /// <summary>
    /// Dados oficiais do IF 360° (Etapa 6 do plano): 4 missões estruturadas
    /// (Q01–Q04) e 12 NPCs classificados (iniciadores, subordinados e
    /// informativos). NPCs legados (Diretor, Secretaria) NÃO estão aqui:
    /// ficam naturalmente neutros na validação central.
    ///
    /// IMPORTANTE: a última etapa de cada missão é a FINALIZAÇÃO do
    /// iniciador; enquanto ela for a etapa atual, a missão está em
    /// ReadyToTurnIn (aguardando validação final — Etapa 3.1).
    /// </summary>
    public static class CatalogoIf360
    {
        // ---------- Ids de missões ----------
        public const string MISSAO_ENSINO_MEDIO = "missao_ensino_medio";   // Q01
        public const string MISSAO_BIBLIOTECA = "missao_biblioteca";      // Q02
        public const string MISSAO_NUCLEO = "missao_nucleo";              // Q03
        public const string MISSAO_REFEITORIO = "missao_refeitorio";      // Q04

        // ---------- Ids de NPCs ----------
        public const string NPC_ANANDA = "NPC_ANANDA";
        public const string NPC_RONEY = "NPC_RONEY";
        public const string NPC_THIAGO = "NPC_THIAGO";
        public const string NPC_JP = "NPC_JP";
        public const string NPC_GLEICE = "NPC_GLEICE";
        public const string NPC_ROSY = "NPC_ROSY";
        public const string NPC_JOSY = "NPC_JOSY";
        public const string NPC_DAVID = "NPC_DAVID";
        public const string NPC_MARCIO = "NPC_MARCIO";
        public const string NPC_JOAO = "NPC_JOAO";
        public const string NPC_CLAUDIO = "NPC_CLAUDIO";
        public const string NPC_ALICE = "NPC_ALICE";

        /// <summary>Constrói o catálogo completo (chamado uma vez pelo QuestManager).</summary>
        public static CatalogoMissoes Criar()
        {
            var c = new CatalogoMissoes();

            AdicionarNPCs(c);
            AdicionarMissoes(c);
            return c;
        }

        private static void AdicionarNPCs(CatalogoMissoes c)
        {
            // ---- Iniciadores (iniciam E finalizam suas missões) ----
            c.AdicionarNPC(new PerfilNPC(NPC_ANANDA, "Ananda", "Prédio do Ensino Médio",
                TipoNPC.MissionInitiator, MISSAO_ENSINO_MEDIO,
                ownedStepIds: new[] { "Q01_APRESENTACAO", "Q01_FINALIZACAO" },
                ownedObjectiveIds: new[] { "Q01_APRESENTACAO", "Q01_FINALIZACAO" }));

            c.AdicionarNPC(new PerfilNPC(NPC_THIAGO, "Thiago", "Biblioteca",
                TipoNPC.MissionInitiator, MISSAO_BIBLIOTECA,
                ownedStepIds: new[] { "Q02_CADASTRO", "Q02_FINALIZACAO" },
                ownedObjectiveIds: new[] { "Q02_CADASTRO", "Q02_FINALIZACAO" }));

            c.AdicionarNPC(new PerfilNPC(NPC_JOSY, "Josy", "Gabinete de Docentes / Núcleo",
                TipoNPC.MissionInitiator, MISSAO_NUCLEO,
                ownedStepIds: new[] { "Q03_APRESENTACAO", "Q03_FINALIZACAO" },
                ownedObjectiveIds: new[] { "Q03_APRESENTACAO", "Q03_FINALIZACAO" }));

            c.AdicionarNPC(new PerfilNPC(NPC_JOAO, "João", "Refeitório",
                TipoNPC.MissionInitiator, MISSAO_REFEITORIO,
                ownedStepIds: new[] { "Q04_APRESENTACAO", "Q04_FINALIZACAO" },
                ownedObjectiveIds: new[] { "Q04_APRESENTACAO", "Q04_FINALIZACAO" }));

            // ---- Subordinados (executam somente suas etapas autorizadas) ----
            c.AdicionarNPC(new PerfilNPC(NPC_RONEY, "Roney", "Administração",
                TipoNPC.MissionSubordinate, MISSAO_ENSINO_MEDIO,
                ownedStepIds: new[] { "Q01_EXPLICACAO_SERVIDOR", "Q01_CONFIGURACAO_SERVIDOR" },
                ownedObjectiveIds: new[] { "Q01_EXPLICACAO_SERVIDOR", "Q01_SERVIDOR_CONECTADO" }));

            c.AdicionarNPC(new PerfilNPC(NPC_JP, "JP", "Laboratório de Química",
                TipoNPC.MissionSubordinate, MISSAO_BIBLIOTECA,
                ownedObjectiveIds: new[] { "REMINDER_CHEMISTRY" }));

            c.AdicionarNPC(new PerfilNPC(NPC_GLEICE, "Gleice", "Laboratório de Física",
                TipoNPC.MissionSubordinate, MISSAO_BIBLIOTECA,
                ownedObjectiveIds: new[] { "REMINDER_PHYSICS" }));

            c.AdicionarNPC(new PerfilNPC(NPC_ROSY, "Rosy", "Laboratório de Biológicas",
                TipoNPC.MissionSubordinate, MISSAO_BIBLIOTECA,
                ownedObjectiveIds: new[] { "REMINDER_BIOLOGY" }));

            c.AdicionarNPC(new PerfilNPC(NPC_DAVID, "David", "Laboratório Bridge (prédio EAD)",
                TipoNPC.MissionSubordinate, MISSAO_NUCLEO,
                ownedObjectiveIds: new[] { "COMPONENTS_RECEIVED" }));

            c.AdicionarNPC(new PerfilNPC(NPC_MARCIO, "Márcio", "Prédio do Ensino Superior",
                TipoNPC.MissionSubordinate, MISSAO_NUCLEO,
                ownedStepIds: new[] { "Q03_COMPILACAO" },
                ownedObjectiveIds: new[] { "COMPILATION" }));

            c.AdicionarNPC(new PerfilNPC(NPC_CLAUDIO, "Cláudio", "Posto de Vendas",
                TipoNPC.MissionSubordinate, MISSAO_REFEITORIO,
                ownedObjectiveIds: new[] { "TICKET_PURCHASED" }));

            // ---- Informativo ----
            c.AdicionarNPC(new PerfilNPC(NPC_ALICE, "Alice", "Assistência Estudantil",
                TipoNPC.InformationalNPC, ""));
        }

        private static void AdicionarMissoes(CatalogoMissoes c)
        {
            // ===================== Q01 — Matrícula e configuração do servidor =====================
            // Iniciadora e finalizadora: Ananda. Roney é subordinado (explica Administração/
            // Secretaria/SET e, ao fim do diálogo autorizado, o minijogo dos fios é liberado —
            // decisão aprovada: sem objeto "painel SET" na cena).
            c.AdicionarMissao(new DefinicaoMissao(
                MISSAO_ENSINO_MEDIO, "Matrícula no Ensino Médio",
                "Ananda recebe o jogador, explica o campus e a matrícula; Roney (Administração) " +
                "explica o problema do servidor no SET e libera o desafio dos 3 cabos; Ananda finaliza.",
                NPC_ANANDA, NPC_ANANDA,
                requiredMissions: new string[0],
                etapas: new[]
                {
                    new DefinicaoEtapa("Q01_APRESENTACAO", MISSAO_ENSINO_MEDIO, 1,
                        new[] { NPC_ANANDA }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("Q01_APRESENTACAO", NPC_ANANDA, true, "Receber as boas-vindas e a orientação de Ananda") },
                        completionEvent: "q01_apresentacao", nextStepId: "Q01_EXPLICACAO_SERVIDOR",
                        descricao: "Falar com Ananda no Prédio do Ensino Médio"),

                    new DefinicaoEtapa("Q01_EXPLICACAO_SERVIDOR", MISSAO_ENSINO_MEDIO, 2,
                        new[] { NPC_RONEY }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("Q01_EXPLICACAO_SERVIDOR", NPC_RONEY, true, "Ouvir Roney sobre o servidor (Administração/Secretaria/SET)") },
                        completionEvent: "q01_explicacao_servidor", nextStepId: "Q01_CONFIGURACAO_SERVIDOR",
                        descricao: "Falar com Roney na Administração"),

                    new DefinicaoEtapa("Q01_CONFIGURACAO_SERVIDOR", MISSAO_ENSINO_MEDIO, 3,
                        new[] { NPC_RONEY }, TipoInteracaoEtapa.Minijogo, "fios",
                        new[] { new DefinicaoObjetivo("Q01_SERVIDOR_CONECTADO", NPC_RONEY, true, "Conectar os 3 cabos do servidor (minijogo)") },
                        completionEvent: "q01_servidor_conectado", nextStepId: "Q01_FINALIZACAO",
                        descricao: "Conectar os três cabos do servidor (desafio de Roney)"),

                    new DefinicaoEtapa("Q01_FINALIZACAO", MISSAO_ENSINO_MEDIO, 4,
                        new[] { NPC_ANANDA }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("Q01_FINALIZACAO", NPC_ANANDA, true, "Retornar a Ananda para concluir a matrícula") },
                        completionEvent: "q01_finalizada", nextStepId: "",
                        descricao: "Retornar a Ananda para finalizar a matrícula")
                }));

            // ===================== Q02 — Cadastro e devolução de livros =====================
            // Iniciador e finalizador: Thiago. A ordem 2 é UMA etapa com 3 objetivos
            // paralelos (JP/Gleice/Rosy em qualquer ordem, 1x cada — Etapa 4.2).
            c.AdicionarMissao(new DefinicaoMissao(
                MISSAO_BIBLIOTECA, "Cadastro e Devolução de Livros",
                "Thiago cadastra o jogador e pede que avise JP, Gleice e Rosy sobre os livros " +
                "atrasados; Thiago confirma os três avisos e finaliza.",
                NPC_THIAGO, NPC_THIAGO,
                requiredMissions: new[] { MISSAO_ENSINO_MEDIO },
                etapas: new[]
                {
                    new DefinicaoEtapa("Q02_CADASTRO", MISSAO_BIBLIOTECA, 1,
                        new[] { NPC_THIAGO }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("Q02_CADASTRO", NPC_THIAGO, true, "Cadastro na Biblioteca com Thiago") },
                        completionEvent: "q02_cadastro", nextStepId: "Q02_LEMBRETES",
                        descricao: "Falar com Thiago na Biblioteca"),

                    new DefinicaoEtapa("Q02_LEMBRETES", MISSAO_BIBLIOTECA, 2,
                        new[] { NPC_JP, NPC_GLEICE, NPC_ROSY }, TipoInteracaoEtapa.Dialogo, "",
                        new[]
                        {
                            new DefinicaoObjetivo("REMINDER_CHEMISTRY", NPC_JP, true, "Avisar JP sobre o livro de Química"),
                            new DefinicaoObjetivo("REMINDER_PHYSICS", NPC_GLEICE, true, "Avisar Gleice sobre o livro de Física"),
                            new DefinicaoObjetivo("REMINDER_BIOLOGY", NPC_ROSY, true, "Avisar Rosy sobre o livro de Biologia")
                        },
                        completionEvent: "q02_lembretes", nextStepId: "Q02_FINALIZACAO",
                        descricao: "Avisar JP, Gleice e Rosy (em qualquer ordem)"),

                    new DefinicaoEtapa("Q02_FINALIZACAO", MISSAO_BIBLIOTECA, 3,
                        new[] { NPC_THIAGO }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("Q02_FINALIZACAO", NPC_THIAGO, true, "Confirmar os três avisos com Thiago") },
                        completionEvent: "q02_finalizada", nextStepId: "",
                        descricao: "Retornar a Thiago para finalizar")
                }));

            // ===================== Q03 — Projeto Display e compilação do jogo =====================
            // Iniciadora e finalizadora: Josy. Márcio somente após David registrar a
            // entrega (COMPONENTS_RECEIVED). O minijogo reporta apenas o objetivo
            // COMPILATION — nunca conclui a missão (Etapa 6/Q03, regra crítica).
            c.AdicionarMissao(new DefinicaoMissao(
                MISSAO_NUCLEO, "Projeto Display e Compilação do Jogo",
                "Josy apresenta o projeto; David entrega os componentes; Márcio orienta e libera " +
                "a compilação (3 estágios internos do minijogo); Josy reconhece e finaliza.",
                NPC_JOSY, NPC_JOSY,
                requiredMissions: new[] { MISSAO_BIBLIOTECA },
                etapas: new[]
                {
                    new DefinicaoEtapa("Q03_APRESENTACAO", MISSAO_NUCLEO, 1,
                        new[] { NPC_JOSY }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("Q03_APRESENTACAO", NPC_JOSY, true, "Conhecer o projeto com Josy") },
                        completionEvent: "q03_apresentacao", nextStepId: "Q03_COMPONENTES",
                        descricao: "Falar com Josy no Gabinete de Docentes"),

                    new DefinicaoEtapa("Q03_COMPONENTES", MISSAO_NUCLEO, 2,
                        new[] { NPC_DAVID }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("COMPONENTS_RECEIVED", NPC_DAVID, true, "Receber os componentes com David") },
                        completionEvent: "q03_componentes", nextStepId: "Q03_COMPILACAO",
                        descricao: "Buscar os componentes com David no laboratório Bridge"),

                    new DefinicaoEtapa("Q03_COMPILACAO", MISSAO_NUCLEO, 3,
                        new[] { NPC_MARCIO }, TipoInteracaoEtapa.Minijogo, "compilacao",
                        new[] { new DefinicaoObjetivo("COMPILATION", NPC_MARCIO, true, "Compilar o jogo com Márcio (3 estágios)") },
                        completionEvent: "q03_compilacao", nextStepId: "Q03_FINALIZACAO",
                        descricao: "Compilar o jogo com Márcio no Prédio do Superior"),

                    new DefinicaoEtapa("Q03_FINALIZACAO", MISSAO_NUCLEO, 4,
                        new[] { NPC_JOSY }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("Q03_FINALIZACAO", NPC_JOSY, true, "Apresentar o resultado a Josy") },
                        completionEvent: "q03_finalizada", nextStepId: "",
                        descricao: "Retornar a Josy para finalizar o projeto")
                }));

            // ===================== Q04 — Refeitório e aquisição de ticket =====================
            // Iniciador e finalizador: João. Cláudio vende o ticket (1x, sem duplicação).
            c.AdicionarMissao(new DefinicaoMissao(
                MISSAO_REFEITORIO, "Refeitório e Aquisição de Ticket",
                "João explica o Refeitório; Cláudio realiza a venda do ticket; João valida e finaliza.",
                NPC_JOAO, NPC_JOAO,
                requiredMissions: new[] { MISSAO_NUCLEO },
                etapas: new[]
                {
                    new DefinicaoEtapa("Q04_APRESENTACAO", MISSAO_REFEITORIO, 1,
                        new[] { NPC_JOAO }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("Q04_APRESENTACAO", NPC_JOAO, true, "Conhecer o Refeitório com João") },
                        completionEvent: "q04_apresentacao", nextStepId: "Q04_TICKET",
                        descricao: "Falar com João no Refeitório"),

                    new DefinicaoEtapa("Q04_TICKET", MISSAO_REFEITORIO, 2,
                        new[] { NPC_CLAUDIO }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("TICKET_PURCHASED", NPC_CLAUDIO, true, "Adquirir o ticket com Cláudio") },
                        completionEvent: "q04_ticket", nextStepId: "Q04_FINALIZACAO",
                        descricao: "Comprar o ticket com Cláudio no Posto de Vendas"),

                    new DefinicaoEtapa("Q04_FINALIZACAO", MISSAO_REFEITORIO, 3,
                        new[] { NPC_JOAO }, TipoInteracaoEtapa.Dialogo, "",
                        new[] { new DefinicaoObjetivo("Q04_FINALIZACAO", NPC_JOAO, true, "Apresentar o ticket a João") },
                        completionEvent: "q04_finalizada", nextStepId: "",
                        descricao: "Retornar a João para finalizar")
                }));
        }
    }
}

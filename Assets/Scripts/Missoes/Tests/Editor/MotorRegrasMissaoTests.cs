using System.Collections.Generic;
using IF360.Missoes;
using NUnit.Framework;

namespace IF360.Missoes.Tests
{
    /// <summary>
    /// Testes do MotorRegrasMissao (Etapa 8 do plano): cenários 1 a 11 +
    /// compatibilidade legada + migração + consistência + round-trip.
    ///
    /// C# puro — rodam em MODO EDIT, sem cena e sem MonoBehaviours.
    ///
    /// EXECUÇÃO: 23/23 PASSARAM em 2026-11-11, executados fora do Unity
    /// (compilados com o Roslyn do editor + NUnit do pacote, perfil 4.8,
    /// runner por reflexão no CLR do PowerShell). Antes disso, o Unity
    /// Test Runner apontou 2 falhas nos testes de "passo" legado — os
    /// EXPECTATIVOS dos testes estavam errados (ver decisão no
    /// RELATORIO_REESTRUTURACAO_MISSOES.md: passo legado = ordem da etapa
    /// ATUAL do fluxo, conforme os limiares requisitoEtapaMinima da cena).
    /// Confirmado no ambiente oficial em 2026-11-11: 23/23 verdes no
    /// Unity Test Runner (EditMode > Run All).
    /// </summary>
    [TestFixture]
    public class MotorRegrasMissaoTests
    {
        private MotorRegrasMissao motor;

        [SetUp]
        public void Preparar()
        {
            motor = new MotorRegrasMissao(CatalogoIf360.Criar());
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        /// <summary>Conduz a Q01 inteira: início → Roney → fios (vitória) → finalização.</summary>
        private void ConcluirQ01()
        {
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);   // início + apresentação
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_RONEY);    // explicação do servidor
            string motivo;
            motor.RegistrarResultadoMinijogo(CatalogoIf360.MISSAO_ENSINO_MEDIO,
                "Q01_SERVIDOR_CONECTADO", true, out motivo);                // vitória dos fios
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);   // finalização
        }

        /// <summary>Conduz a Q02 inteira (lembretes na ordem JP → Gleice → Rosy).</summary>
        private void ConcluirQ02()
        {
            ConcluirQ01();
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_THIAGO);   // início + cadastro
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_JP);
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_GLEICE);
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ROSY);
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_THIAGO);   // finalização
        }

        // =====================================================================
        // Cenário 1 — subordinado NÃO inicia missão (só o iniciador inicia)
        // =====================================================================

        [Test]
        public void Cenario01_SubordinadoNaoIniciaMissao()
        {
            Assert.AreEqual(ResultadoInteracao.NoActiveMission,
                motor.AvaliarInteracao(CatalogoIf360.NPC_RONEY));

            ResultadoEncerramento r = motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_RONEY);

            Assert.IsFalse(r.MissaoIniciada);
            Assert.IsFalse(r.ObjetivoRegistrado);
            Assert.IsFalse(motor.TemMissaoAtiva);
        }

        // =====================================================================
        // Cenário 2 — o iniciador inicia a própria missão (quando disponível)
        // =====================================================================

        [Test]
        public void Cenario02_IniciadorIniciaMissao()
        {
            Assert.AreEqual(ResultadoInteracao.InteractionAllowed,
                motor.AvaliarInteracao(CatalogoIf360.NPC_ANANDA));

            ResultadoEncerramento r = motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);

            Assert.IsTrue(r.MissaoIniciada);
            Assert.AreEqual(CatalogoIf360.MISSAO_ENSINO_MEDIO, motor.MissaoAtiva);
            Assert.IsTrue(r.ObjetivoRegistrado);
            Assert.AreEqual("Q01_APRESENTACAO", r.ObjetivoRegistradoId);
            Assert.IsTrue(r.EtapaConcluida);
            Assert.AreEqual(EstadoMissao.InProgress,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));
        }

        // =====================================================================
        // Cenário 3 — UMA missão ativa por vez
        // =====================================================================

        [Test]
        public void Cenario03_UmaMissaoAtivaPorVez()
        {
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA); // Q01 ativa

            // Thiago é iniciador de Q02, mas Q02 está bloqueada e há outra ativa.
            Assert.AreEqual(ResultadoInteracao.AnotherMissionActive,
                motor.AvaliarInteracao(CatalogoIf360.NPC_THIAGO));

            ResultadoEncerramento r = motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_THIAGO);
            Assert.IsFalse(r.MissaoIniciada);
            Assert.AreEqual(CatalogoIf360.MISSAO_ENSINO_MEDIO, motor.MissaoAtiva);
        }

        // =====================================================================
        // Cenário 4 — cadeia de requisitos (Locked → Available)
        // =====================================================================

        [Test]
        public void Cenario04_CadeiaDeMissoes()
        {
            Assert.AreEqual(EstadoMissao.Locked, motor.ObterEstadoMissao(CatalogoIf360.MISSAO_BIBLIOTECA));
            Assert.AreEqual(ResultadoInteracao.MissingRequirements,
                motor.AvaliarInteracao(CatalogoIf360.NPC_THIAGO));

            ConcluirQ01();

            Assert.AreEqual(EstadoMissao.Available, motor.ObterEstadoMissao(CatalogoIf360.MISSAO_BIBLIOTECA));
            Assert.AreEqual(ResultadoInteracao.InteractionAllowed,
                motor.AvaliarInteracao(CatalogoIf360.NPC_THIAGO));
        }

        // =====================================================================
        // Cenário 5 — etapas em sequência: ninguém avança fora da ordem
        // =====================================================================

        [Test]
        public void Cenario05_EtapasEmSequenciaSemPular()
        {
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA); // etapa 2 (Roney)

            // Ananda só tem a finalização (pendente em OUTRA etapa) → errada agora.
            Assert.AreEqual(ResultadoInteracao.WrongNPCForCurrentStep,
                motor.AvaliarInteracao(CatalogoIf360.NPC_ANANDA));

            // Subordinado de OUTRA missão durante Q01 ativa.
            Assert.AreEqual(ResultadoInteracao.AnotherMissionActive,
                motor.AvaliarInteracao(CatalogoIf360.NPC_DAVID));

            // Roney é o esperado da etapa atual.
            Assert.AreEqual(ResultadoInteracao.InteractionAllowed,
                motor.AvaliarInteracao(CatalogoIf360.NPC_RONEY));
        }

        // =====================================================================
        // Cenário 6 — objetivos paralelos (Q02): qualquer ordem, 1x cada,
        //             "faltam X de 3" dinâmico
        // =====================================================================

        [Test]
        public void Cenario06_ObjetivosParalelosQualquerOrdem()
        {
            ConcluirQ01();
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_THIAGO); // início + cadastro

            ResumoProgresso resumo = motor.ObterResumoProgresso();
            Assert.AreEqual(3, resumo.ObjetivosPendentes);
            Assert.AreEqual(3, resumo.ObjetivosTotaisEtapa);
            Assert.IsTrue(resumo.DescricaoPendentes.StartsWith("Faltam 3 de 3"));
            Assert.IsTrue(resumo.ProximaAcao.Contains("Converse com"));

            // Ordem INVERSA: Rosy → Gleice → JP (a etapa aceita qualquer ordem).
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ROSY);

            resumo = motor.ObterResumoProgresso();
            Assert.AreEqual(2, resumo.ObjetivosPendentes);
            Assert.IsTrue(resumo.DescricaoPendentes.StartsWith("Faltam 2 de 3"));
            Assert.IsTrue(resumo.NomesNPCsPendentes.Contains("JP"));
            Assert.IsTrue(resumo.NomesNPCsPendentes.Contains("Gleice"));
            Assert.IsFalse(resumo.NomesNPCsPendentes.Contains("Rosy"));

            // Repetir Rosy: objetivo já concluído, nada é registrado de novo.
            ResultadoEncerramento dup = motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ROSY);
            Assert.AreEqual(ResultadoInteracao.ObjectiveAlreadyCompleted, dup.Resultado);
            Assert.IsFalse(dup.ObjetivoRegistrado);

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_GLEICE);
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_JP);

            // Etapa de lembretes concluída → última etapa → aguardando finalizador.
            Assert.AreEqual(EstadoMissao.ReadyToTurnIn,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_BIBLIOTECA));
        }

        // =====================================================================
        // Cenário 7 — NPC informativo (Alice) nunca altera progressão
        // =====================================================================

        [Test]
        public void Cenario07_NpcInformativoNuncaAlteraProgresso()
        {
            Assert.AreEqual(ResultadoInteracao.InformationalNpc,
                motor.AvaliarInteracao(CatalogoIf360.NPC_ALICE));

            ResultadoEncerramento r = motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ALICE);

            Assert.IsFalse(r.MissaoIniciada);
            Assert.IsFalse(r.ObjetivoRegistrado);
            Assert.IsFalse(r.MissaoFinalizada);
            Assert.IsFalse(motor.TemMissaoAtiva);
        }

        // =====================================================================
        // Cenário 8 — NPC fora do catálogo (Diretor/Secretaria): neutro e
        //             SEM autorização de minijogo
        // =====================================================================

        [Test]
        public void Cenario08_NpcForaDoCatalogoEhNeutro()
        {
            Assert.AreEqual(ResultadoInteracao.InformationalNpc, motor.AvaliarInteracao("NPC_SECRETARIA"));
            Assert.AreEqual(ResultadoInteracao.InformationalNpc, motor.AvaliarInteracao("NPC_DIRETOR"));
            Assert.AreEqual(ResultadoInteracao.InformationalNpc, motor.AvaliarInteracao(""));

            ResultadoEncerramento r = motor.RegistrarEncerramentoDialogo("NPC_SECRETARIA");
            Assert.IsFalse(r.MissaoIniciada);
            Assert.IsFalse(motor.TemMissaoAtiva);

            AutorizacaoMinijogo aut = motor.AutorizarMinijogo("NPC_SECRETARIA", "fios");
            Assert.IsFalse(aut.Autorizado);
            StringAssert.Contains("catálogo", aut.Motivo);
        }

        // =====================================================================
        // Cenário 9 — FIX Márcio/Roney: objetivo de etapa de MINIJOGO só é
        //             registrado via RegistrarResultadoMinijogo (vitória)
        // =====================================================================

        [Test]
        public void Cenario09_EtapaDeMinijogoSoRegistraComVitoriaAutorizada()
        {
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_RONEY); // agora: etapa dos fios

            // O DIÁLOGO autorizado com o dono da etapa NÃO registra o objetivo.
            ResultadoEncerramento r = motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_RONEY);
            Assert.AreEqual(ResultadoInteracao.InteractionAllowed, r.Resultado);
            Assert.IsFalse(r.ObjetivoRegistrado);
            Assert.IsFalse(r.EtapaConcluida);
            Assert.AreEqual(EstadoMissao.InProgress,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));

            // Autorização central carrega (missão, etapa, objetivo).
            AutorizacaoMinijogo aut = motor.AutorizarMinijogo(CatalogoIf360.NPC_RONEY, "fios");
            Assert.IsTrue(aut.Autorizado);
            Assert.AreEqual(CatalogoIf360.MISSAO_ENSINO_MEDIO, aut.MissionId);
            Assert.AreEqual("Q01_CONFIGURACAO_SERVIDOR", aut.StepId);
            Assert.AreEqual("Q01_SERVIDOR_CONECTADO", aut.ObjectiveId);

            // Derrota NÃO registra (o objetivo continua pendente).
            string motivo;
            Assert.IsFalse(motor.RegistrarResultadoMinijogo(
                CatalogoIf360.MISSAO_ENSINO_MEDIO, "Q01_SERVIDOR_CONECTADO", false, out motivo));
            Assert.AreEqual(ResultadoInteracao.InteractionAllowed,
                motor.AvaliarInteracao(CatalogoIf360.NPC_RONEY));

            // Resultado de OUTRA missão é negado pela validação central.
            Assert.IsFalse(motor.RegistrarResultadoMinijogo(
                CatalogoIf360.MISSAO_BIBLIOTECA, "REMINDER_CHEMISTRY", true, out motivo));

            // Vitória registra o objetivo e conclui a etapa.
            Assert.IsTrue(motor.RegistrarResultadoMinijogo(
                CatalogoIf360.MISSAO_ENSINO_MEDIO, "Q01_SERVIDOR_CONECTADO", true, out motivo));
            Assert.AreEqual(EstadoMissao.ReadyToTurnIn,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));

            // Após a etapa avançar, um segundo reporte é negado.
            Assert.IsFalse(motor.RegistrarResultadoMinijogo(
                CatalogoIf360.MISSAO_ENSINO_MEDIO, "Q01_SERVIDOR_CONECTADO", true, out motivo));
        }

        [Test]
        public void Cenario09b_AutorizacaoDeMinijogoNegadaNosCasosErrados()
        {
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);

            // Etapa atual é DIÁLOGO (explicação) → sem autorização.
            Assert.IsFalse(motor.AutorizarMinijogo(CatalogoIf360.NPC_RONEY, "fios").Autorizado);

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_RONEY); // etapa dos fios

            // Minijogo diferente do esperado pela etapa.
            Assert.IsFalse(motor.AutorizarMinijogo(CatalogoIf360.NPC_RONEY, "compilacao").Autorizado);

            // NPC de outra missão.
            Assert.IsFalse(motor.AutorizarMinijogo(CatalogoIf360.NPC_JOSY, "fios").Autorizado);

            // Minijogos NÃO reabrem após a missão concluir (sem farm de pontos).
            ConcluirQ01();
            Assert.IsFalse(motor.AutorizarMinijogo(CatalogoIf360.NPC_RONEY, "fios").Autorizado);
        }

        // =====================================================================
        // Cenário 10 — ReadyToTurnIn: SOMENTE o finalizador conclui a missão
        // =====================================================================

        [Test]
        public void Cenario10_SoFinalizadorConcluiMissao()
        {
            ConcluirQ01();
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_THIAGO); // início Q02
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_JP);
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_GLEICE);
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ROSY);

            Assert.AreEqual(EstadoMissao.ReadyToTurnIn,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_BIBLIOTECA));
            Assert.AreEqual(ResultadoInteracao.MissionReadyToTurnIn,
                motor.AvaliarInteracao(CatalogoIf360.NPC_THIAGO));

            // Subordinado da própria missão não finaliza.
            ResultadoEncerramento r = motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_JP);
            Assert.IsFalse(r.MissaoFinalizada);
            Assert.AreEqual(CatalogoIf360.MISSAO_BIBLIOTECA, motor.MissaoAtiva);

            // O finalizador conclui e LIBERA o slot de missão ativa.
            r = motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_THIAGO);
            Assert.IsTrue(r.MissaoFinalizada);
            Assert.AreEqual(EstadoMissao.Completed,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_BIBLIOTECA));
            Assert.AreEqual(string.Empty, motor.MissaoAtiva);
        }

        // =====================================================================
        // Cenário 11 — missão concluída não reinicia nem re-registra
        // =====================================================================

        [Test]
        public void Cenario11_MissaoJaConcluidaNaoReabre()
        {
            ConcluirQ01();

            Assert.AreEqual(ResultadoInteracao.MissionAlreadyCompleted,
                motor.AvaliarInteracao(CatalogoIf360.NPC_ANANDA));

            ResultadoEncerramento r = motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);
            Assert.IsFalse(r.MissaoIniciada);
            Assert.IsFalse(r.ObjetivoRegistrado);
            Assert.IsFalse(r.MissaoFinalizada);
            Assert.IsFalse(motor.TemMissaoAtiva);
        }

        // =====================================================================
        // Estados do fluxo completo da Q01 + eventos do motor
        // =====================================================================

        [Test]
        public void EstadosDoFluxoCompletoDaQ01()
        {
            Assert.AreEqual(EstadoMissao.Available,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));
            Assert.AreEqual(EstadoMissao.Locked, motor.ObterEstadoMissao(CatalogoIf360.MISSAO_NUCLEO));
            Assert.AreEqual(4, motor.TotalMissoesCatalogo);
            Assert.AreEqual(0, motor.QuantidadeMissoesCompletas());

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);
            Assert.AreEqual(EstadoMissao.InProgress,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_RONEY);
            string motivo;
            motor.RegistrarResultadoMinijogo(CatalogoIf360.MISSAO_ENSINO_MEDIO,
                "Q01_SERVIDOR_CONECTADO", true, out motivo);
            Assert.AreEqual(EstadoMissao.ReadyToTurnIn,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);
            Assert.AreEqual(EstadoMissao.Completed,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));
            Assert.AreEqual(1, motor.QuantidadeMissoesCompletas());
        }

        [Test]
        public void EventosDoMotorDisparamNaOrdemEsperada()
        {
            int inicios = 0, objetivos = 0, etapas = 0, missoes = 0;
            motor.AoIniciarMissao += delegate { inicios++; };
            motor.AoConcluirObjetivo += delegate { objetivos++; };
            motor.AoConcluirEtapa += delegate { etapas++; };
            motor.AoConcluirMissao += delegate { missoes++; };

            ConcluirQ01();

            Assert.AreEqual(1, inicios);
            Assert.AreEqual(4, objetivos); // apresentação, explicação, servidor, finalização
            Assert.AreEqual(4, etapas);
            Assert.AreEqual(1, missoes);
        }

        // =====================================================================
        // Compatibilidade legada — ObterPassoMissao / MissaoCompleta
        // (variantes de diálogo existentes continuam funcionando)
        // =====================================================================

        [Test]
        public void Legado_PassoEMissaoCompleta()
        {
            Assert.AreEqual(0, motor.ObterPassoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);
            Assert.AreEqual(2, motor.ObterPassoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO)); // etapa atual: explicação
            Assert.AreEqual(2, motor.ObterPassoMissao("missao_administracao")); // alias legado

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_RONEY);
            Assert.AreEqual(3, motor.ObterPassoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO)); // etapa atual: servidor

            string motivo;
            motor.RegistrarResultadoMinijogo(CatalogoIf360.MISSAO_ENSINO_MEDIO,
                "Q01_SERVIDOR_CONECTADO", true, out motivo);
            Assert.AreEqual(4, motor.ObterPassoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO)); // pronto p/ entregar

            // ReadyToTurnIn conta como "completa" para as variantes legadas.
            Assert.IsTrue(motor.MissaoCompleta("missao_administracao"));

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);
            Assert.AreEqual(4, motor.ObterPassoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));
            Assert.IsTrue(motor.MissaoCompleta(CatalogoIf360.MISSAO_ENSINO_MEDIO));

            // missao_assistencia deriva da flag da Alice (informativa).
            Assert.AreEqual(0, motor.ObterPassoMissao("missao_assistencia"));
            motor.MarcarFlag("conversou_missao_assistencia");
            Assert.AreEqual(1, motor.ObterPassoMissao("missao_assistencia"));
        }

        [Test]
        public void Legado_PassoDoNucleoNaCompilacao()
        {
            ConcluirQ02();

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_JOSY); // início Q03
            Assert.AreEqual(2, motor.ObterPassoMissao(CatalogoIf360.MISSAO_NUCLEO)); // etapa atual: componentes (David)

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_DAVID);
            // Etapa atual agora é a compilação (minijogo do Márcio), que mapeia
            // para o passo 5 legado (limiares da cena: Josy Durante>=2, Pos>=5).
            Assert.AreEqual(5, motor.ObterPassoMissao(CatalogoIf360.MISSAO_NUCLEO));

            // Diálogo com Márcio NÃO registra COMPILATION; só a vitória autorizada.
            Assert.IsFalse(motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_MARCIO).ObjetivoRegistrado);

            AutorizacaoMinijogo aut = motor.AutorizarMinijogo(CatalogoIf360.NPC_MARCIO, "compilacao");
            Assert.IsTrue(aut.Autorizado);
            Assert.AreEqual("Q03_COMPILACAO", aut.StepId);
            Assert.AreEqual("COMPILATION", aut.ObjectiveId);

            string motivo;
            Assert.IsTrue(motor.RegistrarResultadoMinijogo(
                CatalogoIf360.MISSAO_NUCLEO, "COMPILATION", true, out motivo));
            Assert.AreEqual(EstadoMissao.ReadyToTurnIn,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_NUCLEO));
            Assert.AreEqual(5, motor.ObterPassoMissao(CatalogoIf360.MISSAO_NUCLEO));

            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_JOSY);
            Assert.AreEqual(5, motor.ObterPassoMissao(CatalogoIf360.MISSAO_NUCLEO)); // concluída
        }

        [Test]
        public void Legado_CompletarMissaoFlatPreservaIdsLegados()
        {
            motor.CompletarMissaoFlat("missao_diretor");
            Assert.IsTrue(motor.MissaoCompleta("missao_diretor"));
            Assert.AreEqual(0, motor.QuantidadeMissoesCompletas()); // fora do catálogo

            motor.CompletarMissaoFlat(CatalogoIf360.MISSAO_ENSINO_MEDIO);
            Assert.AreEqual(EstadoMissao.Completed,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));
            Assert.AreEqual(1, motor.QuantidadeMissoesCompletas());
        }

        // =====================================================================
        // Migração do formato antigo (flat + passos + flags → v2)
        // =====================================================================

        [Test]
        public void Migracao_PreservaProgressoDoFormatoAntigo()
        {
            var legado = new DadosLegado();
            legado.idsCompletas = new List<string> { "missao_administracao", "missao_diretor" };
            legado.missaoIds = new List<string> { "missao_biblioteca" };
            legado.passoAtual = new List<int> { 2 };
            legado.idsFlags = new List<string> { "conversou_jp", "conversou_gleice" };

            motor.MigrarEstadoLegado(legado);

            // missao_administracao (flat) ⇒ missao_ensino_medio concluída.
            Assert.AreEqual(EstadoMissao.Completed,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));
            Assert.IsTrue(motor.MissaoCompleta("missao_administracao"));

            // missao_diretor preservado, mas fora da contagem do catálogo.
            Assert.IsTrue(motor.MissaoCompleta("missao_diretor"));
            Assert.AreEqual(1, motor.QuantidadeMissoesCompletas());

            // Passo 2 da biblioteca (cadastro) + flags jp/gleice (2 lembretes):
            // missão ativa = Q02, parcial, faltando apenas Rosy.
            Assert.AreEqual(CatalogoIf360.MISSAO_BIBLIOTECA, motor.MissaoAtiva);
            Assert.AreEqual(EstadoMissao.InProgress,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_BIBLIOTECA));

            Assert.AreEqual(ResultadoInteracao.ObjectiveAlreadyCompleted,
                motor.AvaliarInteracao(CatalogoIf360.NPC_JP));
            Assert.AreEqual(ResultadoInteracao.ObjectiveAlreadyCompleted,
                motor.AvaliarInteracao(CatalogoIf360.NPC_GLEICE));
            Assert.AreEqual(ResultadoInteracao.InteractionAllowed,
                motor.AvaliarInteracao(CatalogoIf360.NPC_ROSY));

            ResumoProgresso resumo = motor.ObterResumoProgresso();
            Assert.AreEqual(1, resumo.ObjetivosPendentes);
            Assert.IsTrue(resumo.DescricaoPendentes.StartsWith("Faltam 1 de 3"));

            // Flags do mundo preservadas integralmente.
            Assert.IsTrue(motor.TemFlag("conversou_jp"));
            Assert.IsTrue(motor.TemFlag("conversou_gleice"));
        }

        [Test]
        public void Migracao_SemDadosMantemEstadoZerado()
        {
            motor.MigrarEstadoLegado(new DadosLegado());
            Assert.IsFalse(motor.TemMissaoAtiva);
            Assert.AreEqual(0, motor.QuantidadeMissoesCompletas());
            Assert.AreEqual(EstadoMissao.Available,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));
        }

        // =====================================================================
        // Consistência do estado importado
        // =====================================================================

        [Test]
        public void Consistencia_MissaoCompletaPreencheObjetivosObrigatorios()
        {
            var dados = new DadosProgressoSalvos();
            dados.missoesCompletas.Add(CatalogoIf360.MISSAO_ENSINO_MEDIO);
            // objetivosConcluidos vazio — a consistência deve preencher.

            motor.ImportarEstado(dados);

            Assert.AreEqual(EstadoMissao.Completed,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));
            Assert.AreEqual(ResultadoInteracao.MissionAlreadyCompleted,
                motor.AvaliarInteracao(CatalogoIf360.NPC_ANANDA));
        }

        [Test]
        public void Consistencia_TodosObjetivosConcluidosCompletaAMissao()
        {
            var dados = new DadosProgressoSalvos();
            dados.missaoAtiva = CatalogoIf360.MISSAO_BIBLIOTECA;
            dados.objetivosConcluidos.AddRange(new List<string>
            {
                "Q02_CADASTRO", "REMINDER_CHEMISTRY", "REMINDER_PHYSICS",
                "REMINDER_BIOLOGY", "Q02_FINALIZACAO"
            });

            motor.ImportarEstado(dados);

            Assert.AreEqual(EstadoMissao.Completed,
                motor.ObterEstadoMissao(CatalogoIf360.MISSAO_BIBLIOTECA));
            Assert.AreEqual(string.Empty, motor.MissaoAtiva); // slot liberado
        }

        [Test]
        public void Consistencia_MissaoAtivaInvalidaOuJaCompletaEhLiberada()
        {
            var dados = new DadosProgressoSalvos();
            dados.missaoAtiva = "missao_inexistente";
            motor.ImportarEstado(dados);
            Assert.AreEqual(string.Empty, motor.MissaoAtiva);

            var dados2 = new DadosProgressoSalvos();
            dados2.missaoAtiva = CatalogoIf360.MISSAO_ENSINO_MEDIO;
            dados2.missoesCompletas.Add(CatalogoIf360.MISSAO_ENSINO_MEDIO);
            motor.ImportarEstado(dados2);
            Assert.AreEqual(string.Empty, motor.MissaoAtiva);
        }

        // =====================================================================
        // Round-trip de serialização (formato v2)
        // =====================================================================

        [Test]
        public void ExportarImportar_RoundTripPreservaEstado()
        {
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_ANANDA);
            motor.RegistrarEncerramentoDialogo(CatalogoIf360.NPC_RONEY);
            motor.MarcarFlag("conversou_missao_ensino_medio");

            DadosProgressoSalvos dados = motor.ExportarEstado();
            var motor2 = new MotorRegrasMissao(CatalogoIf360.Criar());
            motor2.ImportarEstado(dados);

            Assert.AreEqual(motor.MissaoAtiva, motor2.MissaoAtiva);
            Assert.AreEqual(motor.QuantidadeMissoesCompletas(), motor2.QuantidadeMissoesCompletas());
            Assert.AreEqual(motor.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO),
                motor2.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));
            Assert.IsTrue(motor2.TemFlag("conversou_missao_ensino_medio"));

            // Continua exatamente de onde parou: fios ainda autorizado e pendente.
            AutorizacaoMinijogo aut = motor2.AutorizarMinijogo(CatalogoIf360.NPC_RONEY, "fios");
            Assert.IsTrue(aut.Autorizado);

            string motivo;
            Assert.IsTrue(motor2.RegistrarResultadoMinijogo(
                CatalogoIf360.MISSAO_ENSINO_MEDIO, "Q01_SERVIDOR_CONECTADO", true, out motivo));
            Assert.AreEqual(EstadoMissao.ReadyToTurnIn,
                motor2.ObterEstadoMissao(CatalogoIf360.MISSAO_ENSINO_MEDIO));
        }
    }
}

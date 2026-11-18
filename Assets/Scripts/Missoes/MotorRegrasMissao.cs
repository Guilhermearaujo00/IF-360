using System;
using System.Collections.Generic;

namespace IF360.Missoes
{
    /// <summary>
    /// Motor de regras de missões — C# puro, sem UnityEngine (testável em modo Edit).
    ///
    /// Autoridade central de progressão: todo avanço de missão (iniciar objetivo,
    /// concluir etapa, finalizar missão, autorizar minijogo) passa por este motor.
    /// Nenhum NPC decide progressão sozinho (Etapa 4.1 do plano).
    ///
    /// Repositório de verdade do estado:
    ///   - _missaoAtiva: id da ÚNICA missão em andamento (vazio = nenhuma);
    ///   - _missoesCompletas: ids concluídos (inclui ids legados preservados,
    ///     ex.: missao_diretor — que não contam no total do catálogo);
    ///   - _objetivosConcluidos: conjunto de objectiveIds concluídos; etapas e
    ///     estados são DERIVADOS daqui (consistência garantida ao carregar);
    ///   - _flags: flags do mundo (conversou_*), mantidas por compatibilidade
    ///     com as variantes de diálogo legadas.
    /// </summary>
    public class MotorRegrasMissao
    {
        private readonly CatalogoMissoes _catalogo;
        private readonly Action<string> _log;

        private string _missaoAtiva = "";
        private readonly HashSet<string> _missoesCompletas = new HashSet<string>();
        private readonly HashSet<string> _objetivosConcluidos = new HashSet<string>();
        private readonly HashSet<string> _flags = new HashSet<string>();

        // ---- Eventos (traduzidos pelo QuestManager para logs/UI/save) ----

        /// <summary>(missionId) disparado quando uma missão é iniciada pelo NPC iniciador.</summary>
        public event Action<string> AoIniciarMissao;

        /// <summary>(missionId, objectiveId) disparado quando um objetivo é registrado.</summary>
        public event Action<string, string> AoConcluirObjetivo;

        /// <summary>(missionId, stepId) disparado quando uma etapa fica totalmente cumprida.</summary>
        public event Action<string, string> AoConcluirEtapa;

        /// <summary>(missionId) disparado quando uma missão é finalizada pelo iniciador.</summary>
        public event Action<string> AoConcluirMissao;

        public MotorRegrasMissao(CatalogoMissoes catalogo, Action<string> log = null)
        {
            _catalogo = catalogo ?? throw new ArgumentNullException("catalogo");
            _log = log ?? delegate { };
        }

        // =====================================================================
        // Consultas básicas
        // =====================================================================

        /// <summary>Catálogo de missões/NPCs usado pelo motor.</summary>
        public CatalogoMissoes Catalogo { get { return _catalogo; } }

        /// <summary>Id da missão ativa (vazio quando não há).</summary>
        public string MissaoAtiva { get { return _missaoAtiva; } }

        /// <summary>Há uma missão ocupando o slot (InProgress ou ReadyToTurnIn)?</summary>
        public bool TemMissaoAtiva { get { return !string.IsNullOrEmpty(_missaoAtiva); } }

        /// <summary>Total de missões estruturadas no catálogo.</summary>
        public int TotalMissoesCatalogo { get { return _catalogo.Missoes.Count; } }

        /// <summary>
        /// Missões concluídas que pertencem ao catálogo. Ids legados sem
        /// correspondência (missao_diretor, missao_assistencia) NÃO contam.
        /// </summary>
        public int QuantidadeMissoesCompletas()
        {
            int total = 0;
            for (int i = 0; i < _catalogo.Missoes.Count; i++)
                if (_missoesCompletas.Contains(_catalogo.Missoes[i].MissionId)) total++;
            return total;
        }

        // =====================================================================
        // Flags do mundo (compatibilidade legada)
        // =====================================================================

        public void MarcarFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag) || _flags.Contains(flag)) return;
            _flags.Add(flag);
            _log("[MISSION] Flag marcada: " + flag);
        }

        public bool TemFlag(string flag)
        {
            return !string.IsNullOrEmpty(flag) && _flags.Contains(flag);
        }

        /// <summary>Cópia das flags marcadas (para serialização).</summary>
        public List<string> ObterFlags()
        {
            return new List<string>(_flags);
        }

        // =====================================================================
        // Persistência (formato v2)
        // =====================================================================

        /// <summary>Exporta o estado completo no formato v2 (compatível com JsonUtility).</summary>
        public DadosProgressoSalvos ExportarEstado()
        {
            var dados = new DadosProgressoSalvos();
            dados.versao = 2;
            dados.missaoAtiva = _missaoAtiva;
            dados.missoesCompletas = new List<string>(_missoesCompletas);
            dados.objetivosConcluidos = new List<string>(_objetivosConcluidos);
            dados.flags = new List<string>(_flags);
            return dados;
        }

        /// <summary>
        /// Substitui o estado interno pelos dados v2 salvos e valida a consistência
        /// (Etapa 10 — preservação do progresso salvo).
        /// </summary>
        public void ImportarEstado(DadosProgressoSalvos dados)
        {
            if (dados == null) return;
            _missaoAtiva = dados.missaoAtiva ?? "";
            _missoesCompletas.Clear();
            _objetivosConcluidos.Clear();
            _flags.Clear();
            if (dados.missoesCompletas != null)
                foreach (var id in dados.missoesCompletas) _missoesCompletas.Add(id);
            if (dados.objetivosConcluidos != null)
                foreach (var id in dados.objetivosConcluidos) _objetivosConcluidos.Add(id);
            if (dados.flags != null)
                foreach (var f in dados.flags) _flags.Add(f);
            ValidarConsistencia();
        }

        /// <summary>Zera todo o progresso (novo jogo).</summary>
        public void Resetar()
        {
            _missaoAtiva = "";
            _missoesCompletas.Clear();
            _objetivosConcluidos.Clear();
            _flags.Clear();
            _log("[MISSION] Progresso reiniciado.");
        }

        // =====================================================================
        // Migração do formato antigo (flat + passos + flags → v2)
        // =====================================================================

        /// <summary>Flags legadas que provam objetivos concluídos.</summary>
        private static readonly Dictionary<string, string> MapaFlagsParaObjetivos =
            new Dictionary<string, string>
        {
            { "conversou_missao_ensino_medio", "Q01_APRESENTACAO" },
            { "conversou_missao_administracao", "Q01_EXPLICACAO_SERVIDOR" },
            { "conversou_missao_biblioteca", "Q02_CADASTRO" },
            { "conversou_jp", "REMINDER_CHEMISTRY" },
            { "conversou_gleice", "REMINDER_PHYSICS" },
            { "conversou_rosy", "REMINDER_BIOLOGY" },
            { "conversou_missao_nucleo", "Q03_APRESENTACAO" },
            { "conversou_david", "COMPONENTS_RECEIVED" },
            { "conversou_missao_refeitorio", "Q04_APRESENTACAO" },
            { "conversou_claudio", "TICKET_PURCHASED" }
        };

        /// <summary>Limiar de passo antigo que prova um objetivo concluído.</summary>
        private struct LimiarPasso
        {
            public readonly string MissionId;
            public readonly int PassoMinimo;
            public readonly string ObjectiveId;

            public LimiarPasso(string missionId, int passoMinimo, string objectiveId)
            {
                MissionId = missionId;
                PassoMinimo = passoMinimo;
                ObjectiveId = objectiveId;
            }
        }

        /// <summary>passo antigo &gt;= limite ⇒ objetivo concluído.</summary>
        private static readonly LimiarPasso[] LimiaresPassoLegados =
        {
            new LimiarPasso("missao_ensino_medio", 1, "Q01_APRESENTACAO"),
            new LimiarPasso("missao_ensino_medio", 2, "Q01_EXPLICACAO_SERVIDOR"),
            new LimiarPasso("missao_ensino_medio", 3, "Q01_SERVIDOR_CONECTADO"),
            new LimiarPasso("missao_administracao", 1, "Q01_EXPLICACAO_SERVIDOR"),
            new LimiarPasso("missao_administracao", 2, "Q01_SERVIDOR_CONECTADO"),
            new LimiarPasso("missao_biblioteca", 1, "Q02_CADASTRO"),
            new LimiarPasso("missao_biblioteca", 4, "REMINDER_CHEMISTRY"),
            new LimiarPasso("missao_biblioteca", 4, "REMINDER_PHYSICS"),
            new LimiarPasso("missao_biblioteca", 4, "REMINDER_BIOLOGY"),
            new LimiarPasso("missao_nucleo", 1, "Q03_APRESENTACAO"),
            new LimiarPasso("missao_nucleo", 2, "COMPONENTS_RECEIVED"),
            new LimiarPasso("missao_nucleo", 5, "COMPILATION"),
            new LimiarPasso("missao_refeitorio", 1, "Q04_APRESENTACAO"),
            new LimiarPasso("missao_refeitorio", 2, "TICKET_PURCHASED")
        };

        /// <summary>Id de missão legado concluído no formato flat ⇒ missão do catálogo concluída.</summary>
        private static readonly Dictionary<string, string> EquivalenciaFlat =
            new Dictionary<string, string>
        {
            { "missao_administracao", "missao_ensino_medio" },
            { "missao_ensino_medio", "missao_ensino_medio" },
            { "missao_biblioteca", "missao_biblioteca" },
            { "missao_nucleo", "missao_nucleo" },
            { "missao_refeitorio", "missao_refeitorio" }
        };

        /// <summary>
        /// Converte um save do formato antigo (idsCompletas + missaoIds/passoAtual +
        /// idsFlags) para o modelo v2. Preserva flags e ids legados sem
        /// correspondência (missao_diretor, missao_assistencia).
        /// </summary>
        public void MigrarEstadoLegado(DadosLegado legado)
        {
            if (legado == null) return;
            Resetar();
            _log("[MISSION] Migrando progresso do formato antigo para o v2...");

            // 1) Flags do mundo: preservadas integralmente (variantes legadas dependem delas).
            // 2) Flags que provam objetivos concluídos.
            if (legado.idsFlags != null)
            {
                foreach (var flag in legado.idsFlags)
                {
                    if (string.IsNullOrEmpty(flag)) continue;
                    _flags.Add(flag);
                    string objetivo;
                    if (MapaFlagsParaObjetivos.TryGetValue(flag, out objetivo))
                        MarcarObjetivoMigrado(objetivo, "flag " + flag);
                }
            }

            // 3) Passos antigos (limiares).
            if (legado.missaoIds != null && legado.passoAtual != null)
            {
                for (int i = 0; i < legado.missaoIds.Count && i < legado.passoAtual.Count; i++)
                {
                    string idMissao = legado.missaoIds[i];
                    int passo = legado.passoAtual[i];
                    foreach (var limiar in LimiaresPassoLegados)
                        if (limiar.MissionId == idMissao && passo >= limiar.PassoMinimo)
                            MarcarObjetivoMigrado(limiar.ObjectiveId, "passo " + idMissao + "=" + passo);
                }
            }

            // 4) Missões concluídas no modelo flat.
            if (legado.idsCompletas != null)
            {
                foreach (var id in legado.idsCompletas)
                {
                    if (string.IsNullOrEmpty(id)) continue;
                    string equivalente;
                    if (EquivalenciaFlat.TryGetValue(id, out equivalente))
                    {
                        CompletarMissaoInterna(equivalente, "migração (flat: " + id + ")");
                    }
                    else
                    {
                        // Id legado sem correspondência (missao_diretor, missao_assistencia):
                        // preservado para as consultas legadas (MissaoCompleta).
                        _missoesCompletas.Add(id);
                        _log("[MISSION] Migração: id legado preservado: " + id);
                    }
                }
            }

            // 5) Missão ativa = a missão PARCIAL mais profunda na cadeia (Q01→Q04).
            _missaoAtiva = "";
            for (int i = 0; i < _catalogo.Missoes.Count; i++)
            {
                var defM = _catalogo.Missoes[i];
                if (_missoesCompletas.Contains(defM.MissionId)) continue;
                if (MissaoTemProgresso(defM)) _missaoAtiva = defM.MissionId;
            }
            if (!string.IsNullOrEmpty(_missaoAtiva))
                _log("[MISSION] Migração: missão ativa = " + _missaoAtiva);

            // 6) Consistência final.
            ValidarConsistencia();
        }

        /// <summary>Marca um objetivo concluído durante a migração (com origem no log).</summary>
        private void MarcarObjetivoMigrado(string objectiveId, string origem)
        {
            if (string.IsNullOrEmpty(objectiveId) || _objetivosConcluidos.Contains(objectiveId)) return;
            if (!ObjetivoExisteNoCatalogo(objectiveId)) return; // evita lixo no save
            _objetivosConcluidos.Add(objectiveId);
            _log("[MISSION] Migração: objetivo " + objectiveId + " concluído (" + origem + ").");
        }

        private bool ObjetivoExisteNoCatalogo(string objectiveId)
        {
            for (int m = 0; m < _catalogo.Missoes.Count; m++)
            {
                var etapas = _catalogo.Missoes[m].Steps;
                for (int e = 0; e < etapas.Count; e++)
                    for (int o = 0; o < etapas[e].Objectives.Count; o++)
                        if (etapas[e].Objectives[o].ObjectiveId == objectiveId) return true;
            }
            return false;
        }

        private bool MissaoTemProgresso(DefinicaoMissao defM)
        {
            for (int e = 0; e < defM.Steps.Count; e++)
                for (int o = 0; o < defM.Steps[e].Objectives.Count; o++)
                    if (_objetivosConcluidos.Contains(defM.Steps[e].Objectives[o].ObjectiveId)) return true;
            return false;
        }

        /// <summary>
        /// Consistência do estado carregado:
        ///   (a) missão completa não pode ter objetivos obrigatórios pendentes
        ///       (auto-preenchidos + aviso);
        ///   (b) missão com TODOS os objetivos concluídos deve estar completa;
        ///   (c) missão ativa precisa existir no catálogo e não estar completa.
        /// </summary>
        public void ValidarConsistencia()
        {
            for (int m = 0; m < _catalogo.Missoes.Count; m++)
            {
                var defM = _catalogo.Missoes[m];
                if (!_missoesCompletas.Contains(defM.MissionId))
                {
                    if (MissaoTemProgresso(defM) && TodosObjetivosObrigatoriosConcluidos(defM))
                        CompletarMissaoInterna(defM.MissionId, "consistência (objetivos completos)");
                    continue;
                }
                for (int e = 0; e < defM.Steps.Count; e++)
                {
                    var etapa = defM.Steps[e];
                    for (int o = 0; o < etapa.Objectives.Count; o++)
                    {
                        var obj = etapa.Objectives[o];
                        if (obj.Required && !_objetivosConcluidos.Contains(obj.ObjectiveId))
                        {
                            _objetivosConcluidos.Add(obj.ObjectiveId);
                            _log("[MISSION] Consistência: objetivo " + obj.ObjectiveId +
                                 " preenchido automaticamente (missão " + defM.MissionId + " já completa).");
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(_missaoAtiva))
            {
                var defAtiva = _catalogo.ObterMissao(_missaoAtiva);
                if (defAtiva == null)
                {
                    _log("[MISSION] Consistência: missão ativa desconhecida (" + _missaoAtiva + ") — removida.");
                    _missaoAtiva = "";
                }
                else if (_missoesCompletas.Contains(_missaoAtiva))
                {
                    _log("[MISSION] Consistência: missão ativa já completa (" + _missaoAtiva + ") — slot liberado.");
                    _missaoAtiva = "";
                }
            }
        }

        // =====================================================================
        // Consultas de estado (derivadas de _objetivosConcluidos)
        // =====================================================================

        /// <summary>
        /// Estado da missão: Completed / (ativa: ReadyToTurnIn quando a etapa atual
        /// é a última, senão InProgress) / Available / Locked (por RequiredMissions).
        /// </summary>
        public EstadoMissao ObterEstadoMissao(string missionId)
        {
            var defM = _catalogo.ObterMissao(missionId);
            if (defM == null) return EstadoMissao.Locked;

            if (_missoesCompletas.Contains(missionId)) return EstadoMissao.Completed;

            if (_missaoAtiva == missionId)
            {
                var etapa = ObterEtapaAtual(missionId);
                bool ultimaEtapa = defM.Steps.Count == 0 || etapa == defM.UltimaEtapa;
                return ultimaEtapa ? EstadoMissao.ReadyToTurnIn : EstadoMissao.InProgress;
            }

            return RequisitosCumpridos(defM) ? EstadoMissao.Available : EstadoMissao.Locked;
        }

        /// <summary>
        /// Primeira etapa (por ordem) que ainda tem objetivo obrigatório pendente;
        /// se todas estiverem cumpridas, devolve a última etapa (finalização).
        /// </summary>
        public DefinicaoEtapa ObterEtapaAtual(string missionId)
        {
            var defM = _catalogo.ObterMissao(missionId);
            if (defM == null || defM.Steps.Count == 0) return null;
            for (int i = 0; i < defM.Steps.Count; i++)
                if (!EtapaEstaConcluida(defM.Steps[i])) return defM.Steps[i];
            return defM.UltimaEtapa;
        }

        /// <summary>A etapa está totalmente cumprida conforme a sua regra de conclusão?</summary>
        public bool EtapaEstaConcluida(DefinicaoEtapa etapa)
        {
            if (etapa == null) return false;
            if (etapa.CompletionRule == RegraConclusaoEtapa.AnyObjective)
            {
                for (int i = 0; i < etapa.Objectives.Count; i++)
                    if (etapa.Objectives[i].Required && _objetivosConcluidos.Contains(etapa.Objectives[i].ObjectiveId))
                        return true;
                return etapa.Objectives.Count == 0;
            }

            for (int i = 0; i < etapa.Objectives.Count; i++)
            {
                var obj = etapa.Objectives[i];
                if (obj.Required && !_objetivosConcluidos.Contains(obj.ObjectiveId)) return false;
            }
            return true;
        }

        private bool TodosObjetivosObrigatoriosConcluidos(DefinicaoMissao defM)
        {
            for (int e = 0; e < defM.Steps.Count; e++)
                for (int o = 0; o < defM.Steps[e].Objectives.Count; o++)
                {
                    var obj = defM.Steps[e].Objectives[o];
                    if (obj.Required && !_objetivosConcluidos.Contains(obj.ObjectiveId)) return false;
                }
            return true;
        }

        private bool RequisitosCumpridos(DefinicaoMissao defM)
        {
            for (int i = 0; i < defM.RequiredMissions.Count; i++)
                if (!_missoesCompletas.Contains(defM.RequiredMissions[i])) return false;
            return true;
        }

        /// <summary>
        /// Resumo vivo do progresso da missão ativa — alimenta o HUD e os diálogos
        /// dinâmicos (Etapa 5.1: dados centralizados, "faltam X de 3" dinâmico).
        /// </summary>
        public ResumoProgresso ObterResumoProgresso()
        {
            var resumo = new ResumoProgresso();
            if (!TemMissaoAtiva) return resumo;

            var defM = _catalogo.ObterMissao(_missaoAtiva);
            if (defM == null) return resumo;

            var etapa = ObterEtapaAtual(_missaoAtiva);
            resumo.TemMissaoAtiva = true;
            resumo.MissionId = defM.MissionId;
            resumo.MissionName = defM.MissionName;
            resumo.Estado = ObterEstadoMissao(_missaoAtiva);
            resumo.TotalEtapas = defM.Steps.Count;

            if (etapa == null) return resumo;
            resumo.StepId = etapa.StepId;
            resumo.StepDescricao = etapa.Descricao;
            resumo.StepOrder = etapa.StepOrder;
            resumo.ObjetivosTotaisEtapa = etapa.Objectives.Count;

            // Donos dos objetivos pendentes da etapa (nomes e localizações do catálogo).
            for (int o = 0; o < etapa.Objectives.Count; o++)
            {
                var obj = etapa.Objectives[o];
                if (!obj.Required || _objetivosConcluidos.Contains(obj.ObjectiveId)) continue;
                resumo.ObjetivosPendentes++;
                var perfil = _catalogo.ObterNPC(obj.OwnerNPCId);
                if (perfil != null)
                {
                    resumo.NomesNPCsPendentes.Add(perfil.DisplayName);
                    resumo.LocalizacoesPendentes.Add(perfil.LocationName);
                }
            }

            // "Faltam X de N" (etapas com objetivos paralelos).
            if (etapa.Objectives.Count > 1 && resumo.ObjetivosPendentes > 0)
            {
                string nomes = "";
                for (int i = 0; i < resumo.NomesNPCsPendentes.Count; i++)
                {
                    if (i > 0) nomes += i == resumo.NomesNPCsPendentes.Count - 1 ? " e " : ", ";
                    nomes += resumo.NomesNPCsPendentes[i];
                }
                resumo.DescricaoPendentes = "Faltam " + resumo.ObjetivosPendentes + " de " +
                    etapa.Objectives.Count + ": " + nomes + ".";
            }

            // Próxima ação (tokens dos diálogos dinâmicos e HUD).
            if (resumo.Estado == EstadoMissao.ReadyToTurnIn)
            {
                var finalizador = _catalogo.ObterNPC(defM.FinalNPCId);
                resumo.DefinirProximaAcao("Retorne a " + (finalizador != null ? finalizador.DisplayName : defM.FinalNPCId) +
                    " (" + (finalizador != null ? finalizador.LocationName : "?") + ").");
            }
            else if (etapa.ExpectedInteractionType == TipoInteracaoEtapa.Minijogo)
            {
                var dono = _catalogo.ObterNPC(etapa.ExpectedNPCIds.Count > 0 ? etapa.ExpectedNPCIds[0] : "");
                resumo.DefinirProximaAcao("Complete o desafio de " +
                    (dono != null ? dono.DisplayName : "?") + " (" +
                    (dono != null ? dono.LocationName : "?") + ").");
            }
            else if (resumo.NomesNPCsPendentes.Count > 0)
            {
                string nomes = "";
                for (int i = 0; i < resumo.NomesNPCsPendentes.Count; i++)
                {
                    if (i > 0) nomes += i == resumo.NomesNPCsPendentes.Count - 1 ? " e " : ", ";
                    nomes += resumo.NomesNPCsPendentes[i];
                }
                resumo.DefinirProximaAcao("Converse com " + nomes + ".");
            }
            return resumo;
        }

        // =====================================================================
        // Validação central de interações (Etapa 4.1)
        // =====================================================================

        /// <summary>
        /// Avalia o que acontece se o jogador conversar com este NPC AGORA.
        /// Decisão central — o NPC nunca decide por conta própria.
        /// </summary>
        public ResultadoInteracao AvaliarInteracao(string npcId)
        {
            var npc = _catalogo.ObterNPC(npcId);

            // NPC fora do catálogo (Diretor, Secretaria) ou informativo (Alice):
            // conversa livre, sem progressão.
            if (npc == null || npc.Tipo == TipoNPC.InformationalNPC)
                return ResultadoInteracao.InformationalNpc;

            var defM = _catalogo.ObterMissao(npc.AssociatedMissionId);
            if (defM == null) return ResultadoInteracao.InformationalNpc; // defensivo

            var estado = ObterEstadoMissao(defM.MissionId);
            if (estado == EstadoMissao.Completed)
                return ResultadoInteracao.MissionAlreadyCompleted;

            // Sem missão ativa: só o iniciador da própria missão pode começar algo.
            if (!TemMissaoAtiva)
            {
                if (npc.Tipo == TipoNPC.MissionInitiator)
                {
                    return estado == EstadoMissao.Available
                        ? ResultadoInteracao.InteractionAllowed
                        : ResultadoInteracao.MissingRequirements;
                }
                return ResultadoInteracao.NoActiveMission;
            }

            // Há uma missão ativa e ela não é deste NPC.
            if (_missaoAtiva != defM.MissionId)
                return ResultadoInteracao.AnotherMissionActive;

            // A missão ativa é deste NPC.
            var etapa = ObterEtapaAtual(_missaoAtiva);
            if (etapa == null) return ResultadoInteracao.WrongNPCForCurrentStep; // defensivo

            if (etapa == defM.UltimaEtapa)
            {
                // ReadyToTurnIn: todos os objetivos anteriores estão feitos.
                if (npcId == defM.FinalNPCId)
                    return ResultadoInteracao.MissionReadyToTurnIn;
                if (NpcTemObjetivoPendenteNaMissao(defM, npcId))
                    return ResultadoInteracao.WrongNPCForCurrentStep; // não deveria ocorrer
                if (NpcPossuiObjetivos(defM, npcId))
                    return ResultadoInteracao.ObjectiveAlreadyCompleted;
                return ResultadoInteracao.WrongNPCForCurrentStep;
            }

            // Etapa intermediária: o NPC é esperado nesta etapa?
            if (EtapaEsperaNPC(etapa, npcId))
            {
                return TemObjetivoPendenteDoNPC(etapa, npcId)
                    ? ResultadoInteracao.InteractionAllowed
                    : ResultadoInteracao.ObjectiveAlreadyCompleted;
            }

            // Não é esperado na etapa atual.
            if (NpcTemObjetivoPendenteNaMissao(defM, npcId))
                return ResultadoInteracao.WrongNPCForCurrentStep;
            if (NpcPossuiObjetivos(defM, npcId))
                return ResultadoInteracao.ObjectiveAlreadyCompleted;
            return ResultadoInteracao.WrongNPCForCurrentStep;
        }

        // ---- Auxiliares de NPC x etapa x objetivo ----

        private bool EtapaEsperaNPC(DefinicaoEtapa etapa, string npcId)
        {
            for (int i = 0; i < etapa.ExpectedNPCIds.Count; i++)
                if (etapa.ExpectedNPCIds[i] == npcId) return true;
            for (int i = 0; i < etapa.Objectives.Count; i++)
                if (etapa.Objectives[i].OwnerNPCId == npcId) return true;
            return false;
        }

        /// <summary>Objetivo obrigatório pendente desta etapa pertencente ao NPC (null se não houver).</summary>
        private DefinicaoObjetivo ObterObjetivoPendenteDoNPC(DefinicaoEtapa etapa, string npcId)
        {
            for (int i = 0; i < etapa.Objectives.Count; i++)
            {
                var obj = etapa.Objectives[i];
                if (obj.OwnerNPCId == npcId && obj.Required && !_objetivosConcluidos.Contains(obj.ObjectiveId))
                    return obj;
            }
            return null;
        }

        private bool TemObjetivoPendenteDoNPC(DefinicaoEtapa etapa, string npcId)
        {
            return ObterObjetivoPendenteDoNPC(etapa, npcId) != null;
        }

        private bool NpcTemObjetivoPendenteNaMissao(DefinicaoMissao defM, string npcId)
        {
            for (int e = 0; e < defM.Steps.Count; e++)
                if (TemObjetivoPendenteDoNPC(defM.Steps[e], npcId)) return true;
            return false;
        }

        private bool NpcPossuiObjetivos(DefinicaoMissao defM, string npcId)
        {
            for (int e = 0; e < defM.Steps.Count; e++)
                for (int o = 0; o < defM.Steps[e].Objectives.Count; o++)
                    if (defM.Steps[e].Objectives[o].OwnerNPCId == npcId) return true;
            return false;
        }

        // =====================================================================
        // Registro de eventos (única forma de progressão)
        // =====================================================================

        /// <summary>
        /// Registra o encerramento de um diálogo com o NPC. Regras:
        ///   - objetivos de etapas de DIÁLOGO são registrados AQUI (1x);
        ///   - objetivos de etapas de MINIJOGO NUNCA são registrados aqui —
        ///     somente via RegistrarResultadoMinijogo (fix do fluxo Márcio/Roney);
        ///   - só o iniciador inicia missões (quando Available e sem missão ativa);
        ///   - só o NPC finalizador conclui a missão (MissionReadyToTurnIn).
        /// </summary>
        public ResultadoEncerramento RegistrarEncerramentoDialogo(string npcId)
        {
            var resultado = new ResultadoEncerramento();
            resultado.Resultado = AvaliarInteracao(npcId);

            var npc = _catalogo.ObterNPC(npcId);
            if (npc == null) return resultado; // fora do catálogo: nada a fazer

            if (resultado.Resultado == ResultadoInteracao.InteractionAllowed)
            {
                var defM = _catalogo.ObterMissao(npc.AssociatedMissionId);
                if (defM == null) return resultado;

                // Início de missão: somente o iniciador, somente sem slot ocupado,
                // somente se Available (requisitos de cadeia cumpridos).
                if (!TemMissaoAtiva && npc.Tipo == TipoNPC.MissionInitiator &&
                    ObterEstadoMissao(defM.MissionId) == EstadoMissao.Available)
                {
                    _missaoAtiva = defM.MissionId;
                    resultado.MissaoIniciada = true;
                    resultado.MissaoIniciadaId = defM.MissionId;
                    _log("[MISSION] Missão iniciada: " + defM.MissionId + " (" + defM.MissionName + ").");
                    if (AoIniciarMissao != null) AoIniciarMissao(defM.MissionId);
                }

                // Registro de objetivo: apenas etapas de diálogo.
                if (TemMissaoAtiva && _missaoAtiva == defM.MissionId)
                {
                    var etapa = ObterEtapaAtual(_missaoAtiva);
                    if (etapa != null && etapa.ExpectedInteractionType == TipoInteracaoEtapa.Dialogo)
                    {
                        var obj = ObterObjetivoPendenteDoNPC(etapa, npcId);
                        if (obj != null)
                            RegistrarObjetivoInterno(defM, etapa, obj, resultado);
                    }
                }
            }
            else if (resultado.Resultado == ResultadoInteracao.MissionReadyToTurnIn)
            {
                var defM = _catalogo.ObterMissao(_missaoAtiva);
                if (defM != null && npcId == defM.FinalNPCId)
                {
                    // Registra os objetivos da etapa de finalização e conclui a missão.
                    var etapaFinal = defM.UltimaEtapa;
                    if (etapaFinal != null)
                        for (int o = 0; o < etapaFinal.Objectives.Count; o++)
                        {
                            var obj = etapaFinal.Objectives[o];
                            if (obj.Required && !_objetivosConcluidos.Contains(obj.ObjectiveId))
                                RegistrarObjetivoInterno(defM, etapaFinal, obj, resultado);
                        }
                    FinalizarMissao(defM, resultado);
                }
            }

            return resultado;
        }

        /// <summary>Registra um objetivo, dispara eventos e verifica a conclusão da etapa.</summary>
        private void RegistrarObjetivoInterno(DefinicaoMissao defM, DefinicaoEtapa etapa,
                                              DefinicaoObjetivo obj, ResultadoEncerramento resultado)
        {
            if (_objetivosConcluidos.Contains(obj.ObjectiveId)) return;
            _objetivosConcluidos.Add(obj.ObjectiveId);

            if (resultado != null)
            {
                resultado.ObjetivoRegistrado = true;
                resultado.ObjetivoRegistradoId = obj.ObjectiveId;
            }

            _log("[MISSION] Objetivo concluído: " + obj.ObjectiveId + " (" + obj.Descricao + ").");
            if (AoConcluirObjetivo != null) AoConcluirObjetivo(defM.MissionId, obj.ObjectiveId);

            if (EtapaEstaConcluida(etapa))
            {
                if (resultado != null)
                {
                    resultado.EtapaConcluida = true;
                    resultado.EtapaConcluidaId = etapa.StepId;
                }
                _log("[MISSION] Etapa concluída: " + etapa.StepId + " (" + etapa.StepOrder + "/" +
                     defM.Steps.Count + " de " + defM.MissionId + ").");
                if (AoConcluirEtapa != null) AoConcluirEtapa(defM.MissionId, etapa.StepId);
            }
        }

        /// <summary>Move a missão para concluídas e libera o slot de missão ativa.</summary>
        private void FinalizarMissao(DefinicaoMissao defM, ResultadoEncerramento resultado)
        {
            if (_missoesCompletas.Contains(defM.MissionId)) return;
            _missoesCompletas.Add(defM.MissionId);
            if (_missaoAtiva == defM.MissionId) _missaoAtiva = "";

            if (resultado != null)
            {
                resultado.MissaoFinalizada = true;
                resultado.MissaoFinalizadaId = defM.MissionId;
            }

            _log("[MISSION] Missão concluída: " + defM.MissionId + " (" + defM.MissionName + ").");
            if (AoConcluirMissao != null) AoConcluirMissao(defM.MissionId);
        }

        /// <summary>Conclui uma missão integralmente (migração/consistência/legado flat).</summary>
        private void CompletarMissaoInterna(string missionId, string origem)
        {
            var defM = _catalogo.ObterMissao(missionId);
            if (defM == null) return;
            for (int e = 0; e < defM.Steps.Count; e++)
            {
                var etapa = defM.Steps[e];
                for (int o = 0; o < etapa.Objectives.Count; o++)
                {
                    var obj = etapa.Objectives[o];
                    if (obj.Required && !_objetivosConcluidos.Contains(obj.ObjectiveId))
                        _objetivosConcluidos.Add(obj.ObjectiveId);
                }
            }
            _missoesCompletas.Add(missionId);
            if (_missaoAtiva == missionId) _missaoAtiva = "";
            _log("[MISSION] Missão concluída: " + missionId + " (" + origem + ").");
            if (AoConcluirMissao != null) AoConcluirMissao(missionId);
        }

        // =====================================================================
        // Autorização e resultado de minijogos (Etapa 9)
        // =====================================================================

        /// <summary>
        /// Autorização central para abrir um minijogo. Sem autorização o
        /// minijogo permanece bloqueado; NPCs fora do catálogo (Secretaria)
        /// NUNCA recebem autorização.
        /// </summary>
        public AutorizacaoMinijogo AutorizarMinijogo(string npcId, string idMinijogo)
        {
            var autorizacao = new AutorizacaoMinijogo();
            autorizacao.NPCId = npcId ?? "";
            autorizacao.IdMinijogo = idMinijogo ?? "";

            var npc = _catalogo.ObterNPC(npcId);
            if (npc == null)
            {
                autorizacao.Motivo = "NPC fora do catálogo — minijogo não autorizado.";
                _log("[MISSION] Minijogo NEGADO a '" + npcId + "': " + autorizacao.Motivo);
                return autorizacao;
            }

            if (!TemMissaoAtiva)
            {
                autorizacao.Motivo = "Nenhuma missão ativa.";
                return autorizacao;
            }

            var defM = _catalogo.ObterMissao(npc.AssociatedMissionId);
            if (defM == null || defM.MissionId != _missaoAtiva)
            {
                autorizacao.Motivo = "A missão ativa não é a deste NPC (" + _missaoAtiva + ").";
                return autorizacao;
            }

            var etapa = ObterEtapaAtual(_missaoAtiva);
            if (etapa == null || etapa.ExpectedInteractionType != TipoInteracaoEtapa.Minijogo)
            {
                autorizacao.Motivo = "A etapa atual não é um minijogo.";
                return autorizacao;
            }

            if (etapa.IdMinijogo != autorizacao.IdMinijogo)
            {
                autorizacao.Motivo = "Minijogo diferente do esperado pela etapa (" + etapa.IdMinijogo + ").";
                return autorizacao;
            }

            if (!EtapaEsperaNPC(etapa, npcId))
            {
                autorizacao.Motivo = "NPC não esperado nesta etapa.";
                return autorizacao;
            }

            var obj = ObterObjetivoPendenteDoNPC(etapa, npcId);
            if (obj == null)
            {
                autorizacao.Motivo = "Objetivo desta etapa já concluído.";
                return autorizacao;
            }

            autorizacao.Autorizado = true;
            autorizacao.Motivo = "Autorizado pela etapa " + etapa.StepId + ".";
            autorizacao.MissionId = defM.MissionId;
            autorizacao.StepId = etapa.StepId;
            autorizacao.ObjectiveId = obj.ObjectiveId;
            _log("[MISSION] Minijogo '" + autorizacao.IdMinijogo + "' AUTORIZADO para " + npcId +
                 " (missão " + defM.MissionId + ", etapa " + etapa.StepId + ", objetivo " + obj.ObjectiveId + ").");
            return autorizacao;
        }

        /// <summary>
        /// Registra o resultado de um minijogo. Só é aceito se a missão/objetivo
        /// corresponderem à etapa de minijogo ATUAL (validação central — Etapa 9).
        /// Vitória registra o objetivo (única via para etapas de minijogo);
        /// derrota NÃO registra nada.
        /// </summary>
        public bool RegistrarResultadoMinijogo(string missionId, string objectiveId, bool sucesso, out string motivo)
        {
            motivo = "";

            if (string.IsNullOrEmpty(missionId) || !TemMissaoAtiva || _missaoAtiva != missionId)
            {
                motivo = "A missão ativa não é esta (" + _missaoAtiva + ").";
                return false;
            }

            var defM = _catalogo.ObterMissao(missionId);
            var etapa = ObterEtapaAtual(missionId);
            if (defM == null || etapa == null || etapa.ExpectedInteractionType != TipoInteracaoEtapa.Minijogo)
            {
                motivo = "A etapa atual não é um minijogo.";
                return false;
            }

            DefinicaoObjetivo obj = null;
            for (int i = 0; i < etapa.Objectives.Count; i++)
                if (etapa.Objectives[i].ObjectiveId == objectiveId) obj = etapa.Objectives[i];
            if (obj == null)
            {
                motivo = "O objetivo não pertence à etapa atual (" + etapa.StepId + ").";
                return false;
            }
            if (_objetivosConcluidos.Contains(objectiveId))
            {
                motivo = "Objetivo já concluído.";
                return false;
            }

            if (!sucesso)
            {
                _log("[MISSION] Minijogo '" + etapa.IdMinijogo + "' falhou — objetivo NÃO registrado.");
                return false;
            }

            RegistrarObjetivoInterno(defM, etapa, obj, null);
            return true;
        }

        // =====================================================================
        // Camada de compatibilidade legada (variantes de diálogo existentes)
        // =====================================================================

        /// <summary>
        /// Conclusão de missão por id legado (usada pelas VARIANTES de diálogo).
        /// missao_administracao equivale a missao_ensino_medio. Uma missão conta
        /// como "completa" para variantes quando está Completed OU ReadyToTurnIn
        /// (todos os objetivos cumpridos, aguardando só o iniciador).
        /// </summary>
        public bool MissaoCompleta(string idMissao)
        {
            if (string.IsNullOrEmpty(idMissao)) return false;
            if (idMissao == "missao_administracao") idMissao = "missao_ensino_medio";

            if (_missoesCompletas.Contains(idMissao)) return true;

            var defM = _catalogo.ObterMissao(idMissao);
            if (defM == null) return false;
            var estado = ObterEstadoMissao(idMissao);
            return estado == EstadoMissao.ReadyToTurnIn || estado == EstadoMissao.Completed;
        }

        /// <summary>
        /// Passo "legado" de uma missão — derivado das etapas do modelo novo,
        /// mantendo os limiares que as variantes legadas esperam:
        ///   missao_ensino_medio/administracao: 1 apresentação, 2 explicação,
        ///     3 servidor, 4 quando pronta/concluída;
        ///   missao_biblioteca: 1 cadastro, 2 lembretes, 4 pronta/concluída;
        ///   missao_nucleo: 1 Josy, 2 David, 5 compilação/pronta/concluída;
        ///   missao_refeitorio: 1 apresentação, 2 pronta/concluída;
        ///   missao_assistencia: 1 quando a flag conversou_missao_assistencia existir.
        /// Ids desconhecidos devolvem 0.
        /// </summary>
        public int ObterPassoMissao(string idMissao)
        {
            if (string.IsNullOrEmpty(idMissao)) return 0;
            if (idMissao == "missao_administracao") idMissao = "missao_ensino_medio";
            if (idMissao == "missao_assistencia")
                return TemFlag("conversou_missao_assistencia") || _missoesCompletas.Contains("missao_assistencia") ? 1 : 0;

            var defM = _catalogo.ObterMissao(idMissao);
            if (defM == null) return 0;

            var estado = ObterEstadoMissao(idMissao);
            switch (idMissao)
            {
                case "missao_ensino_medio":
                    return (estado == EstadoMissao.ReadyToTurnIn || estado == EstadoMissao.Completed) ? 4 : ObterOrdemEtapaAtual(defM);
                case "missao_biblioteca":
                    return (estado == EstadoMissao.ReadyToTurnIn || estado == EstadoMissao.Completed) ? 4 : ObterOrdemEtapaAtual(defM);
                case "missao_nucleo":
                    if (estado == EstadoMissao.ReadyToTurnIn || estado == EstadoMissao.Completed) return 5;
                    var etapaNucleo = ObterEtapaAtual(idMissao);
                    if (etapaNucleo == null) return 0;
                    if (etapaNucleo.StepId == "Q03_COMPILACAO") return 5;
                    return etapaNucleo.StepOrder <= 2 ? etapaNucleo.StepOrder : 2;
                case "missao_refeitorio":
                    return (estado == EstadoMissao.ReadyToTurnIn || estado == EstadoMissao.Completed) ? 2 : 1;
            }
            return 0;
        }

        /// <summary>Ordem (1..N) da etapa atual; 0 quando não há missão ativa/sem etapas.</summary>
        private int ObterOrdemEtapaAtual(DefinicaoMissao defM)
        {
            if (defM == null || !TemMissaoAtiva || _missaoAtiva != defM.MissionId) return 0;
            var etapa = ObterEtapaAtual(defM.MissionId);
            return etapa != null ? etapa.StepOrder : 0;
        }

        /// <summary>
        /// Conclusão flat legada (GameManager.CompletarMissao / NPCs fora do
        /// catálogo, ex.: Diretor). Missões do catálogo são concluídas por
        /// completo (com objetivos); ids desconhecidos são apenas preservados.
        /// </summary>
        public void CompletarMissaoFlat(string idMissao)
        {
            if (string.IsNullOrEmpty(idMissao)) return;
            var defM = _catalogo.ObterMissao(idMissao);
            if (defM != null)
            {
                if (_missoesCompletas.Contains(defM.MissionId))
                {
                    ValidarConsistencia();
                    return;
                }
                CompletarMissaoInterna(defM.MissionId, "conclusão flat legada");
                ValidarConsistencia();
                return;
            }
            if (!_missoesCompletas.Contains(idMissao))
            {
                _missoesCompletas.Add(idMissao);
                _log("[MISSION] Id legado concluído (flat): " + idMissao);
            }
        }
    }
}

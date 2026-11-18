namespace IF360
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using IF360.Missoes;

    /// <summary>
    /// AUTORIDADE CENTRAL de missões do IF 360°.
    ///
    /// Hospeda o MotorRegrasMissao (lógica pura, assembly IF360.Missoes) e é o
    /// único ponto por onde o progresso de missões flui:
    ///   - NPCs perguntam "posso conversar/abrir minijogo?" (AvaliarInteracao/
    ///     AutorizarMinijogo) e reportam o encerramento (RegistrarEncerramentoDialogo);
    ///   - minijogos reportam vitória/derrota (RegistrarResultadoMinijogo);
    ///   - o estado vivo (objetivos concluídos) é derivado pelo motor, nunca
    ///     editado diretamente por NPCs;
    ///   - persistência v2 com migração automática do formato antigo.
    ///
    /// Mantém a API legada (MissaoCompleta/ObterPassoAtual/MarcarFlag/...) para
    /// as variantes de diálogo existentes — sem editar cenas ou assets.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instancia { get; private set; }

        /// <summary>Disparado quando uma etapa de missão é concluída (idMissao, ordem da etapa 1..N).</summary>
        public static event Action<string, int> EtapaConcluida;

        /// <summary>Disparado quando uma missão inteira é concluída (idMissao).</summary>
        public static event Action<string> MissaoCompletada;

        /// <summary>Disparado quando o NPC iniciador começa uma missão (idMissao).</summary>
        public static event Action<string> MissaoIniciada;

        private MotorRegrasMissao _motor;
        private DialogosContextuais _dialogos;

        /// <summary>Guarda de sessão: restaura o save apenas uma vez por sessão de jogo.</summary>
        private bool _restauradoNestaSessao;

        // Reset defensivo do singleton/eventos para "Enter Play Mode" sem Domain Reload.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetarInstanciaEstatica()
        {
            Instancia = null;
            EtapaConcluida = null;
            MissaoCompletada = null;
            MissaoIniciada = null;
        }

        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this;

            // O estado das missões precisa sobreviver à troca de cena.
            DontDestroyOnLoad(gameObject);

            _motor = new MotorRegrasMissao(CatalogoIf360.Criar(), msg => Debug.Log(msg));
            _dialogos = new DialogosContextuais(_motor);

            _motor.AoIniciarMissao += EncaminharMissaoIniciada;
            _motor.AoConcluirEtapa += EncaminharEtapaConcluida;
            _motor.AoConcluirMissao += EncaminharMissaoConcluida;

            Debug.Log("[MISSION] QuestManager central iniciado — catálogo com " +
                      _motor.TotalMissoesCatalogo + " missões e " +
                      _motor.Catalogo.NPCs.Count + " NPCs.");
        }

        private void OnDestroy()
        {
            if (_motor != null)
            {
                _motor.AoIniciarMissao -= EncaminharMissaoIniciada;
                _motor.AoConcluirEtapa -= EncaminharEtapaConcluida;
                _motor.AoConcluirMissao -= EncaminharMissaoConcluida;
            }
            if (Instancia == this) Instancia = null;
        }

        // ---------- Encaminhamento de eventos do motor ----------

        private void EncaminharMissaoIniciada(string idMissao)
        {
            MissaoIniciada?.Invoke(idMissao);
        }

        private void EncaminharEtapaConcluida(string idMissao, string stepId)
        {
            EtapaConcluida?.Invoke(idMissao, OrdemDaEtapa(idMissao, stepId));
        }

        private void EncaminharMissaoConcluida(string idMissao)
        {
            MissaoCompletada?.Invoke(idMissao);
        }

        private int OrdemDaEtapa(string idMissao, string stepId)
        {
            var defM = _motor.Catalogo.ObterMissao(idMissao);
            if (defM == null) return 0;
            for (int i = 0; i < defM.Steps.Count; i++)
                if (defM.Steps[i].StepId == stepId) return defM.Steps[i].StepOrder;
            return 0;
        }

        // =====================================================================
        // API CENTRAL (novo modelo — usada pelo NPCBaseNovo e pelos minijogos)
        // =====================================================================

        /// <summary>Motor de regras vivo (exposto para consultas e testes de integração).</summary>
        public MotorRegrasMissao Motor { get { return _motor; } }

        /// <summary>Resolve o perfil do NPC pelo nome exibido na cena (nomeNPC).</summary>
        public PerfilNPC ObterPerfilPorNome(string nomeExibicao)
        {
            return _motor != null ? _motor.Catalogo.ObterNPCPorNome(nomeExibicao) : null;
        }

        public PerfilNPC ObterPerfil(string npcId)
        {
            return _motor != null ? _motor.Catalogo.ObterNPC(npcId) : null;
        }

        /// <summary>Este NPCId pertence ao catálogo? (Diretor/Secretaria não pertencem.)</summary>
        public bool ConheceNPC(string npcId)
        {
            return _motor != null && _motor.Catalogo.ConheceNPC(npcId);
        }

        /// <summary>Validação central: o que acontece se o jogador conversar com este NPC agora?</summary>
        public ResultadoInteracao AvaliarInteracao(string npcId)
        {
            return _motor != null ? _motor.AvaliarInteracao(npcId) : ResultadoInteracao.InformationalNpc;
        }

        /// <summary>
        /// Registra o fim de um diálogo com o NPC (única via de progressão por
        /// conversa). O motor inicia a missão, registra objetivos de etapas de
        /// diálogo ou finaliza a missão, conforme a avaliação central.
        /// </summary>
        public ResultadoEncerramento RegistrarEncerramentoDialogo(string npcId)
        {
            return _motor != null ? _motor.RegistrarEncerramentoDialogo(npcId) : null;
        }

        /// <summary>Autorização central para abrir um minijogo (Etapa 9 do plano).</summary>
        public AutorizacaoMinijogo AutorizarMinijogo(string npcId, string idMinijogo)
        {
            return _motor != null
                ? _motor.AutorizarMinijogo(npcId, idMinijogo)
                : new AutorizacaoMinijogo { Motivo = "QuestManager ausente." };
        }

        /// <summary>Reporte central do resultado de um minijogo (vitória registra o objetivo).</summary>
        public bool RegistrarResultadoMinijogo(string missionId, string objectiveId, bool sucesso, out string motivo)
        {
            if (_motor == null)
            {
                motivo = "QuestManager ausente.";
                return false;
            }
            return _motor.RegistrarResultadoMinijogo(missionId, objectiveId, sucesso, out motivo);
        }

        /// <summary>Resumo vivo da missão ativa (HUD e diálogos dinâmicos).</summary>
        public ResumoProgresso ObterResumoProgresso()
        {
            return _motor != null ? _motor.ObterResumoProgresso() : new ResumoProgresso();
        }

        public EstadoMissao ObterEstadoMissao(string idMissao)
        {
            return _motor != null ? _motor.ObterEstadoMissao(idMissao) : EstadoMissao.Locked;
        }

        /// <summary>Total de missões estruturadas do catálogo (para "X de N" no HUD).</summary>
        public int TotalMissoesCatalogo
        {
            get { return _motor != null ? _motor.TotalMissoesCatalogo : 0; }
        }

        /// <summary>Este resultado deve exibir diálogo dinâmico (em vez da variante legada)?</summary>
        public static bool PrecisaDialogoDinamico(ResultadoInteracao resultado)
        {
            return DialogosContextuais.PrecisaDialogoDinamico(resultado);
        }

        /// <summary>
        /// Falas dinâmicas do NPC para o resultado avaliado (Situações A/B/C ou
        /// MissingRequirements). Devolve null quando o resultado usa variante legada.
        /// </summary>
        public List<FalaDinamica> GerarFalasDinamicas(string npcId, ResultadoInteracao resultado)
        {
            return _dialogos != null ? _dialogos.GerarFalas(resultado, npcId) : null;
        }

        // =====================================================================
        // API LEGADA (variantes de diálogo existentes — compatibilidade)
        // =====================================================================

        /// <summary>
        /// Conclusão flat legada (ex.: Diretor fora do catálogo). Missões do
        /// catálogo são concluídas integralmente; ids desconhecidos são preservados.
        /// </summary>
        public void CompletarMissao(string idMissao)
        {
            if (_motor == null || string.IsNullOrEmpty(idMissao)) return;
            _motor.CompletarMissaoFlat(idMissao);
        }

        /// <summary>Missão "completa" para variantes (Completed OU ReadyToTurnIn; administracao→ensino_medio).</summary>
        public bool MissaoCompleta(string idMissao)
        {
            return _motor != null && _motor.MissaoCompleta(idMissao);
        }

        /// <summary>Missões concluídas do catálogo (ids legados não contam).</summary>
        public int QuantidadeMissoesCompletas()
        {
            return _motor != null ? _motor.QuantidadeMissoesCompletas() : 0;
        }

        /// <summary>Passo legado da missão (derivado das etapas; limiares das variantes).</summary>
        public int ObterPassoAtual(string idMissao)
        {
            return _motor != null ? _motor.ObterPassoMissao(idMissao) : 0;
        }

        /// <summary>Alias legado de ObterPassoAtual (usado pelo GameManager).</summary>
        public int ObterPassoMissao(string idMissao)
        {
            return ObterPassoAtual(idMissao);
        }

        /// <summary>DEPRECADO: o progresso agora vive no motor central (objetivos).</summary>
        public bool ConcluirEtapa(string idMissao)
        {
            Debug.LogWarning("[MISSION] ConcluirEtapa('" + idMissao + "') é obsoleto — o progresso " +
                             "avança via RegistrarEncerramentoDialogo/RegistrarResultadoMinijogo. Ignorado.");
            return false;
        }

        /// <summary>DEPRECADO: o passo é derivado dos objetivos concluídos.</summary>
        public void DefinirPasso(string idMissao, int passo)
        {
            Debug.LogWarning("[MISSION] DefinirPasso('" + idMissao + "', " + passo + ") é obsoleto — " +
                             "o passo é derivado dos objetivos concluídos. Ignorado.");
        }

        // ---------- Flags do mundo ----------

        /// <summary>Marca uma flag. Retorna true se foi marcada agora.</summary>
        public bool MarcarFlag(string idFlag)
        {
            if (_motor == null || string.IsNullOrWhiteSpace(idFlag)) return false;
            if (_motor.TemFlag(idFlag)) return false;
            _motor.MarcarFlag(idFlag);
            return true;
        }

        public bool TemFlag(string idFlag)
        {
            return _motor != null && _motor.TemFlag(idFlag);
        }

        public void LimparFlag(string idFlag)
        {
            // Flags são só compatibilidade legada; limpeza explícita não é
            // suportada pelo repositório central (nunca usada pelo jogo).
            Debug.LogWarning("[MISSION] LimparFlag('" + idFlag + "') não é suportado pelo modelo central. Ignorado.");
        }

        // =====================================================================
        // Persistência (formato v2 + migração do formato antigo)
        // =====================================================================

        public string SerializarMissoes()
        {
            if (_motor == null) return "";
            return JsonUtility.ToJson(_motor.ExportarEstado());
        }

        public void RestaurarMissoes(string json)
        {
            if (_motor == null) return;

            if (string.IsNullOrEmpty(json))
            {
                _motor.Resetar();
                return;
            }

            // Formato v2?
            var v2 = JsonUtility.FromJson<DadosProgressoSalvos>(json);
            if (v2 != null && v2.versao == 2)
            {
                _motor.ImportarEstado(v2);
                Debug.Log("[MISSION] Progresso v2 restaurado (missão ativa: '" +
                          (string.IsNullOrEmpty(v2.missaoAtiva) ? "nenhuma" : v2.missaoAtiva) + "').");
                return;
            }

            // Formato antigo → migração para v2 (preserva o progresso salvo).
            var legado = JsonUtility.FromJson<DadosLegado>(json);
            bool temDadosLegados = legado != null &&
                ((legado.idsCompletas != null && legado.idsCompletas.Count > 0) ||
                 (legado.missaoIds != null && legado.missaoIds.Count > 0) ||
                 (legado.idsFlags != null && legado.idsFlags.Count > 0));

            if (temDadosLegados)
            {
                _motor.MigrarEstadoLegado(legado);
                return;
            }

            _motor.Resetar();
        }

        /// <summary>
        /// Restaura o save apenas uma vez por sessão (o QuestManager é
        /// DontDestroyOnLoad: voltar ao menu e continuar não deve sobrescrever
        /// o progresso vivo em memória).
        /// </summary>
        public void RestaurarSeNecessario(string json)
        {
            if (_restauradoNestaSessao)
            {
                Debug.Log("[MISSION] Progresso já restaurado nesta sessão — mantendo estado em memória.");
                return;
            }
            _restauradoNestaSessao = true;
            RestaurarMissoes(json);
        }

        /// <summary>Zera todo o progresso (novo jogo).</summary>
        public void LimparMissoes()
        {
            if (_motor != null) _motor.Resetar();
            _restauradoNestaSessao = false;
        }
    }
}

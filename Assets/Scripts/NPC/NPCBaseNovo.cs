namespace IF360
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using IF360.Missoes;

    /// <summary>
    /// Uma variante de conversa com condições. O NPC usa a PRIMEIRA variante cujas
    /// condições forem satisfeitas. Deixe uma variante sem condições no fim para
    /// servir de "fallback" (o diálogo que se repete).
    /// </summary>
    [Serializable]
    public class VarianteDialogo
    {
        [Tooltip("Diálogo mostrado quando as condições abaixo forem satisfeitas.")]
        public DialogoData dialogo;

        [Tooltip("Missão que precisa estar COMPLETA. Vazio = ignora esta condição.")]
        public string requisitoMissao = "";

        [Tooltip("Flag que precisa estar marcada (ex.: conversou_com_diretor). Vazio = ignora.")]
        public string requisitoFlag = "";

        [Tooltip("Passo mínimo exigido na missão (a 'requisitoMissao' ou o idMissao do NPC). -1 = ignora.")]
        public int requisitoEtapaMinima = -1;
    }

    /// <summary>
    /// Representa UM NPC: quem ele é, qual é o seu diálogo e como ele reage ao
    /// fim de uma conversa.
    ///
    /// REESTRUTURAÇÃO DAS MISSÕES (modelo central):
    ///   - Este script NÃO decide progressão de missão. Ao interagir, ele
    ///     PERGUNTA ao QuestManager (autoridade central) o que pode acontecer
    ///     e mostra diálogo dinâmico quando o resultado pede orientação;
    ///   - Ao encerrar a conversa, ele REPASSA o encerramento ao QuestManager
    ///     (única via de progressão por diálogo);
    ///   - Minijogos só abrem com AUTORIZAÇÃO central (AutorizarMinijogo);
    ///   - NPCs fora do catálogo (ex.: Diretor, Secretaria) mantêm o
    ///     comportamento legado: conversam, marcam flags e concluem missões
    ///     flat, mas não participam das missões estruturadas (Q01–Q04).
    ///
    /// Comunicação com sistemas externos (inalterada): eventos estáticos
    /// JogadorEntrouNaArea/JogadorSaiuDaArea (interação), DialogoSolicitado
    /// (sistema de diálogo) e MinijogoSolicitado (sistema de minijogo).
    /// </summary>
    public class NPCBaseNovo : MonoBehaviour, IInteragivel
    {
        private const string TAG_PLAYER = "Player";

        // ---------- Comunicação com sistemas externos ----------

        /// <summary>Disparado quando o jogador entra na área de interação deste NPC.</summary>
        public static event Action<NPCBaseNovo> JogadorEntrouNaArea;

        /// <summary>Disparado quando o jogador sai da área (ou quando o NPC é desativado com o jogador dentro).</summary>
        public static event Action<NPCBaseNovo> JogadorSaiuDaArea;

        /// <summary>Disparado por Interagir(): o sistema de diálogo deve abrir a conversa deste NPC.</summary>
        public static event Action<NPCBaseNovo> DialogoSolicitado;

        /// <summary>Disparado por IniciarMinijogo(): o sistema de minijogo deve abrir o desafio.</summary>
        public static event Action<NPCBaseNovo> MinijogoSolicitado;

        // ---------- Dados configuráveis no Inspector ----------

        [Header("Identidade")]
        public string nomeNPC = "NPC";
        public string idMissao = "missao_padrao";
        [Tooltip("Missão que precisa estar COMPLETA para este NPC mostrar 'dialogoDataPosMissao'. Vazio = usa o próprio idMissao.")]
        public string missaoRequisito = "";

        [Header("Catálogo central (opcional)")]
        [Tooltip("Id do NPC no catálogo central (ex.: NPC_RONEY). Vazio = resolvido automaticamente pelo nomeNPC em runtime.")]
        public string npcIdOverride = "";

        [Header("Dados do diálogo")]
        [Tooltip("Dados do diálogo deste NPC (preferencial).")]
        public DialogoData dialogoData;
        [Tooltip("Conversa exibida DEPOIS que a missão (idMissao) deste NPC estiver completa.")]
        public DialogoData dialogoDataPosMissao;
        [Tooltip("LEGADO (NPCs fora do catálogo): completa a missão (idMissao) quando a conversa terminar.")]
        public bool completarMissaoAoTerminar;
        [Tooltip("Fala de reserva, usada enquanto o NPC ainda não tem DialogoData.")]
        [TextArea] public string falaApresentacao = "Olá!";

        [Header("Variantes de diálogo por condição (avaliadas em ordem)")]
        [Tooltip("Cada variante tem um diálogo + condições (missão/flag/etapa). " +
                 "Vence a PRIMEIRA cujas condições baterem. Use uma última sem condições como fallback.")]
        public VarianteDialogo[] variantes;

        [Header("Registro de conversa (flags)")]
        [Tooltip("Ao terminar uma conversa, marca automaticamente a flag 'conversou_<idMissao>' " +
                 "(ou o idFlagConversa abaixo). Outros NPCs podem exigir essa flag.")]
        public bool registrarConversa = true;
        [Tooltip("Nome da flag gravada ao conversar. Vazio = 'conversou_' + idMissao (ou nomeNPC).")]
        public string idFlagConversa = "";

        [Header("Ação após o diálogo")]
        [Tooltip("Se true, este NPC pede um minijogo ao fim da conversa (a abertura real depende de AUTORIZAÇÃO central).")]
        public bool chamarMinijogo = true;

        [Tooltip("Se true, o minijogo é chamado como PARTE da missão. Se false, só é chamado APÓS a missão estar completa (legado).")]
        public bool minijogoComoParteDaMissao = false;

        [Tooltip("Qual minijogo este NPC abre ('fios' = ligar cabos do servidor, 'compilacao' = padrões do projeto).")]
        public string idMinijogo = "fios";

        [Header("Controles legados (só NPCs fora do catálogo)")]
        [Tooltip("LEGADO: conclui ETAPA em vez de missão (não usado pelo modelo central).")]
        public bool usarModeloEtapas = false;

        [Tooltip("LEGADO: só conclui na PRIMEIRA conversa (enquanto a flag de conversa não existir).")]
        public bool concluirApenasNaPrimeiraConversa = false;

        [Tooltip("LEGADO: passo mínimo da missão (idMissao) exigido para concluir algo. -1 = ignora.")]
        public int requisitoEtapaMinimaParaConcluir = -1;

        // ---------- Estado interno ----------

        private bool jogadorPorPerto;
        private bool minijogoTentado;
        private PerfilNPC _perfilCache;
        private DialogoData _dialogoDinamicoAtual;
        private ResultadoInteracao _resultadoInteracaoAtual = ResultadoInteracao.InformationalNpc;
        private AutorizacaoMinijogo _autorizacaoMinijogoAtual;

        public bool JogadorPorPerto => jogadorPorPerto;

        /// <summary>Resultado da última avaliação central deste NPC (log/debug).</summary>
        public ResultadoInteracao UltimaAvaliacao => _resultadoInteracaoAtual;

        /// <summary>
        /// Autorização central do minijogo deste NPC (missão/etapa/objetivo
        /// autorizados). Os minijogos a leem ao reportar a vitória.
        /// </summary>
        public AutorizacaoMinijogo AutorizacaoMinijogoAtual => _autorizacaoMinijogoAtual;

        // IInteragivel: usado pelo PlayerInteraction para escolher o alvo mais próximo.
        public Transform Transform => transform;

        public bool TemDialogoValido =>
            DialogoParaConversa != null || !string.IsNullOrWhiteSpace(falaApresentacao);

        // =====================================================================
        // Catálogo central
        // =====================================================================

        /// <summary>Perfil do NPC no catálogo central (null para NPCs fora do catálogo).</summary>
        public PerfilNPC Perfil
        {
            get { return _perfilCache != null ? _perfilCache : ResolverPerfil(); }
        }

        private PerfilNPC ResolverPerfil()
        {
            var quest = QuestManager.Instancia;
            if (quest == null) return null;
            _perfilCache = !string.IsNullOrWhiteSpace(npcIdOverride)
                ? quest.ObterPerfil(npcIdOverride)
                : quest.ObterPerfilPorNome(nomeNPC);
            return _perfilCache;
        }

        /// <summary>Id do NPC no catálogo central (vazio quando fora do catálogo).</summary>
        public string NPCId
        {
            get { var perfil = Perfil; return perfil != null ? perfil.NPCId : ""; }
        }

        /// <summary>Este NPC pertence ao catálogo central (missões Q01–Q04)?</summary>
        public bool PertenceAoCatalogo
        {
            get { return Perfil != null; }
        }

        // =====================================================================
        // Seleção do diálogo
        // =====================================================================

        /// <summary>
        /// Conversa que o NPC deve mostrar AGORA:
        ///   1) Diálogo DINÂMICO gerado pela avaliação central (Situações A/B/C);
        ///   2) Primeira variante (de 'variantes') cujas condições estejam satisfeitas;
        ///   3) Retrocompatível: 'dialogoDataPosMissao' se a missão requisito estiver completa;
        ///   4) 'dialogoData' (fallback).
        /// </summary>
        public DialogoData DialogoParaConversa
        {
            get
            {
                if (_dialogoDinamicoAtual != null) return _dialogoDinamicoAtual;

                if (variantes != null)
                {
                    foreach (VarianteDialogo v in variantes)
                    {
                        if (v == null || v.dialogo == null) continue;
                        if (CondicoesSatisfeitas(v)) return v.dialogo;
                    }
                }

                if (dialogoDataPosMissao != null && MissaoRequisitoCompletaNoJogo)
                {
                    return dialogoDataPosMissao;
                }

                return dialogoData;
            }
        }

        /// <summary>Flag gravada quando o jogador termina uma conversa com este NPC.</summary>
        public string IdFlagConversa
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(idFlagConversa)) return idFlagConversa;
                string baseId = !string.IsNullOrWhiteSpace(idMissao) ? idMissao : nomeNPC;
                return "conversou_" + baseId;
            }
        }

        private bool CondicoesSatisfeitas(VarianteDialogo v)
        {
            if (!string.IsNullOrWhiteSpace(v.requisitoMissao))
            {
                // Com 'requisitoEtapaMinima' em conjunto, a condição vira "passo >= N"
                // da missão requisitada (compatibilidade legada). Sem etapa, exige a
                // missão COMPLETA (incluindo ReadyToTurnIn no modelo central).
                if (v.requisitoEtapaMinima >= 0)
                {
                    int passo = GameManager.Instance != null ? GameManager.Instance.ObterPassoMissao(v.requisitoMissao) : 0;
                    if (passo < v.requisitoEtapaMinima) return false;
                }
                else if (!MissaoCompletaJogo(v.requisitoMissao))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(v.requisitoFlag) && !FlagMarcadaNoJogo(v.requisitoFlag))
            {
                return false;
            }

            if (v.requisitoEtapaMinima >= 0)
            {
                string idEtapa = !string.IsNullOrWhiteSpace(v.requisitoMissao) ? v.requisitoMissao : idMissao;
                int passo = GameManager.Instance != null ? GameManager.Instance.ObterPassoMissao(idEtapa) : 0;
                if (passo < v.requisitoEtapaMinima) return false;
            }

            return true;
        }

        private static bool MissaoCompletaJogo(string id)
        {
            return GameManager.Instance != null && GameManager.Instance.MissaoCompleta(id);
        }

        private static bool FlagMarcadaNoJogo(string id)
        {
            return GameManager.Instance != null && GameManager.Instance.TemFlag(id);
        }

        /// <summary>Requisito para a conversa do "pós". Sem missaoRequisito, usa o próprio idMissao.</summary>
        private bool MissaoRequisitoCompletaNoJogo
        {
            get
            {
                string idVerificar = string.IsNullOrWhiteSpace(missaoRequisito) ? idMissao : missaoRequisito;
                return GameManager.Instance != null && GameManager.Instance.MissaoCompleta(idVerificar);
            }
        }

        private bool MissaoCompletaNoJogo =>
            GameManager.Instance != null && GameManager.Instance.MissaoCompleta(idMissao);

        // Evita eventos "fantasmas" quando o Enter Play Mode Options está com
        // Domain Reload desativado (os eventos estáticos sobreviveriam ao Play).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetarEventosEstaticos()
        {
            JogadorEntrouNaArea = null;
            JogadorSaiuDaArea = null;
            DialogoSolicitado = null;
            MinijogoSolicitado = null;
        }

        private void Awake()
        {
            bool temTrigger = false;
            foreach (Collider c in GetComponents<Collider>())
            {
                if (c.isTrigger) { temTrigger = true; break; }
            }

            if (!temTrigger)
            {
                Debug.LogWarning($"NPCBaseNovo '{name}': nenhum Collider com 'Is Trigger' neste objeto — " +
                                 "o jogador nunca será detectado como próximo.", this);
            }

            if (!TemDialogoValido)
            {
                Debug.LogWarning($"NPCBaseNovo '{name}': sem DialogoData e sem falaApresentacao — " +
                                 "a interação será recusada.", this);
            }
        }

        // ---------- Proximidade ----------

        private void OnTriggerEnter(Collider outro)
        {
            if (!outro.CompareTag(TAG_PLAYER) || jogadorPorPerto) return;

            jogadorPorPerto = true;
            minijogoTentado = false;
            JogadorEntrouNaArea?.Invoke(this);
            Interacao.NotificarEntrada(this);
        }

        private void OnTriggerExit(Collider outro)
        {
            if (!outro.CompareTag(TAG_PLAYER) || !jogadorPorPerto) return;

            jogadorPorPerto = false;
            JogadorSaiuDaArea?.Invoke(this);
            Interacao.NotificarSaida(this);
        }

        private void OnDisable()
        {
            // NPC desativado/destruído com o jogador dentro: avisa quem estava
            // acompanhando este NPC para não manter uma referência inválida.
            if (!jogadorPorPerto) return;

            jogadorPorPerto = false;
            JogadorSaiuDaArea?.Invoke(this);
            Interacao.NotificarSaida(this);
        }

        // ---------- Interação ----------

        /// <summary>
        /// Ação de "conversar com este NPC". Chamada pelo sistema de interação
        /// do jogador. Antes de solicitar a conversa, PERGUNTA ao QuestManager
        /// (autoridade central) o que pode acontecer e prepara o diálogo
        /// dinâmico quando o resultado pede orientação (Situações A/B/C).
        /// </summary>
        public void Interagir()
        {
            if (!isActiveAndEnabled)
            {
                Debug.LogWarning($"NPCBaseNovo '{name}': Interagir() chamado com o NPC desativado.", this);
                return;
            }

            if (!jogadorPorPerto)
            {
                Debug.LogWarning($"NPCBaseNovo '{name}': Interagir() chamado sem o jogador na área de interação.", this);
                return;
            }

            if (!TemDialogoValido)
            {
                Debug.LogError($"NPCBaseNovo '{name}': não há diálogo para exibir (DialogoData e falaApresentacao vazios).", this);
                return;
            }

            if (DialogoSolicitado == null)
            {
                Debug.LogError($"NPCBaseNovo '{name}': nenhum sistema de diálogo está inscrito em NPCBaseNovo.DialogoSolicitado.", this);
                return;
            }

            AvaliarEPrepararDialogo();
            DialogoSolicitado.Invoke(this);
        }

        /// <summary>
        /// Avaliação central pré-diálogo: consulta o QuestManager e, quando o
        /// resultado pede orientação, gera o diálogo dinâmico deste NPC.
        /// </summary>
        private void AvaliarEPrepararDialogo()
        {
            DescartarDialogoDinamico();
            _resultadoInteracaoAtual = ResultadoInteracao.InformationalNpc;

            var quest = QuestManager.Instancia;
            if (quest == null || string.IsNullOrEmpty(NPCId))
            {
                // Sem QuestManager ou NPC fora do catálogo: variante legada.
                return;
            }

            _resultadoInteracaoAtual = quest.AvaliarInteracao(NPCId);
            Debug.Log("[MISSION] " + nomeNPC + " (" + NPCId + ") avaliado: " + _resultadoInteracaoAtual);

            if (QuestManager.PrecisaDialogoDinamico(_resultadoInteracaoAtual))
            {
                List<FalaDinamica> falas = quest.GerarFalasDinamicas(NPCId, _resultadoInteracaoAtual);
                _dialogoDinamicoAtual = CriarDialogoDinamico(falas);
            }
        }

        /// <summary>Converte as falas dinâmicas do motor em um DialogoData de runtime.</summary>
        private DialogoData CriarDialogoDinamico(List<FalaDinamica> falas)
        {
            if (falas == null || falas.Count == 0) return null;

            var dados = ScriptableObject.CreateInstance<DialogoData>();
            var lista = new DialogoData.FalaDialogo[falas.Count];
            for (int i = 0; i < falas.Count; i++)
            {
                lista[i] = new DialogoData.FalaDialogo
                {
                    nomeFalante = falas[i].NomeFalante,
                    textoFala = falas[i].Texto
                };
            }
            dados.falas = lista;
            dados.velDigitacao = 0.03f;
            return dados;
        }

        private void DescartarDialogoDinamico()
        {
            if (_dialogoDinamicoAtual != null)
            {
                Destroy(_dialogoDinamicoAtual);
                _dialogoDinamicoAtual = null;
            }
        }

        // ---------- Após o diálogo ----------

        /// <summary>
        /// Chamado pelo sistema de diálogo quando a conversa com este NPC
        /// termina normalmente.
        /// </summary>
        public void NotificarDialogoEncerrado()
        {
            AoEncerrarDialogo();
            DescartarDialogoDinamico();

            // Registra o "já conversei com este NPC" como flag do mundo, para que
            // variantes (deste ou de outros NPCs) possam exigir isso como condição.
            if (registrarConversa)
            {
                GameManager.Instance?.MarcarFlag(IdFlagConversa);
            }
        }

        /// <summary>
        /// Reação ao fim da conversa. NPCs do catálogo REPASSAM o encerramento ao
        /// QuestManager (única via de progressão); NPCs fora do catálogo mantêm a
        /// conclusão flat legada. Minijogos só abrem com autorização central.
        /// </summary>
        protected virtual void AoEncerrarDialogo()
        {
            var quest = QuestManager.Instancia;

            // 1) Progressão central (NPCs do catálogo — Q01 a Q04).
            if (quest != null && quest.ConheceNPC(NPCId))
            {
                GameManager.Instance?.RegistrarDialogoEncerrado(NPCId);
            }
            else if (completarMissaoAoTerminar && !MissaoCompletaNoJogo && EmEntregaLegada())
            {
                // 2) NPC fora do catálogo (ex.: Diretor): conclusão flat legada.
                if (usarModeloEtapas)
                {
                    GameManager.Instance?.ConcluirEtapa(idMissao);
                }
                else
                {
                    GameManager.Instance?.CompletarMissao(idMissao);
                }
            }

            // 3) Minijogo: pedido sempre passa pela AUTORIZAÇÃO central.
            if ((chamarMinijogo || minijogoComoParteDaMissao) && !minijogoTentado)
            {
                minijogoTentado = true;
                PedirMinijogoAutorizado();
            }
        }

        /// <summary>Condições legadas de "entrega" (apenas NPCs fora do catálogo).</summary>
        private bool EmEntregaLegada()
        {
            bool emEntrega = string.IsNullOrWhiteSpace(missaoRequisito) || MissaoRequisitoCompletaNoJogo;

            if (emEntrega && concluirApenasNaPrimeiraConversa && FlagMarcadaNoJogo(IdFlagConversa))
            {
                emEntrega = false;
            }

            if (emEntrega && requisitoEtapaMinimaParaConcluir >= 0)
            {
                int passoAtual = GameManager.Instance != null ? GameManager.Instance.ObterPassoMissao(idMissao) : 0;
                if (passoAtual < requisitoEtapaMinimaParaConcluir) emEntrega = false;
            }

            return emEntrega;
        }

        /// <summary>Pede autorização central para abrir o minijogo deste NPC.</summary>
        private void PedirMinijogoAutorizado()
        {
            var quest = QuestManager.Instancia;
            AutorizacaoMinijogo autorizacao = quest != null
                ? quest.AutorizarMinijogo(NPCId, idMinijogo)
                : null;

            if (autorizacao != null && autorizacao.Autorizado)
            {
                // Guarda o contexto autorizado: os minijogos usam missão/objetivo
                // ao reportar a vitória (única via de progressão por minijogo).
                _autorizacaoMinijogoAtual = autorizacao;
                IniciarMinijogo();
                return;
            }

            Debug.Log("[MISSION] Minijogo '" + idMinijogo + "' NÃO autorizado para " + nomeNPC + ": " +
                      (autorizacao != null ? autorizacao.Motivo : "QuestManager ausente."));
        }

        protected virtual void IniciarMinijogo()
        {
            if (MinijogoSolicitado == null)
            {
                Debug.LogWarning($"NPCBaseNovo '{name}': nenhum sistema de minijogo inscrito em NPCBaseNovo.MinijogoSolicitado.", this);
                return;
            }

            MinijogoSolicitado.Invoke(this);
        }
    }
}

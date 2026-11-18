namespace IF360
{
    using TMPro;
    using UnityEngine;
    using IF360.Missoes;

    /// <summary>
    /// Sincronizador definitivo do HUD de missões.
    ///
    /// Lê EXCLUSIVAMENTE dados vivos da autoridade central (QuestManager ->
    /// MotorRegrasMissao -> ResumoProgresso / CatalogoIf360). Nenhum texto de
    /// missão é mantido aqui: título, etapas, descrição e contador vêm do
    /// catálogo e do progresso atual. Assina os três eventos do QuestManager
    /// para atualizar no mesmo frame em que o progresso muda.
    /// </summary>
    public class DynamicMissionHUDBinder : MonoBehaviour
    {
        [Header("Textos do HUD")]
        [SerializeField] private TMP_Text textoTitulo;
        [SerializeField] private TMP_Text textoEtapas;
        [SerializeField] private TMP_Text textoDescricao;
        [SerializeField] private TMP_Text textoProgresso;

        [Header("Painel")]
        [Tooltip("Raiz do card. Se vazio, usa o próprio GameObject.")]
        [SerializeField] private GameObject painel;

        private void OnEnable()
        {
            QuestManager.MissaoIniciada += AoMissaoIniciada;
            QuestManager.EtapaConcluida += AoProgressoAlterado;
            QuestManager.MissaoCompletada += AoProgressoAlterado;

            Atualizar();
        }

        private void Start()
        {
            // Segunda chamada cobre o caso de o QuestManager ter sido criado
            // depois deste componente (ordem de Awake entre objetos da cena).
            Atualizar();
        }

        private void OnDisable()
        {
            QuestManager.MissaoIniciada -= AoMissaoIniciada;
            QuestManager.EtapaConcluida -= AoProgressoAlterado;
            QuestManager.MissaoCompletada -= AoProgressoAlterado;
        }

        private void AoMissaoIniciada(string idMissao)
        {
            MostrarPainel();
            Atualizar();
        }

        private void AoProgressoAlterado(string idMissao, int ordemEtapa)
        {
            Atualizar();
        }

        private void AoProgressoAlterado(string idMissao)
        {
            Atualizar();
        }

        private void MostrarPainel()
        {
            GameObject alvo = painel != null ? painel : gameObject;
            if (!alvo.activeSelf) alvo.SetActive(true);

            CanvasGroup group = alvo.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// Reescreve os quatro campos a partir do estado vivo. Limpa primeiro
        /// para que nenhum texto de edição/placeholder sobreviva em runtime.
        /// </summary>
        public void Atualizar()
        {
            Limpar();

            QuestManager quest = QuestManager.Instancia;
            if (quest == null) return;

            ResumoProgresso resumo = quest.ObterResumoProgresso();
            string concluidas = quest.QuantidadeMissoesCompletas().ToString();
            string total = quest.TotalMissoesCatalogo.ToString();

            if (resumo.TemMissaoAtiva)
            {
                Definir(textoTitulo, resumo.MissionName);
                Definir(textoEtapas, MontarEtapas(resumo));
                Definir(textoDescricao, resumo.ProximaAcao);
            }
            else
            {
                DefinirProximaMissao(quest);
            }

            Definir(textoProgresso, concluidas + " de " + total + " missões concluídas");
        }

        private string MontarEtapas(ResumoProgresso resumo)
        {
            string etapa = "";
            if (resumo.TotalEtapas > 0)
                etapa = "Etapa " + resumo.StepOrder + " de " + resumo.TotalEtapas + ": " + resumo.StepDescricao;

            if (string.IsNullOrEmpty(resumo.DescricaoPendentes)) return etapa;
            return string.IsNullOrEmpty(etapa)
                ? resumo.DescricaoPendentes
                : etapa + "\n" + resumo.DescricaoPendentes;
        }

        /// <summary>
        /// Sem missão ativa: o estado vem do próprio catálogo (próxima missão
        /// não concluída), sem qualquer texto inventado pelo HUD.
        /// </summary>
        private void DefinirProximaMissao(QuestManager quest)
        {
            MotorRegrasMissao motor = quest.Motor;
            if (motor == null || motor.Catalogo == null) return;

            var missoes = motor.Catalogo.Missoes;
            if (missoes == null) return;

            for (int i = 0; i < missoes.Count; i++)
            {
                DefinicaoMissao def = missoes[i];
                if (def == null) continue;
                if (quest.ObterEstadoMissao(def.MissionId) == EstadoMissao.Completed) continue;

                Definir(textoTitulo, def.MissionName);
                Definir(textoDescricao, def.Descricao);

                PerfilNPC iniciador = quest.ObterPerfil(def.StartingNPCId);
                if (iniciador != null)
                    Definir(textoEtapas, iniciador.DisplayName + " — " + iniciador.LocationName);
                return;
            }
        }

        private static void Definir(TMP_Text alvo, string valor)
        {
            if (alvo != null) alvo.text = valor ?? "";
        }

        private void Limpar()
        {
            Definir(textoTitulo, "");
            Definir(textoEtapas, "");
            Definir(textoDescricao, "");
            Definir(textoProgresso, "");
        }
    }
}

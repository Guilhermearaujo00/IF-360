using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// HUD/jornada de missão: mostra o progresso (quantas missões já foram
/// concluídas) e, a cada conclusão, o nome da missão concluída.
///
/// Camada 100% visual: não lê input, não altera estado. Reage ao evento
/// estático QuestManager.MissaoCompletada e consulta QuestManager.Instancia.
/// </summary>
public class HUDMissao : MonoBehaviour
{
    [Header("Textos (TextMeshPro)")]
    [Tooltip("Título fixo do painel (ex.: 'Jornada').")]
    [SerializeField] private TMP_Text textoTitulo;

    [Tooltip("Objetivo atual / último evento de missão.")]
    [SerializeField] private TMP_Text textoObjetivo;

    [Tooltip("Contador de progresso (ex.: '2 missões concluídas').")]
    [SerializeField] private TMP_Text textoProgresso;

    [Header("Nomes amigáveis por id de missão")]
    [Tooltip("Traduz o idMissao (ex.: missao_diretor) para um nome exibido (ex.: 'Falar com o Diretor').")]
    [SerializeField] private List<EntradaMissao> nomesMissoes = new List<EntradaMissao>();

    private void OnEnable()
    {
        QuestManager.MissaoCompletada += AoCompletarMissao;
        AtualizarProgresso();
    }

    private void OnDisable()
    {
        QuestManager.MissaoCompletada -= AoCompletarMissao;
    }

    private void AoCompletarMissao(string idMissao)
    {
        if (textoObjetivo != null)
        {
            textoObjetivo.text = "Concluída: " + NomeDe(idMissao);
        }

        AtualizarProgresso();
    }

    private void AtualizarProgresso()
    {
        if (textoProgresso == null) return;

        int total = QuestManager.Instancia != null
            ? QuestManager.Instancia.QuantidadeMissoesCompletas()
            : 0;

        textoProgresso.text = total + (total == 1 ? " missão concluída" : " missões concluídas");
    }

    private string NomeDe(string idMissao)
    {
        foreach (EntradaMissao entrada in nomesMissoes)
        {
            if (entrada != null && entrada.idMissao == idMissao)
            {
                return string.IsNullOrWhiteSpace(entrada.nome) ? idMissao : entrada.nome;
            }
        }

        return idMissao;
    }

    [System.Serializable]
    public class EntradaMissao
    {
        public string idMissao;
        public string nome;
    }
}

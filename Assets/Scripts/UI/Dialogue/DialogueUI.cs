namespace IF360
{
using TMPro;
using UnityEngine;

/// <summary>
/// Camada 100% visual do diálogo. NÃO tem lógica de estados, NÃO lê input
/// e NÃO abre conversas: apenas exibe/oculta a caixa e os textos que o
/// DialogueManager mandar.
///
/// Tudo é referenciado no Inspector (padrão RequireComponent não usado de
/// propósito): a cena recebe estes objetos prontos pela ferramenta
/// Tools > IFNMG > Configurar Sistema de Diálogo.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Caixa")]
    [Tooltip("Raiz da caixa de diálogo (ativa/desativa com Mostrar/Esconder).")]
    [SerializeField] private GameObject caixa;

    [Header("Textos (TextMeshPro)")]
    [Tooltip("Nome do NPC exibido no topo da caixa.")]
    [SerializeField] private TMP_Text nome;

    [Tooltip("Fala em andamento.")]
    [SerializeField] private TMP_Text texto;

    [Tooltip("Indicador de continuar (ex.: 'Pressione E').")]
    [SerializeField] private GameObject dica;

    // ---------- Caixa ----------

    public void Mostrar()
    {
        if (caixa != null) caixa.SetActive(true);
    }

    public void Esconder()
    {
        if (caixa != null) caixa.SetActive(false);
    }

    // ---------- Textos ----------

    public void DefinirNome(string valor)
    {
        if (nome != null) nome.text = valor;
    }

    public void DefinirTexto(string valor)
    {
        if (texto != null) texto.text = valor;
    }

    public void MostrarIndicador(bool visivel)
    {
        if (dica != null) dica.SetActive(visivel);
    }
}
}

namespace IF360
{
using UnityEngine;

/// <summary>
/// Placa informativa de um prédio/setor (casos em que não há NPC). Quando o
/// jogador está perto e aperta E, o texto é exibido na MESMA caixa de diálogo
/// do jogo: o DialogueManager entra em modo conversa e trava movimento/câmera.
///
/// Reutiliza o canal Interacao, então PlayerInteraction continua sendo o único
/// ouvinte do E — a placa não lê input.
/// </summary>
public class PlacaInformativa : MonoBehaviour, IInteragivel
{
    private const string TAG_PLAYER = "Player";

    [Header("Conteúdo")]
    [Tooltip("Nome exibido no topo da caixa de diálogo.")]
    [SerializeField] private string titulo = "Placa";
    [Tooltip("Texto informativo sobre o prédio/setor.")]
    [TextArea(2, 8)]
    [SerializeField] private string texto = "Informações do prédio/setor.";

    private bool jogadorPorPerto;

    public bool JogadorPorPerto => jogadorPorPerto;
    public Transform Transform => transform;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
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
            Debug.LogWarning($"PlacaInformativa '{name}': nenhum Collider com 'Is Trigger' neste objeto — " +
                             "o jogador nunca será detectado como próximo.", this);
        }
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (!outro.CompareTag(TAG_PLAYER) || jogadorPorPerto) return;

        jogadorPorPerto = true;
        Interacao.NotificarEntrada(this);
    }

    private void OnTriggerExit(Collider outro)
    {
        if (!outro.CompareTag(TAG_PLAYER) || !jogadorPorPerto) return;

        jogadorPorPerto = false;
        Interacao.NotificarSaida(this);
    }

    private void OnDisable()
    {
        if (!jogadorPorPerto) return;

        jogadorPorPerto = false;
        Interacao.NotificarSaida(this);
    }

    public void Interagir()
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            Debug.LogWarning($"PlacaInformativa '{name}': texto vazio — nada a exibir.", this);
            return;
        }

        DialogueManager dialogo = DialogueManager.Instancia;
        if (dialogo == null)
        {
            Debug.LogError($"PlacaInformativa '{name}': DialogueManager não encontrado na cena.", this);
            return;
        }

        dialogo.IniciarDialogoInformacao(titulo, texto);
    }
}
}

namespace IF360
{
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instancia { get; private set; }

    private const string CHAVE_PONTOS = "pontos";
    private const string CHAVE_COLECIONAVEIS = "colecionaveis";
    private const string CHAVE_MISSOES = "missoes";

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    /// <summary>
    /// AUTO-RESTORE: o save volta ao entrar na cena sem depender de alguém
    /// chamar CarregarJogo manualmente (correção do bug "Continuar não carregava").
    /// Usa RestaurarSeNecessario: restaura apenas uma vez por sessão (o
    /// QuestManager é DontDestroyOnLoad e mantém o progresso vivo em memória).
    /// </summary>
    private void Start()
    {
        if (TemSave())
        {
            CarregarJogo();
        }
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    public bool ExisteJogo()
    {
        return PlayerPrefs.HasKey(CHAVE_PONTOS);
    }

    public bool TemSave()
    {
        return PlayerPrefs.HasKey(CHAVE_PONTOS) || PlayerPrefs.HasKey(CHAVE_MISSOES);
    }

    public void SalvarJogo()
    {
        if (GameManager.Instance != null)
        {
            PlayerPrefs.SetInt(CHAVE_PONTOS, GameManager.Instance.pontos);
            PlayerPrefs.SetInt(CHAVE_COLECIONAVEIS, GameManager.Instance.colecionaveisColetados);
        }

        if (QuestManager.Instancia != null)
        {
            PlayerPrefs.SetString(CHAVE_MISSOES, QuestManager.Instancia.SerializarMissoes());
        }

        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Jogo salvo.");
    }

    public void CarregarJogo()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[SaveManager] GameManager ausente para carregar.");
            return;
        }

        GameManager.Instance.pontos = PlayerPrefs.GetInt(CHAVE_PONTOS, 0);
        GameManager.Instance.colecionaveisColetados = PlayerPrefs.GetInt(CHAVE_COLECIONAVEIS, 0);

        if (QuestManager.Instancia != null)
        {
            // Guarda de sessão: não sobrescreve o progresso vivo em memória ao
            // recarregar a cena (voltar ao menu e continuar).
            QuestManager.Instancia.RestaurarSeNecessario(PlayerPrefs.GetString(CHAVE_MISSOES, string.Empty));
        }

        Debug.Log("[SaveManager] Jogo carregado.");
    }

    /// <summary>Existe save? (estático — utilizável no menu, sem SaveManager na cena).</summary>
    public static bool TemSaveEstatico()
    {
        return PlayerPrefs.HasKey(CHAVE_PONTOS) || PlayerPrefs.HasKey(CHAVE_MISSOES);
    }

    /// <summary>
    /// Apaga o save SEM depender de um SaveManager vivo na cena (o menu
    /// principal precisa disto para "Iniciar Jogo" sempre começar do zero).
    /// </summary>
    public static void ApagarSaveEstatico()
    {
        PlayerPrefs.DeleteKey(CHAVE_PONTOS);
        PlayerPrefs.DeleteKey(CHAVE_COLECIONAVEIS);
        PlayerPrefs.DeleteKey(CHAVE_MISSOES);

        if (QuestManager.Instancia != null)
        {
            QuestManager.Instancia.LimparMissoes();
        }

        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Save apagado (estático).");
    }

    public void ApagarJogo()
    {
        PlayerPrefs.DeleteKey(CHAVE_PONTOS);
        PlayerPrefs.DeleteKey(CHAVE_COLECIONAVEIS);
        PlayerPrefs.DeleteKey(CHAVE_MISSOES);

        if (QuestManager.Instancia != null)
        {
            QuestManager.Instancia.LimparMissoes();
        }

        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Jogo apagado.");
    }

    public void LimparSave()
    {
        ApagarJogo();
    }
}
}

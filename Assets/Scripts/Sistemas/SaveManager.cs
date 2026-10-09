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

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    public bool ExisteJogo()
    {
        return PlayerPrefs.HasKey(CHAVE_PONTOS);
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
            QuestManager.Instancia.RestaurarMissoes(PlayerPrefs.GetString(CHAVE_MISSOES, string.Empty));
        }

        Debug.Log("[SaveManager] Jogo carregado.");
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
}
}

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Estado do jogo")]
    public int pontos = 0;
    public int colecionaveisTotal = 0;
    public int colecionaveisColetados = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AdicionarPontos(int valor)
    {
        pontos += valor;
        Debug.Log("Pontos: " + pontos);
    }

    public void RegistrarColecionavel()
    {
        colecionaveisColetados++;
        Debug.Log("Colecionável: " + colecionaveisColetados + "/" + colecionaveisTotal);
    }

    public void CompletarMissao(string idMissao)
    {
        if (QuestManager.Instancia == null) return;

        QuestManager.Instancia.CompletarMissao(idMissao);

        // Auto-save: o progresso de missões não pode se perder ao sair do jogo.
        if (SaveManager.Instancia != null) SaveManager.Instancia.SalvarJogo();
    }

    public bool MissaoCompleta(string idMissao)
    {
        return QuestManager.Instancia != null && QuestManager.Instancia.MissaoCompleta(idMissao);
    }

    public int QuantidadeMissoesCompletas()
    {
        return QuestManager.Instancia != null ? QuestManager.Instancia.QuantidadeMissoesCompletas() : 0;
    }
}

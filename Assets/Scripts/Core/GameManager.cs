namespace IF360
{
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

    /// <summary>Completa uma missão inteira (modelo flat — retrocompatível).</summary>
    public void CompletarMissao(string idMissao)
    {
        if (QuestManager.Instancia == null) return;

        QuestManager.Instancia.CompletarMissao(idMissao);

        // Auto-save: o progresso de missões não pode se perder ao sair do jogo.
        if (SaveManager.Instancia != null) SaveManager.Instancia.SalvarJogo();
    }

    /// <summary>Conclui uma etapa de uma missão (modelo por etapas).</summary>
    public void ConcluirEtapa(string idMissao)
    {
        if (QuestManager.Instancia == null) return;

        QuestManager.Instancia.ConcluirEtapa(idMissao);

        if (SaveManager.Instancia != null) SaveManager.Instancia.SalvarJogo();
    }

    /// <summary>Define explicitamente o passo de uma missão (útil para save/restore).</summary>
    public void DefinirPassoMissao(string idMissao, int passo)
    {
        if (QuestManager.Instancia == null) return;
        QuestManager.Instancia.DefinirPasso(idMissao, passo);
    }

    public bool MissaoCompleta(string idMissao)
    {
        return QuestManager.Instancia != null && QuestManager.Instancia.MissaoCompleta(idMissao);
    }

    public int QuantidadeMissoesCompletas()
    {
        return QuestManager.Instancia != null ? QuestManager.Instancia.QuantidadeMissoesCompletas() : 0;
    }

    /// <summary>Quantas etapas já foram concluídas dessa missão (modelo por etapas).</summary>
    public int ObterPassoMissao(string idMissao)
    {
        return QuestManager.Instancia != null ? QuestManager.Instancia.ObterPassoAtual(idMissao) : 0;
    }

    /// <summary>Marca uma flag do mundo (ex.: conversou_com_ananda). Auto-save se mudou.</summary>
    public void MarcarFlag(string idFlag)
    {
        if (QuestManager.Instancia == null) return;

        if (QuestManager.Instancia.MarcarFlag(idFlag))
        {
            if (SaveManager.Instancia != null) SaveManager.Instancia.SalvarJogo();
        }
    }

    public bool TemFlag(string idFlag)
    {
        return QuestManager.Instancia != null && QuestManager.Instancia.TemFlag(idFlag);
    }
}
}

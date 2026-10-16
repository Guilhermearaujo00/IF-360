namespace IF360
{
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instancia { get; private set; }

    /// <summary>Disparado quando uma etapa de missão é concluída (idMissao, indiceEtapa).</summary>
    public static event Action<string, int> EtapaConcluida;

    /// <summary>Disparado quando uma missão inteira é concluída (idMissao).</summary>
    public static event Action<string> MissaoCompletada;

    [Header("Estado das missões (modelo flat — compatibilidade)")]
    [SerializeField] private List<string> missoesCompletas = new List<string>();

    [Header("Progresso por etapas (novo modelo)")]
    [SerializeField] private List<string> missaoIds = new List<string>();
    [SerializeField] private List<int> passoAtual = new List<int>();

    [Header("Flags do mundo (ex.: conversou_com_ananda)")]
    [SerializeField] private List<string> flags = new List<string>();

    // Dicionários de runtime para acesso rápido
    private Dictionary<string, int> _passoPorMissao = new Dictionary<string, int>();
    private HashSet<string> _flags = new HashSet<string>();

    public IReadOnlyList<string> MissoesCompletas => missoesCompletas;
    public IReadOnlyCollection<string> Flags => _flags;

    // Reset defensivo do singleton/evento para "Enter Play Mode" sem Domain Reload.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarInstanciaEstatica()
    {
        Instancia = null;
        EtapaConcluida = null;
        MissaoCompletada = null;
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;

        // Consistência de ciclo de vida: o estado das missões precisa sobreviver
        // à troca de cena (a fachada GameManager já é DontDestroyOnLoad).
        DontDestroyOnLoad(gameObject);

        // Reconstrói dicionário a partir das listas serializadas
        ReconstruirDicionario();
        ReconstruirFlags();
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    // ---------- API pública (modelo flat — compatibilidade) ----------

    public void CompletarMissao(string idMissao)
    {
        if (string.IsNullOrEmpty(idMissao)) return;
        if (missoesCompletas.Contains(idMissao)) return;

        missoesCompletas.Add(idMissao);
        Debug.Log("[QuestManager] Missão completada (flat): " + idMissao);
        MissaoCompletada?.Invoke(idMissao);
    }

    public bool MissaoCompleta(string idMissao)
    {
        return !string.IsNullOrEmpty(idMissao) && missoesCompletas.Contains(idMissao);
    }

    public int QuantidadeMissoesCompletas()
    {
        return missoesCompletas.Count;
    }

    // ---------- API pública (modelo por etapas) ----------

    /// <summary>
    /// Avança uma etapa da missão. Se a missão não existia no dicionário, começa no passo 0.
    /// Retorna true se a etapa foi avançada, false se a missão já estava completa.
    /// </summary>
    public bool ConcluirEtapa(string idMissao)
    {
        if (string.IsNullOrEmpty(idMissao)) return false;

        // Garante entrada no dicionário
        if (!_passoPorMissao.TryGetValue(idMissao, out int passo))
        {
            passo = 0;
        }

        // Verifica se já completou todas as etapas (precisa do MissaoData para saber total)
        // Se não temos MissaoData, permite avançar indefinidamente — o HUD limita pela contagem.
        int novoPasso = passo + 1;
        _passoPorMissao[idMissao] = novoPasso;
        SincronizarListasSerializadas();

        Debug.Log($"[QuestManager] Etapa concluída: {idMissao} -> passo {novoPasso}");
        EtapaConcluida?.Invoke(idMissao, novoPasso);

        // Verifica se a missão como um todo foi completada (heurística: se o novo passo >= 3 e não está na lista flat)
        // O controle exato de "missão completa" deve vir do MissaoData via HUD/GameManager.
        return true;
    }

    /// <summary>
    /// Define explicitamente o passo atual de uma missão (útil para restaurar save ou pular etapas).
    /// </summary>
    public void DefinirPasso(string idMissao, int passo)
    {
        if (string.IsNullOrEmpty(idMissao)) return;
        _passoPorMissao[idMissao] = Mathf.Max(0, passo);
        SincronizarListasSerializadas();
    }

    public int ObterPassoAtual(string idMissao)
    {
        if (string.IsNullOrEmpty(idMissao)) return 0;
        return _passoPorMissao.TryGetValue(idMissao, out int passo) ? passo : 0;
    }

    public int QuantidadeEtapasConcluidas(string idMissao)
    {
        return ObterPassoAtual(idMissao);
    }

    // ---------- API pública (flags do mundo) ----------

    /// <summary>Marca uma flag. Retorna true se foi marcada agora (false se já existia ou id vazio).</summary>
    public bool MarcarFlag(string idFlag)
    {
        if (string.IsNullOrWhiteSpace(idFlag)) return false;
        if (!_flags.Add(idFlag)) return false;

        SincronizarFlagsSerializadas();
        Debug.Log("[QuestManager] Flag marcada: " + idFlag);
        return true;
    }

    public bool TemFlag(string idFlag)
    {
        return !string.IsNullOrWhiteSpace(idFlag) && _flags.Contains(idFlag);
    }

    public void LimparFlag(string idFlag)
    {
        if (string.IsNullOrWhiteSpace(idFlag)) return;
        if (_flags.Remove(idFlag)) SincronizarFlagsSerializadas();
    }

    // ---------- Persistência ----------

    [Serializable]
    private class EstadoMissoes
    {
        public List<string> idsCompletas = new List<string>();
        public List<string> missaoIds = new List<string>();
        public List<int> passoAtual = new List<int>();
        public List<string> idsFlags = new List<string>();
    }

    public string SerializarMissoes()
    {
        var estado = new EstadoMissoes
        {
            idsCompletas = missoesCompletas,
            missaoIds = missaoIds,
            passoAtual = passoAtual,
            idsFlags = flags
        };
        return JsonUtility.ToJson(estado);
    }

    public void RestaurarMissoes(string json)
    {
        missoesCompletas.Clear();
        missaoIds.Clear();
        passoAtual.Clear();
        flags.Clear();
        _passoPorMissao.Clear();
        _flags.Clear();

        if (string.IsNullOrEmpty(json)) return;

        EstadoMissoes estado = JsonUtility.FromJson<EstadoMissoes>(json);
        if (estado == null) return;

        if (estado.idsCompletas != null)
            missoesCompletas.AddRange(estado.idsCompletas);

        if (estado.missaoIds != null && estado.passoAtual != null && estado.missaoIds.Count == estado.passoAtual.Count)
        {
            missaoIds.AddRange(estado.missaoIds);
            passoAtual.AddRange(estado.passoAtual);
        }

        if (estado.idsFlags != null)
            flags.AddRange(estado.idsFlags);

        ReconstruirDicionario();
        ReconstruirFlags();
    }

    public void LimparMissoes()
    {
        missoesCompletas.Clear();
        missaoIds.Clear();
        passoAtual.Clear();
        flags.Clear();
        _passoPorMissao.Clear();
        _flags.Clear();
    }

    // ---------- Helpers internos ----------

    private void ReconstruirDicionario()
    {
        _passoPorMissao.Clear();
        for (int i = 0; i < missaoIds.Count && i < passoAtual.Count; i++)
        {
            _passoPorMissao[missaoIds[i]] = passoAtual[i];
        }
    }

    private void SincronizarListasSerializadas()
    {
        missaoIds.Clear();
        passoAtual.Clear();
        foreach (var kvp in _passoPorMissao)
        {
            missaoIds.Add(kvp.Key);
            passoAtual.Add(kvp.Value);
        }
    }

    private void ReconstruirFlags()
    {
        _flags.Clear();
        foreach (string f in flags)
        {
            if (!string.IsNullOrWhiteSpace(f)) _flags.Add(f);
        }
    }

    private void SincronizarFlagsSerializadas()
    {
        flags.Clear();
        flags.AddRange(_flags);
    }
}
}

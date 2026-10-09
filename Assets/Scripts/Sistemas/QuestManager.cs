namespace IF360
{
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instancia { get; private set; }

    /// <summary>Disparado quando uma missão nova é registrada como concluída (idMissao).</summary>
    public static event Action<string> MissaoCompletada;

    [Header("Estado das missões")]
    [SerializeField] private List<string> missoesCompletas = new List<string>();

    public IReadOnlyList<string> MissoesCompletas => missoesCompletas;

    // Reset defensivo do singleton/evento para "Enter Play Mode" sem Domain Reload.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarInstanciaEstatica()
    {
        Instancia = null;
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
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    public void CompletarMissao(string idMissao)
    {
        if (string.IsNullOrEmpty(idMissao)) return;
        if (missoesCompletas.Contains(idMissao)) return;

        missoesCompletas.Add(idMissao);
        Debug.Log("[QuestManager] Missão completada: " + idMissao);
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

    // ---------- Persistência (usada pelo SaveManager) ----------

    public string SerializarMissoes()
    {
        return JsonUtility.ToJson(new ListaDeMissoes { ids = missoesCompletas });
    }

    public void RestaurarMissoes(string json)
    {
        missoesCompletas.Clear();

        if (string.IsNullOrEmpty(json)) return;

        ListaDeMissoes lista = JsonUtility.FromJson<ListaDeMissoes>(json);
        if (lista != null && lista.ids != null)
        {
            missoesCompletas.AddRange(lista.ids);
        }
    }

    public void LimparMissoes()
    {
        missoesCompletas.Clear();
    }

    // JsonUtility não serializa coleções na raiz — wrapper obrigatório.
    [System.Serializable]
    private class ListaDeMissoes
    {
        public List<string> ids = new List<string>();
    }
}
}

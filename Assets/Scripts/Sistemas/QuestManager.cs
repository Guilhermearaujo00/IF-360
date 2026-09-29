using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instancia { get; private set; }

    [Header("Estado das missões")]
    [SerializeField] private List<string> missoesCompletas = new List<string>();

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

    public void CompletarMissao(string idMissao)
    {
        if (string.IsNullOrEmpty(idMissao)) return;

        if (!missoesCompletas.Contains(idMissao))
        {
            missoesCompletas.Add(idMissao);
            Debug.Log("[QuestManager] Missão completada: " + idMissao);
        }
    }

    public bool MissaoCompleta(string idMissao)
    {
        return !string.IsNullOrEmpty(idMissao) && missoesCompletas.Contains(idMissao);
    }

    public int QuantidadeMissoesCompletas()
    {
        return missoesCompletas.Count;
    }
}
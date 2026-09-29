using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instancia { get; private set; }

    [Header("Inventário")]
    [SerializeField] private List<string> itens = new List<string>();

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

    public int QuantidadeItens => itens.Count;

    public void AdicionarItem(string idItem)
    {
        if (string.IsNullOrEmpty(idItem) || itens.Contains(idItem)) return;

        itens.Add(idItem);
        Debug.Log("[InventoryManager] Item adicionado: " + idItem);
    }

    public bool TemItem(string idItem)
    {
        return !string.IsNullOrEmpty(idItem) && itens.Contains(idItem);
    }

    public bool RemoverItem(string idItem)
    {
        return itens.Remove(idItem);
    }
}
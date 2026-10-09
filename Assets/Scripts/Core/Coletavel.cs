using UnityEngine;

public class Coletavel : MonoBehaviour
{
    [Header("Identificação")]
    public string nomeColecionavel = "História do IFNMG";

    private void OnTriggerEnter(Collider outro)
    {
        if (!outro.CompareTag("Player")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.RegistrarColecionavel();

        if (InventoryManager.Instancia != null)
            InventoryManager.Instancia.AdicionarItem(nomeColecionavel);

        if (AudioManager.Instancia != null)
            AudioManager.Instancia.TocarConfirmar();

        Debug.Log("Colecionável encontrado: " + nomeColecionavel);
        Destroy(gameObject);
    }
}

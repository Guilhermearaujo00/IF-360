using UnityEngine;

public class Coletavel : MonoBehaviour
{
    [Header("Identificação")]
    public string nomeColecionavel = "História do IFNMG";

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RegistrarColecionavel();

            Debug.Log("Colecionável encontrado: " + nomeColecionavel);
            Destroy(gameObject);
        }
    }
}

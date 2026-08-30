using UnityEngine;

public class NPCBase : MonoBehaviour
{
    [Header("Identificação")]
    public string idMissao = "npc_secretaria";
    public string nomeNPC = "Funcionário da Secretaria";
    [TextArea] public string falaApresentacao = "Sou o funcionário da secretaria...";

    [Header("Interação")]
    public float raioInteracao = 3f;
    public KeyCode teclaInteracao = KeyCode.E;

    private bool jogadorPorPerto = false;

    private void Update()
    {
        if (jogadorPorPerto && Input.GetKeyDown(teclaInteracao))
        {
            Interagir();
        }
    }

    private void Interagir()
    {
        if (GameManager.Instance.MissaoCompleta(idMissao))
        {
            Debug.Log(nomeNPC + ": Você já ajudou! Obrigado!");
        }
        else
        {
            Debug.Log(nomeNPC + ": " + falaApresentacao);
            IniciarMinijogo();
        }
    }

    protected virtual void IniciarMinijogo()
    {
        Debug.Log("Iniciando minijogo padrão...");
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player")) jogadorPorPerto = true;
    }

    private void OnTriggerExit(Collider outro)
    {
        if (outro.CompareTag("Player")) jogadorPorPerto = false;
    }
}

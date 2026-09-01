using UnityEngine;
using UnityEngine.InputSystem;

public class NPCBase : MonoBehaviour
{
    private const string TAG_PLAYER = "Player";

    [Header("Identificação")]
    [SerializeField] private string idMissao = "npc_secretaria";
    [SerializeField] private string nomeNPC = "Funcionário da Secretaria";
    [TextArea] [SerializeField] private string falaApresentacao = "Sou o funcionário da secretaria...";

    private PlayerInputActions input;
    private bool jogadorPorPerto = false;

    protected string NomeNPC => nomeNPC;
    protected string FalaApresentacao => falaApresentacao;

    private void Awake()
    {
        input = new PlayerInputActions();
        input.Player.Interact.performed += _ => Interagir();
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void OnDestroy()
    {
        input.Dispose();
    }

    private void Interagir()
    {
        if (!jogadorPorPerto) return;

        if (GameManager.Instance != null && GameManager.Instance.MissaoCompleta(idMissao))
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
        if (outro.CompareTag(TAG_PLAYER)) jogadorPorPerto = true;
    }

    private void OnTriggerExit(Collider outro)
    {
        if (outro.CompareTag(TAG_PLAYER)) jogadorPorPerto = false;
    }
}

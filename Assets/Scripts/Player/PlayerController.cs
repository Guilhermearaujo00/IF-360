using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velMovimento = 8f;
    [SerializeField] private float forcaPulo = 9f;
    [SerializeField] private float gravidade = 25f;
    [SerializeField] private float velocidadeRotacao = 10f;
    [SerializeField] private float multiplicadorCorrida = 1.8f;

    private CharacterController controller;
    private Camera cam;
    private PlayerInputActions input;
    private Animator animator;

    private float velocidadeVertical;
    private Vector2 moveInput;
    private Vector3 direcaoAtual;
    private bool correndo;
    private bool podeCorrer;

    public bool NoChao => controller.isGrounded;

    public float Speed => direcaoAtual.magnitude * velMovimento;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            podeCorrer = animator.parameters.Any(p => p.name == "Correndo");
        }

        input = new PlayerInputActions();
        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        input.Player.Jump.performed += _ => Pular();
        input.Player.Correr.performed += _ => correndo = true;
        input.Player.Correr.canceled += _ => correndo = false;
    }

    private void OnEnable()
    {
        if (input != null) input.Enable();
    }

    private void OnDisable()
    {
        if (input != null) input.Disable();
    }

    private void OnDestroy()
    {
        if (input != null) input.Dispose();
    }

    private void Update()
    {
        Movimentar();
        AtualizarAnimacao();
    }

    private void AtualizarAnimacao()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        animator.SetFloat("Speed", direcaoAtual.magnitude);
        if (podeCorrer)
        {
            animator.SetBool("Correndo", correndo && controller.isGrounded && direcaoAtual.magnitude > 0.1f);
        }
    }

    private void Movimentar()
    {
        Vector3 frente = cam.transform.forward;
        Vector3 direita = cam.transform.right;
        frente.y = 0f;
        direita.y = 0f;
        frente.Normalize();
        direita.Normalize();

        direcaoAtual = (frente * moveInput.y + direita * moveInput.x).normalized;

        if (direcaoAtual.magnitude > 0.1f)
        {
            Quaternion alvo = Quaternion.LookRotation(direcaoAtual);
            transform.rotation = Quaternion.Slerp(transform.rotation, alvo, Time.deltaTime * velocidadeRotacao);
        }

        if (controller.isGrounded && velocidadeVertical < 0f)
        {
            velocidadeVertical = -2f;
        }

        Vector3 movimento = direcaoAtual * velMovimento * (correndo ? multiplicadorCorrida : 1f);
        movimento.y = velocidadeVertical;

        controller.Move(movimento * Time.deltaTime);
        velocidadeVertical -= gravidade * Time.deltaTime;
    }

    private void Pular()
    {
        if (controller.isGrounded)
        {
            velocidadeVertical = forcaPulo;
            if (animator != null && animator.runtimeAnimatorController != null) animator.SetTrigger("Pular");
        }
    }

    public void TravarControle(bool r)
    {
        if (r) input.Disable();
        else input.Enable();
    }
}

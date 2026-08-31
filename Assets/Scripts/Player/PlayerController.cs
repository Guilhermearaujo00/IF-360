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

    private CharacterController controller;
    private Camera cam;
    private PlayerInputActions input;

    private float velocidadeVertical;
    private Vector2 moveInput;
    private Vector3 direcaoAtual;

    public bool NoChao => controller.isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;

        input = new PlayerInputActions();
        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        input.Player.Jump.performed += _ => Pular();
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void OnDestroy()
    {
        input.Dispose();
    }

    private void Update()
    {
        Movimentar();
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

        Vector3 movimento = direcaoAtual * velMovimento;
        movimento.y = velocidadeVertical;

        controller.Move(movimento * Time.deltaTime);
        velocidadeVertical -= gravidade * Time.deltaTime;
    }

    private void Pular()
    {
        if (controller.isGrounded)
        {
            velocidadeVertical = forcaPulo;
        }
    }

    public void TravarControle(bool r)
    {
        if (r) input.Disable();
        else input.Enable();
    }
}

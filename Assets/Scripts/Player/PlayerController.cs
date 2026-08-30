using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float velMovimento = 8f;
    public float forcaPulo = 9f;
    public float gravidade = 25f;

    private CharacterController controller;
    private float velocidadeVertical;
    private Vector3 direcaoAtual;

    public bool NoChao => controller.isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Movimentar();
    }

    private void Movimentar()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 frente = Camera.main.transform.forward;
        Vector3 direita = Camera.main.transform.right;
        frente.y = 0f;
        direita.y = 0f;
        frente.Normalize();
        direita.Normalize();

        direcaoAtual = (frente * v + direita * h).normalized;

        if (direcaoAtual.magnitude > 0.1f)
        {
            Quaternion alvo = Quaternion.LookRotation(direcaoAtual);
            transform.rotation = Quaternion.Slerp(transform.rotation, alvo, Time.deltaTime * 10f);
        }

        if (controller.isGrounded && velocidadeVertical < 0f)
        {
            velocidadeVertical = -2f;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocidadeVertical = forcaPulo;
        }

        Vector3 movimento = direcaoAtual * velMovimento;
        movimento.y = velocidadeVertical;

        controller.Move(movimento * Time.deltaTime);
        velocidadeVertical -= gravidade * Time.deltaTime;
    }
}

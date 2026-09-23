using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] private Transform alvo;

    [Header("Sensibilidade do mouse")]
    [SerializeField] private float sensibilidade = 3f;
    [SerializeField] private float distanciaMax = 12f;
    [SerializeField] private float distanciaMin = 3f;
    [SerializeField] private float altura = 3f;

    private PlayerInputActions input;
    private float rotacaoX = 0f;
    private float rotacaoY = 0f;
    private float distanciaAtual = 8f;

    private void Awake()
    {
        input = new PlayerInputActions();
        input.Player.Look.performed += ctx => Orbita(ctx.ReadValue<Vector2>());
        input.Player.Zoom.performed += ctx => Zoom(ctx.ReadValue<float>());
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (alvo == null)
            alvo = transform.parent;
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

    private void LateUpdate()
    {
        if (alvo == null) return;

        Quaternion rot = Quaternion.Euler(rotacaoY, rotacaoX, 0f);
        Vector3 pos = alvo.position - rot * Vector3.forward * distanciaAtual + Vector3.up * altura;

        transform.position = pos;
        transform.LookAt(alvo.position + Vector3.up * 1f);
    }

    private void Orbita(Vector2 delta)
    {
        rotacaoX += delta.x * sensibilidade;
        rotacaoY -= delta.y * sensibilidade;
        rotacaoY = Mathf.Clamp(rotacaoY, -40f, 60f);
    }

    private void Zoom(float valor)
    {
        distanciaAtual -= valor * 2f;
        distanciaAtual = Mathf.Clamp(distanciaAtual, distanciaMin, distanciaMax);
    }
}

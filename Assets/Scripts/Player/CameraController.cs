using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Alvo")]
    public Transform alvo;
    public Vector3 offset = new Vector3(0f, 3f, -6f);

    [Header("Sensibilidade do mouse")]
    public float sensibilidade = 3f;
    public float distanciaMax = 12f;
    public float distanciaMin = 3f;

    private float rotacaoX = 0f;
    private float rotacaoY = 0f;
    private float distanciaAtual = 8f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (alvo == null)
            alvo = GameObject.FindWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        if (alvo == null) return;

        rotacaoX += Input.GetAxis("Mouse X") * sensibilidade;
        rotacaoY -= Input.GetAxis("Mouse Y") * sensibilidade;
        rotacaoY = Mathf.Clamp(rotacaoY, -40f, 60f);

        distanciaAtual -= Input.GetAxis("Mouse ScrollWheel") * 2f;
        distanciaAtual = Mathf.Clamp(distanciaAtual, distanciaMin, distanciaMax);

        Quaternion rot = Quaternion.Euler(rotacaoY, rotacaoX, 0f);
        Vector3 pos = alvo.position - rot * Vector3.forward * distanciaAtual + Vector3.up * offset.y;

        transform.position = pos;
        transform.LookAt(alvo.position + Vector3.up * 1f);
    }
}

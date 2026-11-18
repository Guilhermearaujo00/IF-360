namespace IF360
{
using UnityEngine;

/// <summary>
/// Comportamento de "vida" do NPC: acena quando o jogador se aproxima e se
/// vira para ele enquanto estiver por perto. Puramente cosmético — não cria
/// UI nem lê input de interação.
///
/// Parâmetros do Animator (criados por 'Tools > IFNMG 360 > 3 - Preparar
/// Animacoes dos NPCs'; se ausentes, o componente apenas gira o NPC):
///   - "Wave" (trigger): entra no estado de aceno;
///   - "IsTalking" (bool): interrompe o aceno quando um diálogo está aberto.
///
/// Regras:
///   - Ao cruzar o perímetro de saudação (7 m, plano horizontal) dispara
///     "Wave" UMA única vez;
///   - Só volta a acenar depois que o jogador sai do perímetro maior (8 m) —
///     histerese, evita disparos repetidos na borda;
///   - Enquanto por perto, rotaciona suavemente (apenas eixo Y) para o jogador.
/// </summary>
public class NPCAnimacao : MonoBehaviour
{
    private const string TAG_PLAYER = "Player";
    private const string PARAM_WAVE = "Wave";
    private const string PARAM_ISTALKING = "IsTalking";

    [Header("Perímetros (metros, distância horizontal)")]
    [Tooltip("Distância para ACENAR ao jogador.")]
    [SerializeField] private float raioSaudacao = 7f;

    [Tooltip("Distância para DESFAZER a saudação (>= raioSaudacao, para não piscar na borda).")]
    [SerializeField] private float raioSaida = 8f;

    [Header("Rotação")]
    [Tooltip("Graus por segundo ao virar-se para o jogador.")]
    [SerializeField] private float velocidadeRotacao = 180f;

    private Animator animator;
    private Transform jogador;
    private bool perto;            // jogador dentro do perímetro de saudação
    private bool temParamWave;
    private bool temParamIsTalking;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null) return;

        // Só mexe nos parâmetros que existem (evita warnings do Animator).
        foreach (AnimatorControllerParameter parametro in animator.parameters)
        {
            if (parametro.name == PARAM_WAVE) temParamWave = true;
            else if (parametro.name == PARAM_ISTALKING) temParamIsTalking = true;
        }
    }

    private void Update()
    {
        if (jogador == null)
        {
            GameObject achado = GameObject.FindWithTag(TAG_PLAYER);
            if (achado == null) return;
            jogador = achado.transform;
        }

        bool conversando = DialogueManager.Instancia != null && DialogueManager.Instancia.DialogoAtivo;
        if (animator != null && temParamIsTalking)
        {
            animator.SetBool(PARAM_ISTALKING, conversando);
        }

        Vector3 delta = jogador.position - transform.position;
        delta.y = 0f;
        float distancia = delta.magnitude;

        if (!perto && distancia <= raioSaudacao)
        {
            perto = true;
            if (animator != null && temParamWave && !conversando)
            {
                animator.SetTrigger(PARAM_WAVE);
            }
        }
        else if (perto && distancia > raioSaida)
        {
            perto = false;
        }

        if (perto && delta.sqrMagnitude > 0.0001f)
        {
            Quaternion olhar = Quaternion.LookRotation(delta.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, olhar,
                velocidadeRotacao * Time.deltaTime);
        }
    }

    private void OnValidate()
    {
        if (raioSaida < raioSaudacao) raioSaida = raioSaudacao;
    }
}
}

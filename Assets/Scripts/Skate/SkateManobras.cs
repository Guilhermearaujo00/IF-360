using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class SkateManobras : MonoBehaviour
{
    [Header("Manobras no ar")]
    [SerializeField] private float duracaoManobra = 0.5f;
    [SerializeField] private int pontosPorTruque = 100;

    private PlayerController player;
    private PlayerInputActions input;

    private bool manobraAtiva = false;
    private float manobraTimer = 0f;

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        input = new PlayerInputActions();
        input.Player.Kickflip.performed += _ => TentarManobra("Kickflip");
        input.Player.Backflip.performed += _ => TentarManobra("Backflip");
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
        if (!manobraAtiva) return;

        manobraTimer -= Time.deltaTime;
        if (manobraTimer <= 0f)
        {
            manobraAtiva = false;
        }
    }

    private void TentarManobra(string nome)
    {
        if (player == null || player.NoChao || manobraAtiva) return;

        manobraAtiva = true;
        manobraTimer = duracaoManobra;

        if (GameManager.Instance != null)
            GameManager.Instance.AdicionarPontos(pontosPorTruque);

        Debug.Log("Truque: " + nome + " → Pontos +" + pontosPorTruque);
    }
}

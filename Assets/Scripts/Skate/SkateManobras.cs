using UnityEngine;

public class SkateManobras : MonoBehaviour
{
    [Header("Manobras no ar")]
    public KeyCode kickflip = KeyCode.K;
    public KeyCode backflip = KeyCode.L;
    public float velocidadeRotacao = 720f;
    public float duracaoManobra = 0.5f;
    public int pontosPorTruque = 100;

    [Header("Pontos")]
    public int pontos = 0;

    private PlayerController player;
    private bool manobraAtiva = false;
    private float manobraTimer = 0f;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (player == null) return;

        if (!player.NoChao && !manobraAtiva)
        {
            if (Input.GetKeyDown(kickflip))
            {
                IniciarManobra("Kickflip");
            }
            else if (Input.GetKeyDown(backflip))
            {
                IniciarManobra("Backflip");
            }
        }

        if (manobraAtiva)
        {
            manobraTimer -= Time.deltaTime;
            if (manobraTimer <= 0f)
            {
                manobraAtiva = false;
            }
        }
    }

    private void IniciarManobra(string nome)
    {
        manobraAtiva = true;
        manobraTimer = duracaoManobra;
        pontos += pontosPorTruque;

        if (GameManager.Instance != null)
            GameManager.Instance.AdicionarPontos(pontosPorTruque);

        Debug.Log("Truque: " + nome + " → Pontos: " + pontos);
    }
}

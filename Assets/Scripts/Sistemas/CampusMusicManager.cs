using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(AudioSource))]
public class CampusMusicManager : MonoBehaviour
{
    public static CampusMusicManager Instance { get; private set; }

    [Header("Playlist do Campus")]
    [Tooltip("Arraste aqui todas as músicas da pasta MusicaGameplay")]
    public List<AudioClip> faixasMusicais = new List<AudioClip>();

    [Header("Configurações de Reprodução")]
    [Range(0f, 1f)]
    public float volumeMaximo = 0.45f;
    public bool ordemAleatoria = true;
    public float tempoFade = 1.5f;
    public float pausaEntreMusicas = 1.0f;

    [Header("Atalho Opcional (Debug / Jogador)")]
    public bool permitirPularComTeclaM = true;

    private AudioSource audioSource;
    private List<int> filaIndices = new List<int>();
    private int ultimoIndiceTocado = -1;
    private bool trocandoFaixa = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    private void Start()
    {
        if (faixasMusicais == null || faixasMusicais.Count == 0)
        {
            Debug.LogWarning("[CampusMusicManager] Nenhuma música atribuída na lista.");
            return;
        }

        ReconstruirFila();
        StartCoroutine(TocarProximaComFade(0f));
    }

    private void Update()
    {
        if (faixasMusicais.Count == 0 || trocandoFaixa) return;

        // Detecta quando a música atual terminou de tocar
        if (!audioSource.isPlaying && audioSource.clip != null && audioSource.time == 0f)
        {
            StartCoroutine(TocarProximaComFade(pausaEntreMusicas));
        }

        // Leitura compatível com o Novo Input System (tecla M)
        if (permitirPularComTeclaM && TeclaMPressionada())
        {
            PularMusica();
        }
    }

    private bool TeclaMPressionada()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.M);
#endif
    }

    public void PularMusica()
    {
        if (!trocandoFaixa && faixasMusicais.Count > 0)
        {
            StopAllCoroutines();
            StartCoroutine(TocarProximaComFade(0f));
        }
    }

    private void ReconstruirFila()
    {
        filaIndices.Clear();
        for (int i = 0; i < faixasMusicais.Count; i++)
        {
            if (faixasMusicais[i] != null)
                filaIndices.Add(i);
        }

        if (ordemAleatoria && filaIndices.Count > 1)
        {
            for (int i = filaIndices.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = filaIndices[i];
                filaIndices[i] = filaIndices[j];
                filaIndices[j] = temp;
            }

            if (filaIndices[0] == ultimoIndiceTocado)
            {
                int primeiro = filaIndices[0];
                filaIndices.RemoveAt(0);
                filaIndices.Add(primeiro);
            }
        }
    }

    private IEnumerator TocarProximaComFade(float atrasoInicial)
    {
        trocandoFaixa = true;

        if (audioSource.isPlaying)
        {
            float inicioVol = audioSource.volume;
            float t = 0f;
            while (t < tempoFade)
            {
                t += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(inicioVol, 0f, t / tempoFade);
                yield return null;
            }
            audioSource.Stop();
        }

        if (atrasoInicial > 0f)
            yield return new WaitForSecondsRealtime(atrasoInicial);

        if (filaIndices.Count == 0)
            ReconstruirFila();

        int proximoIndice = filaIndices[0];
        filaIndices.RemoveAt(0);
        ultimoIndiceTocado = proximoIndice;

        audioSource.clip = faixasMusicais[proximoIndice];
        audioSource.Play();

        float tempoIn = 0f;
        while (tempoIn < tempoFade)
        {
            tempoIn += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, volumeMaximo, tempoIn / tempoFade);
            yield return null;
        }
        audioSource.volume = volumeMaximo;

        trocandoFaixa = false;
    }

    public void DefinirVolume(float novoVolume)
    {
        volumeMaximo = Mathf.Clamp01(novoVolume);
        if (!trocandoFaixa)
            audioSource.volume = volumeMaximo;
    }
}
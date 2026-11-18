using System.Collections.Generic;
using UnityEngine;
using TMPro;
using IF360;

public class VozesNPCAddon : MonoBehaviour
{
    [System.Serializable]
    public class ConfiguracaoVozNPC
    {
        [Tooltip("Arraste o objeto do NPC da cena aqui (ou deixe vazio se quiser buscar pelo nome)")]
        public NPCBaseNovo npcObjeto;

        [Tooltip("Opcional: Nome exato do NPC caso queira associar por texto")]
        public string nomeDoNPC;

        [Header("Som Estilo Undertale")]
        public AudioClip somDeVoz;
        [Range(0f, 1f)] public float volume = 0.65f;
        [Range(0.5f, 2f)] public float pitchBase = 1.0f;
        [Range(0f, 0.25f)] public float variacaoPitch = 0.08f;

        [Tooltip("Toca o som a cada X letras que aparecem na tela")]
        [Min(1)] public int aCadaXLetras = 2;
    }

    [Header("Referência da UI de Texto")]
    [Tooltip("Arraste aqui o TextMeshProUGUI onde o texto da fala aparece")]
    public TextMeshProUGUI textoDaFalaUI;

    [Header("Lista de Vozes por NPC")]
    public List<ConfiguracaoVozNPC> vozesDosNPCs = new List<ConfiguracaoVozNPC>();

    private AudioSource audioSource;
    private ConfiguracaoVozNPC vozAtual;
    private int ultimoTamanhoTexto = 0;
    private int contadorLetrasValidas = 0;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        NPCBaseNovo.DialogoSolicitado += AoIniciarConversaComNPC;
    }

    private void OnDisable()
    {
        NPCBaseNovo.DialogoSolicitado -= AoIniciarConversaComNPC;
    }

    private void AoIniciarConversaComNPC(NPCBaseNovo npc)
    {
        vozAtual = null;
        ultimoTamanhoTexto = 0;
        contadorLetrasValidas = 0;

        if (npc == null) return;

        foreach (var config in vozesDosNPCs)
        {
            if (config.npcObjeto == npc ||
               (!string.IsNullOrWhiteSpace(config.nomeDoNPC) && config.nomeDoNPC == npc.nomeNPC))
            {
                vozAtual = config;
                break;
            }
        }
    }

    private void Update()
    {
        if (DialogueManager.Instancia == null || !DialogueManager.Instancia.DialogoAtivo)
        {
            ultimoTamanhoTexto = 0;
            return;
        }

        if (textoDaFalaUI == null || vozAtual == null || vozAtual.somDeVoz == null)
            return;

        string textoAtual = textoDaFalaUI.text;
        int tamanhoAtual = textoAtual.Length;

        if (tamanhoAtual > ultimoTamanhoTexto)
        {
            if (tamanhoAtual - ultimoTamanhoTexto <= 2)
            {
                char novaLetra = textoAtual[tamanhoAtual - 1];

                if (char.IsLetterOrDigit(novaLetra))
                {
                    if (contadorLetrasValidas % Mathf.Max(1, vozAtual.aCadaXLetras) == 0)
                    {
                        TocarSom(vozAtual);
                    }
                    contadorLetrasValidas++;
                }
            }
        }
        else if (tamanhoAtual < ultimoTamanhoTexto)
        {
            contadorLetrasValidas = 0;
        }

        ultimoTamanhoTexto = tamanhoAtual;
    }

    private void TocarSom(ConfiguracaoVozNPC config)
    {
        float pitchAleatorio = Random.Range(-config.variacaoPitch, config.variacaoPitch);
        audioSource.pitch = Mathf.Clamp(config.pitchBase + pitchAleatorio, 0.5f, 2.5f);
        audioSource.PlayOneShot(config.somDeVoz, config.volume);
    }
}
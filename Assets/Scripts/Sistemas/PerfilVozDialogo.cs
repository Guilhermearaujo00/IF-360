using UnityEngine;

// A linha abaixo cria a opção no menu do botão direito da Unity
[CreateAssetMenu(fileName = "Voz_NovoPersonagem", menuName = "IF360/Dialogo/Perfil de Voz")]
public class PerfilVozDialogo : ScriptableObject
{
    [Header("1. Associação ao Personagem")]
    [Tooltip("Nome que aparecerá no topo da caixa de diálogo")]
    public string nomePersonagem;

    [Tooltip("Arraste aqui o(s) som(ns) curto(s) exclusivos deste personagem")]
    public AudioClip[] sonsDeFala;

    [Header("2. Timbre e Entonação (Estilo Undertale)")]
    [Range(0f, 1f)]
    public float volume = 0.65f;

    [Tooltip("1.0 = tom original | menor que 1 = mais grave | maior que 1 = mais agudo")]
    [Range(0.5f, 2f)]
    public float pitchBase = 1.0f;

    [Tooltip("Variação aleatória a cada bipe para a fala não soar robótica")]
    [Range(0f, 0.3f)]
    public float variacaoPitch = 0.08f;

    [Header("3. Sincronia com o Texto")]
    [Tooltip("Tempo em segundos entre cada letra aparecer na tela (ex: 0.035)")]
    [Range(0.01f, 0.15f)]
    public float velocidadeDigitacao = 0.035f;

    [Tooltip("Toca o som a cada X letras (2 ou 3 evita poluição sonora)")]
    [Min(1)]
    public int tocarACadaXCaracteres = 2;
}
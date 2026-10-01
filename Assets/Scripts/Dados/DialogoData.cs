using UnityEngine;

[CreateAssetMenu(menuName = "Dados/Diálogo", fileName = "NovoDialogo")]
public class DialogoData : ScriptableObject
{
    [System.Serializable]
    public class FalaDialogo
    {
        [Tooltip("Quem fala esta linha (ex.: Funcionário, Caio). Deixe vazio para usar o nome do NPC.")]
        public string nomeFalante = "";

        [Tooltip("Texto exibido nesta linha.")]
        [TextArea(2, 5)] public string textoFala;
    }

    [Header("Falas na ordem (cada [E] avança para a próxima)")]
    public FalaDialogo[] falas;

    [Header("Digitação")]
    [Tooltip("Segundos por caractere usados em todas as falas. 0 = texto aparece instantâneo.")]
    public float velDigitacao = 0.03f;
}
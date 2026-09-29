using UnityEngine;

[CreateAssetMenu(menuName = "Dados/Diálogo", fileName = "NovoDialogo")]
public class DialogoData : ScriptableObject
{
    [Header("Falas")]
    [Tooltip("Falas na ordem em que serão exibidas.")]
    [TextArea(2, 5)] public string[] falas;

    [Header("Digitação")]
    [Tooltip("Segundos por caractere. 0 = texto aparece instantâneo.")]
    public float velDigitacao = 0.03f;
}
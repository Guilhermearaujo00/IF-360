using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia { get; private set; }

    [Header("Sons")]
    [SerializeField] private AudioClip somDigitar;
    [SerializeField] private AudioClip somConfirmar;
    [SerializeField] private AudioClip somFechar;

    private AudioSource fonte;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        fonte = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    public void TocarSom(AudioClip clip)
    {
        if (clip == null || fonte == null) return;
        fonte.PlayOneShot(clip);
    }

    public void TocarDigitar()
    {
        TocarSom(somDigitar);
    }

    public void TocarConfirmar()
    {
        TocarSom(somConfirmar);
    }

    public void TocarFechar()
    {
        TocarSom(somFechar);
    }
}
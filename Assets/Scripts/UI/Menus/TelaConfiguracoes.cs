namespace IF360
{
using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Tela de configurações (volume, qualidade). Acessada pelo menu de pausa.
/// </summary>
public class TelaConfiguracoes : MenuBase
{
    public Action AoFechar;

    private const string PREFS_VOLUME_MASTER = "volume_master";
    private const string PREFS_VOLUME_MUSICA = "volume_musica";
    private const string PREFS_VOLUME_EFEITOS = "volume_efeitos";
    private const string PREFS_QUALIDADE = "qualidade_grafica";

    private float volumeMaster = 1f;
    private float volumeMusica = 1f;
    private float volumeEfeitos = 1f;
    private int qualidadeIndex = 2;

    protected override void Awake()
    {
        base.Awake();
        CarregarPreferencias();
    }

    protected virtual void ConstruirConteudo()
    {
        AdicionarItem("Volume Geral", null,
            () => $"{(volumeMaster * 100f):F0}%",
            () => AjustarVolume(ref volumeMaster, -0.1f, PREFS_VOLUME_MASTER),
            () => AjustarVolume(ref volumeMaster, 0.1f, PREFS_VOLUME_MASTER));

        AdicionarItem("Volume Música", null,
            () => $"{(volumeMusica * 100f):F0}%",
            () => AjustarVolume(ref volumeMusica, -0.1f, PREFS_VOLUME_MUSICA),
            () => AjustarVolume(ref volumeMusica, 0.1f, PREFS_VOLUME_MUSICA));

        AdicionarItem("Volume Efeitos", null,
            () => $"{(volumeEfeitos * 100f):F0}%",
            () => AjustarVolume(ref volumeEfeitos, -0.1f, PREFS_VOLUME_EFEITOS),
            () => AjustarVolume(ref volumeEfeitos, 0.1f, PREFS_VOLUME_EFEITOS));

        AdicionarItem("Qualidade Gráfica", null,
            () => QualitySettings.names[Mathf.Clamp(qualidadeIndex, 0, QualitySettings.names.Length - 1)],
            () => MudarQualidade(-1),
            () => MudarQualidade(1));

        AdicionarItem("Voltar", Fechar);
    }

    private void CarregarPreferencias()
    {
        volumeMaster = PlayerPrefs.GetFloat(PREFS_VOLUME_MASTER, 1f);
        volumeMusica = PlayerPrefs.GetFloat(PREFS_VOLUME_MUSICA, 1f);
        volumeEfeitos = PlayerPrefs.GetFloat(PREFS_VOLUME_EFEITOS, 1f);
        qualidadeIndex = PlayerPrefs.GetInt(PREFS_QUALIDADE, 2);
        qualidadeIndex = Mathf.Clamp(qualidadeIndex, 0, QualitySettings.names.Length - 1);

        AudioListener.volume = volumeMaster;
        QualitySettings.SetQualityLevel(qualidadeIndex, true);
    }

    private void AjustarVolume(ref float volume, float delta, string prefsKey)
    {
        volume = Mathf.Clamp01(volume + delta);
        PlayerPrefs.SetFloat(prefsKey, volume);
        AudioListener.volume = volumeMaster; // Usa master como volume global
        PlayerPrefs.Save();
    }

    private void MudarQualidade(int direcao)
    {
        qualidadeIndex = Mathf.Clamp(qualidadeIndex + direcao, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(qualidadeIndex, true);
        PlayerPrefs.SetInt(PREFS_QUALIDADE, qualidadeIndex);
        PlayerPrefs.Save();
    }

    protected override void OnFechar()
    {
        base.OnFechar();
        AoFechar?.Invoke();
    }
}
}
namespace IF360
{
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu principal do jogo (cena Menu).
/// </summary>
public class MenuPrincipal : MenuBase
{
    protected override void Awake()
    {
        base.Awake();
        // Desabilita pausa automática via ESC no menu principal
        MenuPausa.Instancia?.Fechar();
    }

    protected override void ConstruirConteudo()
    {
        AdicionarItem("Iniciar Jogo", IniciarJogo);
        AdicionarItem("Continuar", ContinuarJogo);
        AdicionarItem("Configurações", AbrirConfiguracoes);
        AdicionarItem("Créditos", MostrarCreditos);
        AdicionarItem("Sair", SairJogo);
    }

    private void IniciarJogo()
    {
        // Limpa save anterior para novo jogo
        if (SaveManager.Instancia != null) SaveManager.Instancia.LimparSave();
        TelaCarregamento.Carregar("Campus");
    }

    private void ContinuarJogo()
    {
        // Verifica se existe save
        if (SaveManager.Instancia != null && SaveManager.Instancia.TemSave())
        {
            TelaCarregamento.Carregar("Campus");
        }
        else
        {
            // Sem save - inicia novo jogo
            IniciarJogo();
        }
    }

    private void AbrirConfiguracoes()
    {
        var config = gameObject.AddComponent<TelaConfiguracoes>();
        config.AoFechar += () => { this.ativo = true; painel.SetActive(true); };
        this.ativo = false;
        painel.SetActive(false);
        config.Abrir();
    }

    private void MostrarCreditos()
    {
        // TODO: implementar tela de créditos
        Debug.Log("Créditos - a implementar");
    }

    private void SairJogo()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    protected override void Update()
    {
        if (!ativo) return;
        base.Update();

        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            SairJogo();
        }
    }
}
}
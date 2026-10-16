namespace IF360
{
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu principal do jogo (cena Menu).
/// </summary>
public class MenuPrincipal : MenuBase
{
    private TelaConfiguracoes telaConfig;

    protected override void Awake()
    {
        base.Awake();
        MenuPausa.Instancia?.Fechar();
    }

    private void Start()
    {
        Abrir();
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
        if (telaConfig == null)
        {
            telaConfig = gameObject.AddComponent<TelaConfiguracoes>();
            telaConfig.AoFechar += () => { this.ativo = true; if (painel != null) painel.SetActive(true); };
        }
        this.ativo = false;
        if (painel != null) painel.SetActive(false);
        telaConfig.Abrir();
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

    protected override void TratarCancelar()
    {
        if (ativo) SairJogo();
    }
}
}
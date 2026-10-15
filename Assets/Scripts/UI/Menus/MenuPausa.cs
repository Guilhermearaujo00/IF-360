namespace IF360
{
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu de pausa acessível via ESC durante o gameplay.
/// Gerencia sub-menus (Configurações, Inventário) e pausa o jogo (Time.timeScale).
/// </summary>
public class MenuPausa : MenuBase
{
    public static MenuPausa Instancia { get; private set; }

    private TelaConfiguracoes telaConfig;
    private TelaInventario telaInventario;
    private bool subMenuAberto;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetarInstancia() => Instancia = null;

    protected override void Awake()
    {
        base.Awake();
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    protected override void OnAbrir()
    {
        // Pausa o jogo
        Time.timeScale = 0f;
        TravarJogador(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected override void OnFechar()
    {
        // Despausa o jogo
        Time.timeScale = 1f;
        TravarJogador(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        base.OnFechar();
        // Garante que sub-menus estão fechados
        if (telaConfig != null && telaConfig.Ativo) telaConfig.Fechar();
        if (telaInventario != null && telaInventario.Ativo) telaInventario.Fechar();
        subMenuAberto = false;
    }

    protected override void Update()
    {
        if (!ativo) return;

        // Se sub-menu aberto, não processa navegação do menu pausa
        if (subMenuAberto) return;

        base.Update();

        // ESC fecha o menu de pausa
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            Fechar();
        }
    }

    protected override void ConstruirConteudo()
    {
        // Itens do menu pausa
        AdicionarItem("Continuar", Fechar);
        AdicionarItem("Configurações", AbrirConfiguracoes);
        AdicionarItem("Inventário", AbrirInventario);
        AdicionarItem("Menu Principal", VoltarMenuPrincipal);
        AdicionarItem("Sair do Jogo", SairJogo);
    }

    private void AbrirConfiguracoes()
    {
        if (telaConfig == null)
        {
            telaConfig = gameObject.AddComponent<TelaConfiguracoes>();
            telaConfig.AoFechar += () => subMenuAberto = false;
        }
        subMenuAberto = true;
        this.ativo = false; // Desativa navegação deste menu
        painel.SetActive(false);
        telaConfig.Abrir();
    }

    private void AbrirInventario()
    {
        if (telaInventario == null)
        {
            telaInventario = gameObject.AddComponent<TelaInventario>();
            telaInventario.AoFechar += () => subMenuAberto = false;
        }
        subMenuAberto = true;
        this.ativo = false;
        painel.SetActive(false);
        telaInventario.Abrir();
    }

    private void VoltarMenuPrincipal()
    {
        Time.timeScale = 1f;
        TravarJogador(false);
        SceneManager.LoadScene("Menu");
    }

    private void SairJogo()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void TravarJogador(bool travar)
    {
        var player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null) player.TravarControle(travar);

        var camera = Object.FindAnyObjectByType<CameraController>();
        if (camera != null) camera.TravarControle(travar);
    }

    /// <summary>
    /// Chamado externamente para alternar pausa (ex.: botão HUD futuro).
    /// </summary>
    public static void AlternarPausa()
    {
        if (Instancia == null) return;
        if (Instancia.ativo) Instancia.Fechar();
        else Instancia.Abrir();
    }
}
}
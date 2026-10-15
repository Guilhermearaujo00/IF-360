using System;
using IF360;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Configura os menus do jogo nas cenas Campus e Menu.
/// Menu: Tools > IFNMG > ...
/// </summary>
public static class SetupMenus
{
    private const string MENU_TUDO = "Tools/IFNMG/Configurar Menus (Tudo)";
    private const string MENU_PAUSA = "Tools/IFNMG/Configurar Menu Pausa";
    private const string MENU_PRINCIPAL = "Tools/IFNMG/Configurar Menu Principal";

    private const string PATH_CAMPUS = "Assets/Scenes/Campus.unity";
    private const string PATH_MENU = "Assets/Scenes/Menu.unity";

    [MenuItem(MENU_TUDO, priority = 0)]
    public static void ConfigurarTudo()
    {
        if (!PodeEditar()) return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorUtility.DisplayDialog("SetupMenus", "Salve ou descarte as alterações abertas antes de continuar.", "OK");
            return;
        }

        ConfigurarCena(PATH_CAMPUS, AplicarMenuPausa);
        ConfigurarCena(PATH_MENU, AplicarMenuPrincipal);

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("SetupMenus", "Cenas Menu e Campus configuradas e salvas.", "OK");
    }

    [MenuItem(MENU_TUDO, validate = true)]
    private static bool ValidarConfigurarTudo() => !EditorApplication.isPlayingOrWillChangePlaymode;

    [MenuItem(MENU_PAUSA)]
    public static void ConfigurarMenuPausa()
    {
        if (!PodeEditar()) return;
        ConfigurarCena(PATH_CAMPUS, AplicarMenuPausa);
    }

    [MenuItem(MENU_PRINCIPAL)]
    public static void ConfigurarMenuPrincipal()
    {
        if (!PodeEditar()) return;
        ConfigurarCena(PATH_MENU, AplicarMenuPrincipal);
    }

    private static bool PodeEditar()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode) return true;

        EditorUtility.DisplayDialog("SetupMenus", "Saia do Play Mode antes de configurar.", "OK");
        return false;
    }

    /// <summary>
    /// Abre a cena, aplica a configuracao, marca como suja e salva em disco.
    /// </summary>
    private static void ConfigurarCena(string caminho, Action configurar)
    {
        Scene cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        configurar();
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log($"[SetupMenus] Cena '{cena.name}' configurada e salva.");
    }

    // --- Configuracoes por cena ---

    private static void AplicarMenuPausa()
    {
        GameObject systems = GameObject.Find("Systems") ?? new GameObject("Systems");

        if (systems.GetComponent<MenuPausa>() == null)
        {
            systems.AddComponent<MenuPausa>();
            Debug.Log("[SetupMenus] MenuPausa adicionado em 'Systems'.");
        }

        GarantirTelaCarregamento();
    }

    private static void AplicarMenuPrincipal()
    {
        GarantirCanvas();
        GarantirTelaCarregamento();

        GameObject menuGo = GameObject.Find("MenuPrincipal") ?? new GameObject("MenuPrincipal");

        if (menuGo.GetComponent<MenuPrincipal>() == null)
        {
            menuGo.AddComponent<MenuPrincipal>();
            Debug.Log("[SetupMenus] MenuPrincipal adicionado.");
        }
    }

    private static void GarantirCanvas()
    {
        if (UnityEngine.Object.FindAnyObjectByType<Canvas>() != null) return;

        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<GraphicRaycaster>();

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Debug.Log("[SetupMenus] Canvas criado.");
    }

    private static void GarantirTelaCarregamento()
    {
        if (UnityEngine.Object.FindAnyObjectByType<TelaCarregamento>() != null) return;

        var go = new GameObject("TelaCarregamento");
        go.AddComponent<TelaCarregamento>();
        Debug.Log("[SetupMenus] TelaCarregamento criado.");
    }
}
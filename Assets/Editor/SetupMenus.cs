using IF360;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ferramenta de configuração dos menus do jogo.
/// Menu: Tools > IFNMG > Configurar Menus
/// </summary>
public static class SetupMenus
{
    private const string MENU = "Tools/IFNMG/Configurar Menus";
    private const string MENU_PAUSA = "Tools/IFNMG/Adicionar Menu Pausa";
    private const string MENU_PRINCIPAL = "Tools/IFNMG/Configurar Menu Principal";

    [MenuItem(MENU)]
    public static void ConfigurarTudo()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("SetupMenus", "Saia do Play Mode antes de configurar.", "OK");
            return;
        }

        ConfigurarMenuPausa();
        ConfigurarMenuPrincipal();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("SetupMenus: menus configurados.");
    }

    [MenuItem(MENU_PAUSA)]
    public static void ConfigurarMenuPausa()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("SetupMenus", "Saia do Play Mode antes de configurar.", "OK");
            return;
        }

        // Garante objeto Systems
        GameObject systems = GameObject.Find("Systems");
        if (systems == null)
        {
            systems = new GameObject("Systems");
            Undo.RegisterCreatedObjectUndo(systems, "Criar Systems");
        }

        // Adiciona MenuPausa se não existe
        if (systems.GetComponent<MenuPausa>() == null)
        {
            Undo.AddComponent<MenuPausa>(systems);
            Debug.Log("SetupMenus: MenuPausa adicionado em 'Systems'.");
        }
        else
        {
            Debug.Log("SetupMenus: MenuPausa já existe.");
        }

        // Garante TelaCarregamento
        if (Object.FindAnyObjectByType<TelaCarregamento>() == null)
        {
            var go = new GameObject("TelaCarregamento");
            Undo.RegisterCreatedObjectUndo(go, "Criar TelaCarregamento");
            go.AddComponent<TelaCarregamento>();
            Debug.Log("SetupMenus: TelaCarregamento criado (DontDestroyOnLoad).");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    [MenuItem(MENU_PRINCIPAL)]
    public static void ConfigurarMenuPrincipal()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("SetupMenus", "Saia do Play Mode antes de configurar.", "OK");
            return;
        }

        // Deve estar na cena Menu
        var cenaAtiva = SceneManager.GetActiveScene();
        if (cenaAtiva.name != "Menu")
        {
            EditorUtility.DisplayDialog("SetupMenus", "Abra a cena 'Menu' antes de configurar o menu principal.", "OK");
            return;
        }

        // Garante Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGo = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGo, "Criar Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            Debug.Log("SetupMenus: Canvas criado para Menu Principal.");
        }

        // Adiciona MenuPrincipal
        GameObject menuGo = GameObject.Find("MenuPrincipal");
        if (menuGo == null)
        {
            menuGo = new GameObject("MenuPrincipal");
            Undo.RegisterCreatedObjectUndo(menuGo, "Criar MenuPrincipal");
        }

        if (menuGo.GetComponent<MenuPrincipal>() == null)
        {
            Undo.AddComponent<MenuPrincipal>(menuGo);
            Debug.Log("SetupMenus: MenuPrincipal adicionado.");
        }
        else
        {
            Debug.Log("SetupMenus: MenuPrincipal já existe.");
        }

        EditorSceneManager.MarkSceneDirty(cenaAtiva);
    }

    // Helper para encontrar/criar filho (mesmo padrão do SetupSistemaDialogo)
    private static Transform BuscarFilho(Transform pai, string nome)
    {
        foreach (Transform filho in pai)
        {
            if (filho.name == nome) return filho;
            Transform neto = BuscarFilho(filho, nome);
            if (neto != null) return neto;
        }
        return null;
    }

    private static GameObject ObterOuCriarFilho(GameObject pai, string nome)
    {
        Transform existente = BuscarFilho(pai.transform, nome);
        if (existente != null) return existente.gameObject;

        GameObject go = new GameObject(nome);
        Undo.RegisterCreatedObjectUndo(go, "Criar " + nome);
        go.transform.SetParent(pai.transform, false);
        return go;
    }
}

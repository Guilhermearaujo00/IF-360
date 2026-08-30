using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GerarScenes
{
    [MenuItem("Tools/Gerar Cenas base")]
    public static void Gerar()
    {
        if (!System.IO.Directory.Exists("Assets/Scenes"))
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            AssetDatabase.Refresh();
        }

        CriarCena("Campus");
        CriarCena("Menu");

        Debug.Log("Cenas base criadas com sucesso.");
    }

    private static void CriarCena(string nome)
    {
        string path = "Assets/Scenes/" + nome + ".unity";
        if (System.IO.File.Exists(path))
        {
            Debug.Log("Cena já existe: " + path);
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var root = new GameObject("Area_" + nome);
        root.tag = "Untagged";

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("Cena criada: " + path);
    }
}

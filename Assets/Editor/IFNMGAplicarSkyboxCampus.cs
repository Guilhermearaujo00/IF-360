using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGAplicarSkyboxCampus
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";
    private const string MaterialPath = "Assets/BOXOPHOBIC/Skybox Cubemap Extended/Demo/Materials/Skybox Cubemap Extended Day.mat";

    public static void Aplicar()
    {
        Scene scene = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (skybox == null) { Debug.LogError("[IFNMG Skybox] Material nao encontrado: " + MaterialPath); return; }
        RenderSettings.skybox = skybox;
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
        RenderSettings.customReflectionTexture = null;
        DynamicGI.UpdateEnvironment();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, CampusPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[IFNMG Skybox] Aplicado: " + skybox.name);
    }
}

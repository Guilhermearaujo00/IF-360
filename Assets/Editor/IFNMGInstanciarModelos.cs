using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Instancia os 16 modelos .fbx convertidos (Assets/Models/IFNMG) na Campus.unity,
/// agrupados sob "Modelos_IFNMG". Escala 1,1,1, posição (0,0,0), rotação identidade.
/// Idempotente: remove o grupo anterior se existir.
///
/// Batch: -executeMethod IFNMGInstanciarModelos.Instanciar
/// </summary>
public static class IFNMGInstanciarModelos
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";
    private const string ModelsDir = "Assets/Models/IFNMG";
    private const string GroupName = "Modelos_IFNMG";

    [MenuItem("Tools/IFNMG 360/10 - Instanciar modelos FBX na Campus")]
    public static void InstanciarMenu()
    {
        if (!EditorUtility.DisplayDialog("Instanciar modelos",
            "Vai instanciar todos os .fbx de " + ModelsDir + " na Campus, sob '" + GroupName + "'. Continuar?",
            "Instanciar", "Cancelar")) return;
        Instanciar();
    }

    public static void Instanciar()
    {
        Scene campus = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);

        // Remove grupo anterior (idempotência).
        foreach (GameObject go in campus.GetRootGameObjects())
            if (go.name == GroupName) { Object.DestroyImmediate(go); Debug.Log("[IFNMG Modelos] Grupo anterior removido."); }

        GameObject group = new GameObject(GroupName);
        SceneManager.MoveGameObjectToScene(group, campus);
        group.transform.position = Vector3.zero;

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { ModelsDir });
        int ok = 0;
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            if (!path.ToLower().EndsWith(".fbx")) continue;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { Debug.LogWarning("[IFNMG Modelos] Nao carregou: " + path); continue; }
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
            inst.name = Path.GetFileNameWithoutExtension(path);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale = Vector3.one;
            ok++;
            Debug.Log("[IFNMG Modelos] Instanciado: " + inst.name);
        }

        EditorSceneManager.MarkSceneDirty(campus);
        EditorSceneManager.SaveScene(campus, CampusPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[IFNMG Modelos] FIM. Total instanciado: " + ok);
    }
}

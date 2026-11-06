using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGRemoverEstruturaBiologiaDuplicada
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";
    private const string GroupName = "NovasEstruturas_IFNMG";
    private const string DuplicateName = "estrura biologia pronto (2)";

    public static void Remover()
    {
        Scene scene = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);
        GameObject group = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == GroupName) { group = root; break; }
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == GroupName) { group = child.gameObject; break; }
            if (group != null) break;
        }

        if (group == null) { Debug.LogError("[IFNMG] Grupo NovasEstruturas_IFNMG nao encontrado."); return; }
        List<GameObject> removidos = new List<GameObject>();
        foreach (Transform child in group.transform)
            if (child.name == DuplicateName) removidos.Add(child.gameObject);
        foreach (GameObject go in removidos) Object.DestroyImmediate(go);

        Debug.Log("[IFNMG] Instancias removidas: " + removidos.Count);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, CampusPath);
        AssetDatabase.SaveAssets();
    }
}

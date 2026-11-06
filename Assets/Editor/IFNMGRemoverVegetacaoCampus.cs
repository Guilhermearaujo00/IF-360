using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGRemoverVegetacaoCampus
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";
    private const string MapaName = "Mapa_IFNMG_Prototipo";

    public static void Remover()
    {
        Scene campus = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);
        GameObject mapa = null;
        foreach (GameObject go in campus.GetRootGameObjects())
            if (go.name == MapaName) { mapa = go; break; }

        if (mapa == null) { Debug.LogError("[IFNMG Vegetacao] Mapa_IFNMG_Prototipo nao encontrado."); return; }

        Transform vegetacao = null;
        foreach (Transform child in mapa.transform)
            if (child.name == "Vegetacao") { vegetacao = child; break; }

        if (vegetacao == null)
        {
            Debug.Log("[IFNMG Vegetacao] Grupo Vegetacao ja nao existe.");
        }
        else
        {
            int count = vegetacao.GetComponentsInChildren<Transform>(true).Length - 1;
            Object.DestroyImmediate(vegetacao.gameObject);
            Debug.Log("[IFNMG Vegetacao] Grupo removido. Objetos removidos=" + count);
        }

        EditorSceneManager.MarkSceneDirty(campus);
        EditorSceneManager.SaveScene(campus, CampusPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[IFNMG Vegetacao] FIM. Vegetacao pronta para insercao manual.");
    }
}

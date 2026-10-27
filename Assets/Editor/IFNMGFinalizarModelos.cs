using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Finalizacao dos modelos IFNMG na Campus:
///  - Deduplica grupos "Modelos_IFNMG" (mantem o que tem mais filhos diretos = 16 modelos).
///  - Desativa (nao apaga) o grupo "Estruturas" do prototipo (blockouts que se sobrepoem aos modelos reais).
/// Batch: -executeMethod IFNMGFinalizarModelos.Finalizar
/// </summary>
public static class IFNMGFinalizarModelos
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";

    public static void Finalizar()
    {
        Scene campus = EditorSceneManager.OpenScene(CampusPath, UnityEditor.SceneManagement.OpenSceneMode.Single);

        List<Transform> groups = new List<Transform>();
        CollectGroups(campus, groups);

        Transform keep = null;
        int best = -1;
        foreach (Transform g in groups)
        {
            Debug.Log($"[Finalizar] grupo Modelos_IFNMG em '{(g.parent != null ? g.parent.name : "RAIZ")}' filhos_diretos={g.childCount}");
            if (g.childCount > best) { best = g.childCount; keep = g; }
        }
        if (keep == null) { Debug.LogWarning("[Finalizar] nenhum grupo Modelos_IFNMG"); }
        else
        {
            Debug.Log("[Finalizar] mantendo grupo com " + best + " filhos. Modelos:");
            for (int i = 0; i < keep.childCount; i++) Debug.Log("[Finalizar]   " + keep.GetChild(i).name);
        }
        foreach (Transform g in groups)
            if (keep == null || g != keep)
            {
                Debug.Log("[Finalizar] removendo grupo duplicado: " + g.name + " (filhos=" + g.childCount + ")");
                UnityEngine.Object.DestroyImmediate(g.gameObject);
            }

        GameObject mapa = null;
        foreach (GameObject go in campus.GetRootGameObjects())
            if (go.name == "Mapa_IFNMG_Prototipo") { mapa = go; break; }
        if (mapa != null)
        {
            foreach (Transform t in mapa.transform)
            {
                if (t.name != "Estruturas") continue;
                t.gameObject.SetActive(false);
                Debug.Log("[Finalizar] Estruturas (blockouts) DESATIVADA - reversivel.");
                break;
            }
        }

        EditorSceneManager.MarkSceneDirty(campus);
        EditorSceneManager.SaveScene(campus, CampusPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[Finalizar] FIM.");
    }

    private static void CollectGroups(Scene scene, List<Transform> outGroups)
    {
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            if (go.name == "Modelos_IFNMG") outGroups.Add(go.transform);
            foreach (Transform child in go.transform)
                if (child.name == "Modelos_IFNMG") outGroups.Add(child);
        }
    }
}

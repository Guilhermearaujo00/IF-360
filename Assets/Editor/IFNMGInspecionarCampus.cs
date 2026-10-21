using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// READ-ONLY: imprime a árvore hierárquica completa da cena Campus.unity,
/// com contagem de filhos e posição de cada raiz. Não altera nada.
///
/// Batch: -executeMethod IFNMGInspecionarCampus.Inspecionar
/// </summary>
public static class IFNMGInspecionarCampus
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";

    public static void Inspecionar()
    {
        Scene scene = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);
        Debug.Log("[IFNMG Inspecionar Campus] Raizes da cena '" + scene.name + "':");
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            int count = go.GetComponentsInChildren<Transform>(true).Length - 1;
            Debug.Log($"[IFNMG Inspecionar Campus]   ROOT \"{go.name}\"  filhos_totais={count}  pos={go.transform.position}  ativo={go.activeSelf}");
            LogChildren(go.transform, 2);
        }
    }

    private static void LogChildren(Transform t, int depth)
    {
        string pad = new string(' ', depth * 2);
        foreach (Transform c in t)
        {
            int count = c.GetComponentsInChildren<Transform>(true).Length - 1;
            Debug.Log($"[IFNMG Inspecionar Campus]   {pad}\"{c.name}\"  filhos={count}  pos={c.position}  ativo={c.gameObject.activeSelf}");
            if (depth < 2) LogChildren(c, depth + 1);
        }
    }
}

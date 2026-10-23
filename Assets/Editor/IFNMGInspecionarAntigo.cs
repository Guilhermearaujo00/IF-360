using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// READ-ONLY: lista a estrutura interna do ambiente do mapa (Mapa_IFNMG_antigo_DESATIVADO),
/// mostrando prédios/estruturas e suas posicoes no terreno. Nao altera nada.
///
/// Batch: -executeMethod IFNMGInspecionarAntigo.Inspecionar
/// </summary>
public static class IFNMGInspecionarAntigo
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";

    public static void Inspecionar()
    {
        Scene campus = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);

        GameObject mapa = null;
        foreach (GameObject go in campus.GetRootGameObjects())
            if (go.name == "Mapa_IFNMG_antigo_DESATIVADO") { mapa = go; break; }

        if (mapa == null) { Debug.Log("[IFNMG Antigo] mapa nao encontrado"); return; }

        Debug.Log("[IFNMG Antigo] bounds do mapa: " + CalcBounds(mapa.transform));

        foreach (Transform grupo in mapa.transform)
        {
            Debug.Log("[IFNMG Antigo] GRUPO \"" + grupo.name + "\" filhos=" + grupo.childCount);
            if (grupo.name != "Estruturas") continue;
            foreach (Transform b in grupo)
            {
                Debug.Log($"[IFNMG Antigo]   predio \"{b.name}\"  pos={b.position}  scale={b.localScale}  bounds={CalcBounds(b)}  ativo={b.gameObject.activeSelf}");
                if (b.childCount > 0)
                {
                    foreach (Transform sub in b)
                        Debug.Log($"[IFNMG Antigo]     sub \"{sub.name}\" pos={sub.position}");
                }
            }
        }
    }

    private static Bounds CalcBounds(Transform root)
    {
        Renderer[] rs = root.GetComponentsInChildren<Renderer>(true);
        if (rs == null || rs.Length == 0) return new Bounds(root.position, Vector3.zero);
        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }
}

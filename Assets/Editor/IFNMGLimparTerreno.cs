using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Remove da Campus.unity TODOS os objetos que estao "sobre" o TerrenoBase_500x364m
/// (tiles ladrilhados de superficie: grama, pavimento, solo) e tambem prédios/vegetação
/// gerados pelo protótipo, deixando apenas o plano base liso.
/// Preserva: Player, NPCs, Canvas, GameManager, Systems, Camera, Plane, TelaCarregamento,
///           Mapa_IFNMG_antigo_DESATIVADO (ja desativado) e Modelos_IFNMG (modelos importados).
///
/// Batch: -executeMethod IFNMGLimparTerreno.Limpar
/// </summary>
public static class IFNMGLimparTerreno
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";
    private static readonly HashSet<string> PreservarRaizes = new HashSet<string>
    {
        "Main Camera", "Plane", "NPC_Secretaria", "Player", "GameManager", "Systems",
        "Canvas", "NPC_Diretor", "TelaCarregamento", "Mapa_IFNMG_antigo_DESATIVADO",
        "Modelos_IFNMG"
    };

    [MenuItem("Tools/IFNMG 360/11 - Limpar terreno (remover superficies do prototipo)")]
    public static void LimparMenu()
    {
        if (!EditorUtility.DisplayDialog("Limpar terreno",
            "Remove TODOS os objetos do prototipo que estao sobre o TerrenoBase (superficies ladrilhadas, prédios do prototipo, vegetação do prototipo). Mantem modelos importados e o jogo. Continuar?",
            "Limpar", "Cancelar")) return;
        Limpar();
    }

    public static void Limpar()
    {
        Scene campus = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);

        // 1) Remove o grupo do prototipo inteiro (superficies + predios + vegetacao do prototipo).
        foreach (GameObject go in new List<GameObject>(campus.GetRootGameObjects()))
        {
            if (go.name == "Mapa_IFNMG_Prototipo")
            {
                Object.DestroyImmediate(go);
                Debug.Log("[IFNMG Limpar] Removido 'Mapa_IFNMG_Prototipo' (superficies + predios + vegetacao do prototipo).");
            }
        }

        // 2) Remove qualquer raiz que nao esteja na lista de preservacao (defensivo).
        foreach (GameObject go in new List<GameObject>(campus.GetRootGameObjects()))
        {
            if (!PreservarRaizes.Contains(go.name))
            {
                Debug.LogWarning("[IFNMG Limpar] Raiz nao reconhecida removida: " + go.name);
                Object.DestroyImmediate(go);
            }
        }

        EditorSceneManager.MarkSceneDirty(campus);
        EditorSceneManager.SaveScene(campus, CampusPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[IFNMG Limpar] FIM. Terreno base mantido, prototipo removido.");
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Força o AssetDatabase a reindexar a pasta Assets/IFNMG360 (criada por fora do Editor),
/// importa a cena do protótipo e confirma que ela está indexada.
/// SOMENTE indexação/relatório: não altera a cena nem objetos.
///
/// Menu:  Tools/IFNMG 360/3 - Reindexar pasta IFNMG360 (diagnóstico)
/// Batch: -executeMethod IFNMGReindex.ReindexarBatch
/// </summary>
public static class IFNMGReindex
{
    private const string PASTA = "Assets/IFNMG360";
    private const string CENA = "Assets/IFNMG360/Scenes/IFNMG_Mapa_Prototype_v0_1.unity";

    [MenuItem("Tools/IFNMG 360/3 - Reindexar pasta IFNMG360 (diagnóstico)")]
    public static void ReindexarMenu() => Reindexar();

    public static void ReindexarBatch() => Reindexar();

    public static void Reindexar()
    {
        Debug.Log("[IFNMG Reindex] Iniciando refresh do AssetDatabase...");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.Refresh();

        bool pastaOk = AssetDatabase.IsValidFolder(PASTA);
        bool scenesOk = AssetDatabase.IsValidFolder(PASTA + "/Scenes");
        bool cenaOs = File.Exists(CENA);

        var cenaAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(CENA);
        bool cenaIndexada = cenaAsset != null;

        string guid = AssetDatabase.AssetPathToGUID(CENA);

        Debug.Log($"[IFNMG Reindex] pasta IFNMG360: {pastaOk}");
        Debug.Log($"[IFNMG Reindex] pasta Scenes: {scenesOk}");
        Debug.Log($"[IFNMG Reindex] arquivo no disco: {cenaOs}");
        Debug.Log($"[IFNMG Reindex] cena indexada no AssetDatabase: {cenaIndexada}");
        Debug.Log($"[IFNMG Reindex] GUID: {(string.IsNullOrEmpty(guid) ? "(vazio!)" : guid)}");

        if (cenaIndexada)
            AssetDatabase.ImportAsset(CENA, ImportAssetOptions.ForceUpdate);

        // Lista tudo que o AssetDatabase enxerga sob IFNMG360.
        string[] guids = AssetDatabase.FindAssets("", new[] { PASTA });
        Debug.Log($"[IFNMG Reindex] assets indexados sob IFNMG360: {guids.Length}");
        foreach (var g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            Debug.Log($"[IFNMG Reindex]   - {p}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[IFNMG Reindex] FIM. Resultado: cenaIndexada={cenaIndexada}");
    }
}

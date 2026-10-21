using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// READ-ONLY: lista todas as dependências da cena Campus.unity (e de Menu.unity),
/// agrupadas por pasta de topo em Assets/. Nao altera nada.
/// Serve para decidir quais packs podem ser removidos com segurança.
///
/// Batch: -executeMethod IFNMGMapaAssetsEmUso.Mapear
/// </summary>
public static class IFNMGMapaAssetsEmUso
{
    private static readonly string[] Scenes =
    {
        "Assets/Scenes/Campus.unity",
        "Assets/Scenes/Menu.unity",
    };

    public static void Mapear()
    {
        var porPack = new Dictionary<string, HashSet<string>>();

        foreach (string scene in Scenes)
        {
            if (!File.Exists(scene)) { Debug.LogWarning("[IFNMG EmUso] cena ausente: " + scene); continue; }
            string[] deps = AssetDatabase.GetDependencies(scene, true);
            foreach (string d in deps)
            {
                if (d.StartsWith("Assets/Editor") || d.StartsWith("Assets/Scripts")) continue;
                if (d.EndsWith(".cs") || d.EndsWith(".unity")) continue;
                string rel = d.Substring("Assets/".Length);
                int slash = rel.IndexOf('/');
                string pack = slash >= 0 ? rel.Substring(0, slash) : "(raiz Assets)";
                if (!porPack.ContainsKey(pack)) porPack[pack] = new HashSet<string>();
                porPack[pack].Add(d);
            }
        }

        Debug.Log("[IFNMG EmUso] ===== PACKS USADOS PELA(S) CENA(S) =====");
        foreach (var kv in porPack.OrderByDescending(k => k.Value.Count))
        {
            Debug.Log($"[IFNMG EmUso] PACK_USADO\t{kv.Key}\tarquivos={kv.Value.Count}");
        }

        Debug.Log("[IFNMG EmUso] ===== ARQUIVOS USADOS (detalhe) =====");
        foreach (var kv in porPack.OrderBy(k => k.Key))
            foreach (string f in kv.Value.OrderBy(x => x))
                Debug.Log($"[IFNMG EmUso] USADO\t{kv.Key}\t{f}");
    }
}

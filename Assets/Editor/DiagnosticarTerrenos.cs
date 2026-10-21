using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Diagnostica e corrige TerrainData com "Tree prefab at index N is missing".
/// Isso acontece quando o treePrototypes de um TerrainData referencia um prefab
/// que não existe mais no projeto (PPtr nulo).
///
/// Menu:
///   Tools/IFNMG/Terrenos/Diagnosticar Árvores Faltantes  (só lista, não altera)
///   Tools/IFNMG/Terrenos/Corrigir Árvores Faltantes      (remove protótipos nulos
///       e remapeia as instâncias; não apaga árvores válidas)
///
/// Pode rodar com a cena aberta, FORA do Play Mode.
/// </summary>
public static class DiagnosticarTerrenos
{
    private const string MENU_DIAG = "Tools/IFNMG/Terrenos/Diagnosticar Árvores Faltantes";
    private const string MENU_FIX = "Tools/IFNMG/Terrenos/Corrigir Árvores Faltantes";

    [MenuItem(MENU_DIAG)]
    public static void Diagnosticar()
    {
        var sb = new StringBuilder();
        int total = 0, comProblema = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:TerrainData"))
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>(caminho);
            if (td == null) continue;
            total++;

            TreePrototype[] protos = td.treePrototypes;
            var faltando = new List<int>();
            for (int i = 0; i < protos.Length; i++)
                if (protos[i] == null || protos[i].prefab == null)
                    faltando.Add(i);

            if (faltando.Count == 0) continue;
            comProblema++;

            sb.AppendLine($"[FALTANDO] {caminho}");
            sb.AppendLine($"    protótipos={protos.Length}, instâncias={td.treeInstanceCount}, índices nulos: {string.Join(", ", faltando)}");
            for (int i = 0; i < protos.Length; i++)
            {
                var p = protos[i];
                string nome = (p == null || p.prefab == null) ? "<NULL>" : p.prefab.name;
                sb.AppendLine($"    [{i}] {nome}");
            }
        }

        if (comProblema == 0)
            Debug.Log($"[Terrenos] {total} TerrainData verificados: nenhum protótipo de árvore nulo. Nada a corrigir.");
        else
            Debug.Log($"[Terrenos] {total} TerrainData verificados, {comProblema} com árvores faltantes:\n" + sb);

        EditorUtility.DisplayDialog("Terrenos (diagnóstico)",
            comProblema == 0
                ? $"{total} TerrainData verificados.\nNenhum protótipo de árvore nulo."
                : $"{total} TerrainData verificados.\n{comProblema} com árvores faltantes.\nDetalhes no Console.",
            "OK");
    }

    [MenuItem(MENU_FIX)]
    public static void Corrigir()
    {
        if (!EditorUtility.DisplayDialog("Corrigir árvores faltantes",
            "Vou remover os protótipos de árvore cujo prefab não existe mais e remapear as instâncias.\n\n" +
            "Árvores válidas são preservadas. Continuar?",
            "Corrigir", "Cancelar"))
            return;

        int total = 0, corrigidos = 0, protosRemovidos = 0, instRemovidas = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:TerrainData"))
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>(caminho);
            if (td == null) continue;
            total++;

            TreePrototype[] protos = td.treePrototypes;
            var validos = new List<TreePrototype>();
            var mapa = new int[protos.Length];
            for (int i = 0; i < protos.Length; i++)
            {
                bool ok = protos[i] != null && protos[i].prefab != null;
                if (ok) { mapa[i] = validos.Count; validos.Add(protos[i]); }
                else mapa[i] = -1;
            }
            if (validos.Count == protos.Length) continue;

            TreeInstance[] insts = td.treeInstances;
            var novas = new List<TreeInstance>(insts.Length);
            foreach (var inst in insts)
            {
                int novo = (inst.prototypeIndex >= 0 && inst.prototypeIndex < mapa.Length)
                    ? mapa[inst.prototypeIndex] : -1;
                if (novo < 0) { instRemovidas++; continue; }
                TreeInstance copia = inst;
                copia.prototypeIndex = novo;
                novas.Add(copia);
            }

            int removidos = protos.Length - validos.Count;
            td.treePrototypes = validos.ToArray();
            td.treeInstances = novas.ToArray();
            EditorUtility.SetDirty(td);
            protosRemovidos += removidos;
            corrigidos++;

            Debug.Log($"[Terrenos] {caminho}: removidos {removidos} protótipo(s) nulo(s); instâncias descartadas: {insts.Length - novas.Count}.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Terrenos] {total} TerrainData verificados. Corrigidos: {corrigidos}. Protótipos removidos: {protosRemovidos}. Instâncias descartadas: {instRemovidas}.");
        EditorUtility.DisplayDialog("Terrenos (correção)",
            corrigidos == 0
                ? $"{total} TerrainData verificados.\nNada a corrigir."
                : $"Corrigidos: {corrigidos}\nProtótipos removidos: {protosRemovidos}\nInstâncias descartadas: {instRemovidas}\n\nDetalhes no Console.",
            "OK");
    }
}

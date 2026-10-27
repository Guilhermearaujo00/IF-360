using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// READ-ONLY: diagnostica (a) sub-assets de texturas/materiais dentro dos FBX
/// importados no Unity e (b) nomes/posicoes dos filhos de Mapa_IFNMG_Prototipo/Estruturas.
/// Batch: -executeMethod IFNMGDiagModelos.Diag
/// </summary>
public static class IFNMGDiagModelos
{
    public static void Diag()
    {
        AssetDatabase.Refresh();

        Debug.Log("===== SUB-ASSETS DOS FBX =====");
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Models/IFNMG" });
        int shown = 0;
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            if (!path.ToLower().EndsWith(".fbx")) continue;
            if (shown >= 3) break;
            Object[] subs = AssetDatabase.LoadAllAssetsAtPath(path);
            var texs = subs.Where(o => o is Texture2D).Select(o => o.name + "(" + (((Texture2D)o).width) + "x" + (((Texture2D)o).height) + ")").ToList();
            var mats = subs.Where(o => o is Material).Cast<Material>().ToList();
            Debug.Log("[Diag] " + path + "  textures=" + texs.Count + " materials=" + mats.Count);
            Debug.Log("[Diag]   TEXTURES: " + string.Join(" | ", texs.Take(12)));
            foreach (Material m in mats)
            {
                Texture t = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null;
                Debug.Log("[Diag]   MAT " + m.name + " shader=" + (m.shader != null ? m.shader.name : "null") + " mainTex=" + (t != null ? t.name : "AUSENTE"));
            }
            shown++;
        }

        Debug.Log("===== SLOTS EM Estruturas =====");
        Scene campus = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        GameObject mapa = null;
        foreach (GameObject go in campus.GetRootGameObjects())
            if (go.name == "Mapa_IFNMG_Prototipo") { mapa = go; break; }
        if (mapa == null) { Debug.Log("[Diag] sem mapa"); return; }
        Transform est = null;
        foreach (Transform t in mapa.transform) if (t.name == "Estruturas") { est = t; break; }
        if (est == null) { Debug.Log("[Diag] sem Estruturas"); return; }
        foreach (Transform b in est)
            Debug.Log($"[Diag] slot \"{b.name}\" pos={b.position} filhos={b.childCount}");
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGListarPosicoesPredios
{
    public static void Listar()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity", OpenSceneMode.Single);
        Debug.Log("[IFNMG Posicoes] ===== PREDIOS =====");
        foreach (GameObject root in scene.GetRootGameObjects()) ListarRec(root.transform, "");
    }

    private static void ListarRec(Transform t, string path)
    {
        string p = string.IsNullOrEmpty(path) ? t.name : path + "/" + t.name;
        if (t.parent != null && (t.parent.name == "Modelos_IFNMG" || t.parent.name == "NovasEstruturas_IFNMG"))
            Debug.Log($"[IFNMG Posicoes] {t.name} | grupo={t.parent.name} | pos_global={t.position} | pos_local={t.localPosition} | escala={t.lossyScale}");
        foreach (Transform child in t) ListarRec(child, p);
    }
}

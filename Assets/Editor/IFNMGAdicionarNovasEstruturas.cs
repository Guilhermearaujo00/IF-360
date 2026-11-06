using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGAdicionarNovasEstruturas
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";
    private const string Dir = "Assets/Models/IFNMG/NovasEstruturas";
    private const string GroupName = "NovasEstruturas_IFNMG";

    public static void Adicionar()
    {
        Scene scene = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);
        foreach (GameObject root in new List<GameObject>(scene.GetRootGameObjects()))
            if (root.name == GroupName) Object.DestroyImmediate(root);

        GameObject group = new GameObject(GroupName);
        SceneManager.MoveGameObjectToScene(group, scene);

        Vector3[] positions =
        {
            new Vector3(-215f, 0f, -130f),
            new Vector3(215f, 0f, 130f),
            new Vector3(215f, 0f, -130f)
        };

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { Dir });
        System.Array.Sort(guids, (a, b) => string.Compare(AssetDatabase.GUIDToAssetPath(a), AssetDatabase.GUIDToAssetPath(b), System.StringComparison.OrdinalIgnoreCase));
        int added = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null || added >= positions.Length) continue;

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, group.transform) as GameObject;
            if (instance == null) continue;
            instance.name = Path.GetFileNameWithoutExtension(path);
            instance.transform.position = positions[added];
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                instance.transform.position += Vector3.up * -bounds.min.y;
            }
            added++;
            Debug.Log("[IFNMG NovasEstruturas] adicionado: " + instance.name + " pos=" + instance.transform.position);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, CampusPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[IFNMG NovasEstruturas] total=" + added);
    }
}

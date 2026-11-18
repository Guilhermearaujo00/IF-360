#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGRequestedModelsSetup
{
    private const string ScenePath = "Assets/Scenes/Campus.unity";
    private const string ImportFolder = "Assets/Models/IFNMG_Imported";
    private const string BiologyPath = ImportFolder + "/Estrutura_Biologia_Corrigida.fbx";
    private const string HighSchoolPath = ImportFolder + "/Ensino_Medio_Corrigido.fbx";
    private const string RootName = "Modelos_Solicitados_IF360";

    [MenuItem("Tools/IFNMG 360/Aplicar modelos solicitados")]
    public static void Apply()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = GameObject.Find(RootName) ?? new GameObject(RootName);
        RemovePrevious(root.transform, "Biologia_Solicitada");
        RemovePrevious(root.transform, "Ensino_Medio_Solicitado");

        GameObject biologyAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BiologyPath);
        GameObject highSchoolAsset = AssetDatabase.LoadAssetAtPath<GameObject>(HighSchoolPath);
        GameObject biology = InstantiateModel(biologyAsset, "Biologia_Solicitada", root.transform, new Vector3(0f, 0f, 0f));
        GameObject highSchool = InstantiateModel(highSchoolAsset, "Ensino_Medio_Solicitado", root.transform, new Vector3(0f, 0f, 0f));
        if (biology == null || highSchool == null)
            Debug.LogError("Não foi possível carregar um ou ambos os modelos importados.");

        AddBuildingColliders(scene, root.transform);
        ApplyPavementToBase(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[IFNMG] Modelos, pavimento e colisões aplicados sem remover Player ou NPCs.");
    }

    private static GameObject InstantiateModel(GameObject asset, string name, Transform parent, Vector3 position)
    {
        if (asset == null) return null;
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, 0f));
        AddBoundsCollider(instance);
        return instance;
    }

    private static void RemovePrevious(Transform root, string name)
    {
        Transform old = root.Find(name);
        if (old != null) Object.DestroyImmediate(old.gameObject);
    }

    private static void AddBuildingColliders(Scene scene, Transform requestedRoot)
    {
        foreach (GameObject sceneRoot in scene.GetRootGameObjects())
        {
            foreach (Transform child in sceneRoot.GetComponentsInChildren<Transform>(true))
            {
                if (child == requestedRoot || child.IsChildOf(requestedRoot)) continue;
                if (child.name == "Player" || child.name.StartsWith("NPC_")) continue;
                string n = child.name.ToLowerInvariant();
                if (!(n.Contains("predio") || n.Contains("edificio") || n.Contains("edific") || n.Contains("biblioteca") || n.Contains("estrutura") || n.Contains("bloco"))) continue;
                AddBoundsCollider(child.gameObject);
            }
        }
    }

    private static void AddBoundsCollider(GameObject target)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null) return;
        BoxCollider box = target.GetComponent<BoxCollider>();
        if (box == null) box = target.AddComponent<BoxCollider>();
        Bounds bounds = renderer.bounds;
        box.center = target.transform.InverseTransformPoint(bounds.center);
        box.size = target.transform.InverseTransformVector(bounds.size);
    }

    private static void ApplyPavementToBase(Scene scene)
    {
        Material pavement = AssetDatabase.LoadAssetAtPath<Material>("Assets/IFNMG360/Generated/Materials/IFNMG_Pavimento.mat");
        GameObject ground = GameObject.Find("TerrenoBase_500x364m");
        if (ground == null || pavement == null) return;
        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterials = new[] { pavement };
        ground.name = "TerrenoBase_500x364m_Pavimento";
    }
}
#endif

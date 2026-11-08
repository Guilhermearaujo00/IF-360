#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGCampusSurfaceSetup
{
    private const string ScenePath = "Assets/Scenes/Campus.unity";
    private const string GroundName = "TerrenoBase_500x364m";
    private const string PavementName = "Pavimento_Campus";

    [MenuItem("Tools/IFNMG 360/Aplicar pavimento e colisões")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject ground = GameObject.Find(GroundName);
        if (ground == null)
        {
            Debug.LogError("Terreno não encontrado: " + GroundName);
            return;
        }

        Transform oldPavement = ground.transform.Find(PavementName);
        if (oldPavement != null) Object.DestroyImmediate(oldPavement.gameObject);

        Material pavementMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/IFNMG360/Generated/Materials/IFNMG_Pavimento.mat");
        int grassObjects = ReplaceGrassMaterials(scene, pavementMaterial);
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null && pavementMaterial != null)
        {
            Material[] materials = groundRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++) materials[i] = pavementMaterial;
            groundRenderer.sharedMaterials = materials;
            grassObjects++;
        }

        GameObject pavement = null;

        foreach (MeshCollider meshCollider in Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include))
        {
            if (meshCollider.sharedMesh != null && meshCollider.sharedMesh.name == "Cube.015")
                Object.DestroyImmediate(meshCollider);
        }

        int buildingColliders = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (pavement != null && (child == pavement.transform || child.IsChildOf(pavement.transform))) continue;
                string name = child.name.ToLowerInvariant();
                if (!IsBuilding(child, name)) continue;
                MeshFilter filter = child.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    if (filter.sharedMesh.name == "Cube.015")
                    {
                        MeshCollider largeMeshCollider = child.GetComponent<MeshCollider>();
                        if (largeMeshCollider != null) Object.DestroyImmediate(largeMeshCollider);
                        AddBoundsBoxCollider(child);
                    }
                    else
                    {
                        MeshCollider meshCollider = child.GetComponent<MeshCollider>();
                        if (meshCollider == null) meshCollider = child.gameObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = filter.sharedMesh;
                        meshCollider.convex = false;
                    }
                    buildingColliders++;
                }
                else if (child.GetComponent<Collider>() == null && child.GetComponent<Renderer>() != null)
                {
                    AddBoundsBoxCollider(child);
                    buildingColliders++;
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[IFNMG] Materiais gramados convertidos: " + grassObjects + "; colisores adicionados/atualizados: " + buildingColliders);
    }

    private static int ReplaceGrassMaterials(Scene scene, Material pavementMaterial)
    {
        if (pavementMaterial == null) return 0;
        int count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                string objectName = renderer.gameObject.name.ToLowerInvariant();
                bool grassObject = objectName.Contains("grama") || objectName.Contains("grass") || objectName.Contains("green") || objectName.Contains("cell_");
                if (!grassObject) continue;
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    if (material == null) continue;
                    string materialName = material.name.ToLowerInvariant();
                    if (materialName.Contains("grama") || materialName.Contains("grass") || materialName.Contains("mata") || materialName.Contains("green"))
                    {
                        materials[i] = pavementMaterial;
                        changed = true;
                    }
                }
                if (changed)
                {
                    renderer.sharedMaterials = materials;
                    count++;
                }
            }
        }
        return count;
    }

    private static void AddBoundsBoxCollider(Transform child)
    {
        Renderer renderer = child.GetComponent<Renderer>();
        if (renderer == null) return;
        Bounds bounds = renderer.bounds;
        BoxCollider box = child.GetComponent<BoxCollider>();
        if (box == null) box = child.gameObject.AddComponent<BoxCollider>();
        box.center = child.InverseTransformPoint(bounds.center);
        box.size = child.InverseTransformVector(bounds.size);
    }

    private static bool IsBuilding(Transform child, string name)
    {
        if (name.Contains("predio") || name.Contains("prédio") || name.Contains("edific") ||
            name.Contains("building") || name.Contains("house") || name.Contains("school") ||
            name.Contains("biblioteca") || name.Contains("bloco") || name.Contains("estrutura"))
            return true;

        Transform parent = child.parent;
        while (parent != null)
        {
            string parentName = parent.name.ToLowerInvariant();
            if (parentName.Contains("vias") || parentName.Contains("terreno") ||
                parentName.Contains("vegetacao") || parentName.Contains("vegetação") ||
                parentName.Contains("pavimento"))
                return false;

            if (parentName.Contains("campusgenerated") || parentName.Contains("edificacoes") ||
                parentName.Contains("edificações") || parentName.Contains("construcoes") ||
                parentName.Contains("construções") || parentName.Contains("buildings") ||
                parentName.Contains("mapa_ifnmg"))
                return true;
            parent = parent.parent;
        }

        return false;
    }
}
#endif

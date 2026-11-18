// ============================================================
//  Lead Technical Artist — AAA Physics & Backface Culling Fix
//  Cena-alvo : Campus.unity
//  Undo-safe · Prefab-aware · Render-Pipeline Agnostic
//  Menu: Tools > AAA Fix > Fix Physics & Culling (Campus)
// ============================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class PhysicsAndCullingFix
{
    [MenuItem("Tools/AAA Fix/Fix Physics & Culling (Campus)")]
    public static void Run()
    {
        int undoGroup = 0;
        int collidersAdded = 0;
        int materialsFixed = 0;
        int materialsSkipped = 0;
        int foliageSkipped = 0;
        string pipeline = "Built-in";

        try
        {
            // ── 1. Undo Grouping ──────────────────────────────────────────
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Fix Physics & Culling");
            undoGroup = Undo.GetCurrentGroup();

            // ── 2. Render Pipeline Detection ──────────────────────────────
            var srp = GraphicsSettings.currentRenderPipeline;
            bool isURP = srp != null && srp.GetType().Name.Contains("UniversalRenderPipelineAsset");
            bool isHDRP = srp != null && srp.GetType().Name.Contains("HDRenderPipelineAsset");
            pipeline = isURP ? "URP" : (isHDRP ? "HDRP" : "Built-in");

            Shader builtInTwoSidedShader = null;
            if (!isURP && !isHDRP)
            {
                builtInTwoSidedShader = Shader.Find("IFNMG/Standard Two-Sided");
            }

            // ── 3. Search Optimization ────────────────────────────────────
            var renderers = Object.FindObjectsByType<MeshRenderer>(
                FindObjectsInactive.Exclude);

            int total = renderers.Length;

            var modifiedProjectMaterials = new HashSet<Material>();
            var nativeMaterialInstances = new Dictionary<Material, Material>();

            for (int i = 0; i < total; i++)
            {
                var mr = renderers[i];
                if (mr == null) continue;
                var go = mr.gameObject;

                if (i % 500 == 0 || i == total - 1)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                            "Fix Physics & Culling (AAA)",
                            $"Processando {i + 1}/{total}: {go.name}",
                            (float)i / total))
                    {
                        Debug.LogWarning("[PhysicsCullingFix] Operação cancelada pelo usuário.");
                        break;
                    }
                }

                // ==========================================================
                //  A) PHYSICS — MeshCollider Addition & Prefab Safety
                // ==========================================================
                var mf = go.GetComponent<MeshFilter>();
                bool hasMesh = mf != null && mf.sharedMesh != null;

                if (hasMesh && go.GetComponent<Collider>() == null)
                {
                    // AAA Rigor: Excluir lâminas de vegetação/grama para não estrangular a PhysX
                    bool isFoliage = go.name.StartsWith("S_Grass") ||
                                     (go.transform.parent != null && go.transform.parent.name == "Grama_Alinhada");

                    if (isFoliage)
                    {
                        foliageSkipped++;
                    }
                    else
                    {
                        var mc = Undo.AddComponent<MeshCollider>(go);
                        mc.convex = false;
                        mc.sharedMesh = mf.sharedMesh;

                        if (PrefabUtility.IsPartOfPrefabInstance(go))
                        {
                            PrefabUtility.RecordPrefabInstancePropertyModifications(mc);
                        }

                        collidersAdded++;
                    }
                }

                // ==========================================================
                //  B) BACKFACE CULLING — Pipeline-Agnostic Two-Sided Fix
                // ==========================================================
                var sharedMats = mr.sharedMaterials;
                if (sharedMats == null || sharedMats.Length == 0) continue;

                var newMats = new Material[sharedMats.Length];
                bool rendererMatsChanged = false;

                for (int m = 0; m < sharedMats.Length; m++)
                {
                    var mat = sharedMats[m];
                    if (mat == null)
                    {
                        newMats[m] = null;
                        continue;
                    }

                    string shaderName = mat.shader != null ? mat.shader.name : string.Empty;

                    bool alreadyTwoSided = false;
                    if (isURP)
                    {
                        if (mat.HasProperty("_Cull"))
                            alreadyTwoSided = Mathf.Approximately(mat.GetFloat("_Cull"), 0f);
                    }
                    else
                    {
                        if (shaderName == "IFNMG/Standard Two-Sided" || shaderName == "Custom/S_ShaderTree")
                            alreadyTwoSided = true;
                        else if (mat.HasProperty("_Cull"))
                            alreadyTwoSided = Mathf.Approximately(mat.GetFloat("_Cull"), 0f);
                        else if (mat.IsKeywordEnabled("_DOUBLE_SIDED"))
                            alreadyTwoSided = true;
                    }

                    if (alreadyTwoSided)
                    {
                        newMats[m] = mat;
                        materialsSkipped++;
                        continue;
                    }

                    string assetPath = AssetDatabase.GetAssetPath(mat);
                    bool isNativeUnity = string.IsNullOrEmpty(assetPath) ?
                        (mat.name == "Default-Material" || shaderName.StartsWith("Hidden/") || shaderName.StartsWith("UI/")) :
                        assetPath.StartsWith("Resources/");

                    if (isNativeUnity)
                    {
                        Material inst;
                        if (!nativeMaterialInstances.TryGetValue(mat, out inst))
                        {
                            inst = new Material(mat);
                            inst.name = mat.name + "_TwoSided";

                            if (isURP)
                            {
                                if (inst.HasProperty("_Cull")) inst.SetFloat("_Cull", 0f);
                                inst.EnableKeyword("_DOUBLE_SIDED");
                            }
                            else
                            {
                                if (inst.HasProperty("_Cull")) inst.SetFloat("_Cull", 0f);
                                inst.EnableKeyword("_DOUBLE_SIDED");
                                if (builtInTwoSidedShader != null && shaderName == "Standard")
                                    inst.shader = builtInTwoSidedShader;
                            }

                            nativeMaterialInstances[mat] = inst;
                            materialsFixed++;
                        }

                        newMats[m] = inst;
                        rendererMatsChanged = true;
                    }
                    else
                    {
                        if (!modifiedProjectMaterials.Contains(mat))
                        {
                            Undo.RecordObject(mat, "Fix Culling Material");

                            if (isURP)
                            {
                                if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f);
                                mat.EnableKeyword("_DOUBLE_SIDED");
                            }
                            else
                            {
                                if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f);
                                mat.EnableKeyword("_DOUBLE_SIDED");
                                if (builtInTwoSidedShader != null && shaderName == "Standard")
                                    mat.shader = builtInTwoSidedShader;
                            }

                            EditorUtility.SetDirty(mat);
                            modifiedProjectMaterials.Add(mat);
                            materialsFixed++;
                        }

                        newMats[m] = mat;
                    }
                }

                if (rendererMatsChanged)
                {
                    Undo.RecordObject(mr, "Assign Two-Sided Materials");
                    mr.sharedMaterials = newMats;
                    if (PrefabUtility.IsPartOfPrefabInstance(mr))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(mr);
                    }
                }
            }

            // ── 4. Finalization ───────────────────────────────────────────
            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            string report =
                $"[PhysicsCullingFix] ══ Relatório Final ══\n" +
                $"  Pipeline             : {pipeline}\n" +
                $"  MeshRenderers lidos  : {total}\n" +
                $"  Colliders adicionados: {collidersAdded}\n" +
                $"  Materiais corrigidos : {materialsFixed}\n" +
                $"  Materiais já OK      : {materialsSkipped}\n" +
                $"  Foliage preservada   : {foliageSkipped}\n" +
                $"  Undo (Ctrl+Z)        : 1 grupo único";

            Debug.Log(report);
            EditorUtility.DisplayDialog("Fix Physics & Culling — Concluído", report, "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}

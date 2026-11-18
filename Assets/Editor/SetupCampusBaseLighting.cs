// ============================================================
//  Lead Lighting Artist — Setup Campus Base Lighting
//  Cena-alvo : Campus.unity
//  Undo-safe · LookDev Base · Directional Light & Trilight Ambient
//  Menu: Tools > AAA Fix > Setup Campus Base Lighting
// ============================================================

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class SetupCampusBaseLighting
{
    [MenuItem("Tools/AAA Fix/Setup Campus Base Lighting")]
    public static void Run()
    {
        // ── 1. Undo & Group Setup ─────────────────────────────────────
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Setup Campus Base Lighting");
        int undoGroup = Undo.GetCurrentGroup();

        // Pipeline Detection
        var srp = GraphicsSettings.currentRenderPipeline;
        string pipelineName = (srp != null) ? srp.GetType().Name : "Built-in";

        // ── 2. Find or Create Main Directional Light ─────────────────
        var allLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include);
        Light mainDirLight = null;
        float maxIntensity = -1f;

        foreach (var l in allLights)
        {
            if (l != null && l.type == LightType.Directional)
            {
                if (l.intensity > maxIntensity)
                {
                    maxIntensity = l.intensity;
                    mainDirLight = l;
                }
            }
        }

        bool createdNew = false;
        if (mainDirLight == null)
        {
            var lightGO = new GameObject("Directional Light", typeof(Light));
            mainDirLight = lightGO.GetComponent<Light>();
            mainDirLight.type = LightType.Directional;
            Undo.RegisterCreatedObjectUndo(lightGO, "Create Directional Light");
            createdNew = true;
        }
        else
        {
            Undo.RecordObject(mainDirLight, "Configure Directional Light");
            Undo.RecordObject(mainDirLight.transform, "Rotate Directional Light");
        }

        // Configure Directional Light properties
        mainDirLight.transform.rotation = Quaternion.Euler(35f, 110f, 0f);
        mainDirLight.intensity = 1.25f;
        mainDirLight.color = new Color(1.0f, 0.96f, 0.88f);
        mainDirLight.shadows = LightShadows.Soft;
        mainDirLight.shadowStrength = 0.85f;
        mainDirLight.shadowResolution = LightShadowResolution.High;
        mainDirLight.shadowBias = 0.05f;
        mainDirLight.shadowNormalBias = 0.4f;
        mainDirLight.lightmapBakeType = LightmapBakeType.Realtime;

        EditorUtility.SetDirty(mainDirLight);
        EditorUtility.SetDirty(mainDirLight.transform);

        // ── 3. Environment Lighting (RenderSettings) ──────────────────
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.75f, 0.85f, 1.0f);
        RenderSettings.ambientEquatorColor = new Color(0.90f, 0.84f, 0.76f);
        RenderSettings.ambientGroundColor = new Color(0.22f, 0.22f, 0.25f);

        // ── 4. Update Dynamic GI and Environment Probes ───────────────
        DynamicGI.UpdateEnvironment();

        // Finalize Undo & Scene Save
        Undo.CollapseUndoOperations(undoGroup);
        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        string report =
            $"[SetupCampusBaseLighting] ══ Relatório Final ══\n" +
            $"  Pipeline           : {pipelineName}\n" +
            $"  Directional Light  : {mainDirLight.gameObject.name} ({(createdNew ? "Criada nova" : "Existente")})\n" +
            $"  Rotação            : (35°, 110°, 0°)\n" +
            $"  Intensidade        : {mainDirLight.intensity}\n" +
            $"  Cor Luz Solar      : RGB(1.0, 0.96, 0.88)\n" +
            $"  Sombras            : {mainDirLight.shadows} (Força: {mainDirLight.shadowStrength}, Res: {mainDirLight.shadowResolution})\n" +
            $"  Bias / Normal Bias : {mainDirLight.shadowBias} / {mainDirLight.shadowNormalBias}\n" +
            $"  Bake Type          : {mainDirLight.lightmapBakeType}\n" +
            $"  Ambient Mode       : {RenderSettings.ambientMode}\n" +
            $"  Sky / Eq / Ground  : Sky RGB(0.75, 0.85, 1.0) | Eq RGB(0.90, 0.84, 0.76) | Gnd RGB(0.22, 0.22, 0.25)\n" +
            $"  Undo (Ctrl+Z)      : 1 grupo único";

        Debug.Log(report);
        EditorUtility.DisplayDialog("Setup Campus Base Lighting — Concluído", report, "OK");
    }
}

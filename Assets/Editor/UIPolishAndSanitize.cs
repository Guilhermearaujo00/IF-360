// ============================================================
// Lead UI/UX Programmer — AAA UI Polish & Performance Sanitize
// Scene: Campus.unity
// Restrições: Undo-safe · Raycast Sanitize · Non-destructive TMP
// Menu: Tools > AAA Fix > Polish & Sanitize UI (Canvas)
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIPolishAndSanitize
{
    [MenuItem("Tools/AAA Fix/Polish & Sanitize UI (Canvas)")]
    public static void Run()
    {
        int undoGroup = 0;
        int raycastsDisabled = 0;
        int outlinesAdded = 0;
        int backgroundsStyled = 0;
        int textMaterialsInstanced = 0;

        try
        {
            // ── 1. Undo Grouping ──────────────────────────────────────
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("UI Polish & Performance Sanitize");
            undoGroup = Undo.GetCurrentGroup();

            var canvasGO = GameObject.Find("Canvas");
            if (canvasGO == null)
            {
                var canvasComp = Object.FindAnyObjectByType<Canvas>();
                if (canvasComp != null) canvasGO = canvasComp.gameObject;
            }

            if (canvasGO == null)
            {
                Debug.LogWarning("[UIPolish] Nenhum Canvas encontrado na cena.");
                return;
            }

            // Coletar todos os elementos de UI sob o Canvas
            var allImages = canvasGO.GetComponentsInChildren<Image>(true);
            var allTexts  = canvasGO.GetComponentsInChildren<TextMeshProUGUI>(true);

            int totalElements = allImages.Length + allTexts.Length;
            int currentIndex = 0;

            // Cor de fundo padrão AAA: Dark Navy limpo
            Color navyColor;
            if (!ColorUtility.TryParseHtmlString("#1A1A24F2", out navyColor))
            {
                navyColor = new Color(0.10f, 0.10f, 0.14f, 0.95f);
            }

            Color outlineColor;
            if (!ColorUtility.TryParseHtmlString("#303045E6", out outlineColor))
            {
                outlineColor = new Color(0.19f, 0.19f, 0.27f, 0.90f);
            }

            // ── 2 & 3. Processar Imagens e Painéis de Fundo ───────────
            for (int i = 0; i < allImages.Length; i++)
            {
                var img = allImages[i];
                if (img == null) continue;
                currentIndex++;

                EditorUtility.DisplayProgressBar(
                    "UI Polish & Performance Sanitize (AAA)",
                    $"Otimizando Imagem: {img.gameObject.name} ({currentIndex}/{totalElements})",
                    (float)currentIndex / totalElements);

                var go = img.gameObject;
                string goName = go.name.ToLower();

                // Identificar se é um painel de fundo estrutural
                bool isBackgroundPanel = goName.Contains("caixa") ||
                                         goName.Contains("painel") ||
                                         goName.Contains("fundo") ||
                                         goName.Contains("hud") ||
                                         goName.Contains("background");

                if (isBackgroundPanel)
                {
                    // Estilização segura de fundo
                    Undo.RecordObject(img, "Set Panel Background Color");
                    img.color = navyColor;
                    backgroundsStyled++;
                    EditorUtility.SetDirty(img);

                    // Adicionar Outline se não possuir
                    var existingOutline = go.GetComponent<Outline>();
                    if (existingOutline == null)
                    {
                        var outline = Undo.AddComponent<Outline>(go);
                        outline.effectColor = outlineColor;
                        outline.effectDistance = new Vector2(1.5f, -1.5f);
                        outline.useGraphicAlpha = true;
                        outlinesAdded++;
                        EditorUtility.SetDirty(outline);
                    }
                    else
                    {
                        Undo.RecordObject(existingOutline, "Update Outline");
                        existingOutline.effectColor = outlineColor;
                        existingOutline.effectDistance = new Vector2(1.5f, -1.5f);
                        EditorUtility.SetDirty(existingOutline);
                    }

                    if (PrefabUtility.IsPartOfPrefabInstance(go))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(go);
                    }
                }

                // Sanitização de Raycast: verificar se é puramente visual
                bool isInteractive = IsInteractiveElement(go, img);
                if (!isInteractive && img.raycastTarget)
                {
                    Undo.RecordObject(img, "Disable Raycast Target");
                    img.raycastTarget = false;
                    raycastsDisabled++;
                    EditorUtility.SetDirty(img);

                    if (PrefabUtility.IsPartOfPrefabInstance(img))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(img);
                    }
                }
            }

            // ── 4. Processar TextMeshProUGUI (Avançado & Seguro) ──────
            for (int t = 0; t < allTexts.Length; t++)
            {
                var tmp = allTexts[t];
                if (tmp == null) continue;
                currentIndex++;

                EditorUtility.DisplayProgressBar(
                    "UI Polish & Performance Sanitize (AAA)",
                    $"Otimizando Texto: {tmp.gameObject.name} ({currentIndex}/{totalElements})",
                    (float)currentIndex / totalElements);

                var go = tmp.gameObject;

                // Sanitização de Raycast
                bool isInteractive = IsInteractiveElement(go, tmp);
                if (!isInteractive && tmp.raycastTarget)
                {
                    Undo.RecordObject(tmp, "Disable Raycast Target");
                    tmp.raycastTarget = false;
                    raycastsDisabled++;
                    EditorUtility.SetDirty(tmp);

                    if (PrefabUtility.IsPartOfPrefabInstance(tmp))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(tmp);
                    }
                }

                // TextMeshPro Avançado: instanciar material local para sombra/underlay
                // Nunca modificar fontSharedMaterial base globalmente!
                if (tmp.fontSharedMaterial != null)
                {
                    // Criar instância isolada baseada no material compartilhado
                    var localMat = new Material(tmp.fontSharedMaterial);
                    localMat.name = tmp.fontSharedMaterial.name + "_" + go.name + "_UnderlayInstance";

                    // Habilitar UNDERLAY_ON e configurar sombra
                    localMat.EnableKeyword("UNDERLAY_ON");
                    localMat.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.6f));
                    localMat.SetFloat("_UnderlayOffsetX", 1f);
                    localMat.SetFloat("_UnderlayOffsetY", -1f);
                    localMat.SetFloat("_UnderlayDilate", 0.15f);
                    localMat.SetFloat("_UnderlaySoftness", 0.25f);

                    Undo.RecordObject(tmp, "Set Local Font Material Underlay");
                    tmp.fontMaterial = localMat;
                    textMaterialsInstanced++;
                    EditorUtility.SetDirty(tmp);

                    if (PrefabUtility.IsPartOfPrefabInstance(tmp))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(tmp);
                    }
                }
            }

            // ── 5. Finalização & Marcação de Dirty ─────────────────────
            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            string report =
                $"[UIPolishAndSanitize] ══ Relatório Final ══\n" +
                $"  Elementos analisados     : {totalElements}\n" +
                $"  Raycasts desativados     : {raycastsDisabled}\n" +
                $"  Painéis com fundo navy   : {backgroundsStyled}\n" +
                $"  Outlines adicionados     : {outlinesAdded}\n" +
                $"  Materiais TMP instanciados: {textMaterialsInstanced}\n" +
                $"  Undo (Ctrl+Z)            : 1 grupo único";

            Debug.Log(report);
            EditorUtility.DisplayDialog("UI Polish & Performance — Concluído", report, "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    /// <summary>
    /// Determina com precisão se o elemento participa de interações de ponteiro.
    /// </summary>
    private static bool IsInteractiveElement(GameObject go, Graphic graphic)
    {
        // 1. Possui componente Selectable (Button, Toggle, Slider, InputField, etc.)
        if (go.GetComponent<Selectable>() != null) return true;

        // 2. Está sob um Selectable e atua como seu targetGraphic ou label
        var parentSelectable = go.GetComponentInParent<Selectable>();
        if (parentSelectable != null)
        {
            if (parentSelectable.targetGraphic == graphic) return true;
            // Se for botão, manter raycast apenas no alvo principal
        }

        // 3. Possui tratador de eventos ou gatilho
        if (go.GetComponent<IPointerClickHandler>() != null) return true;
        if (go.GetComponent<IPointerDownHandler>() != null) return true;
        if (go.GetComponent<IPointerUpHandler>() != null) return true;
        if (go.GetComponent<EventTrigger>() != null) return true;

        // 4. Possui colisor 2D/3D associado a input
        if (go.GetComponent<Collider2D>() != null || go.GetComponent<Collider>() != null) return true;

        return false;
    }
}

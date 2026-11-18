using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Etapa 3 do plano: prepara os 12 NPCs para animar (Humanoid + controladores).
///
/// O que faz (idempotente — pode rodar mais de uma vez):
///   - Reimporta os FBX como Humanoid: os 12 modelos, os 12 idles (com loop)
///     e o WAVING (sem loop — o aceno acontece uma vez);
///   - Cria um AnimatorController por NPC em Assets/NPCs/Animadores
///     (Anim_{ARQUIVO}.controller):
///       - Layer 0 'Base Layer': estado 'Idle' com o clip individual em loop;
///       - Layer 1 'Waving': estado 'Neutro' (vazio) + 'Waving' (clip WAVING);
///         o trigger 'Wave' entra no aceno; volta ao Neutro ao término do
///         clipe (exit time ~0.95) ou quando 'IsTalking' fica true (diálogo);
///   - Abre a Campus.unity e aplica controller + avatar em cada 'NPC_{Nome}'
///     (criados pelo 'Tools > IFNMG 360 > 2 - Posicionar NPCs no Campus').
///
/// Executar via: Tools > IFNMG 360 > 3 - Preparar Animacoes dos NPCs
/// </summary>
public static class NPCAnimatorSetup
{
    private const string Menu = "Tools/IFNMG 360/3 - Preparar Animacoes dos NPCs";
    private const string CaminhoCampus = "Assets/Scenes/Campus.unity";

    private const string PastaModelos = "Assets/NPCs/Modelos";
    private const string PastaAnimacoes = "Assets/NPCs/Animacoes";
    private const string PastaControladores = "Assets/NPCs/Animadores";

    // (nome exibido na cena, código do arquivo, lado do clip de idle)
    private static readonly (string nome, string arquivo, char lado)[] Npcs =
    {
        ("Ananda", "ANANDA", 'R'),
        ("Roney", "RONEY", 'L'),
        ("Thiago", "THIAGO", 'L'),
        ("JP", "JP", 'L'),
        ("Gleice", "GLEICE", 'L'),
        ("Rosy", "ROSY", 'L'),
        ("Josy", "JOSY", 'R'),
        ("David", "DAVI", 'L'),
        ("Márcio", "MARCO", 'L'),
        ("João", "JOAO", 'L'),
        ("Cláudio", "CLAUDIO", 'L'),
        ("Alice", "ALICE", 'R')
    };

    [MenuItem(Menu)]
    public static void Preparar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Preparar Animacoes",
                "Saia do Play Mode antes de preparar as animações.", "OK");
            return;
        }

        AssetDatabase.Refresh();

        if (!AssetDatabase.IsValidFolder(PastaControladores))
        {
            AssetDatabase.CreateFolder("Assets/NPCs", "Animadores");
        }

        // 1) Importadores: Humanoid em tudo; idles em loop; WAVING pontual.
        ConfigurarImportador($"{PastaAnimacoes}/WAVING.fbx", loop: false);

        foreach ((string _, string arquivo, char lado) in Npcs)
        {
            ConfigurarImportador($"{PastaModelos}/{arquivo}_mixamo.fbx", loop: false);
            ConfigurarImportador($"{PastaAnimacoes}/{arquivo}_mixamo@Catwalk Idle Twist {lado}.fbx", loop: true);
        }

        // 2) Controladores (um por NPC; não substitui os já existentes).
        int criados = 0, existentes = 0;
        foreach ((string _, string arquivo, char lado) in Npcs)
        {
            string caminho = $"{PastaControladores}/Anim_{arquivo}.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(caminho) != null)
            {
                existentes++;
                continue;
            }

            if (CriarController(caminho, arquivo, lado) != null) criados++;
        }

        AssetDatabase.SaveAssets();

        // 3) Aplicar aos NPCs da cena.
        Scene cena = EditorSceneManager.OpenScene(CaminhoCampus, OpenSceneMode.Single);
        int aplicados = 0;
        foreach ((string nome, string arquivo, char _) in Npcs)
        {
            GameObject npc = GameObject.Find("NPC_" + nome);
            if (npc == null)
            {
                Debug.LogWarning($"[IFNMG 360] 'NPC_{nome}' não está na cena — " +
                                  "rode o 'Tools > IFNMG 360 > 2 - Posicionar NPCs no Campus' antes.");
                continue;
            }

            Animator animator = npc.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                Debug.LogWarning($"[IFNMG 360] 'NPC_{nome}' sem Animator (FBX sem esqueleto?).");
                continue;
            }

            AnimatorController controlador = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                $"{PastaControladores}/Anim_{arquivo}.controller");
            if (controlador == null)
            {
                Debug.LogWarning($"[IFNMG 360] Controlador de 'NPC_{nome}' não foi criado — veja erros acima.");
                continue;
            }

            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath($"{PastaModelos}/{arquivo}_mixamo.fbx")
                .OfType<Avatar>().FirstOrDefault();

            Undo.RecordObject(animator, "Aplicar animador");
            animator.runtimeAnimatorController = controlador;
            if (avatar != null) animator.avatar = avatar;
            animator.applyRootMotion = false;
            EditorUtility.SetDirty(animator);
            aplicados++;
        }

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);

        Debug.Log($"[IFNMG 360] Animacoes preparadas: 12 NPCs importados como Humanoid, " +
                  $"{criados} controladores criados ({existentes} já existiam), " +
                  $"{aplicados} NPCs configurados na cena.");
    }

    // ---------- Importadores ----------

    private static bool ConfigurarImportador(string caminho, bool loop)
    {
        ModelImporter importador = AssetImporter.GetAtPath(caminho) as ModelImporter;
        if (importador == null)
        {
            Debug.LogError($"[IFNMG 360] FBX não encontrado (ou sem ModelImporter): {caminho}");
            return false;
        }

        importador.animationType = ModelImporterAnimationType.Human;

        if (loop)
        {
            ModelImporterClipAnimation[] clipes = importador.clipAnimations;
            if (clipes == null || clipes.Length == 0) clipes = importador.defaultClipAnimations;
            if (clipes != null)
            {
                foreach (ModelImporterClipAnimation clipe in clipes) clipe.loopTime = true;
                importador.clipAnimations = clipes;
            }
        }

        importador.SaveAndReimport();
        return true;
    }

    // ---------- Controladores ----------

    private static AnimatorController CriarController(string caminho, string arquivo, char lado)
    {
        AnimationClip clipIdle = CarregarClipe($"{PastaAnimacoes}/{arquivo}_mixamo@Catwalk Idle Twist {lado}.fbx");
        AnimationClip clipAceno = CarregarClipe($"{PastaAnimacoes}/WAVING.fbx");
        if (clipIdle == null || clipAceno == null) return null;

        AnimatorController controlador = AnimatorController.CreateAnimatorControllerAtPath(caminho);

        // Layer 0 (base): idle individual em loop.
        AnimatorControllerLayer camadaBase = controlador.layers[0];
        AnimatorState estadoIdle = camadaBase.stateMachine.AddState("Idle");
        estadoIdle.motion = clipIdle;
        camadaBase.stateMachine.defaultState = estadoIdle;

        // Parâmetros usados pelo NPCAnimacao.
        controlador.AddParameter("Wave", AnimatorControllerParameterType.Trigger);
        controlador.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);

        // Layer 1: aceno compartilhado (WAVING) por cima do idle.
        // AddLayer(string) retorna void; recupera a camada pelo índice e
        // reescreve o array 'layers' para aplicar o peso (padrão da API).
        controlador.AddLayer("Waving");
        AnimatorControllerLayer[] camadas = controlador.layers;
        int idxAceno = camadas.Length - 1;
        camadas[idxAceno].defaultWeight = 1f;
        controlador.layers = camadas;
        AnimatorControllerLayer camadaAceno = camadas[idxAceno];

        AnimatorState neutro = camadaAceno.stateMachine.AddState("Neutro");
        AnimatorState acenando = camadaAceno.stateMachine.AddState("Waving");
        acenando.motion = clipAceno;
        camadaAceno.stateMachine.defaultState = neutro;

        AnimatorStateTransition paraAceno = neutro.AddTransition(acenando);
        paraAceno.AddCondition(AnimatorConditionMode.If, 0f, "Wave");
        paraAceno.hasExitTime = false;
        paraAceno.duration = 0.05f;

        // Volta ao neutro quando o clipe termina...
        AnimatorStateTransition aoTerminar = acenando.AddTransition(neutro);
        aoTerminar.hasExitTime = true;
        aoTerminar.exitTime = 0.95f;
        aoTerminar.duration = 0.1f;

        // ...ou imediatamente, se um diálogo começar no meio do aceno.
        AnimatorStateTransition aoFalar = acenando.AddTransition(neutro);
        aoFalar.AddCondition(AnimatorConditionMode.If, 0f, "IsTalking");
        aoFalar.hasExitTime = false;
        aoFalar.duration = 0.1f;

        EditorUtility.SetDirty(controlador);
        return controlador;
    }

    private static AnimationClip CarregarClipe(string caminho)
    {
        AnimationClip clipe = AssetDatabase.LoadAllAssetsAtPath(caminho)
            .OfType<AnimationClip>().FirstOrDefault();
        if (clipe == null)
        {
            Debug.LogError($"[IFNMG 360] Clipe de animação não encontrado em: {caminho}");
        }
        return clipe;
    }
}

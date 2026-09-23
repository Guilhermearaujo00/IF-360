using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CaioSetupTools
{
    private const string CaminhoModelo = "Assets/Modelos/Personagens/Caio_LowPoly_Corrigido.fbx";
    private static readonly string[] CaminhosAnimacoes =
    {
        "Assets/Modelos/Personagens/Animacoes/CaioParado.fbx",
        "Assets/Modelos/Personagens/Animacoes/CaioImpulsoSkate.fbx",
        "Assets/Modelos/Personagens/Animacoes/CaioSobreoSkate.fbx"
    };
    private const string CaminhoController = "Assets/Modelos/Personagens/Animacoes/AnimCtrSkater.controller";

    [MenuItem("Tools/IFNMG/Configurar Caio (Rig + Animator)")]
    public static void Configurar()
    {
        ConfigurarRigHumanoid();
        CriarController();
        AtribuirNaCena();
    }

    [MenuItem("Tools/IFNMG/Adicionar 'Empurrar' (Shift)")]
    public static void AdicionarEmpurrar()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoController);
        if (controller == null)
        {
            Debug.LogWarning("Controller não encontrado em " + CaminhoController + ". Rode primeiro 'Configurar Caio'.");
            return;
        }

        bool temParametro = controller.parameters.Any(p => p.name == "Correndo");
        if (!temParametro)
        {
            controller.AddParameter("Correndo", AnimatorControllerParameterType.Bool);
        }

        AnimatorStateMachine maquina = controller.layers[0].stateMachine;

        AnimatorState empurrar = ObterEstado(maquina, "Empurrar");
        if (empurrar == null)
        {
            empurrar = maquina.AddState("Empurrar", new Vector3(600f, 0f, 0f));
        }

        AnimationClip clipImpulso = PrimeiroClip(CaminhosAnimacoes[1]);
        if (clipImpulso != null)
        {
            empurrar.motion = clipImpulso;
        }

        AnimatorState sobre = ObterEstado(maquina, "Sobre o Skate");

        if (sobre != null)
        {
            AnimatorStateTransition entrar = sobre.AddTransition(empurrar);
            entrar.hasExitTime = false;
            entrar.duration = 0.1f;
            entrar.AddCondition(AnimatorConditionMode.If, 0f, "Correndo");

            AnimatorStateTransition voltar = empurrar.AddTransition(sobre);
            voltar.hasExitTime = false;
            voltar.duration = 0.1f;
            voltar.AddCondition(AnimatorConditionMode.IfNot, 0f, "Correndo");
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("Estado 'Empurrar' (clip Impulso) e parâmetro 'Correndo' (Bool) adicionados.");
    }

    private static AnimatorState ObterEstado(AnimatorStateMachine maquina, string nome)
    {
        return maquina.states.FirstOrDefault(c => c.state.name == nome).state;
    }

    [MenuItem("Tools/IFNMG/Listar Scripts Faltando")]
    public static void ListarScriptsFaltando()
    {
        int encontrados = 0;

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
        {
            foreach (MonoBehaviour comp in go.GetComponents<MonoBehaviour>())
            {
                if (comp == null)
                {
                    Debug.LogWarning("Script faltando em: " + go.name + " (caminho: " + CaminhoDoObjeto(go) + ")", go);
                    encontrados++;
                }
            }
        }

        if (encontrados == 0)
        {
            Debug.Log("Nenhum script faltando na cena.");
        }
        else
        {
            Debug.Log("Total de scripts faltando: " + encontrados);
        }
    }

    private static string CaminhoDoObjeto(GameObject go)
    {
        string caminho = go.name;
        Transform atual = go.transform;
        while (atual.parent != null)
        {
            atual = atual.parent;
            caminho = atual.name + "/" + caminho;
        }
        return caminho;
    }

    [MenuItem("Tools/IFNMG/Ativar Loop nas animações")]
    public static void AtivarLoop()
    {
        foreach (string caminho in CaminhosAnimacoes)
        {
            ModelImporter importer = AssetImporter.GetAtPath(caminho) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning("Importer não encontrado: " + caminho);
                continue;
            }

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning("Nenhum clip encontrado em " + caminho);
                continue;
            }

            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].loopTime = true;
                clips[i].loopPose = true;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            Debug.Log("Loop ativado em: " + caminho);
        }

        ReatribuirMotions();
        AssetDatabase.SaveAssets();

        Debug.Log("Loop nas animações ativado e motions reatribuídos.");
    }

    private static void ReatribuirMotions()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoController);
        if (controller == null)
        {
            Debug.LogWarning("Controller não encontrado em " + CaminhoController);
            return;
        }

        AnimationClip clipParado = PrimeiroClip(CaminhosAnimacoes[0]);
        AnimationClip clipImpulso = PrimeiroClip(CaminhosAnimacoes[1]);
        AnimationClip clipSobre = PrimeiroClip(CaminhosAnimacoes[2]);

        AnimatorStateMachine maquina = controller.layers[0].stateMachine;
        AtribuirMotion(maquina, "Idle", clipParado);
        AtribuirMotion(maquina, "Sobre o Skate", clipSobre);
        AtribuirMotion(maquina, "Empurrar", clipImpulso);

        EditorUtility.SetDirty(controller);
    }

    private static void AtribuirMotion(AnimatorStateMachine maquina, string nome, AnimationClip clip)
    {
        AnimatorState estado = ObterEstado(maquina, nome);
        if (estado != null && clip != null)
        {
            estado.motion = clip;
        }
    }

    private static void ConfigurarRigHumanoid()
    {
        string[] caminhos = new string[] { CaminhoModelo }.Concat(CaminhosAnimacoes).ToArray();

        foreach (string caminho in caminhos)
        {
            ModelImporter importer = AssetImporter.GetAtPath(caminho) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning("Importer não encontrado: " + caminho);
                continue;
            }

            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.SaveAndReimport();
                Debug.Log("Rig configurado como Humanoid: " + caminho);
            }
            else
            {
                Debug.Log("Rig já é Humanoid: " + caminho);
            }
        }

        AssetDatabase.Refresh();
    }

    private static AnimationClip PrimeiroClip(string caminhoFbx)
    {
        AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(caminhoFbx)
            .OfType<AnimationClip>()
            .FirstOrDefault();

        if (clip == null)
        {
            Debug.LogError("Nenhum clip de animação encontrado em " + caminhoFbx);
        }

        return clip;
    }

    private static void CriarController()
    {
        AnimationClip clipParado = PrimeiroClip(CaminhosAnimacoes[0]);
        AnimationClip clipImpulso = PrimeiroClip(CaminhosAnimacoes[1]);
        AnimationClip clipSobre = PrimeiroClip(CaminhosAnimacoes[2]);

        Debug.Log(
            "Clips: Parado=" + (clipParado ? clipParado.name : "ausente") +
            " | Impulso=" + (clipImpulso ? clipImpulso.name : "ausente") +
            " | Sobre o skate=" + (clipSobre ? clipSobre.name : "ausente"));

        if (clipParado == null || clipSobre == null)
        {
            Debug.LogError("É necessário ao menos os clips Parado e Sobre o Skate. Abortando.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoController) != null)
        {
            AssetDatabase.DeleteAsset(CaminhoController);
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(CaminhoController);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Pular", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine maquina = controller.layers[0].stateMachine;

        AnimatorState idle = maquina.states.Length > 0 ? maquina.states[0].state : maquina.AddState("Idle", Vector3.zero);
        idle.name = "Idle";
        idle.motion = clipParado;

        AnimatorState sobre = maquina.AddState("Sobre o Skate", new Vector3(300f, 0f, 0f));
        sobre.motion = clipSobre;

        AnimatorStateTransition entrar = idle.AddTransition(sobre);
        entrar.hasExitTime = false;
        entrar.duration = 0.15f;
        entrar.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        AnimatorStateTransition sair = sobre.AddTransition(idle);
        sair.hasExitTime = false;
        sair.duration = 0.15f;
        sair.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("AnimCtrSkater criado: " + CaminhoController);
        Debug.Log("Estado 'Pular' fica no gatilho para o clip de Ollie (ainda não importado).");
    }

    private static void AtribuirNaCena()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoController);
        if (controller == null)
        {
            Debug.LogError("Controller não encontrado em " + CaminhoController);
            return;
        }

        GameObject alvo = GameObject.Find("Caio_LowPoly_Corrigido");
        if (alvo == null)
        {
            alvo = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)
                .Select(t => t.gameObject)
                .FirstOrDefault(g => g.name.Contains("Caio"));
        }

        if (alvo == null)
        {
            Debug.LogWarning("Não encontrou o GameObject do Caio na cena. Atribua o controller manualmente.");
            return;
        }

        Animator animator = alvo.GetComponent<Animator>();
        if (animator == null)
        {
            animator = alvo.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        EditorUtility.SetDirty(alvo);
        Debug.Log("Animator configurado em: " + alvo.name + " (applyRootMotion = false)");
    }
}
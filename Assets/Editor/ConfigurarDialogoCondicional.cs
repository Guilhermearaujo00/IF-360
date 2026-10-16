using IF360;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Configura o par de NPCs "Secretaria" + "Diretor" para demonstrar o DIÁLOGO
/// CONDICIONAL (ver seção 19 de Docs/ARQUITETURA_DIALOGO.md):
///
///   - Diretor: ao terminar a conversa marca a flag 'conversou_npc_diretor'
///     (automático) e completa a missão 'missao_diretor'.
///   - Secretaria: usa VARIANTES de diálogo. A conversa de "finalizado" só
///     aparece depois que o jogador falou com o Diretor (requisitoFlag). Antes
///     disso, repete a conversa de espera (variante fallback sem condição).
///
/// Fluxo esperado:
///   Secretaria (espera) -> Diretor (confirma + flag) -> Secretaria (finalizado).
///
/// É idempotente: pode ser executada novamente sem duplicar nada.
/// </summary>
public static class ConfigurarDialogoCondicional
{
    private const string MENU =
        "Tools/IFNMG/Configurar NPCs Secretaria e Diretor (Dialogo Condicional)";

    private const string FLAG_DIRETOR = "conversou_npc_diretor";
    private const string FLAG_SECRETARIA = "conversou_npc_secretaria";

    [MenuItem(MENU)]
    public static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Dialogo Condicional",
                "Saia do Play Mode antes de configurar os NPCs.", "OK");
            return;
        }

        NPCBaseNovo diretor = EncontrarNPC("NPC_Diretor");
        NPCBaseNovo secretaria = EncontrarNPC("NPC_Secretaria");

        if (diretor == null && secretaria == null)
        {
            EditorUtility.DisplayDialog("Dialogo Condicional",
                "Não encontrei 'NPC_Diretor' nem 'NPC_Secretaria' na cena aberta.\n" +
                "Abra a cena Campus e tente de novo.", "OK");
            return;
        }

        DialogoData dirPre = EncontrarDialogoDataPorNome("Dialogo_Diretor_Pre");
        DialogoData secPre = EncontrarDialogoDataPorNome("Dialogo_Secretaria_Pre");
        DialogoData secPos = EncontrarDialogoDataPorNome("Dialogo_Secretaria_Pos");

        if (diretor != null)
        {
            Undo.RecordObject(diretor, "Configurar Diretor");
            diretor.nomeNPC = "Diretor";
            if (string.IsNullOrEmpty(diretor.idMissao)) diretor.idMissao = "missao_diretor";
            if (dirPre != null) diretor.dialogoData = dirPre;
            diretor.completarMissaoAoTerminar = true;
            diretor.chamarMinijogo = false;
            diretor.registrarConversa = true;
            diretor.idFlagConversa = FLAG_DIRETOR;
            EditorUtility.SetDirty(diretor);
        }

        if (secretaria != null)
        {
            Undo.RecordObject(secretaria, "Configurar Secretaria");
            secretaria.nomeNPC = "Secretaria";
            if (string.IsNullOrEmpty(secretaria.idMissao)) secretaria.idMissao = "missao_diretor";
            secretaria.missaoRequisito = "missao_diretor";
            if (secPre != null) secretaria.dialogoData = secPre;
            if (secPos != null) secretaria.dialogoDataPosMissao = secPos;
            secretaria.completarMissaoAoTerminar = false;
            secretaria.chamarMinijogo = true;
            secretaria.registrarConversa = true;
            secretaria.idFlagConversa = FLAG_SECRETARIA;

            // Variantes avaliadas em ordem: vence a PRIMEIRA cujas condições batem.
            // A última (sem condição) é o fallback que "repete".
            secretaria.variantes = new VarianteDialogo[]
            {
                new VarianteDialogo { dialogo = secPos, requisitoFlag = FLAG_DIRETOR },
                new VarianteDialogo { dialogo = secPre }
            };
            EditorUtility.SetDirty(secretaria);
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("ConfigurarDialogoCondicional: NPCs ajustados.\n" +
                  "  Fluxo: Secretaria (espera) -> Diretor (confirma + flag '" + FLAG_DIRETOR +
                  "') -> Secretaria (finalizado).\n" +
                  "  Salve a cena (Ctrl+S) para gravar.");
    }

    private static NPCBaseNovo EncontrarNPC(string nome)
    {
        GameObject go = GameObject.Find(nome);
        return go != null ? go.GetComponent<NPCBaseNovo>() : null;
    }

    private static DialogoData EncontrarDialogoDataPorNome(string nome)
    {
        string[] guids = AssetDatabase.FindAssets("t:DialogoData");
        foreach (string guid in guids)
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            DialogoData so = AssetDatabase.LoadAssetAtPath<DialogoData>(caminho);
            if (so != null && so.name == nome) return so;
        }

        Debug.LogWarning("ConfigurarDialogoCondicional: asset de diálogo '" + nome +
                         "' não encontrado em Assets/.");
        return null;
    }
}

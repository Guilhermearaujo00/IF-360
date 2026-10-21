using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MESCLA o ambiente do protótipo IFNMG360 dentro da cena principal Campus.unity.
///
/// Estratégia (reversível e segura):
///   1) Backup extra do .unity antes de qualquer alteração.
///   2) O ambiente ANTIGO (raiz "Mapa_IFNMG") NÃO é apagado: é renomeado para
///      "Mapa_IFNMG_antigo_DESATIVADO" e desativado (m_active=false). Reversível.
///   3) O ambiente do protótipo é copiado da cena IFNMG_Mapa_Prototype_v0_1.unity
///      para dentro do Campus, agrupado sob "Mapa_IFNMG_Prototipo".
///   4) Tudo que é do JOGO (Player, NPCs, Canvas, GameManager, Systems, Camera,
///      TelaCarregamento, Plane) é PRESERVADO intacto.
///
/// Menu:  Tools/IFNMG 360/9 - Mesclar protótipo na Campus
/// Batch: -executeMethod IFNMG_MesclarCampusNoPrototipo.Mesclar
/// </summary>
public static class IFNMG_MesclarCampusNoPrototipo
{
    private const string CampusPath = "Assets/Scenes/Campus.unity";
    private const string PrototypePath = "Assets/IFNMG360/Scenes/IFNMG_Mapa_Prototype_v0_1.unity";
    private const string OldRootName = "Mapa_IFNMG";
    private const string DisabledName = "Mapa_IFNMG_antigo_DESATIVADO";
    private const string NewRootName = "Mapa_IFNMG_Prototipo";

    [MenuItem("Tools/IFNMG 360/9 - Mesclar protótipo na Campus")]
    public static void MesclarMenu()
    {
        if (!EditorUtility.DisplayDialog("Mesclar protótipo na Campus",
            "Isso vai: (1) desativar o ambiente antigo 'Mapa_IFNMG' (reversível), (2) inserir o ambiente do protótipo na Campus.\n\nA cena principal será modificada. Continuar?",
            "Mesclar", "Cancelar")) return;
        Mesclar();
    }

    public static void Mesclar()
    {
        string absCampus = Path.Combine(Directory.GetCurrentDirectory(), CampusPath);
        string backup = absCampus + ".backup_mescla_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        if (File.Exists(absCampus)) File.Copy(absCampus, backup, true);
        Debug.Log("[IFNMG Mescla] Backup criado: " + backup);

        Scene campus = EditorSceneManager.OpenScene(CampusPath, OpenSceneMode.Single);

        // 1) Desativa (não apaga) o ambiente antigo.
        GameObject oldRoot = null;
        foreach (GameObject go in campus.GetRootGameObjects())
            if (go.name == OldRootName) { oldRoot = go; break; }

        if (oldRoot == null)
            Debug.LogWarning("[IFNMG Mescla] Não encontrei a raiz '" + OldRootName + "' na Campus. Seguindo sem desativar ambiente antigo.");
        else
        {
            oldRoot.name = DisabledName;
            oldRoot.SetActive(false);
            Debug.Log("[IFNMG Mescla] Ambiente antigo renomeado para '" + DisabledName + "' e desativado.");
        }

        // 2) Remove raiz de protótipo existente (idempotência).
        foreach (GameObject go in campus.GetRootGameObjects())
            if (go.name == NewRootName) { Object.DestroyImmediate(go); Debug.Log("[IFNMG Mescla] Removida raiz de protótipo anterior."); }

        // 3) Abre a cena do protótipo e INSTANCIA suas raízes dentro da Campus.
        Scene proto = EditorSceneManager.OpenScene(PrototypePath, OpenSceneMode.Additive);
        GameObject container = new GameObject(NewRootName);
        container.transform.position = Vector3.zero;
        SceneManager.MoveGameObjectToScene(container, campus);

        int moved = 0;
        if (proto.IsValid() && proto.isLoaded)
        {
            GameObject protoRoot = null;
            foreach (GameObject go in proto.GetRootGameObjects())
                if (go.name == "IFNMG_Mapa_Prototype_v0_1") { protoRoot = go; break; }

            if (protoRoot == null)
                Debug.LogError("[IFNMG Mescla] Não encontrei a raiz 'IFNMG_Mapa_Prototype_v0_1' na cena do protótipo.");
            else
            {
                Transform[] children = new Transform[protoRoot.transform.childCount];
                for (int i = 0; i < protoRoot.transform.childCount; i++) children[i] = protoRoot.transform.GetChild(i);
                foreach (Transform child in children)
                {
                    GameObject copy = Object.Instantiate(child.gameObject, container.transform);
                    copy.name = child.name;
                    copy.transform.position = child.position;
                    copy.transform.rotation = child.rotation;
                    copy.transform.localScale = child.localScale;
                    moved++;
                }
                Debug.Log("[IFNMG Mescla] " + moved + " grupos do protótipo instanciados sob '" + NewRootName + "'.");
            }
        }
        EditorSceneManager.CloseScene(proto, true);

        // 4) Salva a Campus.
        EditorSceneManager.MarkSceneDirty(campus);
        EditorSceneManager.SaveScene(campus, CampusPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[IFNMG Mescla] FIM. Campus salva com ambiente do protótipo. Mapa antigo preservado como '" + DisabledName + "'.");
    }
}

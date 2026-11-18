using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGAlinharRoad014
{
    private const string CENA = "Assets/Scenes/Campus.unity";
    private const int COPIA_MIN = 1;
    private const int COPIA_MAX = 24;
    private const float ESPACAMENTO_PADRAO = 21.2828f;
    private const float ESCALA_PADRAO = 1.6371387f;
    private const float Y_ALTURA_PADRAO = 0f;

    private static readonly Quaternion ROTACAO_PADRAO = Quaternion.Euler(0f, 224.503f, 0f);

    [MenuItem("Tools/IFNMG 360/Alinhar Road_014 (1 a 24)")]
    public static void Executar()
    {
        try
        {
            ExecutarInterno();
        }
        catch (Exception ex)
        {
            Debug.LogError("[Road014] FALHA: " + ex);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else throw;
        }
    }

    private static void ExecutarInterno()
    {
        string cenaFull = Path.GetFullPath(CENA);
        if (!File.Exists(cenaFull))
            throw new FileNotFoundException("Cena nao encontrada", cenaFull);

        string backupDir = Path.GetFullPath("Backups");
        Directory.CreateDirectory(backupDir);
        string backup = Path.Combine(backupDir, "Campus.before-road014-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity");
        File.Copy(cenaFull, backup, true);
        Debug.Log("[Road014] Backup criado: " + backup);

        Scene cena = EditorSceneManager.OpenScene(CENA, OpenSceneMode.Single);

        // Posicao inicial preservada: Road_014 (1)
        GameObject go1 = EncontrarPorNomeEmCena("Road_014 (1)");
        if (go1 == null)
            throw new Exception("Objeto Road_014 (1) nao encontrado na cena.");

        // Garantir y=0 para o segmento 1
        Vector3 pos1 = go1.transform.position;
        pos1.y = Y_ALTURA_PADRAO;
        go1.transform.position = pos1;
        Debug.Log("[Road014] Posicao inicial preservada em y=0: " + pos1.ToString("0.###"));

        int alinhados = 0;
        for (int n = COPIA_MIN; n <= COPIA_MAX; n++)
        {
            string nome = "Road_014 (" + n + ")";
            GameObject go = EncontrarPorNomeEmCena(nome);
            if (go == null)
            {
                Debug.LogWarning("[Road014] Nao encontrado: " + nome);
                continue;
            }

            // Manter eulerY = 224.503 (direcao diagonal consistente)
            go.transform.rotation = ROTACAO_PADRAO;

            // Ajustar escala para o padrao da referencia
            go.transform.localScale = new Vector3(ESCALA_PADRAO, ESCALA_PADRAO, ESCALA_PADRAO);

            // Posicionamento com espacamento uniforme
            if (n == 1)
            {
                // Segmento 1 ja foi posicionado acima (y=0, posicao inicial preservada)
                // Manter posicao original exceto y
                Debug.Log("[Road014] Segmento 1: posicao preservada exceto y=0 em " + pos1.ToString("0.###"));
                alinhados++;
                continue;
            }

            // Calcular posicao ao longo da direcao diagonal com espacamento uniforme
            // Direcao baseada na rotacao eulerY = 224.503
            // Vector3.forward rotacionado por 224.503 graus ao redor do eixo Y
            Quaternion rot = Quaternion.Euler(0f, 224.503f, 0f);
            Vector3 direcao = rot * Vector3.forward;
            // Normalizar (já deve estar normalizado, mas garantir)
            direcao = direcao.normalized;

            // Distancia total: (n-1) * espacamento
            float distancia = (n - 1) * ESPACAMENTO_PADRAO;

            // Posicao = posicao_inicial + distancia * direcao
            Vector3 posicao = pos1 + distancia * direcao;
            // Garantir y=0
            posicao.y = Y_ALTURA_PADRAO;

            go.transform.position = posicao;
            Debug.Log($"[Road014] Alinhado: {nome} pos={posicao.ToString("0.###")} dist={distancia:0.###} dir={direcao.ToString("0.###")}");
            alinhados++;
        }

        if (alinhados == 0)
            throw new Exception("Nenhuma copia Road_014 (1..24) encontrada na cena.");

        EditorSceneManager.MarkSceneDirty(cena);
        Debug.Log("[Road014] Concluido: " + alinhados + " copias alinhadas. Cena NAO salva — revise e salve manualmente no editor (Ctrl+S), ou feche sem salvar para descartar.");
    }

    private static GameObject EncontrarPorNomeEmCena(string nome)
    {
        Scene cena = SceneManager.GetActiveScene();
        foreach (GameObject raiz in cena.GetRootGameObjects())
        {
            if (raiz.name == nome) return raiz;
            foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == nome) return t.gameObject;
            }
        }
        return null;
    }
}

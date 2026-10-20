using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gera um LAYOUT BASE do campus na cena aberta, a partir dos packs de ambiente
/// importados (Palmov Island / Park / StarsandShells). É um ponto de partida
/// reproduzível: ruas em anel, prédios, praça com fonte, postes, bancos e
/// árvores. Rode com a cena Campus aberta, FORA do Play Mode.
///
/// Tudo é criado sob um único objeto raiz "Campus_Ambiente". Rodar de novo
/// recria o ambiente do zero (apaga e refaz o que ele mesmo criou), sem mexer
/// no resto da cena (Player, NPCs, UI, Systems).
///
/// Os tiles são medidos em runtime (bounds dos renderers) e assentados no chão
/// automaticamente, então funciona mesmo sem saber o tamanho/pivô de cada prefab.
/// Ajuste fino (rotação dos cantos, posição dos prédios) é esperado e fácil à mão.
/// </summary>
public static class GerarLayoutCampus
{
    private const string MENU = "Tools/IFNMG/Gerar Layout Base do Campus";
    private const string RAIZ = "Campus_Ambiente";
    private const string P = "Assets/Palmov Island/Low Poly Houses Free Pack/Prefabs/";

    // Caminhos dos prefabs usados (verificados no projeto).
    private const string ROAD_RETA    = P + "Roads/paved road straight.prefab";
    private const string ROAD_CURVA   = P + "Roads/paved road turn.prefab";
    private const string FONTE        = P + "Environment/fountain.prefab";
    private const string BANCO        = P + "Environment/bench.prefab";
    private const string POSTE        = P + "Environment/lamppost.prefab";
    private const string LIXEIRA      = P + "Environment/trash can.prefab";
    private const string PREDIO_CENTRAL = P + "Houses/city hall.prefab";
    private const string PREDIO_2     = P + "Houses/post office.prefab";
    private const string PREDIO_3     = P + "Houses/catholic temple.prefab";
    private const string PREDIO_4     = P + "Houses/pizzeria house.prefab";

    private static readonly string[] ARVORES =
    {
        P + "Trees/tree.prefab",
        P + "Trees/round tree.prefab",
        P + "Trees/fir tree 1.prefab",
        P + "Trees/cottage tree 1.prefab"
    };

    private static readonly string[] ARBUSTOS =
    {
        P + "Plants/bush 1.prefab",
        P + "Plants/bush 2.prefab",
        P + "Plants/bush 3.prefab",
        P + "Plants/plants.prefab"
    };

    [MenuItem(MENU)]
    public static void Gerar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Gerar Layout",
                "Saia do Play Mode antes de gerar o layout.", "OK");
            return;
        }

        Scene cena = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (cena.path == null || !cena.name.Contains("Campus"))
        {
            bool continuar = EditorUtility.DisplayDialog("Gerar Layout",
                "A cena ativa é '" + cena.name + "'. O layout foi pensado para a cena Campus.\n" +
                "Deseja gerar aqui mesmo?", "Gerar aqui", "Cancelar");
            if (!continuar) return;
        }

        GameObject antigo = GameObject.Find(RAIZ);
        if (antigo != null)
        {
            bool recriar = EditorUtility.DisplayDialog("Gerar Layout",
                "Já existe '" + RAIZ + "' nesta cena.\nApagar e gerar de novo?", "Apagar e refazer", "Cancelar");
            if (!recriar) return;
            Object.DestroyImmediate(antigo);
        }

        GameObject raiz = new GameObject(RAIZ);
        Undo.RegisterCreatedObjectUndo(raiz, "Gerar Layout do Campus");

        Transform ruas = Grupo(raiz, "Ruas");
        Transform predios = Grupo(raiz, "Predios");
        Transform praca = Grupo(raiz, "Praca");
        Transform natureza = Grupo(raiz, "Natureza");

        PrepararChao(raiz);
        float raio = GerarAnelDeRuas(ruas);
        PosicionarPredios(predios, raio);
        PreencherPraca(praca, raio);
        EspalharNatureza(natureza, raio);

        EditorSceneManager.MarkSceneDirty(cena);

        Debug.Log("[GerarLayoutCampus] Ambiente gerado sob '" + RAIZ + "'.\n" +
                  "  Raio do anel de ruas: " + raio.ToString("N1") + "m.\n" +
                  "  Ajuste rotação dos cantos e posições à mão e salve a cena (Ctrl+S).");

        Selection.activeGameObject = raiz;
        EditorGUIUtility.PingObject(raiz);
    }

    // ------------------------------------------------------------------ chão

    private static void PrepararChao(GameObject raiz)
    {
        Material grama = ObterMaterialGrama();

        GameObject chao = GameObject.Find("Plane");
        if (chao != null && chao.transform.parent == null)
        {
            chao.transform.localScale = new Vector3(20f, 1f, 20f);
            var mr = chao.GetComponent<MeshRenderer>();
            if (mr != null && grama != null) mr.sharedMaterial = grama;
            chao.name = "Chao";
            Debug.Log("[GerarLayoutCampus] Reaproveitei o 'Plane' existente como chão (200x200m).");
            return;
        }

        chao = GameObject.CreatePrimitive(PrimitiveType.Plane);
        chao.name = "Chao";
        chao.transform.SetParent(raiz.transform);
        chao.transform.position = new Vector3(0f, -0.02f, 0f);
        chao.transform.localScale = new Vector3(20f, 1f, 20f);
        if (grama != null) chao.GetComponent<MeshRenderer>().sharedMaterial = grama;
    }

    private static Material ObterMaterialGrama()
    {
        const string caminho = "Assets/Materiais/Gramado.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (mat != null) return mat;

        if (!AssetDatabase.IsValidFolder("Assets/Materiais"))
        {
            AssetDatabase.CreateFolder("Assets", "Materiais");
        }

        Shader sh = Shader.Find("Standard");
        if (sh == null) return null;

        mat = new Material(sh) { color = new Color(0.32f, 0.52f, 0.28f) };
        AssetDatabase.CreateAsset(mat, caminho);
        AssetDatabase.SaveAssets();
        return mat;
    }

    // ------------------------------------------------------------ ruas (anel)

    /// <summary>Cria um anel retangular de ruas e devolve o raio (distância do centro ao lado).</summary>
    private static float GerarAnelDeRuas(Transform pai)
    {
        GameObject tReta = Instanciar(ROAD_RETA, pai, Vector3.zero, 0f);
        GameObject tCurva = Instanciar(ROAD_CURVA, pai, Vector3.zero, 0f);

        if (tReta == null || tCurva == null)
        {
            Debug.LogWarning("[GerarLayoutCampus] Prefabs de rua não encontrados — pulando o anel de ruas.");
            return 20f;
        }

        if (!Medir(tReta, out Vector3 rSize, out Vector3 rCenter, out _)) rSize = Vector3.one * 8f;
        Medir(tCurva, out Vector3 cSize, out Vector3 cCenter, out _);

        float comp = Mathf.Max(rSize.x, rSize.z);   // comprimento da reta
        bool retaNoX = rSize.x >= rSize.z;
        float meiaCurva = Mathf.Max(cSize.x, cSize.z) * 0.5f;

        const int porLado = 3;                       // retas por lado
        float a = meiaCurva + porLado * comp * 0.5f; // distância centro -> lado

        float inicio = -a + meiaCurva;
        float passo = comp;

        // Lados ao longo de X (topo z=+a e base z=-a)
        float rotX = retaNoX ? 0f : 90f;
        for (int i = 0; i < porLado; i++)
        {
            float x = inicio + passo * (i + 0.5f);
            ColocarTile(Instanciar(ROAD_RETA, pai, Vector3.zero, rotX), new Vector2(x, a), rotX, rCenter);
            ColocarTile(Instanciar(ROAD_RETA, pai, Vector3.zero, rotX), new Vector2(x, -a), rotX, rCenter);
        }

        // Lados ao longo de Z (esquerda x=-a e direita x=+a)
        float rotZ = retaNoX ? 90f : 0f;
        for (int i = 0; i < porLado; i++)
        {
            float z = inicio + passo * (i + 0.5f);
            ColocarTile(Instanciar(ROAD_RETA, pai, Vector3.zero, rotZ), new Vector2(-a, z), rotZ, rCenter);
            ColocarTile(Instanciar(ROAD_RETA, pai, Vector3.zero, rotZ), new Vector2(a, z), rotZ, rCenter);
        }

        // Cantos (rotação em 90° — ajuste à mão se o curvar apontar para o lado errado)
        ColocarTile(Instanciar(ROAD_CURVA, pai, Vector3.zero, 0f), new Vector2(a, a), 0f, cCenter);
        ColocarTile(Instanciar(ROAD_CURVA, pai, Vector3.zero, 90f), new Vector2(a, -a), 90f, cCenter);
        ColocarTile(Instanciar(ROAD_CURVA, pai, Vector3.zero, 180f), new Vector2(-a, -a), 180f, cCenter);
        ColocarTile(Instanciar(ROAD_CURVA, pai, Vector3.zero, 270f), new Vector2(-a, a), 270f, cCenter);

        return a;
    }

    // --------------------------------------------------------------- prédios

    private static void PosicionarPredios(Transform pai, float raio)
    {
        string[] prefabs = { PREDIO_CENTRAL, PREDIO_2, PREDIO_3, PREDIO_4 };
        Vector2[] dirs =
        {
            new Vector2(0f, 1f),   // norte
            new Vector2(1f, 0f),   // leste
            new Vector2(0f, -1f),  // sul
            new Vector2(-1f, 0f)   // oeste
        };

        float dist = raio + 12f;

        for (int i = 0; i < prefabs.Length; i++)
        {
            Vector2 d = dirs[i];
            Vector3 pos = new Vector3(d.x * dist, 0f, d.y * dist);
            float rotY = Mathf.Atan2(-d.x, -d.y) * Mathf.Rad2Deg; // frente voltada ao centro

            GameObject go = Instanciar(prefabs[i], pai, pos, rotY);
            AssentarNoChao(go);
        }
    }

    // ----------------------------------------------------------------- praça

    private static void PreencherPraca(Transform pai, float raio)
    {
        GameObject fonte = Instanciar(FONTE, pai, Vector3.zero, 0f);
        AssentarNoChao(fonte);

        float r = raio - 2f;
        int voltas = 8;
        for (int i = 0; i < voltas; i++)
        {
            float ang = (Mathf.PI * 2f / voltas) * i;
            Vector3 pos = new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
            float rotY = -ang * Mathf.Rad2Deg; // olhando para o centro

            string prefab = (i % 2 == 0) ? POSTE : BANCO;
            GameObject go = Instanciar(prefab, pai, pos, rotY);
            AssentarNoChao(go);
        }

        // Duas lixeiras junto à fonte
        for (int i = 0; i < 2; i++)
        {
            float ang = (i == 0) ? 0.6f : Mathf.PI + 0.6f;
            Vector3 pos = new Vector3(Mathf.Cos(ang) * 4f, 0f, Mathf.Sin(ang) * 4f);
            GameObject go = Instanciar(LIXEIRA, pai, pos, 0f);
            AssentarNoChao(go);
        }
    }

    // --------------------------------------------------------------- natureza

    private static void EspalharNatureza(Transform pai, float raio)
    {
        List<Vector2> pontos = new List<Vector2>();
        float rdIni = raio + 4f;
        float rdFim = raio + 34f;

        int arvores = 0;
        int limite = 400;
        while (arvores < 60 && limite-- > 0)
        {
            Vector2 p = PontoAleatorio(rdIni, rdFim);
            if (!LongeDeOutros(p, pontos, 3.5f)) continue;

            pontos.Add(p);
            string prefab = ARVORES[Random.Range(0, ARVORES.Length)];
            GameObject go = Instanciar(prefab, pai, new Vector3(p.x, 0f, p.y), Random.value * 360f);
            AssentarNoChao(go);
            arvores++;
        }

        int arbustos = 0;
        limite = 400;
        while (arbustos < 40 && limite-- > 0)
        {
            Vector2 p = PontoAleatorio(raio + 3f, raio + 22f);
            if (!LongeDeOutros(p, pontos, 2f)) continue;

            pontos.Add(p);
            string prefab = ARBUSTOS[Random.Range(0, ARBUSTOS.Length)];
            GameObject go = Instanciar(prefab, pai, new Vector3(p.x, 0f, p.y), Random.value * 360f);
            AssentarNoChao(go);
            arbustos++;
        }
    }

    private static Vector2 PontoAleatorio(float rIni, float rFim)
    {
        float ang = Random.value * Mathf.PI * 2f;
        float r = Mathf.Lerp(rIni, rFim, Random.value);
        return new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r);
    }

    private static bool LongeDeOutros(Vector2 p, List<Vector2> pontos, float dist)
    {
        float d2 = dist * dist;
        foreach (Vector2 q in pontos)
        {
            if ((p - q).sqrMagnitude < d2) return false;
        }
        return true;
    }

    // --------------------------------------------------------------- helpers

    private static Transform Grupo(GameObject raiz, string nome)
    {
        GameObject g = new GameObject(nome);
        g.transform.SetParent(raiz.transform);
        return g.transform;
    }

    private static GameObject Instanciar(string caminho, Transform pai, Vector3 pos, float rotY)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
        if (prefab == null)
        {
            Debug.LogWarning("[GerarLayoutCampus] Prefab não encontrado: " + caminho);
            return null;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pai);
        go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        go.transform.position = pos;
        return go;
    }

    private static void ColocarTile(GameObject go, Vector2 alvoXZ, float rotY, Vector3 centerLocal)
    {
        if (go == null) return;

        Quaternion rot = Quaternion.Euler(0f, rotY, 0f);
        Vector3 off = rot * centerLocal;

        go.transform.rotation = rot;
        go.transform.position = new Vector3(alvoXZ.x - off.x, 0f, alvoXZ.y - off.z);
        AssentarNoChao(go);
    }

    /// <summary>Levanta o objeto para que o ponto mais baixo dos seus renderers fique em y=0.</summary>
    private static void AssentarNoChao(GameObject go)
    {
        if (go == null) return;
        if (!Medir(go, out _, out _, out float minY)) return;

        Vector3 p = go.transform.position;
        p.y -= minY;
        go.transform.position = p;
    }

    private static bool Medir(GameObject go, out Vector3 size, out Vector3 center, out float minY)
    {
        size = Vector3.one;
        center = Vector3.zero;
        minY = 0f;

        if (go == null) return false;

        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return false;

        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);

        size = b.size;
        center = b.center;
        minY = b.min.y;
        return true;
    }
}

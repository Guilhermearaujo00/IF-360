using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gera um LAYOUT BASE do campus na cena aberta, a partir dos packs de ambiente
/// importados. É um ponto de partida reproduzível: ruas em anel, prédios, praça
/// com fonte, postes, bancos e árvores. Rode com a cena Campus aberta, FORA do
/// Play Mode.
///
/// Tudo é criado sob um único objeto raiz "Campus_Ambiente". Rodar de novo
/// recria o ambiente do zero (apaga e refaz o que ele mesmo criou), sem mexer
/// no resto da cena (Player, NPCs, UI, Systems).
///
/// Kits suportados (troque KIT_ATIVO abaixo):
///   - Pandazole : Pandazole_Ultimate_Pack (prédios + ruas + natureza coerentes).
///   - Palmov    : Palmov Island / Low Poly Houses Free Pack.
/// Bancos/postes/fonte vêm do Palmov em ambos os kits (o Pandazole City não tem).
///
/// Os tiles são medidos em runtime (bounds dos renderers) e assentados no chão
/// automaticamente, então funciona mesmo sem saber o tamanho/pivô de cada prefab.
/// Ajuste fino (rotação dos cantos, posição dos prédios) é esperado e fácil à mão.
/// </summary>
public static class GerarLayoutCampus
{
    private const string MENU = "Tools/IFNMG/Gerar Layout Base do Campus";
    private const string RAIZ = "Campus_Ambiente";

    // ------------------------------------------------------------------ KIT
    public enum Kit { Pandazole, Palmov }

    /// <summary>Troque aqui para escolher quais packs alimentam o layout.</summary>
    private static readonly Kit KIT_ATIVO = Kit.Pandazole;

    // Caminhos preenchidos por DefinirKit() conforme KIT_ATIVO.
    private static string _reta;
    private static string _curva;
    private static string _fonte;
    private static string _banco;
    private static string _poste;
    private static string _lixeira;
    private static string[] _predios;
    private static string[] _arvores;
    private static string[] _arbustos;

    private static void DefinirKit()
    {
        // Props de praça compartilhados (Palmov — o Pandazole City não tem bancos/postes/fonte).
        const string PALMOV_ENV = "Assets/Palmov Island/Low Poly Houses Free Pack/Prefabs/Environment/";
        _fonte = PALMOV_ENV + "fountain.prefab";
        _banco = PALMOV_ENV + "bench.prefab";
        _poste = PALMOV_ENV + "lamppost.prefab";
        _lixeira = PALMOV_ENV + "trash can.prefab";

        if (KIT_ATIVO == Kit.Pandazole)
        {
            const string C = "Assets/Pandazole_Ultimate_Pack/Pandazole City Town Pack/Prefabs/";
            const string N = "Assets/Pandazole_Ultimate_Pack/Pandazole Nature Environment Pack/Prefabs/";

            _reta = C + "Env_Road_Straight_01.prefab";
            _curva = C + "Env_Road_Cornor_01.prefab";

            _predios = new[]
            {
                C + "Env_CommercialBuilding_01.prefab",
                C + "Env_CompanyBuilding_01.prefab",
                C + "Env_Motel_01.prefab",
                C + "Env_ResidentBuilding_01.prefab"
            };

            _arvores = new[]
            {
                N + "Tree_01_Spring.prefab",
                N + "Tree_02_Spring.prefab",
                N + "Tree_03_Spring.prefab",
                C + "Prop_Tree_01.prefab"
            };

            _arbustos = new[]
            {
                N + "Bush_01.prefab",
                N + "Bush_02.prefab",
                N + "Bush_03.prefab",
                C + "Prop_Bush_01.prefab"
            };
        }
        else // Palmov
        {
            const string P = "Assets/Palmov Island/Low Poly Houses Free Pack/Prefabs/";

            _reta = P + "Roads/paved road straight.prefab";
            _curva = P + "Roads/paved road turn.prefab";

            _predios = new[]
            {
                P + "Houses/city hall.prefab",
                P + "Houses/post office.prefab",
                P + "Houses/catholic temple.prefab",
                P + "Houses/pizzeria house.prefab"
            };

            _arvores = new[]
            {
                P + "Trees/tree.prefab",
                P + "Trees/round tree.prefab",
                P + "Trees/fir tree 1.prefab",
                P + "Trees/cottage tree 1.prefab"
            };

            _arbustos = new[]
            {
                P + "Plants/bush 1.prefab",
                P + "Plants/bush 2.prefab",
                P + "Plants/bush 3.prefab",
                P + "Plants/plants.prefab"
            };
        }
    }

    [MenuItem(MENU)]
    public static void Gerar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Gerar Layout",
                "Saia do Play Mode antes de gerar o layout.", "OK");
            return;
        }

        Scene cena = SceneManager.GetActiveScene();
        if (!cena.name.Contains("Campus"))
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

        DefinirKit();

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

        Debug.Log("[GerarLayoutCampus] Ambiente gerado (kit: " + KIT_ATIVO + ") sob '" + RAIZ + "'.\n" +
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

    private static float GerarAnelDeRuas(Transform pai)
    {
        GameObject tReta = Instanciar(_reta, pai, Vector3.zero, 0f);
        GameObject tCurva = Instanciar(_curva, pai, Vector3.zero, 0f);

        if (tReta == null || tCurva == null)
        {
            Debug.LogWarning("[GerarLayoutCampus] Prefabs de rua não encontrados — pulando o anel de ruas.");
            return 20f;
        }

        if (!Medir(tReta, out Vector3 rSize, out Vector3 rCenter, out _)) rSize = Vector3.one * 8f;
        Medir(tCurva, out Vector3 cSize, out Vector3 cCenter, out _);

        float comp = Mathf.Max(rSize.x, rSize.z);
        bool retaNoX = rSize.x >= rSize.z;
        float meiaCurva = Mathf.Max(cSize.x, cSize.z) * 0.5f;

        const int porLado = 3;
        float a = meiaCurva + porLado * comp * 0.5f;

        float inicio = -a + meiaCurva;
        float passo = comp;

        float rotX = retaNoX ? 0f : 90f;
        for (int i = 0; i < porLado; i++)
        {
            float x = inicio + passo * (i + 0.5f);
            ColocarTile(Instanciar(_reta, pai, Vector3.zero, rotX), new Vector2(x, a), rotX, rCenter);
            ColocarTile(Instanciar(_reta, pai, Vector3.zero, rotX), new Vector2(x, -a), rotX, rCenter);
        }

        float rotZ = retaNoX ? 90f : 0f;
        for (int i = 0; i < porLado; i++)
        {
            float z = inicio + passo * (i + 0.5f);
            ColocarTile(Instanciar(_reta, pai, Vector3.zero, rotZ), new Vector2(-a, z), rotZ, rCenter);
            ColocarTile(Instanciar(_reta, pai, Vector3.zero, rotZ), new Vector2(a, z), rotZ, rCenter);
        }

        ColocarTile(Instanciar(_curva, pai, Vector3.zero, 0f), new Vector2(a, a), 0f, cCenter);
        ColocarTile(Instanciar(_curva, pai, Vector3.zero, 90f), new Vector2(a, -a), 90f, cCenter);
        ColocarTile(Instanciar(_curva, pai, Vector3.zero, 180f), new Vector2(-a, -a), 180f, cCenter);
        ColocarTile(Instanciar(_curva, pai, Vector3.zero, 270f), new Vector2(-a, a), 270f, cCenter);

        return a;
    }

    // --------------------------------------------------------------- prédios

    private static void PosicionarPredios(Transform pai, float raio)
    {
        Vector2[] dirs =
        {
            new Vector2(0f, 1f),
            new Vector2(1f, 0f),
            new Vector2(0f, -1f),
            new Vector2(-1f, 0f)
        };

        float dist = raio + 12f;

        for (int i = 0; i < _predios.Length; i++)
        {
            Vector2 d = dirs[i % dirs.Length];
            Vector3 pos = new Vector3(d.x * dist, 0f, d.y * dist);
            float rotY = Mathf.Atan2(-d.x, -d.y) * Mathf.Rad2Deg;

            GameObject go = Instanciar(_predios[i], pai, pos, rotY);
            AssentarNoChao(go);
        }
    }

    // ----------------------------------------------------------------- praça

    private static void PreencherPraca(Transform pai, float raio)
    {
        AssentarNoChao(Instanciar(_fonte, pai, Vector3.zero, 0f));

        float r = raio - 2f;
        const int voltas = 8;
        for (int i = 0; i < voltas; i++)
        {
            float ang = (Mathf.PI * 2f / voltas) * i;
            Vector3 pos = new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
            float rotY = -ang * Mathf.Rad2Deg;

            string prefab = (i % 2 == 0) ? _poste : _banco;
            AssentarNoChao(Instanciar(prefab, pai, pos, rotY));
        }

        for (int i = 0; i < 2; i++)
        {
            float ang = (i == 0) ? 0.6f : Mathf.PI + 0.6f;
            Vector3 pos = new Vector3(Mathf.Cos(ang) * 4f, 0f, Mathf.Sin(ang) * 4f);
            AssentarNoChao(Instanciar(_lixeira, pai, pos, 0f));
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
            string prefab = _arvores[Random.Range(0, _arvores.Length)];
            AssentarNoChao(Instanciar(prefab, pai, new Vector3(p.x, 0f, p.y), Random.value * 360f));
            arvores++;
        }

        int arbustos = 0;
        limite = 400;
        while (arbustos < 40 && limite-- > 0)
        {
            Vector2 p = PontoAleatorio(raio + 3f, raio + 22f);
            if (!LongeDeOutros(p, pontos, 2f)) continue;

            pontos.Add(p);
            string prefab = _arbustos[Random.Range(0, _arbustos.Length)];
            AssentarNoChao(Instanciar(prefab, pai, new Vector3(p.x, 0f, p.y), Random.value * 360f));
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
        if (string.IsNullOrEmpty(caminho)) return null;

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

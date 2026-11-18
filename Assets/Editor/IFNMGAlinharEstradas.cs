using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IFNMGAlinharEstradas
{
    private const string CENA = "Assets/Scenes/Campus.unity";
    private const string PREFAB_ESTRADA = "Assets/CubexCube - Free City Pack I/Prefabs/Roads/Road.prefab";
    private const string NOME_RAIZ = "Mapa_IFNMG_Prototipo";
    private const string NOME_VIAS = "Vias";

    private const int TOTAL_ALVO = 65;
    private const float TOL_DUPLICADA = 0.01f;
    private const float COMPRIMENTO_MIN = 5f;
    private const float COMPRIMENTO_MAX = 30f;
    private const int ADICAO_MAX = 50;

    private class Estrada
    {
        public GameObject go;
        public Transform tr;
        public Vector3 pos;
        public float proj;
        public float perp;
    }

    [MenuItem("Tools/IFNMG 360/Alinhar estradas (Vias)")]
    public static void Executar()
    {
        try
        {
            ExecutarInterno();
        }
        catch (Exception ex)
        {
            Debug.LogError("[Estradas] FALHA: " + ex);
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
        string backup = Path.Combine(backupDir, "Campus.before-estradas-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity");
        File.Copy(cenaFull, backup, true);
        Debug.Log("[Estradas] Backup criado: " + backup);

        Scene cena = EditorSceneManager.OpenScene(CENA, OpenSceneMode.Single);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_ESTRADA);
        if (prefab == null)
            throw new Exception("Prefab de estrada nao encontrado: " + PREFAB_ESTRADA);
        string prefabPath = AssetDatabase.GetAssetPath(prefab);

        Bounds nativeBounds = CalcularBoundsLocais(prefab);
        float comprimentoNativo = nativeBounds.size.z;
        float larguraNativa = nativeBounds.size.x;
        Debug.Log("[Estradas] Prefab nativo: size=" + nativeBounds.size.ToString("0.###")
            + " comprimento(Z)=" + comprimentoNativo.ToString("0.###")
            + " largura(X)=" + larguraNativa.ToString("0.###"));
        if (comprimentoNativo < COMPRIMENTO_MIN || comprimentoNativo > COMPRIMENTO_MAX)
            throw new Exception("Comprimento nativo inesperado: " + comprimentoNativo.ToString("0.###")
                + " m (esperado entre " + COMPRIMENTO_MIN + " e " + COMPRIMENTO_MAX + ")");

        Transform vias = EncontrarTransform(NOME_RAIZ + "/" + NOME_VIAS);
        if (vias == null)
            throw new Exception("Transform nao encontrado: " + NOME_RAIZ + "/" + NOME_VIAS);

        Transform predios = EncontrarTransform(NOME_RAIZ + "/Modelos_IFNMG");
        string hashAntes = predios != null ? HashTransform(predios) : "";
        Debug.Log("[Estradas] Hash predios antes: " + hashAntes);

        List<Estrada> estradas = new List<Estrada>();
        for (int i = 0; i < vias.childCount; i++)
        {
            Transform ch = vias.GetChild(i);
            GameObject go = ch.gameObject;
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(go)) continue;
            if (PrefabPathDe(go) != prefabPath) continue;
            estradas.Add(new Estrada { go = go, tr = ch, pos = ch.position });
        }

        int instanciasAntes = estradas.Count;
        Debug.Log("[Estradas] Instancias do prefab Road sob Vias: " + instanciasAntes);
        if (instanciasAntes == 0)
            throw new Exception("Nenhuma instancia do prefab Road encontrada sob Vias.");

        int duplicadas = 0;
        List<Estrada> unicas = new List<Estrada>();
        for (int i = 0; i < estradas.Count; i++)
        {
            Estrada e = estradas[i];
            bool dup = false;
            for (int j = 0; j < unicas.Count; j++)
            {
                Estrada u = unicas[j];
                float dx = e.pos.x - u.pos.x;
                float dz = e.pos.z - u.pos.z;
                if (dx * dx + dz * dz < TOL_DUPLICADA * TOL_DUPLICADA)
                {
                    dup = true;
                    break;
                }
            }

            if (dup)
            {
                Debug.Log("[Estradas] Duplicada removida: " + e.go.name + " em " + e.pos.ToString("0.###"));
                UnityEngine.Object.DestroyImmediate(e.go);
                duplicadas++;
            }
            else
            {
                unicas.Add(e);
            }
        }

        Debug.Log("[Estradas] Unicas: " + unicas.Count + " | duplicadas removidas: " + duplicadas);
        if (unicas.Count < 2)
            throw new Exception("Estradas unicas insuficientes: " + unicas.Count);

        float cx = 0f, cz = 0f;
        for (int i = 0; i < unicas.Count; i++)
        {
            cx += unicas[i].pos.x;
            cz += unicas[i].pos.z;
        }
        cx /= unicas.Count;
        cz /= unicas.Count;

        float sxx = 0f, szz = 0f, sxz = 0f;
        for (int i = 0; i < unicas.Count; i++)
        {
            float dx = unicas[i].pos.x - cx;
            float dz = unicas[i].pos.z - cz;
            sxx += dx * dx;
            szz += dz * dz;
            sxz += dx * dz;
        }

        float theta = 0.5f * Mathf.Atan2(2f * sxz, sxx - szz);
        Vector2 d = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));

        Estrada minX = unicas[0];
        Estrada maxX = unicas[0];
        for (int i = 1; i < unicas.Count; i++)
        {
            if (unicas[i].pos.x < minX.pos.x) minX = unicas[i];
            if (unicas[i].pos.x > maxX.pos.x) maxX = unicas[i];
        }

        Vector2 primeiroUltimo = new Vector2(maxX.pos.x - minX.pos.x, maxX.pos.z - minX.pos.z);
        if (Vector2.Dot(d, primeiroUltimo) < 0f) d = -d;

        Vector2 normal = new Vector2(d.y, -d.x);
        float yaw = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;

        float tMin = float.MaxValue;
        float tMax = float.MinValue;
        float perpAbs = 0f;
        float perpMax = 0f;
        for (int i = 0; i < unicas.Count; i++)
        {
            Estrada e = unicas[i];
            float dx = e.pos.x - cx;
            float dz = e.pos.z - cz;
            e.proj = dx * d.x + dz * d.y;
            e.perp = dx * normal.x + dz * normal.y;
            if (e.proj < tMin) tMin = e.proj;
            if (e.proj > tMax) tMax = e.proj;
            float ap = Mathf.Abs(e.perp);
            perpAbs += ap;
            if (ap > perpMax) perpMax = ap;
        }

        float span = tMax - tMin;
        float perpMedio = perpAbs / unicas.Count;
        Debug.Log("[Estradas] Eixo: yaw=" + yaw.ToString("0.###")
            + " direcao=(" + d.x.ToString("0.######") + "," + d.y.ToString("0.######") + ")"
            + " span=" + span.ToString("0.###")
            + " | perp medio=" + perpMedio.ToString("0.###") + " max=" + perpMax.ToString("0.###"));

        if (span <= 1f)
            throw new Exception("Comprimento do eixo invalido: " + span);

        int alvo = TOTAL_ALVO;
        int faltam = alvo - unicas.Count;
        if (faltam < 0)
            throw new Exception("Existem " + unicas.Count + " estradas unicas, mais que o alvo " + alvo + ".");
        if (faltam > ADICAO_MAX)
            throw new Exception("Adicao excessiva de estradas: " + faltam);

        float espacamento = span / (alvo - 1);
        float escalaAlvo = espacamento / comprimentoNativo;
        if (espacamento <= 0f || escalaAlvo <= 0f)
            throw new Exception("Espacamento/escala invalidos.");
        if (escalaAlvo < 0.01f || escalaAlvo > 50f)
            throw new Exception("Escala alvo fora do esperado: " + escalaAlvo);

        Debug.Log("[Estradas] Alvo: " + alvo
            + " | adicionar=" + faltam
            + " | espacamento=" + espacamento.ToString("0.######")
            + " | escala=" + escalaAlvo.ToString("0.######"));

        float yMedio = 0f;
        for (int i = 0; i < unicas.Count; i++) yMedio += unicas[i].pos.y;
        yMedio /= unicas.Count;

        unicas.Sort((a, b) => a.proj.CompareTo(b.proj));

        List<GameObject> todas = new List<GameObject>();
        for (int i = 0; i < unicas.Count; i++) todas.Add(unicas[i].go);

        for (int k = 0; k < faltam; k++)
        {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(vias, false);
            todas.Add(go);
        }

        Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 escala = Vector3.one * escalaAlvo;

        for (int i = 0; i < todas.Count; i++)
        {
            Transform tr = todas[i].transform;
            float t = tMin + i * espacamento;
            Vector2 p2 = new Vector2(cx, cz) + d * t;
            tr.position = new Vector3(p2.x, yMedio, p2.y);
            tr.rotation = rot;
            tr.localScale = escala;
            todas[i].name = "Road_" + (i + 1).ToString("000");
        }

        string hashDepois = predios != null ? HashTransform(predios) : "";
        Debug.Log("[Estradas] Hash predios depois: " + hashDepois);
        if (hashAntes != hashDepois)
            throw new Exception("Hash dos predios mudou. Abortando sem salvar a cena.");

        EditorSceneManager.MarkSceneDirty(cena);
        bool salvo = EditorSceneManager.SaveScene(cena, CENA);
        if (!salvo)
            throw new Exception("Falha ao salvar a cena.");
        AssetDatabase.SaveAssets();

        Debug.Log("[Estradas] Cena salva: " + CENA
            + " | estradas=" + todas.Count
            + " | yaw=" + yaw.ToString("0.###")
            + " | espacamento=" + espacamento.ToString("0.######"));

        EscreverRelatorio(todas, instanciasAntes, duplicadas, unicas.Count, faltam,
            comprimentoNativo, larguraNativa, yaw, d, span, espacamento, escalaAlvo,
            yMedio, hashAntes, hashDepois, backup);
    }

    private static Bounds CalcularBoundsLocais(GameObject prefab)
    {
        GameObject temp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        try
        {
            temp.transform.position = Vector3.zero;
            temp.transform.rotation = Quaternion.identity;
            temp.transform.localScale = Vector3.one;
            Renderer[] rs = temp.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) return new Bounds(Vector3.zero, Vector3.one);
            Bounds b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(temp);
        }
    }

    private static string PrefabPathDe(GameObject go)
    {
        if (!PrefabUtility.IsAnyPrefabInstanceRoot(go)) return "";
        return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
    }

    private static Transform EncontrarTransform(string caminho)
    {
        GameObject go = GameObject.Find(caminho);
        if (go != null) return go.transform;

        Scene cena = SceneManager.GetActiveScene();
        foreach (GameObject raiz in cena.GetRootGameObjects())
        {
            Transform t = EncontrarRec(raiz.transform, caminho.Substring(caminho.LastIndexOf('/') + 1));
            if (t != null) return t;
        }
        return null;
    }

    private static Transform EncontrarRec(Transform t, string nome)
    {
        if (t.name == nome) return t;
        for (int i = 0; i < t.childCount; i++)
        {
            Transform f = EncontrarRec(t.GetChild(i), nome);
            if (f != null) return f;
        }
        return null;
    }

    private static string HashTransform(Transform raiz)
    {
        StringBuilder sb = new StringBuilder();
        Stack<Transform> pilha = new Stack<Transform>();
        pilha.Push(raiz);
        while (pilha.Count > 0)
        {
            Transform t = pilha.Pop();
            sb.Append(t.name).Append('|')
              .Append(t.localPosition.x.ToString("R")).Append(',')
              .Append(t.localPosition.y.ToString("R")).Append(',')
              .Append(t.localPosition.z.ToString("R")).Append('|')
              .Append(t.localRotation.x.ToString("R")).Append(',')
              .Append(t.localRotation.y.ToString("R")).Append(',')
              .Append(t.localRotation.z.ToString("R")).Append(',')
              .Append(t.localRotation.w.ToString("R")).Append('|')
              .Append(t.localScale.x.ToString("R")).Append(',')
              .Append(t.localScale.y.ToString("R")).Append(',')
              .Append(t.localScale.z.ToString("R")).Append('\n');

            for (int i = t.childCount - 1; i >= 0; i--)
                pilha.Push(t.GetChild(i));
        }

        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }

    private static string F(float v)
    {
        return v.ToString("0.######", CultureInfo.InvariantCulture);
    }

    private static void EscreverRelatorio(List<GameObject> todas, int instanciasAntes, int duplicadas,
        int unicas, int adicionadas, float comprimentoNativo, float larguraNativa, float yaw,
        Vector2 direcao, float span, float espacamento, float escala, float yMedio,
        string hashAntes, string hashDepois, string backup)
    {
        string dir = Path.GetFullPath("Library");
        Directory.CreateDirectory(dir);
        string caminho = Path.Combine(dir, "estradas_alinhadas_report.json");

        StringBuilder sb = new StringBuilder();
        sb.Append("{\n");
        sb.Append("  \"cena\": \"").Append(CENA.Replace("\\", "/")).Append("\",\n");
        sb.Append("  \"data\": \"").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("\",\n");
        sb.Append("  \"backup\": \"").Append(backup.Replace("\\", "/")).Append("\",\n");
        sb.Append("  \"prefab\": \"").Append(PREFAB_ESTRADA.Replace("\\", "/")).Append("\",\n");
        sb.Append("  \"comprimento_nativo_m\": ").Append(F(comprimentoNativo)).Append(",\n");
        sb.Append("  \"largura_nativa_m\": ").Append(F(larguraNativa)).Append(",\n");
        sb.Append("  \"instancias_antes\": ").Append(instanciasAntes).Append(",\n");
        sb.Append("  \"duplicadas_removidas\": ").Append(duplicadas).Append(",\n");
        sb.Append("  \"unicas\": ").Append(unicas).Append(",\n");
        sb.Append("  \"adicionadas\": ").Append(adicionadas).Append(",\n");
        sb.Append("  \"total\": ").Append(todas.Count).Append(",\n");
        sb.Append("  \"yaw_graus\": ").Append(F(yaw)).Append(",\n");
        sb.Append("  \"direcao_x\": ").Append(F(direcao.x)).Append(",\n");
        sb.Append("  \"direcao_z\": ").Append(F(direcao.y)).Append(",\n");
        sb.Append("  \"comprimento_eixo_m\": ").Append(F(span)).Append(",\n");
        sb.Append("  \"espacamento_m\": ").Append(F(espacamento)).Append(",\n");
        sb.Append("  \"escala\": ").Append(F(escala)).Append(",\n");
        sb.Append("  \"y_medio\": ").Append(F(yMedio)).Append(",\n");
        sb.Append("  \"hash_predios_antes\": \"").Append(hashAntes).Append("\",\n");
        sb.Append("  \"hash_predios_depois\": \"").Append(hashDepois).Append("\",\n");
        sb.Append("  \"instancias\": [\n");
        for (int i = 0; i < todas.Count; i++)
        {
            Transform tr = todas[i].transform;
            sb.Append("    {\"i\": ").Append(i + 1)
              .Append(", \"nome\": \"").Append(todas[i].name).Append("\"")
              .Append(", \"x\": ").Append(F(tr.position.x))
              .Append(", \"y\": ").Append(F(tr.position.y))
              .Append(", \"z\": ").Append(F(tr.position.z))
              .Append(", \"yaw\": ").Append(F(tr.eulerAngles.y))
              .Append(", \"escala\": ").Append(F(tr.localScale.x))
              .Append("}");
            if (i < todas.Count - 1) sb.Append(",");
            sb.Append("\n");
        }
        sb.Append("  ]\n");
        sb.Append("}\n");

        File.WriteAllText(caminho, sb.ToString());
        Debug.Log("[Estradas] Relatorio: " + caminho);
    }
}

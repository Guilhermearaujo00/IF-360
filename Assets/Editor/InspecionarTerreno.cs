using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Auditoria SOMENTE LEITURA do terreno/piso da cena Campus.
/// Mede os limites reais do piso, extremos por quadrante e propõe qual canto
/// corresponde ao canto superior-esquerdo (noroeste) do mapa de referência.
/// NÃO altera nem salva a cena. NÃO gera objetos.
///
/// Saída: Docs/MontagemCena/AUDITORIA_TERRENO_CAMPUS.md + auditoria_terreno.json
/// Menu:  Tools/IFNMG/Ambientação/Inspecionar Terreno e Limites
/// Batch: -executeMethod InspecionarTerreno.InspecionarBatch
/// </summary>
public static class InspecionarTerreno
{
    private const string CENA = "Assets/Scenes/Campus.unity";
    private const string SAIDA_MD = "Docs/MontagemCena/AUDITORIA_TERRENO_CAMPUS.md";
    private const string SAIDA_JSON = "Docs/MontagemCena/auditoria_terreno.json";

    [MenuItem("Tools/IFNMG/Ambientação/Inspecionar Terreno e Limites")]
    public static void InspecionarMenu() => Inspecionar();

    public static void InspecionarBatch() => Inspecionar();

    public static void Inspecionar()
    {
        var cena = EditorSceneManager.OpenScene(CENA, OpenSceneMode.Single);

        var pisos = new List<Renderer>();
        var todos = new List<Renderer>();

        foreach (var go in cena.GetRootObjects())
            Coletar(go.transform, pisos, todos);

        if (todos.Count == 0)
        {
            Debug.LogWarning("[InspecionarTerreno] Nenhum renderer encontrado.");
            return;
        }

        Bounds boundsTudo = Encapsular(todos);
        Bounds boundsPiso = pisos.Count > 0 ? Encapsular(pisos) : boundsTudo;

        // Extremos do piso por direção.
        var extremos = new Dictionary<string, (Vector3 ponto, string nome)>
        {
            { "minX", (new Vector3(float.MaxValue, 0, 0), "") },
            { "maxX", (new Vector3(float.MinValue, 0, 0), "") },
            { "minZ", (new Vector3(0, 0, float.MaxValue), "") },
            { "maxZ", (new Vector3(0, 0, float.MinValue), "") },
        };

        foreach (var r in pisos)
        {
            var b = r.bounds;
            var centro = b.center;
            if (centro.x < extremos["minX"].ponto.x) extremos["minX"] = (centro, r.gameObject.name);
            if (centro.x > extremos["maxX"].ponto.x) extremos["maxX"] = (centro, r.gameObject.name);
            if (centro.z < extremos["minZ"].ponto.z) extremos["minZ"] = (centro, r.gameObject.name);
            if (centro.z > extremos["maxZ"].ponto.z) extremos["maxZ"] = (centro, r.gameObject.name);
        }

        // Cobertura por amostragem no bounding box do piso (grid 2 m).
        int amostras = 0, comPiso = 0;
        var semPiso = new List<string>();
        const float passo = 2f;
        for (float x = boundsPiso.min.x; x <= boundsPiso.max.x; x += passo)
            for (float z = boundsPiso.min.z; z <= boundsPiso.max.z; z += passo)
            {
                amostras++;
                if (TemPisoProximo(pisos, x, z, 1.2f)) comPiso++;
                else if (semPiso.Count < 40) semPiso.Add($"({x:0.0}, {z:0.0})");
            }

        // Y de cada canto do bounding box do piso (via amostra de renderers).
        var cantos = new (string nome, float x, float z)[]
        {
            ("NW (minX, maxZ)", boundsPiso.min.x, boundsPiso.max.z),
            ("NE (maxX, maxZ)", boundsPiso.max.x, boundsPiso.max.z),
            ("SW (minX, minZ)", boundsPiso.min.x, boundsPiso.min.z),
            ("SE (maxX, minZ)", boundsPiso.max.x, boundsPiso.min.z),
        };

        var sb = new StringBuilder();
        sb.AppendLine("# Auditoria do terreno — cena Campus (somente leitura)");
        sb.AppendLine();
        sb.AppendLine($"- Data: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"- Cena: `{CENA}`");
        sb.AppendLine("- Nenhum objeto foi criado/movido/removido; a cena não foi salva.");
        sb.AppendLine("- Escala de trabalho: 1 unidade Unity = 1 m (provisório, não confirmado).");
        sb.AppendLine();
        sb.AppendLine("## 1. Renderers");
        sb.AppendLine();
        sb.AppendLine($"- Renderers totais: **{todos.Count}**");
        sb.AppendLine($"- Renderers candidatos a piso (baixos/grandes, Y ≤ 0,3 m): **{pisos.Count}**");
        sb.AppendLine();
        sb.AppendLine("## 2. Limites medidos (bounds)");
        sb.AppendLine();
        sb.AppendLine("| Conjunto | min X | max X | min Y | max Y | min Z | max Z | X×Y×Z (m) |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");
        sb.AppendLine($"| Tudo | {boundsTudo.min.x:0.00} | {boundsTudo.max.x:0.00} | {boundsTudo.min.y:0.00} | {boundsTudo.max.y:0.00} | {boundsTudo.min.z:0.00} | {boundsTudo.max.z:0.00} | {boundsTudo.size.x:0.0} × {boundsTudo.size.y:0.0} × {boundsTudo.size.z:0.0} |");
        sb.AppendLine($"| Piso | {boundsPiso.min.x:0.00} | {boundsPiso.max.x:0.00} | {boundsPiso.min.y:0.00} | {boundsPiso.max.y:0.00} | {boundsPiso.min.z:0.00} | {boundsPiso.max.z:0.00} | {boundsPiso.size.x:0.0} × {boundsPiso.size.y:0.0} × {boundsPiso.size.z:0.0} |");
        sb.AppendLine();
        sb.AppendLine($"**Dimensão do piso: {boundsPiso.size.x:0.0} × {boundsPiso.size.z:0.0} m** (comparar com a estimativa 500 × 330).");
        sb.AppendLine();
        sb.AppendLine("## 3. Extremos do piso (renderer mais a oeste/leste/norte/sul)");
        sb.AppendLine();
        sb.AppendLine("| Direção | X | Z | Renderer |");
        sb.AppendLine("|---|---|---|---|");
        sb.AppendLine($"| minX (oeste) | {extremos["minX"].ponto.x:0.00} | {extremos["minX"].ponto.z:0.00} | {extremos["minX"].nome} |");
        sb.AppendLine($"| maxX (leste) | {extremos["maxX"].ponto.x:0.00} | {extremos["maxX"].ponto.z:0.00} | {extremos["maxX"].nome} |");
        sb.AppendLine($"| minZ (sul) | {extremos["minZ"].ponto.x:0.00} | {extremos["minZ"].ponto.z:0.00} | {extremos["minZ"].nome} |");
        sb.AppendLine($"| maxZ (norte) | {extremos["maxZ"].ponto.x:0.00} | {extremos["maxZ"].ponto.z:0.00} | {extremos["maxZ"].nome} |");
        sb.AppendLine();
        sb.AppendLine("## 4. Cantos do bounding box do piso");
        sb.AppendLine();
        sb.AppendLine("| Canto | X | Z | Y medido |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var c in cantos)
        {
            sb.AppendLine($"| {c.nome} | {c.x:0.00} | {c.z:0.00} | {YProximo(pisos, c.x, c.z):0.00} |");
        }
        sb.AppendLine();
        sb.AppendLine("## 5. Cobertura por amostragem (grid 2 m)");
        sb.AppendLine();
        sb.AppendLine($"- Pontos: {comPiso}/{amostras} ({(amostras > 0 ? 100f * comPiso / amostras : 0f):0.0}%).");
        sb.AppendLine("- Estimativa por bounds, **não** prova de vão real.");
        sb.AppendLine();
        if (semPiso.Count > 0)
        {
            sb.AppendLine("Primeiros pontos sem piso próximo:");
            sb.AppendLine();
            sb.AppendLine(string.Join(", ", semPiso));
            sb.AppendLine();
        }
        sb.AppendLine("## 6. Candidato a canto Noroeste (superior-esquerdo do mapa)");
        sb.AppendLine();
        sb.AppendLine("Convenção do mapa de referência: `u` cresce para a direita (leste), `v` cresce para baixo (sul).");
        sb.AppendLine("Assumindo que o topo do mapa = norte = **+Z** e a esquerda = oeste = **-X** (a confirmar):");
        sb.AppendLine();
        sb.AppendLine($"- **Canto NW provável = (minX, maxZ) = ({boundsPiso.min.x:0.00}, {boundsPiso.max.z:0.00})**");
        sb.AppendLine($"- Canto NE provável = ({boundsPiso.max.x:0.00}, {boundsPiso.max.z:0.00})");
        sb.AppendLine($"- Canto SW provável = ({boundsPiso.min.x:0.00}, {boundsPiso.min.z:0.00})");
        sb.AppendLine($"- Canto SE provável = ({boundsPiso.max.x:0.00}, {boundsPiso.min.z:0.00})");
        sb.AppendLine();
        sb.AppendLine("> **ATENÇÃO:** a correspondência norte↔+Z e oeste↔-X é uma suposição, não um dado.");
        sb.AppendLine("> Sem um marco real confirmado (medida de um edifício ou distância entre dois marcos),");
        sb.AppendLine("> NÃO gerar objetos. Este documento apenas registra a geometria existente.");

        Directory.CreateDirectory(Path.GetDirectoryName(SAIDA_MD));
        File.WriteAllText(SAIDA_MD, sb.ToString(), new UTF8Encoding(false));

        var json = new StringBuilder();
        json.AppendLine("{");
        json.AppendLine($"  \"dimensao_piso_m\": {{ \"x\": {boundsPiso.size.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, \"z\": {boundsPiso.size.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)} }},");
        json.AppendLine($"  \"piso_min\": {{ \"x\": {boundsPiso.min.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, \"y\": {boundsPiso.min.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, \"z\": {boundsPiso.min.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)} }},");
        json.AppendLine($"  \"piso_max\": {{ \"x\": {boundsPiso.max.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, \"y\": {boundsPiso.max.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, \"z\": {boundsPiso.max.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)} }},");
        json.AppendLine($"  \"canto_nw_suposto\": {{ \"x\": {boundsPiso.min.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, \"z\": {boundsPiso.max.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)} }},");
        json.AppendLine($"  \"renderers_totais\": {todos.Count},");
        json.AppendLine($"  \"renderers_piso\": {pisos.Count},");
        json.AppendLine($"  \"amostras\": {amostras},");
        json.AppendLine($"  \"amostras_com_piso\": {comPiso}");
        json.AppendLine("}");
        File.WriteAllText(SAIDA_JSON, json.ToString(), new UTF8Encoding(false));

        Debug.Log($"[InspecionarTerreno] Piso {boundsPiso.size.x:0.0} x {boundsPiso.size.z:0.0} m; NW suposto " +
                  $"({boundsPiso.min.x:0.0}, {boundsPiso.max.z:0.0}). Relatórios em {SAIDA_MD} e {SAIDA_JSON}. Nada alterado.");
    }

    private static IEnumerable<GameObject> GetRootObjects(this Scene s)
    {
        foreach (var go in s.GetRootGameObjects()) yield return go;
    }

    private static void Coletar(Transform t, List<Renderer> pisos, List<Renderer> todos)
    {
        foreach (var r in t.GetComponentsInChildren<Renderer>(true))
        {
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            if (!todos.Contains(r)) todos.Add(r);
            var b = r.bounds;
            if (b.min.y <= 0.3f && (b.size.x >= 8f || b.size.z >= 8f)) pisos.Add(r);
        }
    }

    private static Bounds Encapsular(List<Renderer> rs)
    {
        var b = rs[0].bounds;
        for (int i = 1; i < rs.Count; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    private static bool TemPisoProximo(List<Renderer> pisos, float x, float z, float tol)
    {
        foreach (var r in pisos)
        {
            var b = r.bounds;
            if (x >= b.min.x - tol && x <= b.max.x + tol && z >= b.min.z - tol && z <= b.max.z + tol) return true;
        }
        return false;
    }

    private static float YProximo(List<Renderer> pisos, float x, float z)
    {
        foreach (var r in pisos)
        {
            var b = r.bounds;
            if (x >= b.min.x && x <= b.max.x && z >= b.min.z && z <= b.max.z) return b.min.y;
        }
        return 0f;
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Exporta a LISTA DE PREFABS dos packs de ambiente para uma tabela Markdown,
/// com pasta do pacote, caminho, nome e tamanho em metros (bounds dos renderers
/// já com a escala do import). Rode FORA do Play Mode.
///
/// Saída: Docs/PREFABS_TABELA.md
/// Colunas: Categoria | Pacote | Caminho | Nome | X (m) | Y (m) | Z (m)
///
/// A categoria (Prédios / Vegetação / Vias / Céu / Terrain / Props) é inferida
/// pelo nome do prefab e, quando não bate, pelo pacote de origem. É um ponto de
/// partida: ajuste o dicionário abaixo se algum caso cair na categoria errada.
/// </summary>
public static class ExportarTabelaPrefabs
{
    private const string MENU = "Tools/IFNMG/Exportar Tabela de Prefabs (Markdown)";
    private const string SAIDA = "Docs/PREFABS_TABELA.md";

    // Pastas que NÃO são packs de ambiente (não entram na lista).
    private static readonly HashSet<string> Ignorar = new HashSet<string>
    {
        "Editor", "Scripts", "Resources", "Prefabs", "Dados", "Scenes",
        "Input", "Art", "Modelos", "TextMesh Pro", "_Recovery", "Audio"
    };

    // Categoria padrão por pacote (fallback quando o nome não decide).
    private static readonly Dictionary<string, string> CategoriaDoPacote = new Dictionary<string, string>
    {
        // Vegetação
        { "Anime Trees", "Vegetação" },
        { "BigRookGames", "Vegetação" },
        { "BluBlu Games", "Vegetação" },
        { "3D set of stylized nature - GHIBLI style", "Vegetação" },
        { "Lowpoly Style", "Vegetação" },
        { "Nature", "Vegetação" },
        { "PolyG", "Vegetação" },
        { "Ultrasonic", "Vegetação" },
        { "VertexRageStudio", "Vegetação" },
        { "VoxelNest", "Vegetação" },
        { "Vegetation_Stylized_Pack_ByLuxArtStudios", "Vegetação" },
        { "JC_StylizedNature_Lite", "Vegetação" },
        { "3D_Game_Assets_Flora", "Vegetação" },
        { "AGWYN's Low Poly Destructible Vegetation", "Vegetação" },
        { "Holotna", "Vegetação" },
        { "IL3DN", "Vegetação" },
        { "lowpoly-nature", "Vegetação" },
        // Céu
        { "BOXOPHOBIC", "Céu" },
        { "SimpleSky", "Céu" },
        { "Fantasy Skybox FREE", "Céu" },
        // Terrain
        { "Rowlan", "Terrain" },
        { "Dzeruza", "Terrain" },
    };

    [MenuItem(MENU)]
    public static void Exportar()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        var linhas = new List<Linha>();

        foreach (string guid in guids)
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            if (!caminho.StartsWith("Assets/", StringComparison.Ordinal)) continue;
            if (!caminho.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;

            string pacote = caminho.Substring("Assets/".Length).Split('/')[0];
            if (Ignorar.Contains(pacote)) continue;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
            if (prefab == null) continue;

            Vector3 tamanho = MedirTamanho(prefab);
            if (tamanho == Vector3.zero) continue; // sem renderer

            string nome = prefab.name;
            linhas.Add(new Linha
            {
                Categoria = Categorizar(pacote, nome),
                Pacote = pacote,
                Caminho = caminho,
                Nome = nome,
                X = tamanho.x,
                Y = tamanho.y,
                Z = tamanho.z
            });
        }

        linhas = linhas
            .OrderBy(l => l.Categoria)
            .ThenBy(l => l.Pacote)
            .ThenBy(l => l.Nome)
            .ToList();

        Escrever(linhas);
        Debug.Log($"[ExportarTabelaPrefabs] {linhas.Count} prefabs → {SAIDA}");
        EditorUtility.RevealInFinder(Path.GetFullPath(SAIDA));
    }

    /// <summary>Encapsula os bounds (mundo) de todos os renderers do prefab.</summary>
    private static Vector3 MedirTamanho(GameObject prefab)
    {
        GameObject inst = null;
        try
        {
            inst = UnityEngine.Object.Instantiate(prefab);
            inst.transform.position = Vector3.zero;
            inst.transform.rotation = Quaternion.identity;

            var renderers = inst.GetComponentsInChildren<Renderer>(true);
            bool tem = false;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var r in renderers)
            {
                if (r is ParticleSystemRenderer) continue;
                if (r is TrailRenderer || r is LineRenderer) continue;
                if (!tem) { bounds = r.bounds; tem = true; }
                else bounds.Encapsulate(r.bounds);
            }
            return tem ? bounds.size : Vector3.zero;
        }
        finally
        {
            if (inst != null) UnityEngine.Object.DestroyImmediate(inst);
        }
    }

    private static string Categorizar(string pacote, string nome)
    {
        string n = nome.ToLowerInvariant();

        if (Contem(n, "tree", "bush", "grass", "flower", "plant", "rock", "cliff",
                       "leaf", "foliage", "pine", "branch", "shrub", "fern", "mushroom",
                       "cactus", "tree", "stump", "log", "hedge", "vine", "crop", "trunk",
                       "arvore", "árvore", "planta", "grama", "flor"))
            return "Vegetação";

        if (Contem(n, "road", "street", "sidewalk", "curb", "traffic", "rail", "track",
                       "highway", "asphalt", "crosswalk", "intersection", "roundabout",
                       "pavement", "lane", "r_junction", "t_junction", "rua", "via", "pista"))
            return "Vias";

        if (CategoriaDoPacote.TryGetValue(pacote, out string cat))
            return cat;

        if (Contem(n, "building", "house", "shop", "market", "school", "church", "tower",
                       "facade", "apartment", "store", "cafe", "bank", "hospital", "office",
                       "prison", "hotel", "museum", "library", "station", "prédio", "predio"))
            return "Prédios";

        return "Props";
    }

    private static bool Contem(string texto, params string[] termos)
    {
        foreach (string t in termos)
            if (texto.Contains(t)) return true;
        return false;
    }

    private static void Escrever(List<Linha> linhas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# IFNMG 360° — Lista de Prefabs (packs de ambiente)");
        sb.AppendLine();
        sb.AppendLine($"Gerado por `Tools/IFNMG/Exportar Tabela de Prefabs`. Total: **{linhas.Count}** prefabs.");
        sb.AppendLine("Tamanho em metros = bounds dos renderers já com a escala do import.");
        sb.AppendLine();

        foreach (var grupo in linhas.GroupBy(l => l.Categoria).OrderBy(g => g.Key))
        {
            sb.AppendLine($"## {grupo.Key} ({grupo.Count()})");
            sb.AppendLine();
            sb.AppendLine("| Pacote | Caminho | Nome | X (m) | Y (m) | Z (m) |");
            sb.AppendLine("|---|---|---|---:|---:|---:|");
            foreach (var l in grupo)
                sb.AppendLine($"| {l.Pacote} | {l.Caminho} | {l.Nome} | {M(l.X)} | {M(l.Y)} | {M(l.Z)} |");
            sb.AppendLine();
        }

        string full = Path.GetFullPath(SAIDA);
        Directory.CreateDirectory(Path.GetDirectoryName(full));
        File.WriteAllText(full, sb.ToString(), new UTF8Encoding(false));
        AssetDatabase.Refresh();
    }

    private static string M(float v) =>
        v.ToString("0.00", CultureInfo.InvariantCulture);

    private class Linha
    {
        public string Categoria;
        public string Pacote;
        public string Caminho;
        public string Nome;
        public float X, Y, Z;
    }
}

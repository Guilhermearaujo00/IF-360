using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class IFNMGFixDuplicateScene
{
    private const string ScenePath = "Assets/Scenes/Campus.unity";

    public static void Fix()
    {
        string absolute = Path.GetFullPath(ScenePath);
        string backup = absolute + ".before_duplicate_fix";
        File.Copy(absolute, backup, true);

        string text = File.ReadAllText(absolute, Encoding.UTF8);
        string normalized = text.Replace("\r\n", "\n");
        string[] parts = normalized.Split(new[] { "--- !u!" }, StringSplitOptions.None);
        var seen = new HashSet<string>();
        var output = new StringBuilder(parts[0]);
        int removed = 0;

        for (int i = 1; i < parts.Length; i++)
        {
            string part = parts[i];
            Match header = Regex.Match(part, @"^\d+\s+&(-?\d+)");
            if (header.Success)
            {
                string id = header.Groups[1].Value;
                if (!seen.Add(id))
                {
                    removed++;
                    Debug.Log("[IFNMG FixDuplicate] removido bloco duplicado: " + id);
                    continue;
                }
            }
            output.Append("--- !u!").Append(part);
        }

        File.WriteAllText(absolute, output.ToString().Replace("\n", Environment.NewLine), new UTF8Encoding(false));
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("[IFNMG FixDuplicate] blocos removidos=" + removed);
        Debug.Log("[IFNMG FixDuplicate] backup=" + backup);
        Debug.Log("[IFNMG FixDuplicate] cena corrigida=" + ScenePath);
    }
}

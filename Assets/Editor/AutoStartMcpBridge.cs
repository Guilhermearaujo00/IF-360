using UnityEditor;
using MCPForUnity.Editor;

[InitializeOnLoad]
public static class AutoStartMcpBridge
{
    static AutoStartMcpBridge()
    {
        EditorApplication.delayCall += () =>
        {
            try
            {
                McpCiBoot.StartStdioForCi();
                UnityEngine.Debug.Log("[MCP] Stdio Bridge automatically started for Antigravity!");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning("[MCP] Failed to auto-start stdio bridge: " + ex.Message);
            }
        };
    }
}

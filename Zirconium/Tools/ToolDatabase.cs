using System.Text;
using System.Text.Json;
using System.Reflection;

namespace Zirconium.Tools;

public static class ToolDatabase //fix this entire class
{
    /// <summary>
    /// Tools are signed by caller
    /// </summary>
    private static List<(string, Tool)> Tools = new();
    public static void Add(string name, Tool tool) => Tools.Add((name, tool));
    public static List<string> ParseToolCalls(string input)
    {
        var result = new List<string>();
        int depth = 0;
        int start = -1;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '{')
            {
                if (depth == 0)
                    start = i;

                depth++;
            }
            else if (c == '}')
            {
                depth--;

                if (depth < 0)
                    throw new Exception("Invalid input: unmatched closing brace");

                if (depth == 0 && start != -1)
                {
                    result.Add(input.Substring(start, i - start + 1));
                    start = -1;
                }
            }
        }

        if (depth != 0)
            throw new Exception("Invalid input: unmatched opening brace");

        return result;
    }

    public static void LoadScannerResults(string agent, string tool_name, MemoryTable results)
    {
        if (Tools.Find(x => x.Item1 == agent && x.Item2.Name == tool_name).Item2 is Scanner scanner)
            scanner.ScanResults = results;
    }

    public static string CallTool(string json, List<Tool> tools)
    {
        //var root = JsonDocument.Parse(json).RootElement;
        //string tool_name = root.GetProperty("tool_name").GetString()!;
        //if (tool_name == "nmap")
        //{
        //    string target = root.GetProperty("target").GetString()!;
        //    Nmap nmap = (Nmap)(tools.Where(tool => tool.Name == "nmap").First()); //fix this
        //    string arguments = root.GetProperty("arguments").GetString()!;
        //    nmap.Scan(target, arguments);
        //    return nmap.scan_out;
        //}
        //return string.Empty;

        //look for tool name inside of tools
        // if it is a scanner -> do it and get the results

        var root = JsonDocument.Parse(json).RootElement;
        string tool_name = root.GetProperty("tool_name").GetString()!;

        Tool? tool = tools.Find(t => t.Name == tool_name);
        if (tool == null)
            return string.Empty;
        IReadOnlyList<string> p = GetScanParameterNames((Scanner)tool);

        var scanMethod = tool.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == nameof(Scanner.Scan))
            .First(m => m.GetParameters().Select(param => param.Name).SequenceEqual(p));

        var args = p.Select(key => (object?)root.GetProperty(key).GetString()).ToArray();
        var result = scanMethod.Invoke(tool, args);
        return string.Empty;
    }


    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    public static string ScannerUsage(Scanner scanner)
    {
        StringBuilder sb = new("{\n");
        sb = sb.Append($"\"tool_name\": {scanner.Name}");
        List<string> tool_arguments = GetScanParameterNames(scanner).ToList();
        foreach (string arg in tool_arguments)
            sb = sb.Append($",\n\"{arg}\": <{arg}>");
        sb.Append("\n}\n");
        return sb.ToString();
    }

    public static IReadOnlyList<string> GetScanParameterNames(Type scannerType)
    {
        ArgumentNullException.ThrowIfNull(scannerType);

        if (!typeof(Scanner).IsAssignableFrom(scannerType))
            throw new ArgumentException($"{scannerType.FullName} must derive from Scanner.", nameof(scannerType));

        var scans = scannerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == nameof(Scanner.Scan))
            .Where(m => !m.IsSpecialName)
            .DistinctBy(SignatureKey)
            .ToArray();

        return scans.Length switch
        {
            0 => throw new InvalidOperationException($"No public Scan overloads were found on {scannerType.FullName}."),
            1 => ParameterNames(scans[0]),
            > 2 => throw new InvalidOperationException($"Too many Scan overloads on {scannerType.FullName}."),
            2 => ParameterNames(ChooseBestOverload(scans)),
            _ => throw new InvalidOperationException("Unexpected reflection state.")
        };
    }

    public static IReadOnlyList<string> GetScanParameterNames(Scanner scanner)
        => GetScanParameterNames(scanner.GetType());

    private static MethodInfo ChooseBestOverload(MethodInfo[] scans)
    {
        bool IsSingleObjectScan(MethodInfo m)
        {
            var p = m.GetParameters();
            return p.Length == 1 && p[0].ParameterType == typeof(object);
        }

        var nonObjectScans = scans.Where(m => !IsSingleObjectScan(m)).ToArray();

        return nonObjectScans.Length switch
        {
            1 => nonObjectScans[0],
            > 1 => nonObjectScans
                .OrderByDescending(m => m.GetParameters().Length)
                .First(),
            _ => scans.Single(IsSingleObjectScan)
        };
    }

    private static IReadOnlyList<string> ParameterNames(MethodInfo method) =>
        method.GetParameters()
              .Select(p => p.Name ?? string.Empty)
              .ToArray();

    private static string SignatureKey(MethodInfo method) =>
        $"{method.Name}({string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName ?? p.ParameterType.Name))})";
}

//public static class ToolDatabase
//{
//    // agent -> (tool name -> tool)
//    public static Dictionary<string, Dictionary<string, Tool>> Tools
//        = new();

//    public static void Register(string agent, Tool tool)
//    {
//        if (!Tools.TryGetValue(agent, out var toolMap))
//        {
//            toolMap = new Dictionary<string, Tool>();
//            Tools[agent] = toolMap;
//        }

//        toolMap[tool.Name] = tool; // overwrite = update
//    }
//}
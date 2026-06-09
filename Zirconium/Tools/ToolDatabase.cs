using System.Text;
using System.Text.Json;
using System.Reflection;
using Zirconium.Agents;

namespace Zirconium.Tools;

public static class ToolDatabase
{
    /// <summary>
    /// Tools are signed by caller
    /// </summary>
    private static List<(string, Tool)> Tools = new();
    public static Stack<(ToolAgent, string)> ToolCallStack = new();
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

    public static async Task<string> CallTool(this ToolAgent agent, string json)
    {
        ToolCallStack.Push((agent, json)); await UIBinder.CallStackUpdate();
        uint num_tries = 0;
        string? result = null;
        while(num_tries < Config.ToolNumberOfTriesOnFailedUse && result  == null)
        {
            try
            {
                result = await RouteToolCall(json, agent.Tools);
                //wait for delay
            }
            catch (Exception e) 
            {
                var x = result;
                result = null;
                // :/ you basically have to get the caller involved also and also do this: mechanical tool? -> caller at fault. agentic tool? -> either caller or api at fault...
            }
            ++num_tries;
        }
        ToolCallStack.Pop(); await UIBinder.CallStackUpdate();
        return result!;
    }
    private static async Task<string> RouteToolCall(string json, List<Tool> tools)
    {
        var root = JsonDocument.Parse(json).RootElement;
        string tool_name = root.GetProperty("tool_name").GetString()!;

        Tool? tool = tools.Find(t => t.Name == tool_name);
        if (tool == null)
            throw new Exception($"The tool set {tools} does not contain the tool {tool_name}");
        string function = DecodedUsage(tool);
        IReadOnlyList<string> p = ReflectionUtils.GetParamNames(tool.GetType(), function);
        MethodInfo method = tool.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == function)
            .First(m => m.GetParameters().Select(param => param.Name).SequenceEqual(p));

        var args = p.Select(key => (object?)root.GetProperty(key).GetString()).ToArray().ConvertTypes(method);
        var call = method.Invoke(tool, args)!; 
        string result = call is Task<string> task ? await task : (string)call;
        return DecodedFeedback(tool, result);
    }

    public static string Usage(Tool tool)
    {
        StringBuilder sb = new("{\n");
        sb = sb.Append($"\"tool_name\": {tool.Name}");
        if (Config.ToolExplicitDescription && tool.ObtainingSource.Equals(Config.ZirconiumProject))
            sb.Append($",\n\"description\": {tool.Description}");
        IReadOnlyList<string> tool_arguments = GetToolParams(tool);
        foreach (string arg in tool_arguments)
            sb = sb.Append($",\n\"{arg}\": <{arg}>");
        sb.Append("\n}\n");
        return sb.ToString();
    }

    public static IReadOnlyList<string> GetToolParams(Tool tool) => ReflectionUtils.GetParamNames(tool.GetType(), DecodedUsage(tool));

    private static string DecodedUsage(Tool tool)
    {
        if (tool is Scanner)
            return nameof(Scanner.Scan);
        if (tool is ToolAgent)
            return nameof(ToolAgent.Ask);
        return string.Empty;
    }

    private static string DecodedFeedback(Tool tool, string result)
    {
        if (tool is Scanner scanner)
            return scanner.scan_out;
        if (tool is ToolAgent agent)
            return result;
        return string.Empty;
    }
}
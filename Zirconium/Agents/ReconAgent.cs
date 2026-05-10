using System.Text.Json;
using System.Text.RegularExpressions;
using Zirconium.Tools;
using Zirconium.Tools.Recon;

namespace Zirconium.Agents;

public class ReconAgent
{
    public GroqAgent agent;
    public List<Tool> tools
     = new List<Tool>() { new Nmap() };

    string context = """
        You are a Recon Agent that is able to Passive and Active Reconnaissance tools. Available tools to call are:
        {
            "tool_name": "nmap",
            "target" : <ip>,
            "arguments" : <arg>
        }
        Do not call tools directly, provide the json in the response and the system would parse it!
        """;

    public ReconAgent() { agent = new GroqAgent("openai/gpt-oss-safeguard-20b", 512); /*groq agent: qwen/qwen3-32b*/ }

    public static string ExtractJson(string input)
    {
        var match = Regex.Match(input, @"```json\s*(\{.*?\})\s*```", RegexOptions.Singleline);

        if (!match.Success)
            return string.Empty;

        return match.Groups[1].Value;
    }

    public async Task<string> Ask(string prompt)
    {
        context += prompt;
        var response = await agent.Ask(context);
        context += response;
        string tool_call = ExtractJson(response);
        if (tool_call != string.Empty)
        {
            var json = JsonDocument.Parse(tool_call).RootElement;
            Nmap nmap = new Nmap();
            string target = json.GetProperty("target").GetString()!;
            string arguments = json.GetProperty("arguments").GetString()!;
            nmap.Scan(target, arguments);
            context += $"Result: {nmap.scan_out}";
            response = await agent.Ask(context);
            context += response;
            return response;
        }

        return response;
    }
}

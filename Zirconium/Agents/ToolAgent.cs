using Zirconium.Tools;
namespace Zirconium.Agents;

public class ToolAgent : Tool
{
    public string Name { get; }

    public string Description { get; }
    public string SystemPrompt { get; }
    public Agent Agent { get; }
    public List<Tool> Tools { get; }

    public string ObtainingSource => Config.ZirconiumProject;

    public ToolAgent(string name, string description, Agent agent, List<Tool> tools)
    {
        Name = name; Description = description; Agent = agent; 
        SystemPrompt = $"You are a {Name} that {Description}.\n";

        ToolDatabase.AddRange(Name, tools);
        Tools = tools.Where(t => t.VerifyInstall()).ToList();
        if (Tools.Count > 0)
        {
            SystemPrompt += "Available tools to call are:\n";
            foreach (Tool tool in Tools) 
                SystemPrompt += ToolDatabase.Usage(tool);

            if (Config.ToolCallJSON)
                SystemPrompt += "Do not call tools directly, provide the json in the response and the system would parse it! All JSON in your response will result in a tool call!\n";
        }
    }
    public virtual async Task<string> Ask(string prompt)
    {
        prompt = SystemPrompt + prompt;
        string response = await Agent.Ask(prompt);
        List<string> tool_calls = ToolDatabase.ParseToolCalls(response);
        if (tool_calls.Count() > 0)
        {
            string tool_results = string.Empty;
            foreach (string tool_call in tool_calls)
            {
                string tool_out = await this.CallTool(tool_call); //await ToolDatabase.CallTool(tool_call, Tools);
                tool_results = $"{tool_results} {tool_out}\n";
            }
            return tool_results;
        }
        return response;
    }

    public bool VerifyInstall() => true;

    public string Version() => Config.ZirconiumCurrentVersion;
}

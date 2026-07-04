using Zirconium.Tools;

namespace Zirconium.Agents;

public class ReasoningAgent : ToolAgent
{
    public ReasoningType Reasoning { get; }
    public Context Context { get; }
    public ReasoningAgent(string name, string description, Agent agent, List<Tool> tools, ReasoningType reasoning, MemoryType memoryType) : base(name, description, agent, tools)
    {
        Reasoning = reasoning; 
        Context = new Context(memoryType);
    }

    private const uint MaxReasoningIterations = 12;
    public override async Task<string> Ask(string prompt)
    {
        prompt = SystemPrompt + Context.CombineContext(prompt);
        string response = await Agent.Ask(prompt);
        List<string> tool_calls = ToolDatabase.ParseToolCalls(response);
        string result;

        if (tool_calls.Count == 0)
            result = response;
        else
        {
            string tool_results = await ExecuteToolCalls(tool_calls);

            switch (Reasoning)
            {
                case ReasoningType.None:
                    result = tool_results;
                    break;

                case ReasoningType.Assist:
                    string assistPrompt = SystemPrompt + Context.Get() +
                        $"\n{prompt}\nTool results:\n{tool_results}\nProvide a clean response to the user based on these tool results.";
                    result = await Agent.Ask(assistPrompt);
                    break;

                case ReasoningType.Reasoning:
                    string reasoningPrompt = SystemPrompt + Context.Get() +
                        $"\n{prompt}\nTool results:\n{tool_results}\nContinue reasoning only if you cannot provide your final response.";
                    Context.Add($"Tool results:\n{tool_results}");
                    response = await Agent.Ask(reasoningPrompt);
                    tool_calls = ToolDatabase.ParseToolCalls(response);

                    uint iterations = 1;
                    while (tool_calls.Count > 0 && iterations < MaxReasoningIterations)
                    {
                        tool_results = await ExecuteToolCalls(tool_calls);
                        reasoningPrompt = SystemPrompt + Context.Get() +
                            $"\n{prompt}\nTool results:\n{tool_results}\nContinue reasoning or provide your final response.";
                        Context.Add($"Tool results:\n{tool_results}");
                        response = await Agent.Ask(reasoningPrompt);
                        tool_calls = ToolDatabase.ParseToolCalls(response);
                        iterations++;
                    }
                    result = response;
                    break;

                default:
                    result = tool_results;
                    break;
            }
        }

        Context.Add($"User: {prompt}\nAssistant: {result}");
        if(Context.MemoryType == MemoryType.Continuous)
            Context.Clear();
        return result;
    }
}

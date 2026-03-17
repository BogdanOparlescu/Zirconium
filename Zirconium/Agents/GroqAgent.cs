namespace Zirconium.Agents;

public class GroqAgent : Agent
{
    public string model;
    public uint max_output_tokens;
    public string reasoning_effort;
    public bool enableBrowserSearch;
    public bool enableCodeInterpreter;

    public GroqAgent(string model, uint max_output_tokens = 8192, string reasoning_effort = "medium", bool enableBrowserSearch=false, bool enableCodeInterpreter=false):base(ApiKeys.Groq()!)
    {
        this.model = model;
        this.max_output_tokens = max_output_tokens;
        this.reasoning_effort = reasoning_effort;
        this.enableBrowserSearch = enableBrowserSearch;
        this.enableCodeInterpreter = enableCodeInterpreter;

        max_tokens_alias = "max_completion_tokens";
        endpoint = "https://api.groq.com/openai/v1/chat/completions";
    }

    public Task<string> Ask(string prompt)
    {
        return base.Ask(prompt, model, max_output_tokens, reasoning_effort, enableBrowserSearch, enableCodeInterpreter);
    }
}

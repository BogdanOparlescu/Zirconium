using Zirconium.Agents;
using Zirconium.Tools;
using Zirconium.Tools.CodeScanners;
using Zirconium.Tools.Recon;

namespace Zirconium;

public sealed class Orchestra
{
    private static Orchestra? _instance;
    public static Orchestra Instance => _instance ??= new Orchestra();
    
    private Orchestra()
    {
        Initialize();
    }

    private ToolAgent? _Root { get; set; }
    public ToolAgent? _Selected { get; set; }
    private void Initialize()
    {
        ToolAgent reconA = new ToolAgent("Recon Agent", "Calls passive and active recon tools",
                                new GroqAgent("openai/gpt-oss-120b", 2048, "high", 0.8),
                                new List<Tool>() { new Nmap(), new Trivy(), new Gobuster(), new Subfinder() });
        ToolAgent main = new ReasoningAgent("Vulnerability Detection Agent", "Orchestrates vulnerability findings",
                            new CerebrasAgent("zai-glm-4.7", 4096, "high"),
                            new List<Tool>() { reconA, new Nmap() },
                            ReasoningType.Reasoning, MemoryType.Persistent);

        _Selected = main;
        _Root = main;
    }

    public ToolAgent RootAgent() => _Root!;
    public async Task<string> Process(string query) => await _Selected!.Ask(query);
}
